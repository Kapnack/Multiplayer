using Assets.MultiplayerArchitecture.Code.Entities;
using ImageCampus.ToolBox.Services;

namespace Assets.Code.Entities
{
    internal class EntityView : ViewComponent
    {
        protected EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
        public uint OwnerNetworkID { get; private set; }
        public uint ArchitectureID { get; private set; }

        static public string SetIDMethod => nameof(SetID);

        protected Entity ArchitectureEntity => EntityRegistry.Get<Entity>(ArchitectureID, OwnerNetworkID);

        private void SetID(uint ownerNetworkID, uint architectureID)
        {
            OwnerNetworkID = ownerNetworkID;
            ArchitectureID = architectureID;
        }
    }
}