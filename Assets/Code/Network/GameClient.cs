using Assets.Code.Architecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Services;
using KapNet;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Code.Architecture.Code.Client
{
    public class GameClient : MonoBehaviour, IClient, IInitable, IDisposable, ITickable, IService
    {

        private ClientConnection connection;
        public bool IsPersistance => false;

        public uint MyID { private set; get; }

        Dictionary<uint, GameObject> entitiesByClient = new Dictionary<uint, GameObject>();

        [SerializeField] private GameObject usersPrefabs;

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
            connection = new ClientConnection(this);

            connection.Init();
        }

        public void Tick(float deltaTime)
        {
            connection.Tick(deltaTime);

            if (MyID == 0)
                return;

            byte[] payload = new byte[12];

            BitConverter.GetBytes(entitiesByClient[MyID].transform.position.x).CopyTo(payload, 0);
            BitConverter.GetBytes(entitiesByClient[MyID].transform.position.y).CopyTo(payload, 4);
            BitConverter.GetBytes(entitiesByClient[MyID].transform.position.z).CopyTo(payload, 8);

            Send(payload);
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
            if (entitiesByClient.ContainsKey(clientID))
                return;

            GameObject goObject = Instantiate(usersPrefabs);

            goObject.transform.position = Vector3.up * 2;

            Renderer renderer = goObject.GetComponent<Renderer>();

            if (clientID != MyID)
                renderer.material.color = Color.red;
            else
            {
                goObject.AddComponent<PlayerController>();
                renderer.material.color = Color.green;
            }


            entitiesByClient[clientID] = goObject;
        }

        public void OnClientLeft(uint clientID)
        {
            Destroy(entitiesByClient[clientID]);
            entitiesByClient.Remove(clientID);
        }

        public void OnHandShake(uint myID)
        {
            MyID = myID;
            OnClienJoined(MyID);
        }

        public void OnPayloadRecieve(byte[] payload, uint clientID)
        {
            if (entitiesByClient.ContainsKey(clientID))
                entitiesByClient[clientID].transform.position = new Vector3
                   (
            BitConverter.ToSingle(payload, 0),
            BitConverter.ToSingle(payload, 4),
            BitConverter.ToSingle(payload, 8)
            );
        }

        public void OnServerShutDown()
        {
            enabled = false;
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
