using Assets.Code.Architecture.Code.Math;
using ImageCampus.ToolBox.Events;

namespace Assets.Code.Architecture.Code.Entities.Events
{
    public struct ClientMoveEvent : IEvent
    {
        public uint clientID;
        public Coordinate position;

        public void Assign(params object[] parameters)
        {
            clientID = (uint)parameters[0];
            position = (Coordinate)parameters[1];
        }

        public void Reset()
        {
            clientID = default(uint);
            position = default(Coordinate);
        }
    }
}
