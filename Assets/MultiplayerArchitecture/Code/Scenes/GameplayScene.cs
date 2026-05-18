using Assets.MultiplayerArchitecture.Code.Entities;
using ImageCampus.ToolBox.Services;

namespace MultiplayerArchitecture.Code.Scenes
{
    internal class GameplayScene : BaseScene
    {
        NetworkFactory NetworkFactory => ServiceProvider.Instance.GetService<NetworkFactory>();

        EntityLogic entityLogic;

        public override void Init()
        {
            ServiceProvider.Instance.AddService<NetworkRegistry>(new NetworkRegistry());
            ServiceProvider.Instance.AddService<NetworkFactory>(new NetworkFactory());

            entityLogic = new EntityLogic(); 

            entityLogic.Init();
        }

        public override void LateInit()
        {
            entityLogic.LateInit();
        }

        public override void Tick(float deltaTime)
        {
            entityLogic.Tick(deltaTime);
        }

        public override void Dispose()
        {
            entityLogic.Dispose();
            NetworkFactory.Dispose();
            ServiceProvider.Instance.RemoveService<NetworkFactory>();
            ServiceProvider.Instance.RemoveService<NetworkRegistry>();
        }
    }
}
