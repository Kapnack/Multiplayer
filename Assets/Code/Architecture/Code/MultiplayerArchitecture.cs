using Assets.Code.Architecture.Code.Client;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Services;
using System;

public class MultiplayerArchitecture : IInitable, ITickable, IDisposable
{
    GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();

    public void Init()
    {
        ServiceProvider.Instance.AddService<GameClient>(new GameClient());
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
