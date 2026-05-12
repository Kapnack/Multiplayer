using ImageCampus.ToolBox.Events;

namespace MultiplayerArchitecture
{
    public enum Scene
    {
        None,
        MainMenu,
        Gameplay
    }

    public struct ChangeSceneEvent : IEvent
    {
        public Scene scene;

        public void Assign(params object[] parameters)
        {
            scene = (Scene)parameters[0];
        }

        public void Reset()
        {
            scene = default(Scene);
        }
    }
}