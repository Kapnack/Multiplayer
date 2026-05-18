using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using MultiplayerArchitecture.Entities;
using MultiplayerView;
using MutliplayerView.Game.Mapping;
using System;
using System.Collections.Generic;

namespace Assets.Code.Entities
{
    internal class NetworkRegistryView : IService, IDisposable
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        public bool IsPersistance => false;

        Dictionary<uint, Dictionary<uint, EntityView>> entities;

        public int ClientsAmount => entities.Count;

        private Dictionary<uint, Dictionary<Type, List<uint>>> entityIdsPerType;
        internal EntityView Get(uint ownerNetworkID, uint objectNetworkID) => entities[ownerNetworkID][objectNetworkID];

        public IEnumerable<PlayerController> PlayerView(uint ownerNetworkID) => FilterEntities<PlayerController>(ownerNetworkID);
        public IEnumerable<ChasingBulletView> ChasingBulletView(uint ownerNetworkID) => FilterEntities<ChasingBulletView>(ownerNetworkID);

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


            if (!entityIdsPerType.TryGetValue(entityView.OwnerNetworkID, out Dictionary<Type, List<uint>> entityOwnerIdsPerType))
            {
                entityOwnerIdsPerType = new Dictionary<Type, List<uint>>();
                entityIdsPerType.Add(entityView.OwnerNetworkID, entityOwnerIdsPerType);
            }

            Type currentEntityType = null;

            do
            {
                currentEntityType = currentEntityType == null ? entityView.GetType() : currentEntityType.BaseType;

                if (!entityOwnerIdsPerType.ContainsKey(currentEntityType))
                    entityOwnerIdsPerType.Add(currentEntityType, new List<uint>());

                entityOwnerIdsPerType[currentEntityType].Add(entityView.ArchitectureID);

            } while (currentEntityType != typeof(EntityView));
        }

        private IEnumerable<EntityType> FilterEntities<EntityType>(uint ownerNetworkID) where EntityType : EntityView
        {
            if (entityIdsPerType.TryGetValue(ownerNetworkID, out Dictionary<Type, List<uint>> userEntitiesList))
                if (userEntitiesList.TryGetValue(typeof(EntityType), out List<uint> entityList))
                    foreach (uint objectNetworkID in entityList)
                        yield return entities[ownerNetworkID][objectNetworkID] as EntityType;
        }

        public bool Contains(uint ownerNetworkID, uint objectNetworkID)
        {
            if (!entities.TryGetValue(ownerNetworkID, out Dictionary<uint, EntityView> idByType))
                return false;

            return idByType.ContainsKey(objectNetworkID);
        }

        public IEnumerable<EntityType> AllOfType<EntityType>() where EntityType : EntityView
        {
            foreach (var ownerPair in entityIdsPerType)
            {
                uint ownerId = ownerPair.Key;
                Dictionary<Type, List<uint>> typesDict = ownerPair.Value;

                if (!typesDict.TryGetValue(typeof(EntityType), out List<uint> entityList))
                    continue;

                foreach (uint objectNetworkID in entityList)
                {
                    yield return entities[ownerId][objectNetworkID] as EntityType;
                }
            }
        }

        private void OnEntityDestroyed(in EntityDestroyedEvent entityDestroyedEvent)
        {
            if (!Contains(entityDestroyedEvent.ownerNetworkID, entityDestroyedEvent.objectNetworkID))
                return;

            EntityView entityView = entities[entityDestroyedEvent.ownerNetworkID][entityDestroyedEvent.objectNetworkID];
            entityView.Dispose();
            entities[entityDestroyedEvent.ownerNetworkID].Remove(entityDestroyedEvent.objectNetworkID);
            entityIdsPerType[entityDestroyedEvent.ownerNetworkID][entityView.GetType()].Remove(entityDestroyedEvent.objectNetworkID);
            UnityEngine.Object.Destroy(entityView.gameObject);
        }

        public void Dispose()
        {
            foreach (KeyValuePair<uint, Dictionary<uint, EntityView>> ownerDictionary in entities)
                foreach (KeyValuePair<uint, EntityView> entityDictionary in ownerDictionary.Value)
                    UnityEngine.Object.Destroy(entityDictionary.Value);
        }
    }
}
