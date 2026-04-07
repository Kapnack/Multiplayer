using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;

public sealed class InputManager : ITickable
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private const float friction = 4.0f;
    private const float aceleration = 15.0f;

    private Vector3 velocity;

    public void Tick(float deltaTime)
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(h, 0, v);

        velocity += input * (aceleration * deltaTime);

        velocity -= velocity * (friction * deltaTime);

        if (velocity.magnitude > Mathf.Epsilon)
            EventBus.Raise<PlayerMoveEvent>(velocity.x, velocity.y, velocity.z);
    }
}
