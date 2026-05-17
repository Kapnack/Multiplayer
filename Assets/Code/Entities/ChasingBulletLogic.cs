using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using System;
using System.Linq;

namespace Assets.Code.Entities
{
    internal class ChasingBulletLogic : ITickable, IInitable, IDisposable
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        NetworkRegistryView NetworkRegistryView => ServiceProvider.Instance.GetService<NetworkRegistryView>();
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();

        Random random = new Random();

        public ChasingBulletLogic()
        { }

        public void Init()
        {
            EventBus.Subscribe<EntityViewCreatedEvent<ChasingBullet>>(OnBulletCreated);
        }
        public void LateInit()
        {

        }

        private void OnBulletCreated(in EntityViewCreatedEvent<ChasingBullet> entityViewCreatedEvent)
        {
            if (entityViewCreatedEvent.ownerNetworkID != GameClient.MyID)
                return;

            ChasingBulletView newLocalBulletView =
                NetworkRegistryView.GetAs<ChasingBulletView>(entityViewCreatedEvent.ownerNetworkID, entityViewCreatedEvent.objectNetworkID);

            uint randomUserID = 0;
            PlayerController target;

            do
            {
                randomUserID = (uint)random.Next(1, NetworkRegistryView.ClientsAmount);

                target = NetworkRegistryView.PlayerView(randomUserID)
                    .First<PlayerController>();

            } while (randomUserID == GameClient.MyID || randomUserID == 0);

            newLocalBulletView.SetTarget(target.transform);
        }


        public void Tick(float deltaTime)
        {
            foreach (ChasingBulletView chasingBullet in NetworkRegistryView.ChasingBulletView(GameClient.MyID))
                chasingBullet.Tick(deltaTime);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<EntityViewCreatedEvent<ChasingBullet>>(OnBulletCreated);
        }
    }
}