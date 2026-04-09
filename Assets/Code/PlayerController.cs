using Assets.MultiplayerArchitecture.Code.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private const float friction = 4.0f;
    private const float aceleration = 15.0f;

    private Vector3 velocity;

    EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(h, 0, v);

        velocity += input * (aceleration * Time.deltaTime);

        velocity -= velocity * (friction * Time.deltaTime);

        if (velocity.magnitude > Mathf.Epsilon || Mathf.Approximately(velocity.magnitude, 0))
        {
            Vector3 newPos = transform.position + velocity * Time.deltaTime;
            EventBus.Raise<PlayerMove>(new Coordinate(newPos.x, newPos.y, newPos.z));
        }
    }
}
