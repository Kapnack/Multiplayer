using ImageCampus.ToolBox.Events;

struct CreateEntity : IEvent
{
    public uint ownerNetworkID;
    public EntityType entityType;

    public void Assign(params object[] parameters)
    {
        ownerNetworkID = (uint)parameters[0];
        entityType = (EntityType)parameters[1];
    }

    public void Reset()
    {
        ownerNetworkID = default(uint);
        entityType = default(EntityType);
    }
}
