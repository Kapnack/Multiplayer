
using Assets.Code.Architecture.Code.Entities;
using Assets.Code.Architecture.Code.Entities.Events;
using ImageCampus.ToolBox.Blueprints;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

public class EntityFactory : IService, IDisposable
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    public bool IsPersistance => false;

    private uint lastAssignedEntityId;

    Dictionary<Type, ConstructorInfo> constructores;


    private Dictionary<Type, object> creationSubsctiptions;
    private MethodInfo registerEntityMethod;
    private MethodInfo subscriveToCreationMethod;
    private MethodInfo unsubscriveMethod;
    private MethodInfo raiseEntityCreatedMethod;

    public EntityFactory()
    {
        lastAssignedEntityId = Entity.UNASSIGNED_ENTITY_ID;

        constructores = new Dictionary<Type, ConstructorInfo>();

        registerEntityMethod = EntityRegistry.GetType().GetMethod(EntityRegistry.RegisterMethodName, BindingFlags.NonPublic | BindingFlags.Instance);

        raiseEntityCreatedMethod = GetType().GetMethod(nameof(RaiseEntityCreated), BindingFlags.NonPublic | BindingFlags.Instance);

        subscriveToCreationMethod = GetType().GetMethod(nameof(SubscribeToCreation), BindingFlags.NonPublic | BindingFlags.Instance);
        unsubscriveMethod = typeof(EventBus).GetMethod(nameof(EventBus.Unsubscribe), BindingFlags.Public | BindingFlags.Instance);

        RegisterEntityMethods();
    }

    public void Create<TEntity>(uint networkID) where TEntity : Entity
    {
        ++lastAssignedEntityId;

        object newEntity = constructores[typeof(TEntity)].Invoke(new object[] { lastAssignedEntityId, networkID });

        registerEntityMethod.Invoke(EntityRegistry, new object[] { newEntity });

        List<Type> entityTypes = new List<Type>();
        Type currentType = null;

        do
        {
            currentType = currentType == null ? newEntity.GetType() : currentType.BaseType;
            entityTypes.Add(currentType);

        } while (currentType != typeof(Entity));

        for (int i = entityTypes.Count - 1; i >= 0; i--)
        {
            raiseEntityCreatedMethod.MakeGenericMethod(entityTypes[i]).Invoke(this, new object[] { newEntity });
        }
    }

    private void SubscribeToCreation<EntityType>() where EntityType : Entity
    {
        EventBus.EventCallback<SpawnRequestAcceptedEvent<EntityType>> callback =
            EventBus.SubscribeAndReturn<SpawnRequestAcceptedEvent<EntityType>>(SpawnEntity);
        creationSubsctiptions.Add(typeof(SpawnRequestAcceptedEvent<EntityType>), callback);

        void SpawnEntity(in SpawnRequestAcceptedEvent<EntityType> spawnRequestAcceptedEvent)
        {
            Create<EntityType>(spawnRequestAcceptedEvent.networkID);
        }
    }

    private void RaiseEntityCreated<EntityType>(EntityType newEntity) where EntityType : Entity
    {
        EventBus.Raise<EntityCreatedEvent<EntityType>>(newEntity.ID, newEntity.networkID, newEntity.position);
    }

    private void RegisterEntityMethods()
    {
        foreach (Type type in Assembly.GetCallingAssembly().GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            if (!typeof(Entity).IsAssignableFrom(type))
                continue;

            RegisterConstructors(type);
        }

        void RegisterConstructors(Type entityType)
        {
            foreach (ConstructorInfo constructor in entityType.GetConstructors())
            {
                ParameterInfo[] parameters = constructor.GetParameters();

                if (parameters.Length != 2 || parameters[0].ParameterType != typeof(uint) || parameters[1].ParameterType != typeof(uint))
                    continue;

                constructores[entityType] = constructor;
                break;
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
    }
}