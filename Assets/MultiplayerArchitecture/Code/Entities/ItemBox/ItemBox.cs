using Assets.MultiplayerArchitecture.Code.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using MultiplayerArchitecture.Entities;

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

public class ItemBox : Entity
{
    EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private ItemBox(uint ownerNetworkID, uint objectNetworkID, Coordinate coordinate) : base(ownerNetworkID, objectNetworkID, coordinate)
    {
    }
}
