using Assets.MultiplayerArchitecture.Code.Entities.Events;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

namespace Assets.Code.Entities
{
    internal class CarLogic : IInitable, ITickable, IDisposable
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        NetworkRegistryView EntityRegistryView => ServiceProvider.Instance.GetService<NetworkRegistryView>();

        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();

        public void Init()
        {
            EventBus.Subscribe<NetworkObjectMoveEvent>(OnEntityMove);
        }

        public void LateInit()
        {

        }

        public void Tick(float deltaTime)
        {
            foreach (PlayerController entity in EntityRegistryView.PlayerView(GameClient.MyID))
                entity.Tick(deltaTime);
        }

        private void OnEntityMove(in NetworkObjectMoveEvent networkClientMove)
        {
            if (!EntityRegistryView.Contains(networkClientMove.ownerNetworkID, networkClientMove.objectNetworkID))
                return;

            EntityRegistryView.Get(networkClientMove.ownerNetworkID, networkClientMove.objectNetworkID).transform.position = new
                Vector3(networkClientMove.coordinate.x, networkClientMove.coordinate.y, networkClientMove.coordinate.z);

            EntityRegistryView.Get(networkClientMove.ownerNetworkID, networkClientMove.objectNetworkID).transform.rotation =
                Quaternion.Euler(networkClientMove.eulerRotation.x, networkClientMove.eulerRotation.y, networkClientMove.eulerRotation.z);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<NetworkObjectMoveEvent>(OnEntityMove);
        }
    }
}
