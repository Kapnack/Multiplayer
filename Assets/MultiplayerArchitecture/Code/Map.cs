using ImageCampus.ToolBox.Services;

namespace MultiplayerArchitecture
{
    public class Map : IService
    {
        private Coordinate[] coordinate;

        public Coordinate this[int index] => coordinate[index];

        public int SpawnPointCount => coordinate.Length;

        public Map(Coordinate[] coordinate)
        {
            this.coordinate = coordinate;
        }

        public bool IsPersistance => false;
    }
}
