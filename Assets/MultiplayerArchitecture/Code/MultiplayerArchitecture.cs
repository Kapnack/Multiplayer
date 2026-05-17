using Assets.MultiplayerArchitecture.Code.Network;
using Assets.MultiplayerArchitecture.Code.Scenes;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using MultiplayerArchitecture.Code.Scenes;
using System;

namespace Multiplayer.Arch
{
    public class MultiplayerArchitecture : IInitable, ITickable, IDisposable
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        BaseScene currentScene;

        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();

        public MultiplayerArchitecture()
        {
            ServiceProvider.Instance.AddService<EventBus>(new EventBus());
            ServiceProvider.Instance.AddService<GameClient>(new GameClient());
        }

        public void Init()
        {
            EventBus.Subscribe<ChangeSceneEvent>(OnSceneChange);
            GameClient.Init();
        }

        public void LateInit()
        {
            EventBus.Raise<ChangeSceneEvent>(Scene.MainMenu);
            GameClient.LateInit();
        }

        public void Tick(float deltaTime)
        {
            currentScene.Tick(deltaTime);
            GameClient.Tick(deltaTime);
        }

        private void OnSceneChange(in ChangeSceneEvent changeSceneEvent)
        {
            if (currentScene != null)
                currentScene.Dispose();

            currentScene = changeSceneEvent.scene == Scene.MainMenu ? new MainMenuScene() : new GameplayScene();

            currentScene.Init();
            currentScene.LateInit();
        }

        public void Dispose()
        {
            currentScene.Dispose();
            GameClient.Dispose();
        }
    }
}