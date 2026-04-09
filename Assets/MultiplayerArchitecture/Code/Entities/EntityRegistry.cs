using ImageCampus.ToolBox.Services;
using System.Collections.Generic;

namespace Assets.MultiplayerArchitecture.Code.Entities
{
    public class EntityRegistry : IService
    {
        public bool IsPersistance => false;

        private Dictionary<uint, Entity> entities = new Dictionary<uint, Entity>();

        internal void Add(Entity entity)
        {
            entities[entity.ID] = entity;
        }

        internal void Remove(uint objectID)
        {
            entities.Remove(objectID);
        }

        internal Entity this[uint entityID] => entities[entityID];
    }
}
