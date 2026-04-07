
using Assets.Code.Architecture.Code.Entities;
using Assets.Code.Architecture.Code.Entities.Events;
using ImageCampus.ToolBox.Blueprints;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

internal class EntityFactory : IService
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    public bool IsPersistance => false;

    private uint lastAssignedEntityId;

    Dictionary<Type, ConstructorInfo> constructores;

    private MethodInfo registerEntityMethod;

    public EntityFactory()
    {
        this.lastAssignedEntityId = Entity.UNASSIGNED_ENTITY_ID;

        constructores = new Dictionary<Type, ConstructorInfo>();

        registerEntityMethod = EntityRegistry.GetType().GetMethod(EntityRegistry.RegisterMethodName, BindingFlags.NonPublic | BindingFlags.Instance);

        RegisterEntityMethods();
    }

    public uint Create<TEntity>() where TEntity : Entity
    {
        ++lastAssignedEntityId;

        object newEntity = constructores[typeof(TEntity)].Invoke(new object[] { lastAssignedEntityId });

        registerEntityMethod.Invoke(EntityRegistry, new object[] { newEntity });

        return lastAssignedEntityId;
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

                if (parameters.Length != 1 || parameters[0].ParameterType != typeof(uint))
                    continue;

                constructores[entityType] = constructor;
                break;
            }
        }
    }
}