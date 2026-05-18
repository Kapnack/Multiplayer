using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using MultiplayerArchitecture.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Assets.MultiplayerArchitecture.Code.Entities
{
    public class NetworkFactory : IService, IDisposable
    {
        private delegate void OnCreateEntity(uint ownerNetworkID, uint objectNetworkID);

        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        NetworkRegistry NetworkRegistry => ServiceProvider.Instance.GetService<NetworkRegistry>();
        public bool IsPersistance => false;

        private uint currentEntityID = 0;

        private Dictionary<string, Type> entityClassNameToType;
        private Dictionary<Type, ConstructorInfo> entityConstructors;
        private MethodInfo registerNetworkObjectMethod;
        private MethodInfo raiseEntityCreatedMethod;

        private Dictionary<Type, object> creationSubsctiptions;
        private MethodInfo subscriveToCreationMethod;
        private MethodInfo unsubscriveMethod;
        private MethodInfo raiseEntityRequestAcceptedMethd;

        internal NetworkFactory()
        {
            entityClassNameToType = new Dictionary<string, Type>();
            entityConstructors = new Dictionary<Type, ConstructorInfo>();
            creationSubsctiptions = new Dictionary<Type, object>();
            entityClassNameToType = new Dictionary<string, Type>();

            registerNetworkObjectMethod = NetworkRegistry.GetType().GetMethod(NetworkRegistry.RegisterMethodName, BindingFlags.NonPublic | BindingFlags.Instance);

            raiseEntityCreatedMethod = GetType().GetMethod(nameof(RaiseEntityCreated), BindingFlags.NonPublic | BindingFlags.Instance);

            subscriveToCreationMethod = GetType().GetMethod(nameof(SubscribeToCreation), BindingFlags.NonPublic | BindingFlags.Instance);
            raiseEntityRequestAcceptedMethd = GetType().GetMethod(nameof(RaseEntityRequestAccepted), BindingFlags.NonPublic | BindingFlags.Instance);
            unsubscriveMethod = typeof(EventBus).GetMethod(nameof(EventBus.Unsubscribe), BindingFlags.Public | BindingFlags.Instance);

            RegisterEntityMethods();

            EventBus.Subscribe<NetworkSpawnRequestAcceptedEvent>(RaiseEventAsGeneric);
        }

        private void RaiseEventAsGeneric(in NetworkSpawnRequestAcceptedEvent spawnRequestAcceptedEvent)
        {
            Type raisingType = entityClassNameToType[spawnRequestAcceptedEvent.entityTypeName];
            raiseEntityRequestAcceptedMethd.MakeGenericMethod(raisingType).Invoke(this, new object[] { spawnRequestAcceptedEvent });
        }

        private void CreateInstance<EntityType>(uint ownerNetworkID, uint objectNetworkID, Coordinate coordinate)
        {
            object newEntity = entityConstructors[typeof(EntityType)].Invoke(new object[] { ownerNetworkID, objectNetworkID, coordinate });

            registerNetworkObjectMethod.Invoke(NetworkRegistry, new object[] { newEntity as Entity });

            List<Type> entityTypes = new List<Type>();
            Type currentType = null;

            do
            {
                currentType = currentType == null ? newEntity.GetType() : currentType.BaseType;
                entityTypes.Add(currentType);
            } while (currentType != typeof(Entity));

            for (int i = entityTypes.Count - 1; i >= 0; i--)
                raiseEntityCreatedMethod.MakeGenericMethod(entityTypes[i]).Invoke(this, new object[] { newEntity });
        }

        private void RaseEntityRequestAccepted<EntityType>(NetworkSpawnRequestAcceptedEvent spawnRequestAcceptedEvent) where EntityType : Entity
        {
            EventBus.Raise<NetworkSpawnRequestAcceptedEvent<EntityType>>(spawnRequestAcceptedEvent.coordinateToSpawn, spawnRequestAcceptedEvent.entityTypeName);
        }

        private void SubscribeToCreation<EntityType>() where EntityType : Entity
        {
            EventBus.EventCallback<NetworkSpawnRequestAcceptedEvent<EntityType>> callback =
                EventBus.SubscribeAndReturn<NetworkSpawnRequestAcceptedEvent<EntityType>>(NetworkSpawnEntity);
            creationSubsctiptions.Add(typeof(NetworkSpawnRequestAcceptedEvent<EntityType>), callback);

            void NetworkSpawnEntity(in NetworkSpawnRequestAcceptedEvent<EntityType> spawnRequestAcceptedEvent)
            {
                CreateInstance<EntityType>(spawnRequestAcceptedEvent.ownerNetworkID, spawnRequestAcceptedEvent.objectNetworkID, spawnRequestAcceptedEvent.coordinateToSpawn);
            }

            EventBus.EventCallback<LocalSpawnRequestAcceptedEvent<EntityType>> localCallback = EventBus.SubscribeAndReturn<LocalSpawnRequestAcceptedEvent<EntityType>>(SpawnEntity);
            creationSubsctiptions.Add(typeof(LocalSpawnRequestAcceptedEvent<EntityType>), localCallback);

            void SpawnEntity(in LocalSpawnRequestAcceptedEvent<EntityType> spawnRequestAcceptedEvent)
            {
                CreateInstance<EntityType>(spawnRequestAcceptedEvent.ownerNetworkID, ++currentEntityID, spawnRequestAcceptedEvent.coordinateToSpawn);
            }
        }

        private void RaiseEntityCreated<EntityType>(EntityType newEntity) where EntityType : Entity
        {
            EventBus.Raise<EntityCreatedEvent<EntityType>>(newEntity.ownerNetworkID, newEntity.objectNetworkID, newEntity.coordinate);
        }

        private void RegisterEntityMethods()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsClass && !type.IsAbstract)
                {
                    if (typeof(Entity).IsAssignableFrom(type))
                    {
                        RegisterEntity(type);
                        subscriveToCreationMethod.MakeGenericMethod(type).Invoke(this, new object[0]);
                        entityClassNameToType.Add(type.Name, type);
                    }
                }
            }

            void RegisterEntity(Type entityType)
            {
                foreach (ConstructorInfo constructor in entityType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    ParameterInfo[] parameters = constructor.GetParameters();
                    if (parameters.Length == 3 &&
                        parameters[0].ParameterType == typeof(uint) &&
                        parameters[1].ParameterType == typeof(uint) &&
                        parameters[2].ParameterType == typeof(Coordinate))
                    {
                        entityConstructors.Add(entityType, constructor);
                        break;
                    }
                }
            }
        }

        private void UnsubscribeToCreation()
        {
            foreach (KeyValuePair<Type, object> methodsToUnsubscribe in creationSubsctiptions)
            {
                unsubscriveMethod.MakeGenericMethod(methodsToUnsubscribe.Key).
                    Invoke(EventBus, new object[] { methodsToUnsubscribe.Value });
            }
        }

        public void Dispose()
        {
            UnsubscribeToCreation();
            EventBus.Unsubscribe<NetworkSpawnRequestAcceptedEvent>(RaiseEventAsGeneric);
        }
    }
}