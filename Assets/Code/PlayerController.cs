using Assets.MultiplayerArchitecture.Code.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    [SerializeField] private float acceleration = 20.0f;
    [SerializeField] private float maxSpeed = 20.0f;
    [SerializeField] private float turnSpeed = 120.0f;
    [SerializeField] private float drag = 0f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Move();
        Turn();
        LimitSpeed();
        SendMovementEvent();
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

    private void Turn()
    {
        float input = Input.GetAxis("Horizontal");

        if (rb.linearVelocity.magnitude > 0.1f)
        {
            float speedFactor = rb.linearVelocity.magnitude / maxSpeed;
            float turn = input * turnSpeed * speedFactor * Time.fixedDeltaTime;

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

    private void SendMovementEvent()
    {
        Vector3 velocity = rb.linearVelocity;

        if (velocity.sqrMagnitude > Mathf.Epsilon)
        {
            Vector3 newPos = transform.position + velocity * Time.fixedDeltaTime;

            EventBus.Raise<PlayerMove>(new Coordinate(newPos.x, newPos.y, newPos.z));
        }
    }

}
