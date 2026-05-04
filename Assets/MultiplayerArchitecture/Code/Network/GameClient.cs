using Assets.MultiplayerArchitecture.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Entities.Events;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using KapNet;
using System;
using System.Collections.Generic;

namespace Assets.MultiplayerArchitecture.Code.Network
{
    public class GameClient : IClient, IInitable, IDisposable, ITickable, IService
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        EntityFactory EntityFactory => ServiceProvider.Instance.GetService<EntityFactory>();

        public ClientConnection connection { get; private set; }

        public double Ping => connection.Ping;

        public bool IsPersistance => false;

        public uint MyID { private set; get; }

        Dictionary<uint, uint> players = new Dictionary<uint, uint>();
        public uint this[uint clientID] => players[clientID];

        public void Init()
        {
            connection = new ClientConnection(this);

            connection.Init();
        }

        public void Tick(float deltaTime)
        {
            connection.Tick(deltaTime);
        }

        public void LateInit()
        {
            connection.LateInit();
        }

        public void Send(byte[] data, PacketMetaData metadata = PacketMetaData.None)
        {
            connection.Send(PacketType.Data, data, metadata);
        }

        public void OnClienJoined(uint clientID)
        {
            players[clientID] = EntityFactory.Create(clientID);
        }

        public void OnClientLeft(uint clientID)
        {
            if (!players.ContainsKey(clientID))
                return;

            EventBus.Raise<ClientLeft>(clientID, players[clientID]);
            players.Remove(clientID);
        }

        public void OnHandShake(uint myID)
        {
            MyID = myID;

            players[MyID] = EntityFactory.Create(MyID);
        }

        public void OnPayloadRecieve(byte[] payload, uint clientID)
        {
            if (!players.ContainsKey(clientID))
                return;

            float x = BitConverter.ToSingle(payload, 0);
            float y = BitConverter.ToSingle(payload, sizeof(float));
            float z = BitConverter.ToSingle(payload, sizeof(float) * 2);

            Coordinate newCoordinate = new Coordinate(x, y, z);

            EventBus.Raise<NetworkClientMove>(clientID, newCoordinate);
        }

        public void OnServerShutDown()
        {
            EventBus.Raise<ServerShutDown>();
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        //void SendPosition(Vector3 pos)
        //{
        //    byte[] payload = new byte[sizeof(float) * 3];
        //
        //    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, payload, 0, sizeof(float));
        //    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, payload, sizeof(float), sizeof(float));
        //    Buffer.BlockCopy(BitConverter.GetBytes(pos.z), 0, payload, sizeof(float) * 2, sizeof(float));
        //
        //    Send(payload);
        //}
    }
}
