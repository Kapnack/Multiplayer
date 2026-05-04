using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Services;
using System;
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

        int.TryParse(port.text, out int result);

        GameClient.connection.Connect(ip.text, result);

        GameClient.Send
    }
}
