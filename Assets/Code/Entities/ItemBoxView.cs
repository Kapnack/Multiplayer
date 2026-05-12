using Assets.Code.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;
using ZooArchitect.View.Mapping;

[ViewOf(typeof(ItemBox))]
internal class ItemBoxView : EntityView
{
    EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    public ItemBoxView() : base()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.TryGetComponent<EntityView>(out EntityView entityView))
            return;

        EventBus.Raise<IteamBoxCollectedEvent>(OwnerNetworkID, ArchitectureID, entityView.OwnerNetworkID, entityView.ArchitectureID);
    }
}
