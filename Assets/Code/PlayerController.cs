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
using Assets.MultiplayerArchitecture.Code.Entities;

[ViewOf(typeof(Player))]
public class PlayerController : EntityView
{
    EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    [SerializeField] private float acceleration = 20.0f;
    private const float maxSpeed = 20.0f;
    [SerializeField] private float currentMaxSpeed = 20.0f;
    [SerializeField] private float turnSpeed = 150.0f;
    [SerializeField] private float drag = 0f;
    private float spinningTimmer = 0f;
    private bool isSpinning = false;
    private const float speedReduction = 0.20f;
    private Rigidbody rb;

    public override void Init()
    {
        base.Init();

        rb = GetComponent<Rigidbody>();
    }

    public override void Tick(float deltaTime)
    {
        if (!isSpinning)
        {
            Move();
            Turn(deltaTime);
            LimitSpeed();
            SendMovementEvent(deltaTime);
        }
        else
        {
            spinningTimmer += deltaTime;

            float spinSpeed = 720f;
            transform.Rotate(Vector3.up, spinSpeed * deltaTime, Space.Self);

            float wobble = Mathf.Sin(spinningTimmer * 20f) * 15f;
            transform.rotation *= Quaternion.Euler(0f, wobble, 0f);

            if (spinningTimmer > 3f)
            {
                isSpinning = false;
                spinningTimmer = 0f;
            }
        }

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

        if (rb.linearVelocity.magnitude > Mathf.Epsilon)
        {
            float speedFactor = rb.linearVelocity.magnitude / currentMaxSpeed;
            float turn = input * turnSpeed * speedFactor * deltaTime;

            Quaternion rot = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * rot);
        }
    }

    private void LimitSpeed()
    {
        if (rb.linearVelocity.magnitude > currentMaxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentMaxSpeed;
        }
    }

    private void SendMovementEvent(float deltaTime)
    {
        Vector3 velocity = rb.linearVelocity;

        if (velocity.sqrMagnitude > Mathf.Epsilon)
        {
            Vector3 newPos = transform.position + velocity * deltaTime;

            EventBus.Raise<LocalObjectMoveEvent>(ArchitectureID, new Coordinate(newPos.x, newPos.y, newPos.z), 
                new Rotation(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w));
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent<BananaView>(out BananaView bananaView) || collider.TryGetComponent<ChasingBulletView>(out ChasingBulletView chasingBullet))
        {
            isSpinning = true;
        }

        if (collider.TryGetComponent<OilView>(out OilView oilView))
        {
            currentMaxSpeed = maxSpeed * speedReduction;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<OilView>(out OilView oilView))
        {
            currentMaxSpeed = maxSpeed;
        }
    }

}
