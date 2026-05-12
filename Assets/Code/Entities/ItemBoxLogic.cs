using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

        private List<ItemBoxOnCooldown> itemBoxesOnCooldown;

        private const float Iteam_Box_Cooldown = 3;

        public ItemBoxLogic()
        {
            itemBoxesOnCooldown = new List<ItemBoxOnCooldown>();
        }

        public void Init()
        {
            EventBus.Subscribe<IteamBoxCollectedEvent>(OnItemBoxCollected);
        }

        public void LateInit()
        {

        }

        public void Tick(float deltaTime)
        {
            foreach (ItemBoxOnCooldown itemBox in itemBoxesOnCooldown)
                if (itemBox.timeOfCollision - Time.realtimeSinceStartup > Iteam_Box_Cooldown)
                    OnEndOfItemBoxCooldown(itemBox);
        }

        private void OnItemBoxCollected(in IteamBoxCollectedEvent iteamBoxCollectedEvent)
        {
            itemBoxesOnCooldown.Add(new ItemBoxOnCooldown(iteamBoxCollectedEvent.ownerNetworkID, iteamBoxCollectedEvent.objectNetworkID, Time.realtimeSinceStartup));
            EntityRegistryView.Get(iteamBoxCollectedEvent.ownerNetworkID, iteamBoxCollectedEvent.objectNetworkID).enabled = false;
        }

        private void OnEndOfItemBoxCooldown(in ItemBoxOnCooldown itemBoxOnCooldown)
        {
            EntityRegistryView.Get(itemBoxOnCooldown.ownerNetworkID, itemBoxOnCooldown.objectNetworkID).enabled = true;
            itemBoxesOnCooldown.Remove(itemBoxOnCooldown);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<IteamBoxCollectedEvent>(OnItemBoxCollected);
        }
    }
}
