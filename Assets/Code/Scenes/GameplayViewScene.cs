using Assets.Code.Entities;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture.Code.Scenes;
using UnityEngine;

namespace Assets.Code.Scenes
{
    internal class GameplayViewScene : BaseScene
    {
        private EntityFactoryView entityFactoryView;
        private EntityLogicView entityLogicView;

        public override void Init()
        {
        }

        public override void Init(params object[] parameters)
        {
            Init();

            ServiceProvider.Instance.AddService<EntityRegistryView>(new EntityRegistryView());
            entityFactoryView = new EntityFactoryView((GameObject)parameters[0], (Camera)parameters[1]);
            entityLogicView = new EntityLogicView();

            entityFactoryView.Init();
            entityLogicView.Init();
        }

        public override void LateInit()
        {
            entityFactoryView.LateInit();
            entityLogicView.LateInit();
        }

        public override void Tick(float deltaTime)
        {
            
        }

        public override void Dispose()
        {
            entityLogicView.Dispose();
            entityFactoryView.Dispose();
            ServiceProvider.Instance.RemoveService<EntityRegistryView>();
        }
    }
}
