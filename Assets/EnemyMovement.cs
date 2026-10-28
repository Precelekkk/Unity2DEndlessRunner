using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 5f;
    public float verticalSpeed = 0.5f;
    public float minY = 3f;
    public float maxY = 5f;

    void Update()
    {
        // Move the object left continuously
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        // Calculate vertical movement within the specified range
        float newY = Mathf.PingPong(Time.time * verticalSpeed, maxY - minY) + minY;

        // Apply vertical movement
        transform.position = new Vector2(transform.position.x, newY);
    }
}