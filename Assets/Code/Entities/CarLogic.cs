using Assets.MultiplayerArchitecture.Code.Entities.Events;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Code.Entities
{
    internal class CarLogic : IInitable, ITickable, IDisposable
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        NetworkRegistryView EntityRegistryView => ServiceProvider.Instance.GetService<NetworkRegistryView>();

        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();

        MapView MapView => ServiceProvider.Instance.GetService<MapView>();

        public void Init()
        {
            EventBus.Subscribe<NetworkObjectMoveEvent>(OnEntityMove);
        }

        public void LateInit()
        {

        }

        public void Tick(float deltaTime)
        {
            if (!ServiceProvider.Instance.ContainsService<NetworkRegistryView>())
                return;

            IEnumerable<PlayerController> players = EntityRegistryView.PlayerView(GameClient.MyID);

            foreach (PlayerController player in players)
            {
                if (player.Finished)
                    continue;

                player.Tick(deltaTime);
            }

            players = EntityRegistryView.AllOfType<PlayerController>();

            foreach (PlayerController player in players)
            {
                if (player.Finished)
                    continue;

                CheckCheckpoint(player);
                UpdateProgress(player);
            }

            UpdateRanking(players);
        }

        private void OnEntityMove(in NetworkObjectMoveEvent networkClientMove)
        {
            if (!EntityRegistryView.Contains(networkClientMove.ownerNetworkID, networkClientMove.objectNetworkID))
                return;

            EntityRegistryView.Get(networkClientMove.ownerNetworkID, networkClientMove.objectNetworkID).transform.position = new
                Vector3(networkClientMove.coordinate.x, networkClientMove.coordinate.y, networkClientMove.coordinate.z);

            EntityRegistryView.Get(networkClientMove.ownerNetworkID, networkClientMove.objectNetworkID).transform.rotation =
                new Quaternion(networkClientMove.rotation.x, networkClientMove.rotation.y, networkClientMove.rotation.z, networkClientMove.rotation.w);
        }

        private void UpdateRanking(IEnumerable<PlayerController> players)
        {
            List<PlayerController> sorted = new List<PlayerController>(players);

            sorted.Sort((a, b) => b.RaceProgress.CompareTo(a.RaceProgress));

            for (int i = 0; i < sorted.Count; i++)
            {
                sorted[i].RacePosition = i + 1;
            }
        }

        private void CheckCheckpoint(PlayerController player)
        {
            Vector3[] checkpoints = MapView.raceMarkers;

            int nextIndex = player.CurrentCheckpointIndex;
            Vector3 targetCheckpoint = checkpoints[nextIndex];

            float distance = Vector3.Distance(player.transform.position, targetCheckpoint);

            float checkpointRadius = 8f;

            if (distance < checkpointRadius)
            {
                player.CurrentCheckpointIndex++;

                if (player.CurrentCheckpointIndex >= checkpoints.Length)
                {
                    player.CurrentCheckpointIndex = 0;
                    player.Lap++;

                    if (player.Lap >= 3)
                    {
                        player.Finished = true;
                        Debug.Log($"Player {player.name} WON!");
                    }
                }
            }
        }

        private void UpdateProgress(PlayerController player)
        {
            int totalCheckpoints = MapView.raceMarkers.Length;

            float baseProgress = player.Lap * totalCheckpoints + player.CurrentCheckpointIndex;

            Vector3 nextCheckpoint = MapView.raceMarkers[player.CurrentCheckpointIndex];
            float distance = Vector3.Distance(player.transform.position, nextCheckpoint);

            float inverseDistance = 1f / (distance + 1f);

            player.RaceProgress = baseProgress + inverseDistance;
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<NetworkObjectMoveEvent>(OnEntityMove);
        }
    }
}
