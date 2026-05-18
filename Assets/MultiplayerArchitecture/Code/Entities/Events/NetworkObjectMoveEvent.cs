using ImageCampus.ToolBox.Events;
using MultiplayerArchitecture;

namespace Assets.MultiplayerArchitecture.Code.Entities.Events
{
    public struct NetworkObjectMoveEvent : IEvent
    {
        public uint ownerNetworkID;
        public uint objectNetworkID;
        public Coordinate coordinate;
        public Coordinate eulerRotation;

        public void Assign(params object[] parameters)
        {
            ownerNetworkID = (uint)parameters[0];
            objectNetworkID = (uint)parameters[1];
            coordinate = (Coordinate)parameters[2];
            eulerRotation = (Coordinate)parameters[3];
        }

        public void Reset()
        {
            ownerNetworkID = default(uint);
            objectNetworkID = default(uint);
            coordinate = default(Coordinate);
            eulerRotation = default(Coordinate);
        }
    }
}