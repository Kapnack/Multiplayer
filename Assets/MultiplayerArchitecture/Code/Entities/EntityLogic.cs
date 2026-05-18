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
            EventBus.Subscribe<ClientLeft>(OnClientLeft);
        }

        private void OnClientLeft(in ClientLeft clientLeft)
        {

        }

        public void LateInit()
        {

        }

        public void Tick(float deltaTime)
        {

        }

        private void OnEntityMove(in NetworkObjectMoveEvent networkClientMove)
        {
            if (!EntityRegistry.Contains(networkClientMove.ownerNetworkID, networkClientMove.objectNetworkID))
                return;

            EntityRegistry[networkClientMove.ownerNetworkID][networkClientMove.objectNetworkID].coordinate = networkClientMove.coordinate;
            EntityRegistry[networkClientMove.ownerNetworkID][networkClientMove.objectNetworkID].rotationEuler = networkClientMove.eulerRotation;
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<NetworkObjectMoveEvent>(OnEntityMove);
            EventBus.Unsubscribe<ClientLeft>(OnClientLeft);
        }
    }
}
