using ImageCampus.ToolBox.Events;

namespace Assets.MultiplayerArchitecture.Code.Entities.Events
{
    public struct NetworkClientMove : IEvent
    {
        public uint networkID;
        public Coordinate coordinate;

        public void Assign(params object[] parameters)
        {
            networkID = (uint)parameters[0];
            coordinate = (Coordinate)parameters[1];
        }

        public void Reset()
        {
            networkID = default(uint);
            coordinate = default(Coordinate);
        }
    }
}