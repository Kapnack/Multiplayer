using Assets.Code.Scenes;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using System;
using UnityEngine.UI;

namespace Assets.Code
{
    internal class MainMenuViewScene : BaseSceneView
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();

        private InputField nameText;
        private InputField ipText;
        private InputField levelText;
        private Button connectMap1;
        private Button connectMap2;

        public MainMenuViewScene(InputField nameText, InputField ipText, InputField levelText, Button connectMap1, Button connectMap2)
        {
            this.nameText = nameText;
            this.ipText = ipText;
            this.levelText = levelText;
            this.connectMap1 = connectMap1;
            this.connectMap2 = connectMap2;

            connectMap1.onClick.AddListener(() => { EventBus.Raise<ChangeSceneEvent>(Scene.Gameplay); });

            this.nameText.gameObject.SetActive(true);
            this.ipText.gameObject.SetActive(true);
            this.connectMap1.gameObject.SetActive(true);
            this.levelText.gameObject.SetActive(true);
            this.connectMap2.gameObject.SetActive(true);
        }

        public override void Init()
        {

        }

        public override void LateInit()
        {
            connectMap1.onClick.AddListener(OnConnectToMap1);
            connectMap2.onClick.AddListener(OnConnectToMap2);
        }

        public override void Tick(float deltaTime)
        {

        }

        private bool ShouldConnect()
        {
            return !(nameText.Equals("") || ipText.Equals(""));
        }

        private void SendHandshake(string userName, int mapID)
        {
            byte[] payload = new byte[sizeof(int) * 2 + userName.Length];

            BitConverter.GetBytes(mapID).CopyTo(payload, 0);

            //GameClient.connection.SendHandshake(new byte[0]);
        }

        private void OnConnectToMap1()
        {
            if (!ShouldConnect())
                return;

            SendHandshake(nameText.text, 1);
        }

        private void OnConnectToMap2()
        {
            if (!ShouldConnect())
                return;

            SendHandshake(nameText.text, 2);
        }

        public override void Dispose()
        {
            nameText.gameObject.SetActive(false);
            ipText.gameObject.SetActive(false);
            levelText.gameObject.SetActive(false);
            connectMap1.gameObject.SetActive(false);
            connectMap2.gameObject.SetActive(false);
        }
    }
}