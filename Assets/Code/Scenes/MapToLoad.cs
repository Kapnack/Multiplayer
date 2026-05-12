using ImageCampus.ToolBox.Services;

namespace Assets.Code.Scenes
{
    public enum Maps
    {
        Forest,
        Snow
    }

    class MapToLoad : IService
    {
        public bool IsPersistance => true;

        public Maps mapToLoad;
    }
}
