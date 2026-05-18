using Assets.MultiplayerArchitecture.Code.Entities.ItemBox;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;

namespace Assets.MultiplayerArchitecture.Code.Entities
{
    internal class ItemController : IInitable
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        Random random;

        Type lastPowerUp;

        public ItemController()
        {
            random = new Random();

            lastPowerUp = null;
        }

        public void Init()
        {
            EventBus.Subscribe<IteamBoxCollectedEvent>(OnPowerUpCollected);
        }

        public void LateInit()
        {

        }

        private void OnPowerUpCollected(in IteamBoxCollectedEvent callback)
        {
            Type newPowerUp = null;

            do
            {
                newPowerUp = ItemTypes.GetItem(random.Next(0, ItemTypes.Count));

            } while (ItemTypes.Count > 1 && newPowerUp.Equals(lastPowerUp));

        }

    }
}
