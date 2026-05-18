using Assets.MultiplayerArchitecture.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Entities.Events;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using KapNet;
using MultiplayerArchitecture;
using MultiplayerArchitecture.Entities;
using NPOI.SS.Formula.Functions;
using System;

namespace Assets.MultiplayerArchitecture.Code.Network
{
    public class GameClient : IClient, IInitable, IDisposable, ITickable, IService
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();  

        Map Map => ServiceProvider.Instance.GetService<Map>();

        NetworkRegistry NetworkRegistry => ServiceProvider.Instance.GetService<NetworkRegistry>();

        public ClientConnection connection { get; private set; }

        public uint MyID => connection.NetworkID;

        public double Ping => connection.Ping;

        public bool IsPersistance => false;

        public GameClient()
        {
            connection = new ClientConnection(this);
        }

        public void Init()
        {
            connection.Init();

            EventBus.Subscribe<LocalObjectMoveEvent>(OnLocalEntityMove);
            EventBus.Subscribe<LocalEntityViewCreatedEvent<Entity>>(OnLocalEntityCreated);
        }

        private void OnLocalEntityMove(in LocalObjectMoveEvent localObjectMoveEvent)
        {
            connection.Send(PacketType.Position, PacketMetaData.None, connection.NetworkID, localObjectMoveEvent.objectNetworkID,
                localObjectMoveEvent.coordinate.x, localObjectMoveEvent.coordinate.y, localObjectMoveEvent.coordinate.z, 
                localObjectMoveEvent.rotation.x, localObjectMoveEvent.rotation.y, localObjectMoveEvent.rotation.z, localObjectMoveEvent.rotation.w);
        }

        private void OnLocalEntityCreated(in LocalEntityViewCreatedEvent<Entity> entityCreatedEvent)
        {
            string entityCreated = NetworkRegistry.Get(entityCreatedEvent.ownerNetworkID, entityCreatedEvent.objectNetworkID).GetType().Name;

            connection.Send(PacketType.Spawn, PacketMetaData.Reliable, connection.NetworkID, entityCreatedEvent.objectNetworkID,
                entityCreatedEvent.coordinate.x, entityCreatedEvent.coordinate.y, entityCreatedEvent.coordinate.z, entityCreated);
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

            EventBus.Raise<LocalSpawnRequestAcceptedEvent<Player>>(connection.NetworkID, Map[(int)MyID % Map.SpawnPointCount]);
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

        public void OnPositionRecieve(uint clientID, uint entityID, Coordinate coordinate, Rotation rotation)
        {
            if (!connection.NetworkID.Equals(clientID))
                EventBus.Raise<NetworkObjectMoveEvent>(clientID, entityID, coordinate, rotation);
        }

        public void OnServerShutDown()
        {
            connection.Disconnect();
            EventBus.Raise<ChangeSceneEvent>(Scene.MainMenu);
        }

        public void Dispose()
        {
            connection.Dispose();
            EventBus.Unsubscribe<LocalObjectMoveEvent>(OnLocalEntityMove);
            EventBus.Unsubscribe<LocalEntityViewCreatedEvent<Entity>>(OnLocalEntityCreated);
        }
    }
}
