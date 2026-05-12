using Assets.MultiplayerArchitecture.Code.Entities.Events;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using System;

namespace Assets.MultiplayerArchitecture.Code.Entities
{
    internal class EntityLogic : IInitable, ITickable, IDisposable
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();
        NetworkRegistry EntityRegistry => ServiceProvider.Instance.GetService<NetworkRegistry>();

        public void Init()
        {
            EventBus.Subscribe<NetworkObjectMoveEvent>(OnEntityMove);
            EventBus.Subscribe<LocalObjectMoveEvent>(OnPlayerMove);
            EventBus.Subscribe<ClientLeft>(OnClientLeft);
        }

        private void OnClientLeft(in ClientLeft clientLeft)
        {
            
        }

        private void OnPlayerMove(in LocalObjectMoveEvent playerMove)
        {
            EntityRegistry[GameClient.MyID][1].coordinate = playerMove.coordinate;

            if (GameClient.MyID == 0)
                return;

            Coordinate currentPos = EntityRegistry[GameClient.MyID][1].coordinate;

            byte[] payload = new byte[sizeof(uint) + sizeof(float) * 3];

            BitConverter.GetBytes(GameClient.MyID).CopyTo(payload, 0);
            BitConverter.GetBytes(currentPos.x).CopyTo(payload, sizeof(uint));
            BitConverter.GetBytes(currentPos.y).CopyTo(payload, sizeof(uint) + sizeof(float));
            BitConverter.GetBytes(currentPos.z).CopyTo(payload, sizeof(uint) + sizeof(float) * 2);

            GameClient.Send(payload);
        }

        public void LateInit()
        {

        }

        public void Tick(float deltaTime)
        {

        }

        private void OnEntityMove(in NetworkObjectMoveEvent networkClientMove)
        {
            EntityRegistry[networkClientMove.ownerNetworkID][networkClientMove.objectNetworkID].coordinate = networkClientMove.coordinate;
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<NetworkObjectMoveEvent>(OnEntityMove);
            EventBus.Unsubscribe<LocalObjectMoveEvent>(OnPlayerMove);
            EventBus.Unsubscribe<ClientLeft>(OnClientLeft);
        }
    }
}
