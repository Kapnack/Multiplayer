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

        private NetworkFactoryView entityFactoryView;
        private EntityLogicView entityLogicView;
        private TMP_Text pingText;

        private string pingFormat;

        public GameplayViewScene(GameObject selectedMap, TMP_Text pingText, GameObject userPrefabs, Camera camera)
        {
            ServiceProvider.Instance.AddService<NetworkRegistryView>(new NetworkRegistryView());

            this.pingText = pingText;
            Object.Instantiate(selectedMap);

            entityFactoryView = new NetworkFactoryView(userPrefabs, camera);
            entityLogicView = new EntityLogicView();
        }

        public override void Init()
        {
            pingFormat = pingText.text;
            pingText.text = string.Format(pingFormat, GameClient.Ping);

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
            ServiceProvider.Instance.RemoveService<NetworkRegistryView>();
        }
    }
}
