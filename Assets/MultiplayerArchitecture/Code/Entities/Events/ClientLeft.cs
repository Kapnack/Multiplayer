using ImageCampus.ToolBox.Events;

namespace Assets.MultiplayerArchitecture.Code.Entities.Events
{
    public struct ClientLeft : IEvent
    {
        public uint networkID;

        public void Assign(params object[] parameters)
        {
            networkID = (uint)parameters[0];
        }

        public void Reset()
        {
            networkID = default(uint);
        }
    }
}
