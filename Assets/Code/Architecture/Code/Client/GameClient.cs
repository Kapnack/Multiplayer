using Assets.Code.Architecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Services;
using KapNet;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Assets.Code.Architecture.Code.Client
{
    internal class GameClient : IClient, IInitable, IDisposable, ITickable, IService
    {
        private ClientConnection connection;
        public bool IsPersistance => false;

        uint myID = 0;

        Dictionary<uint, Player> players = new Dictionary<uint, Player>();

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

        public void Send(byte[] data, PacketMetaData metadata)
        {
            connection.Send(data, metadata);
        }

        public void OnClienJoined(uint clientID)
        {
            players[clientID] = null;
        }

        public void OnClientLeft(uint clientID)
        {
            players.Remove(clientID);
        }

        public void OnHandShake(uint myID)
        {
            this.myID = myID;
        }

        public void OnPayloadRecieve(byte[] data, uint clientID)
        {

        }

        public void OnServerShutDown()
        {

        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
