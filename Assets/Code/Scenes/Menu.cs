using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Services;
using KapNet;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    private GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();

    [SerializeField] private InputField nameText;
    [SerializeField] private InputField levelText;
    [SerializeField] private InputField ip;
    [SerializeField] private InputField port;
    [SerializeField] private Button connect;

    private void Awake()
    {
        connect.onClick.AddListener(AttentToConnect);
    }

    private void AttentToConnect()
    {
        if (nameText.text.Equals("") || ip.text.Equals("") || port.text.Equals(""))
            return;

        ServiceProvider.Instance.AddService<GameClient>(new GameClient());

        GameClient.Init();

        int.TryParse(port.text, out int levelID);

        GameClient.connection.Connect(ip.text, levelID);

        byte[] payload = new byte[sizeof(uint) + sizeof(int) * 2 + nameText.text.Length * sizeof(byte)];

        BitConverter.GetBytes(GameClient.MyID).CopyTo(payload, 0);
        BitConverter.GetBytes(levelID).CopyTo(payload, sizeof(uint));
        BitConverter.GetBytes(nameText.text.Length).CopyTo(payload, sizeof(uint));
        Encoding.UTF8.GetBytes(nameText.text).CopyTo(payload, sizeof(uint) * 2);

        GameClient.connection.SendPacket(PacketType.Handshake, payload, PacketMetaData.Reliable);
    }
}
