using Assets.Code.Scenes;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using KapNet;
using MultiplayerArchitecture;
using System;
using UnityEngine.UI;

namespace Assets.Code
{
    internal class MainMenuViewScene : BaseSceneView
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();
        private MapToLoad MapToLoad => ServiceProvider.Instance.GetService<MapToLoad>();

        private InputField nameText;
        private InputField ipText;
        private Button connectMap1;
        private Button connectMap2;

        public MainMenuViewScene(InputField nameText, InputField ipText, Button connectMap1, Button connectMap2)
        {
            this.nameText = nameText;
            this.ipText = ipText;
            this.connectMap1 = connectMap1;
            this.connectMap2 = connectMap2;

            this.nameText.gameObject.SetActive(true);
            this.ipText.gameObject.SetActive(true);
            this.connectMap1.gameObject.SetActive(true);
            this.connectMap2.gameObject.SetActive(true);
        }

        public override void Init()
        {

        }

        public override void LateInit()
        {
            connectMap1.onClick.AddListener(OnConnectToMap1);
            connectMap2.onClick.AddListener(OnConnectToMap2);
            connectMap1.onClick.AddListener(() => { EventBus.Raise<ChangeSceneEvent>(Scene.Gameplay); });
            connectMap2.onClick.AddListener(() => { EventBus.Raise<ChangeSceneEvent>(Scene.Gameplay); });
        }

        public override void Tick(float deltaTime)
        {

        }

        private bool ShouldConnect()
        {
            return !(nameText.text.Equals("") || ipText.text.Equals(""));
        }

        private void SendHandshake(string userName, int mapID)
        {
            GameClient.connection.Connect(ipText.text, 7777);
            GameClient.connection.Send(PacketType.Handshake, PacketMetaData.Reliable, mapID, userName.Length, userName);
        }

        private void OnConnectToMap1()
        {
            if (!ShouldConnect())
                return;

            MapToLoad.mapToLoad = Maps.Forest;
            SendHandshake(nameText.text, 1);
        }

        private void OnConnectToMap2()
        {
            if (!ShouldConnect())
                return;

            MapToLoad.mapToLoad = Maps.Snow;
            SendHandshake(nameText.text, 2);
        }

        public override void Dispose()
        {
            nameText.gameObject.SetActive(false);
            ipText.gameObject.SetActive(false);
            connectMap1.gameObject.SetActive(false);
            connectMap2.gameObject.SetActive(false);
        }
    }
}