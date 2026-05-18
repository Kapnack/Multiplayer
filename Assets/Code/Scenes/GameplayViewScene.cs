using Assets.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using TMPro;
using UnityEngine;

namespace Assets.Code.Scenes
{
    internal class GameplayViewScene : BaseSceneView
    {
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        NetworkRegistryView NetworkRegistryView => ServiceProvider.Instance.GetService<NetworkRegistryView>();

        private NetworkFactoryView entityFactoryView;
        private EntityLogicView entityLogicView;
        private TMP_Text pingText;

        private GameObject spawnedMap;

        private string pingFormat;

        public GameplayViewScene(GameObject selectedMap, GameObject bulletPrefab, GameObject bananaPrefab, GameObject oilPrefab, TMP_Text pingText, GameObject userPrefabs, Camera camera)
        {
            ServiceProvider.Instance.AddService<NetworkRegistryView>(new NetworkRegistryView());

            this.pingText = pingText;
            pingText.gameObject.SetActive(true);
            spawnedMap = Object.Instantiate(selectedMap);
            ServiceProvider.Instance.AddService<MapView>(spawnedMap.GetComponent<MapView>());

            entityFactoryView = new NetworkFactoryView(userPrefabs, bulletPrefab, bananaPrefab, oilPrefab, camera);
            entityLogicView = new EntityLogicView();
        }

        public override void Init()
        {
            pingFormat = pingText.text;
            pingText.text = string.Format(pingFormat, GameClient.Ping);

            GameClient.Init();
            entityFactoryView.Init();
            entityLogicView.Init();
        }

        public override void LateInit()
        {
            entityFactoryView.LateInit();
            entityLogicView.LateInit();
            GameClient.LateInit();
        }

        public override void Tick(float deltaTime)
        {
            entityLogicView.Tick(deltaTime);
            pingText.text = string.Format(pingFormat, GameClient.Ping);
            GameClient.Tick(deltaTime);
        }

        public override void Dispose()
        {
            entityLogicView.Dispose();
            entityFactoryView.Dispose();

            pingText.gameObject.SetActive(false);

            NetworkRegistryView.Dispose();

            Object.Destroy(spawnedMap);

            ServiceProvider.Instance.RemoveService<NetworkRegistryView>();
            ServiceProvider.Instance.RemoveService<MapView>();
            ServiceProvider.Instance.RemoveService<Map>();
        }
    }
}
