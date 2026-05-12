using Assets.MultiplayerArchitecture.Code.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using System;
using System.Collections.Generic;

namespace Assets.Code.Entities
{
    internal class NetworkRegistryView : IService
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        public bool IsPersistance => false;

        Dictionary<uint, Dictionary<uint, EntityView>> entities;

        private Dictionary<uint, Dictionary<Type, List<uint>>> entityIdsPerType;
        internal EntityView Get(uint ownerNetworkID, uint objectNetworkID) => entities[ownerNetworkID][objectNetworkID];

        public EntityType GetAs<EntityType>(uint ownerNetworkID, uint objectNetworkID) where EntityType : EntityView
        {
            if (objectNetworkID == Entity.UNASSIGNED_ENTITY_ID)
                throw new NullReferenceException("Entity id 0 represents a null entity");

            if (!entities.TryGetValue(ownerNetworkID, out Dictionary<uint, EntityView> entitiesOfOwner))
                throw new KeyNotFoundException("Owner: " + ownerNetworkID.ToString());
            
            if (entitiesOfOwner.ContainsKey(objectNetworkID))
                throw new KeyNotFoundException("Object: " + objectNetworkID.ToString());

            if (entitiesOfOwner[objectNetworkID] is not EntityType)
                throw new InvalidCastException($"An attempt was made to obtain a type {entitiesOfOwner[objectNetworkID].GetType().Name}"
                                             + $"entity as type {typeof(EntityType).Name} from the EntityRegistry");

            return entitiesOfOwner[objectNetworkID] as EntityType;
        }

        public string RegisterMethodName => nameof(OnEntityCreated);

        public NetworkRegistryView()
        {
            entities = new Dictionary<uint, Dictionary<uint, EntityView>>();
            entityIdsPerType = new Dictionary<uint, Dictionary<Type, List<uint>>>();
            EventBus.Subscribe<EntityDestroyedEvent>(OnEntityDestroyed);
        }

        private void OnEntityCreated(EntityView entityView)
        {
            if (!entities.TryGetValue(entityView.OwnerNetworkID, out Dictionary<uint, EntityView> ownerTypesPerID))
            {
                ownerTypesPerID = new Dictionary<uint, EntityView>();
                entities.Add(entityView.OwnerNetworkID, ownerTypesPerID);
            }

            entities[entityView.OwnerNetworkID].Add(entityView.ArchitectureID, entityView);

            Type currentEntityType = entityView.GetType();

            if (!entityIdsPerType.TryGetValue(entityView.OwnerNetworkID, out Dictionary<Type, List<uint>> entityOwnerIdsPerType))
            {
                entityOwnerIdsPerType = new Dictionary<Type, List<uint>>();
                entityIdsPerType.Add(entityView.OwnerNetworkID, entityOwnerIdsPerType);
            }

            do
            {
                currentEntityType = currentEntityType == null ? entityView.GetType() : currentEntityType.BaseType;

                if (!entityOwnerIdsPerType.ContainsKey(currentEntityType))
                    entityOwnerIdsPerType.Add(currentEntityType, new List<uint>());

                entityOwnerIdsPerType[currentEntityType].Add(entityView.ArchitectureID);

            } while (currentEntityType != typeof(EntityView));
        }

        private void OnEntityDestroyed(in EntityDestroyedEvent entityDestroyedEvent)
        {
            EntityView entityView = entities[entityDestroyedEvent.ownerNetworkID][entityDestroyedEvent.objectNetworkID];
            entityView.Dispose();
            entities[entityDestroyedEvent.ownerNetworkID].Remove(entityDestroyedEvent.objectNetworkID);
            UnityEngine.Object.Destroy(entityView.gameObject);
        }
    }
}
