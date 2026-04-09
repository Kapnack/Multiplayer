using Assets.Code.Architecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using KapNet;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Code.Architecture.Code.Client
{
    public class GameClient : MonoBehaviour, IClient, IInitable, IDisposable, ITickable, IService
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        private ClientConnection connection;
        public bool IsPersistance => false;

        public GameObject playerPrefab;

        public uint MyID { private set; get; }

        Dictionary<uint, GameObject> players = new Dictionary<uint, GameObject>();

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            LateInit();
        }

        private void Update()
        {
            Tick(0);
        }

        private void OnApplicationQuit()
        {
            Dispose();
        }

        public void Init()
        {
            Application.runInBackground = true;

            Screen.fullScreen = false;
            Screen.SetResolution(800, 600, false);

            connection = new ClientConnection(this);

            connection.Init();
        }

        public void Tick(float deltaTime)
        {
            connection.Tick(deltaTime);

            if (MyID == 0)
                return;

            SendPosition(players[MyID].transform.position);
        }

        public void LateInit()
        {
            connection.LateInit();
        }

        public void Send(byte[] data, PacketMetaData metadata = PacketMetaData.None)
        {
            connection.Send(data, metadata);
        }

        public void OnClienJoined(uint clientID)
        {
            SpawnPlayer(clientID, Vector3.up * 3);
        }

        public void OnClientLeft(uint clientID)
        {
            Destroy(players[clientID]);
            players.Remove(clientID);
        }

        public void OnHandShake(uint myID)
        {
            MyID = myID;

            SpawnPlayer(MyID, Vector3.up * 3);
        }

        public void OnPayloadRecieve(byte[] payload, uint clientID)
        {
            if (!players.ContainsKey(clientID))
                return;

            float x = BitConverter.ToSingle(payload, 0);
            float y = BitConverter.ToSingle(payload, 4);
            float z = BitConverter.ToSingle(payload, 8);

            players[clientID].transform.position = new Vector3(x, y, z);
        }

        public void OnServerShutDown()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        void SpawnPlayer(uint id, Vector3 pos)
        {
            if (players.ContainsKey(id))
                return;

            GameObject obj = Instantiate(playerPrefab, pos, Quaternion.identity);

            players[id] = obj;

            if (id == MyID)
            {
                obj.GetComponent<Renderer>().material.color = Color.green;
                obj.AddComponent<PlayerController>();
            }
            else
            {
                obj.GetComponent<Renderer>().material.color = Color.red;
            }
        }

        void SendPosition(Vector3 pos)
        {
            byte[] payload = new byte[sizeof(float) * 3];

            Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, payload, 0, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, payload, sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(pos.z), 0, payload, sizeof(float) * 2, sizeof(float));

            Send(payload);
        }
    }
}
