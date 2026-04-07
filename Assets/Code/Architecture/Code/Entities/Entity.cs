
using Assets.Code.Architecture.Code.Math;

public class Entity
{
    public const uint UNASSIGNED_ENTITY_ID = 0;

    public uint ID;

    public Coordinate position;

    public Entity(uint ID)
    {
        this.ID = ID;
    }
}