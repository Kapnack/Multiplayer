using Assets.Code.Scenes;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
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
        private Button connectMap1;
        private Button connectMap2;

        public override void Init()
        {

        }

        public override void Init(params object[] parameters)
        {
            Init();

            nameText = (InputField)parameters[0];
            ipText = (InputField)parameters[1];
            connectMap1 = (Button)parameters[2];
            connectMap2 = (Button)parameters[3];
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
            GameClient.connection.Connect(ipText.text, 7777);

            byte[] payload = new byte[sizeof(int) * 2 + userName.Length];

            BitConverter.GetBytes(mapID).CopyTo(payload, 0);

            GameClient.connection.SendHandshake(new byte[0]);
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

        }
    }
}