using ImageCampus.ToolBox.Events;

namespace Assets.MultiplayerArchitecture.Code.Network
{
    public struct ServerShutDown : IEvent
    {
        public void Assign(params object[] parameters)
        {
        }

        public void Reset()
        {
        }
    }
}