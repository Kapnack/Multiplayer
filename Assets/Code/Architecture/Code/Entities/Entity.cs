
using Assets.Code.Architecture.Code.Math;

public class Entity
{
    public const uint UNASSIGNED_ENTITY_ID = 0;

    public uint ID;
    public uint networkID;
    public Coordinate position;

    public Entity(uint ID, uint networkID)
    {
        this.ID = ID;
        this.networkID = networkID;
    }
}