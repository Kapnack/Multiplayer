using Assets.MultiplayerArchitecture.Code.Entities.Events;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;

namespace Assets.MultiplayerArchitecture.Code.Entities
{
    internal class EntityFactory : IService
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
        public bool IsPersistance => false;

        private uint currentEntityID = 0;

        public uint Create(uint networkID)
        {
           ++currentEntityID;

            Entity newEntity = new Entity(currentEntityID, networkID);

            EntityRegistry.Add(newEntity);

            EventBus.Raise<EntityCreated>(networkID, currentEntityID);

            return currentEntityID;
        }
    }
}