using ImageCampus.ToolBox.Events;

struct CreateRemoteEntity : IEvent
{
    public uint ownerNetworkID;
    public EntityType entityType;
    public uint objectNetworkID;

    public void Assign(params object[] parameters)
    {
        ownerNetworkID = (uint)parameters[0];
        entityType = (EntityType)parameters[1];
        objectNetworkID = (uint)parameters[2];
    }

    public void Reset()
    {
        ownerNetworkID = default(uint);
        entityType = default(EntityType);
        objectNetworkID = default(uint);
    }
}
