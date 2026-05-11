using Assets.MultiplayerArchitecture.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;

namespace MultiplayerArchitecture.Code.Scenes
{
    internal class GameplayScene : BaseScene
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();
        EntityFactory EntityFactory => ServiceProvider.Instance.GetService<EntityFactory>();

        EntityLogic entityLogic;

        public override void Init()
        {
            ServiceProvider.Instance.AddService<EntityRegistry>(new EntityRegistry());
            ServiceProvider.Instance.AddService<EntityFactory>(new EntityFactory());
            ServiceProvider.Instance.AddService<GameClient>(new GameClient());

            entityLogic = new EntityLogic(); 

            GameClient.Init();
            entityLogic.Init();

            GameClient.connection.Connect("127.0.0.1", 7777);
            GameClient.connection.SendHandshake(new byte[0]);
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
            EntityFactory.Dispose();
            GameClient.Dispose();

            ServiceProvider.Instance.ClearAllNonPersistanceServices();
        }
    }
}
