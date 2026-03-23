using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float friction = 4.0f;
    private float aceleration = 15.0f;
    private Vector3 velocity;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(h, 0, v);

        velocity += input * (aceleration * Time.deltaTime);

        velocity -= velocity * (friction * Time.deltaTime);

        transform.position += velocity * Time.deltaTime;
    }
}
