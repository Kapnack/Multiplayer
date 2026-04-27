using Assets.MultiplayerArchitecture.Code.Entities.Events;
using Assets.MultiplayerArchitecture.Code.Network;
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
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();

        public void Init()
        {
            EventBus.Subscribe<NetworkClientMove>(OnEntityMove);
            EventBus.Subscribe<PlayerMove>(OnPlayerMove);
            EventBus.Subscribe<ClientLeft>(OnClientLeft);
        }


        public void LateInit()
        {

        }
        private void OnClientLeft(in ClientLeft callback)
        {
            UnityEngine.Object.Destroy(EntityRegistryView[callback.objectID].gameObject);
            EntityRegistryView.Remove(GameClient[callback.networkID]);
        }

        private void OnEntityMove(in NetworkClientMove networkClientMove)
        {
            EntityRegistryView[GameClient[networkClientMove.networkID]].transform.position = new Vector3(networkClientMove.coordinate.x, networkClientMove.coordinate.y, networkClientMove.coordinate.z);
        }

        private void OnPlayerMove(in PlayerMove playerMove)
        {
            EntityRegistryView[GameClient[GameClient.MyID]].transform.position = new Vector3(playerMove.coordinate.x, playerMove.coordinate.y, playerMove.coordinate.z);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<NetworkClientMove>(OnEntityMove);
            EventBus.Unsubscribe<PlayerMove>(OnPlayerMove);
            EventBus.Unsubscribe<ClientLeft>(OnClientLeft);
        }
    }
}
