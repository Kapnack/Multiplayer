using Assets.Code.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using UnityEngine;
using UnityEngine.AI;
using ZooArchitect.View.Mapping;

[ViewOf(typeof(ChasingBullet))]
public class ChasingBulletView : EntityView
{
    EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    public Transform player;
    public float behindDistance = 5f;
    public float strikeDistance = 2f;

    private NavMeshAgent agent;
    private bool isStriking = false;

    public override void Init()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        if (player == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        isStriking = distanceToPlayer < strikeDistance || Mathf.Approximately(distanceToPlayer, strikeDistance);

        if (isStriking)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            Vector3 targetPosition = player.position - (player.forward * behindDistance);
            agent.SetDestination(targetPosition);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<PlayerController>(out PlayerController player))
            return;

        EventBus.Raise<ChasingBulletCollideEvent>(OwnerNetworkID, ArchitectureID, player.OwnerNetworkID, player.ArchitectureID);
    }
}