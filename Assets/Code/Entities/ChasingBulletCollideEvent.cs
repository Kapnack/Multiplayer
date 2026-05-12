using ImageCampus.ToolBox.Events;

public struct ChasingBulletCollideEvent : IEvent
{
    uint ownerNetworkID;
    uint objecNetworkID;
    uint ownerNetworkIDCollision;
    uint objectNetworkIDCollision;

    public void Assign(params object[] parameters)
    {
        ownerNetworkID = (uint)parameters[0];
        objecNetworkID = (uint)parameters[1];
        ownerNetworkIDCollision = (uint)parameters[2];
        objectNetworkIDCollision = (uint)parameters[3];
    }
    public void Reset()
    {
        ownerNetworkID = default(uint);
        objecNetworkID = default(uint);
        ownerNetworkIDCollision = default(uint);
        objectNetworkIDCollision = default(uint);
    }
}