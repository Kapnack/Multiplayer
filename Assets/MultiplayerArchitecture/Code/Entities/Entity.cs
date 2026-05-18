namespace MultiplayerArchitecture.Entities
{
    public class Entity
    {
        public const uint UNASSIGNED_ENTITY_ID = 0;
        public Coordinate coordinate;
        public Rotation rotationEuler;
        public uint ownerNetworkID;
        public uint objectNetworkID;

        protected Entity(uint ownerNetworkID, uint objectNetworkID, Coordinate coordinate)
        {
            this.ownerNetworkID = ownerNetworkID;
            this.objectNetworkID = objectNetworkID;
            this.coordinate = coordinate;
        }
    }
}
