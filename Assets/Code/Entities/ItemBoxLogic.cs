using Assets.MultiplayerArchitecture.Code.Entities.ItemBox;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Assets.Code.Entities
{
    public struct ItemBoxOnCooldown
    {
        public uint ownerNetworkID;
        public uint objectNetworkID;
        public float timeOfCollision;

        public ItemBoxOnCooldown(uint ownerNetworkID, uint objectNetworkID, float TimeOfCollision)
        {
            this.ownerNetworkID = ownerNetworkID;
            this.objectNetworkID = objectNetworkID;
            this.timeOfCollision = TimeOfCollision;
        }
    }

    internal class ItemBoxLogic : IInitable, ITickable, IDisposable
    {
        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        private NetworkRegistryView EntityRegistryView => ServiceProvider.Instance.GetService<NetworkRegistryView>();

        private GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();

        private List<ItemBoxOnCooldown> itemBoxesOnCooldown;

        System.Random random;

        Type itemToUse;
        Type previousPowerUp;

        private const float Iteam_Box_Cooldown = 3;

        public ItemBoxLogic()
        {
            itemBoxesOnCooldown = new List<ItemBoxOnCooldown>();

            random = new System.Random();

            previousPowerUp = null;
            itemToUse = null;
        }

        public void Init()
        {
            EventBus.Subscribe<IteamBoxCollectedEvent>(OnItemBoxCollected);
            EventBus.Subscribe<SpawnItemRequestEvent>(OnItemSpawnedRequested);
        }

        public void LateInit()
        {

        }

        public void Tick(float deltaTime)
        {
            List<ItemBoxOnCooldown> toRemove = new List<ItemBoxOnCooldown>();

            foreach (ItemBoxOnCooldown itemBox in itemBoxesOnCooldown)
                if (Time.realtimeSinceStartup - itemBox.timeOfCollision > Iteam_Box_Cooldown)
                {
                    OnEndOfItemBoxCooldown(itemBox);
                    toRemove.Add(itemBox);
                }

            foreach (ItemBoxOnCooldown itemBox in toRemove)
                itemBoxesOnCooldown.Remove(itemBox);
        }

        private void OnEndOfItemBoxCooldown(in ItemBoxOnCooldown itemBoxOnCooldown)
        {
            EntityRegistryView.Get(itemBoxOnCooldown.ownerNetworkID, itemBoxOnCooldown.objectNetworkID).gameObject.SetActive(true);
        }

        private void OnItemBoxCollected(in IteamBoxCollectedEvent iteamBoxCollectedEvent)
        {
            itemBoxesOnCooldown.Add(new ItemBoxOnCooldown(iteamBoxCollectedEvent.ownerNetworkID, iteamBoxCollectedEvent.objectNetworkID, Time.realtimeSinceStartup));
            EntityRegistryView.Get(iteamBoxCollectedEvent.ownerNetworkID, iteamBoxCollectedEvent.objectNetworkID).gameObject.SetActive(false);

            if (itemToUse != null)
                return;

            do
            {
                itemToUse = ItemTypes.GetItem(random.Next(0, ItemTypes.Count));

            } while (ItemTypes.Count > 1 && itemToUse.Equals(previousPowerUp));
        }

        private void OnItemSpawnedRequested(in SpawnItemRequestEvent spawnItemRequestEvent)
        {
            if (itemToUse == null)
                return;

            Type openEventType = typeof(LocalSpawnRequestAcceptedEvent<>);
            Type closedEventType = openEventType.MakeGenericType(itemToUse);

            MethodInfo raiseMethod = typeof(EventBus).GetMethod("Raise")
                .MakeGenericMethod(closedEventType);

            object[] parameters = new object[] { GameClient.MyID, spawnItemRequestEvent.coordinate };

            raiseMethod.Invoke(EventBus, new object[] { parameters });

            Debug.Log(itemToUse.Name + ": Spawn Accepted.");

            previousPowerUp = itemToUse;
            itemToUse = null;
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<IteamBoxCollectedEvent>(OnItemBoxCollected);
            EventBus.Unsubscribe<SpawnItemRequestEvent>(OnItemSpawnedRequested);
        }
    }
}
