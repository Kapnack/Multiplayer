using Assets.MultiplayerArchitecture.Code.Entities.Events;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;

namespace Assets.MultiplayerArchitecture.Code.Entities
{
    internal class EntityLogic : IInitable, ITickable, IDisposable
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();
        EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

        public void Init()
        {
            EventBus.Subscribe<NetworkClientMove>(OnEntityMove);
            EventBus.Subscribe<PlayerMove>(OnPlayerMove);
            EventBus.Subscribe<ClientLeft>(OnClientLeft);
        }

        private void OnClientLeft(in ClientLeft clientLeft)
        {
            EntityRegistry.Remove(clientLeft.objectID);
        }

        private void OnPlayerMove(in PlayerMove playerMove)
        {
            EntityRegistry[GameClient[GameClient.MyID]].coordinate = playerMove.coordinate;

            if (GameClient.MyID == 0)
                return;

            Coordinate currentPos = EntityRegistry[GameClient[GameClient.MyID]].coordinate;

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

        private void OnEntityMove(in NetworkClientMove networkClientMove)
        {
            EntityRegistry[GameClient[networkClientMove.networkID]].coordinate = networkClientMove.coordinate;
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<NetworkClientMove>(OnEntityMove);
            EventBus.Unsubscribe<PlayerMove>(OnPlayerMove);
            EventBus.Unsubscribe<ClientLeft>(OnClientLeft);
        }
    }
}
