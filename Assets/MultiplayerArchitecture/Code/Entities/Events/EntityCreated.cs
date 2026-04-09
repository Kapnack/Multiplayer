using ImageCampus.ToolBox.Events;

namespace Assets.MultiplayerArchitecture.Code.Entities.Events
{
    public struct EntityCreated : IEvent
    {
        public uint networkClientID;
        public uint objectID;

        public void Assign(params object[] parameters)
        {
            networkClientID = (uint)parameters[0];
            objectID = (uint)parameters[1];
        }

        public void Reset()
        {
            networkClientID = default(uint);
            objectID = default(uint);
        }
    }
}
