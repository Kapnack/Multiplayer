using Assets.MultiplayerArchitecture.Code.Entities;
using ImageCampus.ToolBox.Events;

namespace MultiplayerArchitecture
{
    public struct SpawnRequestAcceptedEvent<EntityType> : IEvent
    {
        public uint ownerNetworkID;
        public uint objectNetworkID;
        public Coordinate coordinateToSpawn;

        public void Assign(params object[] parameters)
        {
            ownerNetworkID = (uint)parameters[0];
            objectNetworkID = (uint)parameters[1];
            coordinateToSpawn = (Coordinate)parameters[2];
        }

        public void Reset()
        {
            ownerNetworkID = default(uint);
            objectNetworkID = default(uint);
            coordinateToSpawn = default(Coordinate);
        }
    }

    public struct SpawnRequestAcceptedEvent : IEvent
    {
        public uint ownerNetworkID;
        public uint objectNetworkID;
        public Coordinate coordinateToSpawn;
        public string entityTypeName;

        public void Assign(params object[] parameters)
        {
            ownerNetworkID = (uint)parameters[0];
            objectNetworkID = (uint)parameters[1];
            coordinateToSpawn = (Coordinate)parameters[2];
        }

        public void Reset()
        {
            ownerNetworkID = default(uint);
            objectNetworkID = default(uint);
            coordinateToSpawn = default(Coordinate);
        }
    }
}
