using Assets.MultiplayerArchitecture.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using MultiplayerView;
using MutliplayerView.Game.Mapping;

namespace Assets.Code.Entities
{
    [ViewOf(typeof(Oil))]
    public class OilView : EntityView
    {
        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        private GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();

        private void OnTriggerEnter(UnityEngine.Collider other)
        {
            if (other.TryGetComponent(out PlayerController playerController))
            {
                if (playerController.OwnerNetworkID != GameClient.MyID)
                    return;


            }
                
        }

        private void OnTriggerExit(UnityEngine.Collider other)
        {
            if (other.TryGetComponent(out PlayerController playerController))
            {
                if (playerController.OwnerNetworkID != GameClient.MyID)
                    return;


            }
        }
    }
}
