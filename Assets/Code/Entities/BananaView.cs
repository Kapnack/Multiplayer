using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using MultiplayerView;
using MutliplayerView.Game.Mapping;
using Unity.VisualScripting;

namespace Assets.Code.Entities
{
    [ViewOf(typeof(Banana))]
    public class BananaView : EntityView
    {
        ImageCampus.ToolBox.Events.EventBus EventBus => ServiceProvider.Instance.GetService<ImageCampus.ToolBox.Events.EventBus>();
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();

        private void OnTriggerEnter(UnityEngine.Collider other)
        {
            if (!other.TryGetComponent(out PlayerController playerController))
                return;

            if (OwnerNetworkID == GameClient.MyID)
                return;

            EventBus.Raise<EntityDestroyedEvent>(OwnerNetworkID, ArchitectureID);
        }
    }
}
