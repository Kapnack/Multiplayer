namespace Assets.MultiplayerArchitecture.Code.Entities
{
    public class Entity
    {
        public const uint UNASSIGNED_ENTITY_ID = 0;
        public Coordinate coordinate;
        public uint ownerNetworkID;
        public uint objectNetworkID;

        public Entity(uint ownerNetworkID, uint objectNetworkID, Coordinate coordinate)
        {
            this.ownerNetworkID = ownerNetworkID;
            this.objectNetworkID = objectNetworkID;
            this.coordinate = coordinate;
        }
    }
}
