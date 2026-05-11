using Assets.MultiplayerArchitecture.Code.Entities.Events;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

namespace Assets.Code.Entities
{
    internal class CarLogic : IInitable, IDisposable
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        EntityRegistryView EntityRegistryView => ServiceProvider.Instance.GetService<EntityRegistryView>();

        public void Init()
        {
            EventBus.Subscribe<NetworkObjectMoveEvent>(OnEntityMove);
        }

        public void LateInit()
        {

        }

        private void OnEntityMove(in NetworkObjectMoveEvent networkClientMove)
        {
            EntityRegistryView.Get(networkClientMove.ownerNetworkID, networkClientMove.ownerNetworkID).transform.position = new Vector3(networkClientMove.coordinate.x, networkClientMove.coordinate.y, networkClientMove.coordinate.z);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<NetworkObjectMoveEvent>(OnEntityMove);
        }
    }
}
