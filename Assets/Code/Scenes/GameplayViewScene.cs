using Assets.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using TMPro;
using UnityEngine;

namespace Assets.Code.Scenes
{
    internal class GameplayViewScene : BaseSceneView
    {
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        private NetworkFactoryView entityFactoryView;
        private EntityLogicView entityLogicView;
        private TMP_Text pingText;

        private string pingFormat;

        public GameplayViewScene(GameObject selectedMap, GameObject bulletPrefab, GameObject bananaPrefab, GameObject oilPrefab, TMP_Text pingText, GameObject userPrefabs, Camera camera)
        {
            ServiceProvider.Instance.AddService<NetworkRegistryView>(new NetworkRegistryView());

            this.pingText = pingText;
            ServiceProvider.Instance.AddService<MapView>(Object.Instantiate(selectedMap).GetComponent<MapView>());

            entityFactoryView = new NetworkFactoryView(userPrefabs, bulletPrefab, bananaPrefab, oilPrefab, camera);
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
