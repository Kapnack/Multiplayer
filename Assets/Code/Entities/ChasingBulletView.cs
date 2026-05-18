using Assets.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Entities.Events;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using MultiplayerView;
using MutliplayerView.Game.Mapping;
using UnityEngine;

[ViewOf(typeof(ChasingBullet))]
public class ChasingBulletView : EntityView
{
    EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    MapView MapView => ServiceProvider.Instance.GetService<MapView>();

    [SerializeField] private Transform targetPlayer;

    private float speed = 30f;
    private float rotationSpeed = 10f;
    private float markerReachDistance = 3f;

    private float playerDetectionRadius = 15f;

    private int currentMarkerIndex = 0;
    private bool isStrikingPlayer = false;

    public override void Init()
    {
        base.Init();
    }

    public override void LateInit()
    {
        base.LateInit();

        if (MapView != null && MapView.raceMarkers != null && MapView.raceMarkers.Length > 0)
        {
            currentMarkerIndex = GetNearestMarkerToPosition(transform.position);
        }
    }

    public void SetTarget(Transform targetTrasform)
    {
        targetPlayer = targetTrasform;
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        if (targetPlayer == null) return;

        Vector3 targetPos;

        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);
        if (distanceToPlayer <= playerDetectionRadius)
        {
            isStrikingPlayer = true;
        }

        if (isStrikingPlayer)
        {
            targetPos = targetPlayer.position;
        }
        else
        {
            targetPos = MapView.raceMarkers[currentMarkerIndex];

            if (Vector3.Distance(transform.position, targetPos) < markerReachDistance)
            {
                currentMarkerIndex = (currentMarkerIndex + 1) % MapView.raceMarkers.Length;
            }
        }

        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, deltaTime * rotationSpeed);
        }

        transform.position += transform.forward * speed * deltaTime;

        EventBus.Raise<LocalObjectMoveEvent>(ArchitectureID, new Coordinate(transform.position.x, transform.position.y, transform.position.z), new Coordinate(transform.rotation.x, transform.rotation.y, transform.rotation.z));
    }

    private int GetNearestMarkerToPosition(Vector3 position)
    {
        int closest = 0;
        float minDist = float.MaxValue;

        for (int i = 0; i < MapView.raceMarkers.Length; i++)
        {
            float d = Vector3.Distance(position, MapView.raceMarkers[i]);
            if (d < minDist)
            {
                minDist = d;
                closest = i;
            }
        }
        return closest;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<PlayerController>(out PlayerController player))
            return;

        if (player.OwnerNetworkID == OwnerNetworkID)
            return;

        EventBus.Raise<ChasingBulletCollideEvent>(OwnerNetworkID, ArchitectureID, player.OwnerNetworkID, player.ArchitectureID);
        EventBus.Raise<EntityDestroyedEvent>(OwnerNetworkID, ArchitectureID);
    }
}