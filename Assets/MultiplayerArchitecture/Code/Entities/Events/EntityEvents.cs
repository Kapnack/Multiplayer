using ImageCampus.ToolBox.Events;

namespace MultiplayerArchitecture
{
    public struct NetworkSpawnRequestAcceptedEvent<EntityType> : IEvent
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

    public struct LocalSpawnRequestAcceptedEvent<EntityType> : IEvent
    {
        public uint ownerNetworkID;
        public Coordinate coordinateToSpawn;

        public void Assign(params object[] parameters)
        {
            ownerNetworkID = (uint)parameters[0];
            coordinateToSpawn = (Coordinate)parameters[1];
        }

        public void Reset()
        {
            ownerNetworkID = default(uint);
            coordinateToSpawn = default(Coordinate);
        }
    }

    public struct NetworkSpawnRequestAcceptedEvent : IEvent
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
