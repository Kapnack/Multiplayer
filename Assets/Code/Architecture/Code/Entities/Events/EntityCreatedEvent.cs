using Assets.Code.Architecture.Code.Math;
using ImageCampus.ToolBox.Events;

namespace Assets.Code.Architecture.Code.Entities.Events
{
    public struct EntityCreatedEvent<EntityType> : IEvent where EntityType : Entity
    {
        public uint entityID;
        public uint networkID;
        public Coordinate position;

        public void Assign(params object[] parameters)
        {
            entityID = (uint)parameters[0];
            networkID = (uint)parameters[1];
            position = (Coordinate)parameters[2];
        }

        public void Reset()
        {
            entityID = default(uint);
            networkID = default(uint);
            position = default(Coordinate);
        }
    }
}
