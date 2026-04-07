using Assets.Code.Architecture.Code.Math;
using ImageCampus.ToolBox.Events;

namespace Assets.Code.Architecture.Code.Entities.Events
{
    public struct EntityCreatedEvent<EntityType> : IEvent where EntityType : Entity
    {
        public uint entityID;
        public Coordinate position;

        public void Assign(params object[] parameters)
        {
            entityID = (uint)parameters[0];
            position = (Coordinate)parameters[1];
        }

        public void Reset()
        {
            entityID = default(uint);
            position = default(Coordinate);
        }
    }
}
