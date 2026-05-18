using ImageCampus.ToolBox.Events;
using MultiplayerArchitecture;

namespace Assets.MultiplayerArchitecture.Code.Entities
{
    public struct LocalObjectMoveEvent : IEvent
    {
        public uint objectNetworkID;
        public Coordinate coordinate;
        public Rotation rotation;

        public void Assign(params object[] parameters)
        {
            objectNetworkID = (uint)parameters[0];
            coordinate = (Coordinate)parameters[1];
            rotation = (Rotation)parameters[2];
        }

        public void Reset()
        {
            objectNetworkID = default(uint);
            coordinate = default(Coordinate);
            rotation = default(Rotation);
        }
    }
}