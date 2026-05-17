using Assets.Code;
using Assets.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Entities.Events;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using MultiplayerArchitecture.Entities;
using UnityEngine;
using MutliplayerView.Game.Mapping;
using MultiplayerView;

[ViewOf(typeof(Player))]
public class PlayerController : EntityView
{
    EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    [SerializeField] private float acceleration = 20.0f;
    [SerializeField] private float maxSpeed = 20.0f;
    [SerializeField] private float turnSpeed = 150.0f;
    [SerializeField] private float drag = 0f;
    private Rigidbody rb;

    public override void Init()
    {
        base.Init();

        rb = GetComponent<Rigidbody>();
    }

    public override void Tick(float deltaTime)
    {
        Move();
        Turn(deltaTime);
        LimitSpeed();
        SendMovementEvent(deltaTime);

        if (Input.GetKeyDown(KeyCode.E))
            EventBus.Raise<SpawnItemRequestEvent>(new Coordinate(transform.position.x, transform.position.y, transform.position.z));
    }

    private void Move()
    {
        float input = Input.GetAxis("Vertical");

        if (input != 0f)
        {
            rb.AddForce(transform.forward * input * acceleration, ForceMode.Acceleration);
        }

        rb.linearDamping = drag;
    }

    private void Turn(float deltaTime)
    {
        float input = Input.GetAxis("Horizontal");

        if (rb.linearVelocity.magnitude > 0.1f)
        {
            float speedFactor = rb.linearVelocity.magnitude / maxSpeed;
            float turn = input * turnSpeed * speedFactor * deltaTime;

            Quaternion rot = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * rot);
        }
    }

    private void LimitSpeed()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void SendMovementEvent(float deltaTime)
    {
        Vector3 velocity = rb.linearVelocity;

        if (velocity.sqrMagnitude > Mathf.Epsilon)
        {
            Vector3 newPos = transform.position + velocity * deltaTime;

            EventBus.Raise<NetworkObjectMoveEvent>(OwnerNetworkID, ArchitectureID, new Coordinate(newPos.x, newPos.y, newPos.z));
        }
    }

}
