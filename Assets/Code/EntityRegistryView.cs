using Assets.Code.Architecture.Code.Entities.Events;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;

namespace Assets.Code
{
    public class EntityRegistryView : IService
    {
        public bool IsPersistance => false;

        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        public string RegisterMethodName => nameof(Register);

        private Dictionary<uint, EntityView> entities;
        private Dictionary<Type, List<uint>> entityIdsPerType;


        public EntityRegistryView()
        {
            entities = new Dictionary<uint, EntityView>();
            entityIdsPerType = new Dictionary<Type, List<uint>>();
            EventBus.Subscribe<EntityDestroyedEvent>(OnEntityDestroyed);
        }

        private void Register(EntityView entityView)
        {
            entities.Add(entityView.ArchitectureEnitityID, entityView);
            Type currentEntityType = entityView.GetType();

            do
            {
                currentEntityType = currentEntityType == null ? entityView.GetType() : currentEntityType.BaseType;

                if (!entityIdsPerType.ContainsKey(currentEntityType))
                    entityIdsPerType.Add(currentEntityType, new List<uint>());

                entityIdsPerType[currentEntityType].Add(entityView.ArchitectureEnitityID);

            } while (currentEntityType != typeof(EntityView));
        }

        public EntityType GetAs<EntityType>(uint ID) where EntityType : EntityView
        {
            if (ID == Entity.UNASSIGNED_ENTITY_ID)
            {
                throw new NullReferenceException("Entity id 0 represents a null entity");
            }

            if (!entities.ContainsKey(ID))
            {
                throw new KeyNotFoundException(ID.ToString());
            }

            if (entities[ID] is not EntityType)
            {
                throw new InvalidCastException($"An attempt was made to obtain a type {entities[ID].GetType().Name}"
                                             + $"entity as type {typeof(EntityType).Name} from the EntityRegistry");
            }

            return entities[ID] as EntityType;
        }

        private void OnEntityDestroyed(in EntityDestroyedEvent entityDestroyedEvent)
        {
            EntityView entityView = entities[entityDestroyedEvent.entityID];
            entities.Remove(entityDestroyedEvent.entityID);

            UnityEngine.Object.Destroy(entityView.gameObject);
        }
    }
}
