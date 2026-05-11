using Assets.MultiplayerArchitecture.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Entities.Events;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using KapNet;
using System;

namespace Assets.MultiplayerArchitecture.Code.Network
{
    public class GameClient : IClient, IInitable, IDisposable, ITickable, IService
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        public ClientConnection connection { get; private set; }

        public double Ping => connection.Ping;

        public bool IsPersistance => false;

        public uint MyID { private set; get; }

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
            EventBus.Raise<CreateRemoteEntity>(clientID, EntityType.Player, 1);
        }

        public void OnClientLeft(uint clientID)
        {
            EventBus.Raise<ClientLeft>(clientID);
        }

        public void OnHandShake(uint myID)
        {
            MyID = myID;

            EventBus.Raise<CreateEntity>(MyID, EntityType.Player);
        }

        public void OnPayloadRecieve(byte[] payload, uint clientID)
        {
            float x = BitConverter.ToSingle(payload, 0);
            float y = BitConverter.ToSingle(payload, sizeof(float));
            float z = BitConverter.ToSingle(payload, sizeof(float) * 2);

            Coordinate newCoordinate = new Coordinate(x, y, z);

            EventBus.Raise<NetworkObjectMoveEvent>(clientID, newCoordinate);
        }

        public void OnServerShutDown()
        {
            EventBus.Raise<ServerShutDown>();
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
