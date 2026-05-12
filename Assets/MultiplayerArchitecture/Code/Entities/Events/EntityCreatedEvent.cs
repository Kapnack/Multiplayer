using Assets.MultiplayerArchitecture.Code.Entities;
using ImageCampus.ToolBox.Events;

namespace MultiplayerArchitecture
{
    public struct EntityCreatedEvent<EntityType> : IEvent
    {
        public uint ownerNetworkID;
        public uint objectNetworkID;
        public Coordinate coordinate;

        public void Assign(params object[] parameters)
        {
            ownerNetworkID = (uint)parameters[0];
            objectNetworkID = (uint)parameters[1];
            coordinate = (Coordinate)parameters[2];
        }

        public void Reset()
        {
            ownerNetworkID = default(uint);
            objectNetworkID = default(uint);
            coordinate = default(Coordinate);
        }
    }
}
