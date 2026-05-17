using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using MultiplayerView;
using MutliplayerView.Game.Mapping;

namespace Assets.Code.Entities
{
    [ViewOf(typeof(Banana))]
    public class BananaView : EntityView
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        private void OnTriggerEnter(UnityEngine.Collider other)
        {
            if (other.TryGetComponent(out PlayerController playerController))
                EventBus.Raise<EntityDestroyedEvent>(OwnerNetworkID, ArchitectureID);
        }
    }
}
