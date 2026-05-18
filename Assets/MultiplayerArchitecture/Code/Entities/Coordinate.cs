namespace MultiplayerArchitecture
{
    public struct Coordinate
    {
        public float x;
        public float y;
        public float z;

        public Coordinate(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public struct Rotation
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Rotation(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
    }
}
