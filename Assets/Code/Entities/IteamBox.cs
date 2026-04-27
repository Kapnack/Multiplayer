using Assets.Code.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;

public struct IteamBoxCollected : IEvent
{
    uint itemBoxID;

    public void Assign(params object[] parameters)
    {
        itemBoxID = (uint)parameters[0];
    }

    public void Reset()
    {
        itemBoxID = default(uint);
    }
}

internal class IteamBox : EntityView
{
    EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private uint itemBoxID;

    public IteamBox(uint itemBoxID) : base()
    {
        this.itemBoxID = itemBoxID;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.tag.Equals("Car"))
            return;

        EventBus.Raise<IteamBoxCollected>(itemBoxID);
    }
}
