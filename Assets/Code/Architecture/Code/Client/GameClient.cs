using Assets.Code.Architecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using KapNet;
using System;

namespace Assets.Code.Architecture.Code.Client
{
    internal class GameClient : IClient, IInitable, IDisposable, ITickable
    {
        private ClientConnection connection;

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

        }

        public void OnClientLeft(uint clientID)
        {

        }

        public void OnHandShake(uint myID)
        {

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
