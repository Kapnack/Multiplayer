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
        EntityLogic entityLogic;

        public override void Init()
        {
            ServiceProvider.Instance.AddService<EntityRegistry>(new EntityRegistry());
            ServiceProvider.Instance.AddService<EntityFactory>(new EntityFactory());

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
            GameClient.Dispose();

            ServiceProvider.Instance.ClearAllNonPersistanceServices();
        }

        public override void Init(params object[] parameters)
        {

        }
    }
}
