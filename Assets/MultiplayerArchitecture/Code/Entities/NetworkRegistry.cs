using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using MultiplayerArchitecture.Entities;
using System;
using System.Collections.Generic;

namespace Assets.MultiplayerArchitecture.Code.Entities
{
    public class NetworkRegistry : IService
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        public bool IsPersistance => false;

        private Dictionary<uint, Dictionary<uint, Entity>> entities;
        private Dictionary<uint, Dictionary<Type, List<uint>>> entityIdsPerType;

        public string RegisterMethodName => nameof(Register);

        IEnumerable<Entity> GetEntitiesOf(uint ownerNetworkID) => FilterEntities<Entity>(ownerNetworkID);

        public NetworkRegistry()
        {
            entities = new Dictionary<uint, Dictionary<uint, Entity>>();
            entityIdsPerType = new Dictionary<uint, Dictionary<Type, List<uint>>>();

            EventBus.Subscribe<EntityDestroyedEvent>(OnEntityDestroyed);
        }

        private void OnEntityDestroyed(in EntityDestroyedEvent entityDestroyedEvent)
        {
            Entity entityView = entities[entityDestroyedEvent.ownerNetworkID][entityDestroyedEvent.objectNetworkID];
            entities[entityDestroyedEvent.ownerNetworkID].Remove(entityDestroyedEvent.objectNetworkID);
        }

        public bool Contains(uint owerNetworkID, uint objectNetworkID)
        {
            if (!entities.TryGetValue(owerNetworkID, out Dictionary<uint, Entity> idByType))
                return false;

            return idByType.ContainsKey(objectNetworkID);
        }

        private void Register(Entity entity)
        {
            if (!entities.ContainsKey(entity.ownerNetworkID))
                entities[entity.ownerNetworkID] = new Dictionary<uint, Entity>();

            entities[entity.ownerNetworkID].Add(entity.objectNetworkID, entity);
            Type currentEntityType = entity.GetType();

            do
            {
                currentEntityType = currentEntityType == null ? entity.GetType() : currentEntityType.BaseType;

                if (!entityIdsPerType.TryGetValue(entity.ownerNetworkID, out Dictionary<Type, List<uint>> ownerTypesPerIDs))
                {
                    ownerTypesPerIDs = new Dictionary<Type, List<uint>>();
                    entityIdsPerType.Add(entity.ownerNetworkID, ownerTypesPerIDs);
                }

                if (!ownerTypesPerIDs.TryGetValue(entity.GetType(), out List<uint> entityList))
                {
                    entityList = new List<uint>();
                    ownerTypesPerIDs.Add(entity.GetType(), entityList);
                }

                entityList.Add(entity.objectNetworkID);

            } while (currentEntityType != typeof(Entity));
        }


        public EntityType Get<EntityType>(uint ownerNetworkID, uint objectNetworkID) where EntityType : Entity
        {
            return (EntityType)entities[ownerNetworkID][objectNetworkID];
        }

        public Entity Get(uint ownerNetworkID, uint objectNetworkID)
        {
            return entities[ownerNetworkID][objectNetworkID];
        }

        public IEnumerable<EntityType> AllOfType<EntityType>() where EntityType : Entity
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

        public IEnumerable<EntityType> FilterEntities<EntityType>(uint ownerNetworkID) where EntityType : Entity
        {
            if (entityIdsPerType.ContainsKey(ownerNetworkID))
                if (entityIdsPerType[ownerNetworkID].ContainsKey(typeof(EntityType)))
                    foreach (uint ID in entityIdsPerType[ownerNetworkID][typeof(EntityType)])
                        yield return entities[ID] as EntityType;
        }

        internal Dictionary<uint, Entity> this[uint ownerID] => entities[ownerID];
    }
}
