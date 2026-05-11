using ImageCampus.ToolBox.Events;

namespace MultiplayerArchitecture
{
    public struct EntityDestroyedEvent : IEvent
    {
        public uint ownerNetworkID;
        public uint objectNetworkID;
        public void Assign(params object[] parameters)
        {
            ownerNetworkID = (uint)parameters[0];
            objectNetworkID = (uint)parameters[1];
        }

        public void Reset()
        {
            ownerNetworkID = default(uint);
            objectNetworkID = default(uint);
        }
    }
}
