using Assets.Code.Architecture.Code.Client;
using Assets.Code.Architecture.Code.Entities;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerServer.src;
using System;

public class MultiplayerArchitecture : IInitable, ITickable, IDisposable
{
    GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();

    public void Init()
    {
        ServiceProvider.Instance.AddService<EventBus>(new EventBus());
        ServiceProvider.Instance.AddService<Time>(new Time());
        ServiceProvider.Instance.AddService<GameClient>(new GameClient());
        ServiceProvider.Instance.AddService<EntityRegistry>(new EntityRegistry());
        ServiceProvider.Instance.AddService<EntityFactory>(new EntityFactory());

        GameClient.Init();
    }

    public void LateInit()
    {
        GameClient.LateInit();
    }

    public void Tick(float deltaTime)
    {
        GameClient.Tick(deltaTime);
    }

    public void Dispose()
    {
        GameClient.Dispose();
    }
}
