using ImageCampus.ToolBox.Services;
using System.Collections.Generic;

namespace Assets.Code.Entities
{
    internal class EntityRegistryView : IService
    {
        public bool IsPersistance => false;

        Dictionary<uint, EntityView> entities = new Dictionary<uint, EntityView>();

        internal EntityView this[uint entityID] => entities[entityID];

        public void OnEntityCreated(uint entityID, EntityView entityView)
        {
            entities[entityID] = entityView;
        }
    }
}
