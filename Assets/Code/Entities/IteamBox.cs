using Assets.Code.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;

public struct IteamBoxCollectedEvent : IEvent
{
    public uint ownerNetworkID;
    public uint objectNetworkID;
    public uint ownerNetworkIDCollision;
    public uint objectNetworkIDCollision;

    public void Assign(params object[] parameters)
    {
        ownerNetworkID = (uint)parameters[0];
        objectNetworkID = (uint)parameters[1];
        ownerNetworkIDCollision = (uint)parameters[2];
        objectNetworkIDCollision = (uint)parameters[3];
    }

    public void Reset()
    {
        ownerNetworkID = default(uint);
        objectNetworkID = default(uint);
        ownerNetworkIDCollision = default(uint);
        objectNetworkIDCollision = default(uint);
    }
}

internal class IteamBox : EntityView
{
    EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    public IteamBox() : base()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.TryGetComponent<EntityView>(out EntityView entityView))
            return;

        EventBus.Raise<IteamBoxCollectedEvent>(OwnerNetworkID, ArchitectureID, entityView.OwnerNetworkID, entityView.ArchitectureID);
    }
}
