using Assets.MultiplayerArchitecture.Code.Entities;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture.Entities;
using ZooArchitect.View.Mapping;

namespace Assets.Code.Entities
{
    [ViewOf(typeof(Entity))]
    public class EntityView : ViewComponent
    {
        protected NetworkRegistry EntityRegistry => ServiceProvider.Instance.GetService<NetworkRegistry>();
        public uint OwnerNetworkID;
        public uint ArchitectureID;

        static public string SetIDMethod => nameof(SetID);

        protected Entity ArchitectureEntity => EntityRegistry.Get<Entity>(ArchitectureID, OwnerNetworkID);

        private void SetID(uint ownerNetworkID, uint architectureID)
        {
            OwnerNetworkID = ownerNetworkID;
            ArchitectureID = architectureID;
        }
    }
}