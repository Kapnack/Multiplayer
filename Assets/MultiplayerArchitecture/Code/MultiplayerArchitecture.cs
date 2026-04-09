
using Assets.MultiplayerArchitecture.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;

namespace Multiplayer.Arch
{
    public class MultiplayerArchitecture : IInitable, ITickable, IDisposable
    {
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();
        EntityLogic entityLogic = new EntityLogic();

        public void Init()
        {
            ServiceProvider.Instance.AddService<EventBus>(new EventBus());
            ServiceProvider.Instance.AddService<GameClient>(new GameClient());
            ServiceProvider.Instance.AddService<EntityRegistry>(new EntityRegistry());
            ServiceProvider.Instance.AddService<EntityFactory>(new EntityFactory());

            GameClient.Init();
            entityLogic.Init();

        }

        public void LateInit()
        {
            GameClient.LateInit();
            entityLogic.LateInit();
        }

        public void Tick(float deltaTime)
        {
            GameClient.Tick(deltaTime);
            entityLogic.Tick(deltaTime);
        }

        public void Dispose()
        {
            entityLogic.Dispose();
            GameClient.Dispose();
        }
    }
}