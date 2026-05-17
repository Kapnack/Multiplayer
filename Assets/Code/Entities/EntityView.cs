using Assets.Code;
using Assets.MultiplayerArchitecture.Code.Entities;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture.Entities;
using MutliplayerView.Game.Mapping;
using UnityEngine;

namespace MultiplayerView
{
    [ViewOf(typeof(Entity))]
    public class EntityView : ViewComponent
    {
        protected NetworkRegistry EntityRegistry => ServiceProvider.Instance.GetService<NetworkRegistry>();
        protected MapView MapView => ServiceProvider.Instance.GetService<MapView>();

        public uint OwnerNetworkID;
        public uint ArchitectureID;

        static public string SetIDMethod => nameof(SetID);

        protected Entity ArchitectureEntity => EntityRegistry.Get<Entity>(ArchitectureID, OwnerNetworkID);

        private void SetID(uint ownerNetworkID, uint architectureID)
        {
            OwnerNetworkID = ownerNetworkID;
            ArchitectureID = architectureID;
        }

    private int GetClosestMarkerIndex(Vector3 position)
    {
        int closest = 0;
        float minDist = float.MaxValue;
        for (int i = 0; i < MapView.raceMarkers.Length; i++)
        {
            float dist = Vector3.Distance(position, MapView.raceMarkers[i]);
            if (dist < minDist)
            {
                minDist = dist;
                closest = i;
            }
        }
        return closest;
    }
    }
}