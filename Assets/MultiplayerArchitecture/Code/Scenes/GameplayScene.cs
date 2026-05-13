using Assets.MultiplayerArchitecture.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;

namespace MultiplayerArchitecture.Code.Scenes
{
    internal class GameplayScene : BaseScene
    {
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();
        NetworkFactory NetworkFactory => ServiceProvider.Instance.GetService<NetworkFactory>();

        EntityLogic entityLogic;

        public override void Init()
        {
            ServiceProvider.Instance.AddService<NetworkRegistry>(new NetworkRegistry());
            ServiceProvider.Instance.AddService<GameClient>(new GameClient());
            ServiceProvider.Instance.AddService<NetworkFactory>(new NetworkFactory());

            entityLogic = new EntityLogic(); 

            GameClient.Init();
            entityLogic.Init();
        }

        public override void LateInit()
        {
            GameClient.LateInit();
            entityLogic.LateInit();
        }

        public override void Tick(float deltaTime)
        {
            GameClient.Tick(deltaTime);
            entityLogic.Tick(deltaTime);
        }

        public override void Dispose()
        {
            entityLogic.Dispose();
            NetworkFactory.Dispose();
            GameClient.Dispose();

            ServiceProvider.Instance.ClearAllNonPersistanceServices();
        }
    }
}
