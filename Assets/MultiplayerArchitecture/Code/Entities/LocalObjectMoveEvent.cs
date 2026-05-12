using ImageCampus.ToolBox.Events;
using MultiplayerArchitecture;

namespace Assets.MultiplayerArchitecture.Code.Entities
{
    internal struct LocalObjectMoveEvent : IEvent
    {
        public uint objectNetworkID;
        public Coordinate coordinate;

        public void Assign(params object[] parameters)
        {
            objectNetworkID = (uint)parameters[0];
            coordinate = (Coordinate)parameters[1];
        }

        public void Reset()
        {
            objectNetworkID = default(uint);
            coordinate = default(Coordinate);
        }
    }
}