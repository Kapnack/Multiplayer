using Assets.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Services;
using TMPro;
using UnityEngine;

namespace Assets.Code.Scenes
{
    internal class GameplayViewScene : BaseSceneView
    {
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();

        private EntityFactoryView entityFactoryView;
        private EntityLogicView entityLogicView;
        private TMP_Text pingText;

        private string pingFormat;

        public GameplayViewScene(TMP_Text pingText)
        {
            this.pingText = pingText;
        }

        public override void Init()
        {
            pingFormat = pingText.text;
            pingText.text = string.Format(pingFormat, GameClient.Ping);
        }

        [System.Obsolete]
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
            entityLogicView.Tick(deltaTime);

            pingText.text = string.Format(pingFormat, GameClient.Ping);
        }

        public override void Dispose()
        {
            entityLogicView.Dispose();
            entityFactoryView.Dispose();
            ServiceProvider.Instance.RemoveService<EntityRegistryView>();
        }
    }
}
