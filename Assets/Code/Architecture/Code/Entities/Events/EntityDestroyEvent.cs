using ImageCampus.ToolBox.Events;

namespace Assets.Code.Architecture.Code.Entities.Events
{
    public struct EntityDestroyedEvent : IEvent
    {
        public uint entityID;
        public void Assign(params object[] parameters)
        {
            entityID = (uint)parameters[0];
        }

        public void Reset()
        {
            entityID = default(uint);
        }
    }
}
