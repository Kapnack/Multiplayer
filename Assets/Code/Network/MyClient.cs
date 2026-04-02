using Assets.Code.Network.packets;
using ImageCampus.ToolBox.Services;
using ServerArquitecture.src.Server.Packets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class MyClient : MonoBehaviour
{
    UdpClient client;
    IPEndPoint serverEndPoint;

    public GameObject playerPrefab;

    uint myID;

    Dictionary<uint, GameObject> players = new Dictionary<uint, GameObject>();

    Queue<Action> mainThreadQueue = new Queue<Action>();

    PacketFactory PacketFactory => ServiceProvider.Instance.GetService<PacketFactory>();

    void Start()
    {
        Application.runInBackground = true;

        Screen.fullScreen = false;
        Screen.SetResolution(800, 600, false);

        client = new UdpClient();
        serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);

        client.BeginReceive(OnReceive, null);

        ServiceProvider.Instance.AddService<PacketFactory>(new PacketFactory());

        SendHandshake();
    }

    void Update()
    {
        lock (mainThreadQueue)
            lock (mainThreadQueue)
            {
                while (mainThreadQueue.Count > 0)
                    mainThreadQueue.Dequeue().Invoke();
            }

        if (myID == 0)
            return;

        if (players.ContainsKey(myID))
            SendPosition(players[myID].transform.position);

        SendPing();
    }

    void SendHandshake()
    {
        Send(PacketFactory.Create(PacketType.Handshake));
    }
    void SendPing()
    {
        Send(PacketFactory.Create(PacketType.Ping));
    }

    void SendPosition(Vector3 pos)
    {
        byte[] payload = new byte[16];

        Buffer.BlockCopy(BitConverter.GetBytes(myID), 0, payload, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, payload, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, payload, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(pos.z), 0, payload, 12, 4);

        Send(PacketFactory.Create(PacketType.Data, payload));
    }

    void Send(byte[] data)
    {
        client.Send(data, data.Length, serverEndPoint);
    }

    void OnReceive(IAsyncResult result)
    {
        byte[] data = client.EndReceive(result, ref serverEndPoint);

        if (PacketUtility.CalculateCheckSum(data, 0, sizeof(int) * 2) != PacketUtility.GetCheckSum1(data) ||
            PacketUtility.CalculateCheckSum(data, 0, sizeof(int)) != PacketUtility.GetCheckSum2(data))
            return;

        PacketType type = PacketUtility.GetType(data);
        byte[] payload = PacketUtility.GetPayload(data);

        lock (mainThreadQueue)
        {
            mainThreadQueue.Enqueue(() =>
            {
                switch (type)
                {
                    case PacketType.HandshakeResponse:
                        HandleHandshake(payload);
                        break;

                    case PacketType.Spawn:
                        HandleSpawn(payload);
                        break;

                    case PacketType.Data:
                        HandleData(payload);
                        break;

                    case PacketType.Disconnect:
                        HandleDisconnect(payload);
                        break;
                }
            });
        }

        client.BeginReceive(OnReceive, null);
    }

    void HandleHandshake(byte[] payload)
    {
        myID = BitConverter.ToUInt32(payload, 0);
        Debug.Log("My ID: " + myID);

        SpawnPlayer(myID, Vector3.one * 3);

        if (players.ContainsKey(myID))
        {
            SendPosition(players[myID].transform.position);
        }
    }

    void HandleSpawn(byte[] payload)
    {
        uint id = BitConverter.ToUInt32(payload, 0);

        if (players.ContainsKey(id)) return;

        Debug.Log("Spawn player: " + id);

        SpawnPlayer(id, Vector3.one * 3);
    }

    void HandleData(byte[] payload)
    {
        uint id = BitConverter.ToUInt32(payload, 0);

        if (id == myID)
            return;

        float x = BitConverter.ToSingle(payload, 4);
        float y = BitConverter.ToSingle(payload, 8);
        float z = BitConverter.ToSingle(payload, 12);

        if (!players.ContainsKey(id))
            return;

        players[id].transform.position = new Vector3(x, y, z);
    }

    void HandleDisconnect(byte[] payload)
    {
        uint id = BitConverter.ToUInt32(payload, 0);

        if (!players.ContainsKey(id)) return;

        Debug.Log("Remove player: " + id);

        Destroy(players[id]);
        players.Remove(id);
    }
    void SpawnPlayer(uint id, Vector3 pos)
    {
        GameObject obj = Instantiate(playerPrefab, pos, Quaternion.identity);

        players[id] = obj;

        if (id == myID)
        {
            obj.GetComponent<Renderer>().material.color = Color.green;

            obj.AddComponent<PlayerController>();
        }
        else
        {
            obj.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    void OnApplicationQuit()
    {
        Send(PacketFactory.Create(PacketType.Disconnect));
        client.Close();
    }
}