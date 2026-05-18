using Assets.MultiplayerArchitecture.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Entities.Events;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using KapNet;
using MultiplayerArchitecture;
using MultiplayerArchitecture.Entities;
using Org.BouncyCastle.Asn1.Cms;
using System;

namespace Assets.MultiplayerArchitecture.Code.Network
{
    public class GameClient : IClient, IInitable, IDisposable, ITickable, IService
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        NetworkRegistry NetworkRegistry => ServiceProvider.Instance.GetService<NetworkRegistry>();

        public ClientConnection connection { get; private set; }

        public uint MyID => connection.NetworkID;

        public double Ping => connection.Ping;

        public bool IsPersistance => false;

        public GameClient()
        {
        }

        public void Init()
        {
            connection = new ClientConnection(this);

            EventBus.Subscribe<LocalObjectMoveEvent>(OnLocalEntityMove);
            EventBus.Subscribe<EntityViewCreatedEvent<Entity>>(OnLocalEntityCreated);

            connection.Init();
        }

        private void OnLocalEntityMove(in LocalObjectMoveEvent localObjectMoveEvent)
        {
            connection.Send(PacketType.Position, PacketMetaData.None, connection.NetworkID, localObjectMoveEvent.objectNetworkID,
                localObjectMoveEvent.coordinate.x, localObjectMoveEvent.coordinate.y, localObjectMoveEvent.coordinate.z);
        }

        private void OnLocalEntityCreated(in EntityViewCreatedEvent<Entity> entityCreatedEvent)
        {
            string entityCreated = NetworkRegistry.Get(entityCreatedEvent.ownerNetworkID, entityCreatedEvent.objectNetworkID).GetType().Name;

            connection.Send(PacketType.Spawn, PacketMetaData.None, connection.NetworkID, entityCreatedEvent.objectNetworkID,
                entityCreatedEvent.coordinate.x, entityCreatedEvent.coordinate.y, entityCreatedEvent.coordinate.z,
               entityCreated.Length, entityCreated);
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
            connection.Send(PacketType.Data, metadata, data);
        }

        public void OnClientLeft(uint clientID)
        {
            EventBus.Raise<ClientLeft>(clientID);
        }

        public void OnHandShake(uint myID)
        {
            connection.NetworkID = myID;

            EventBus.Raise<LocalSpawnRequestAcceptedEvent<Player>>(connection.NetworkID, new Coordinate(0f, 0f, 0f));
        }

        public void OnSpawn(uint clientID, uint entityID, Coordinate coordinate, string entityToSpawn)
        {
            if (!connection.NetworkID.Equals(clientID))
                EventBus.Raise<NetworkSpawnRequestAcceptedEvent>(clientID, entityID, coordinate, entityToSpawn);
        }

        public void OnDestroyEntity(uint clientID, uint entityID)
        {
            if (!connection.NetworkID.Equals(clientID))
                EventBus.Raise<EntityDestroyedEvent>(clientID, entityID);
        }

        public void OnPositionRecieve(uint clientID, uint entityID, Coordinate coordinate)
        {
            if (!connection.NetworkID.Equals(clientID))
                EventBus.Raise<NetworkObjectMoveEvent>(clientID, entityID, coordinate);
        }

        public void OnServerShutDown()
        {
            EventBus.Raise<ServerShutDown>();
        }

        public void Dispose()
        {
            connection.Dispose();
            EventBus.Unsubscribe<LocalObjectMoveEvent>(OnLocalEntityMove);
            EventBus.Unsubscribe<EntityViewCreatedEvent<Entity>>(OnLocalEntityCreated);
        }
    }
}
