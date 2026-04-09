namespace Assets.MultiplayerArchitecture.Code.Entities
{
    internal class Entity
    {
        public Coordinate coordinate;
        public uint ID;
        public uint networkID;

        public Entity(uint ID, uint networkID)
        {
            this.ID = ID;
            this.networkID = networkID;
        }
    }
}
