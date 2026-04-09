using ImageCampus.ToolBox.Events;

namespace Assets.MultiplayerArchitecture.Code.Entities.Events
{
    public struct ClientLeft : IEvent
    {
        uint networkID;
        uint objectID;

        public void Assign(params object[] parameters)
        {
            networkID = (uint)parameters[0];
            objectID = (uint)parameters[1];
        }

        public void Reset()
        {
            networkID = default(uint);
            objectID = default(uint);
        }
    }
}
