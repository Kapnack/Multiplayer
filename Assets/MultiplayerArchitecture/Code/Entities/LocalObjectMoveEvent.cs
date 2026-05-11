using ImageCampus.ToolBox.Events;

namespace Assets.MultiplayerArchitecture.Code.Entities
{
    internal struct LocalObjectMoveEvent : IEvent
    {
        public uint objectNetworkID;
        public Coordinate coordinate;

        public void Assign(params object[] parameters)
        {
            objectNetworkID = (uint)parameters[1];
            coordinate = (Coordinate)parameters[0];
        }

        public void Reset()
        {
            objectNetworkID = default(uint);
            coordinate = default(Coordinate);
        }
    }
}