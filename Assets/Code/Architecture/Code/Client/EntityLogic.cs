using Assets.Code.Architecture.Code.Entities;
using Assets.Code.Architecture.Code.Entities.Events;
using Assets.Code.Architecture.Code.Math;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;

namespace Assets.Code.Architecture.Code.Client
{
    internal class EntityLogic : IInitable, IDisposable
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();
        EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

        public void Init()
        {
            EventBus.Subscribe<PlayerMoveEvent>(OnUserMove);
            EventBus.Subscribe<ClientMoveEvent>(OnClientMove);
        }

        public void LateInit()
        {

        }

        private void OnUserMove(in PlayerMoveEvent userMoveEvent)
        {
            if (GameClient.MyID == 0)
                return;

            Coordinate newPosition = new Coordinate(userMoveEvent.x, userMoveEvent.y, userMoveEvent.z);

            EntityRegistry[GameClient[GameClient.MyID]].position = newPosition;

            byte[] payload = new byte[sizeof(float) * 3];

            BitConverter.GetBytes(newPosition.x).CopyTo(payload, 0);
            BitConverter.GetBytes(newPosition.y).CopyTo(payload, sizeof(float));
            BitConverter.GetBytes(newPosition.z).CopyTo(payload, sizeof(float) * 2);

            GameClient.Send(payload);
        }

        private void OnClientMove(in ClientMoveEvent clientMoveEvent)
        {
            EntityRegistry[GameClient[clientMoveEvent.clientID]].position = clientMoveEvent.position;
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<PlayerMoveEvent>(OnUserMove);
        }
    }
}
