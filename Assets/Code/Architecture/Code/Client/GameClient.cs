using Assets.Code.Architecture.Code.Entities.Events;
using Assets.Code.Architecture.Code.Math;
using Assets.Code.Architecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using KapNet;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Assets.Code.Architecture.Code.Client
{
    public class GameClient : IClient, IInitable, IDisposable, ITickable, IService
    {
        EntityFactory EntityFactory => ServiceProvider.Instance.GetService<EntityFactory>();
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        private ClientConnection connection;
        public bool IsPersistance => false;

        public uint MyID { private set; get; }

        Dictionary<uint, uint> entitiesByClient = new Dictionary<uint, uint>();

        public uint this[uint clientID] => entitiesByClient[clientID];


        public void Init()
        {
            connection = new ClientConnection(this);

            EventBus.Unsubscribe<EntityCreatedEvent<Entity>>(OnEntityCreated);

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
            connection.Send(data, metadata);
        }

        public void OnClienJoined(uint clientID)
        {
            EventBus.Raise<SpawnRequestAcceptedEvent<Entity>>(clientID);
        }

        public void OnClientLeft(uint clientID)
        {
            entitiesByClient.Remove(clientID);
        }

        public void OnHandShake(uint myID)
        {
            MyID = myID;
            EventBus.Raise<SpawnRequestAcceptedEvent<Entity>>(MyID);
        }

        public void OnPayloadRecieve(byte[] payload, uint clientID)
        {
            Coordinate clientNewPos = new Coordinate(BitConverter.ToSingle(payload, 0), BitConverter.ToSingle(payload, sizeof(float)), BitConverter.ToSingle(payload, sizeof(float) * 2));

            EventBus.Raise<ClientMoveEvent>(clientID, clientNewPos);
        }

        private void OnEntityCreated(in EntityCreatedEvent<Entity> entityCreatedEvent)
        {
            entitiesByClient[entityCreatedEvent.networkID] = entityCreatedEvent.entityID;
        }

        public void OnServerShutDown()
        {

        }

        public void Dispose()
        {
            EventBus.Unsubscribe<EntityCreatedEvent<Entity>>(OnEntityCreated);
            connection.Dispose();
        }
    }
}
