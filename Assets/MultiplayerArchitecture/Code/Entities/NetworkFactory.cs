using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Assets.MultiplayerArchitecture.Code.Entities
{
    internal class NetworkFactory : IService, IDisposable
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

            EventBus.Subscribe<SpawnRequestAcceptedEvent>(RaiseEventAsGeneric);
        }

        private void RaiseEventAsGeneric(in SpawnRequestAcceptedEvent spawnRequestAcceptedEvent)
        {
            Type raisingType = entityClassNameToType[spawnRequestAcceptedEvent.entityTypeName];
            raiseEntityRequestAcceptedMethd.MakeGenericMethod(raisingType).Invoke(this, new object[] { spawnRequestAcceptedEvent });
        }

        private void CreateInstance<EntityType>(uint ownerNetworkID, uint objectNetworkID, Coordinate coordinate)
        {
            object newEntity = entityConstructors[typeof(EntityType)].Invoke(new object[] { ownerNetworkID, objectNetworkID, coordinate });

            registerNetworkObjectMethod.Invoke(NetworkRegistry, new object[] { newEntity });
        }

        private void RaseEntityRequestAccepted<EntityType>(SpawnRequestAcceptedEvent spawnRequestAcceptedEvent) where EntityType : Entity
        {
            EventBus.Raise<SpawnRequestAcceptedEvent<EntityType>>(spawnRequestAcceptedEvent.coordinateToSpawn, spawnRequestAcceptedEvent.entityTypeName);
        }

        private void SubscribeToCreation<EntityType>() where EntityType : Entity
        {
            EventBus.EventCallback<SpawnRequestAcceptedEvent<EntityType>> callback =
                EventBus.SubscribeAndReturn<SpawnRequestAcceptedEvent<EntityType>>(SpawnEntity);
            creationSubsctiptions.Add(typeof(SpawnRequestAcceptedEvent<EntityType>), callback);

            void SpawnEntity(in SpawnRequestAcceptedEvent<EntityType> spawnRequestAcceptedEvent)
            {
                CreateInstance<EntityType>(spawnRequestAcceptedEvent.ownerNetworkID, spawnRequestAcceptedEvent.objectNetworkID, spawnRequestAcceptedEvent.coordinateToSpawn);
            }
        }

        private void RaiseEntityCreated<EntityType>(EntityType newEntity) where EntityType : Entity
        {
            EventBus.Raise<EntityCreated<EntityType>>(newEntity.ownerNetworkID, newEntity.objectNetworkID, newEntity.coordinate);
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
                    if (parameters.Length == 2 &&
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
            EventBus.Unsubscribe<SpawnRequestAcceptedEvent>(RaiseEventAsGeneric);
        }
    }
}