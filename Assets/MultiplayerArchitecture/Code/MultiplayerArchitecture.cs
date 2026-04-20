using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using MultiplayerArchitecture.Code.Scenes;

namespace Multiplayer.Arch
{
    public class MultiplayerArchitecture : IInitable, ITickable, IDisposable
    {
        BaseScene currentScene;

        public void Init()
        {
            ServiceProvider.Instance.AddService<EventBus>(new EventBus());
            currentScene = new GameplayScene();
            currentScene.Init();
        }

        public void LateInit()
        {
            currentScene.LateInit();
        }

        public void Tick(float deltaTime)
        {
            currentScene.Tick(deltaTime);
        }

        public void Dispose()
        {
            currentScene.Dispose();
        }
    }
}