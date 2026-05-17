using ImageCampus.ToolBox.Events;
using MultiplayerArchitecture;

namespace Assets.Code.Entities
{
    internal struct SpawnItemRequestEvent : IEvent
    {
        public Coordinate coordinate;

        public void Assign(params object[] parameters)
        {
            coordinate = (Coordinate)parameters[0];
        }

        public void Reset()
        {
            coordinate = default(Coordinate);
        }
    }
}