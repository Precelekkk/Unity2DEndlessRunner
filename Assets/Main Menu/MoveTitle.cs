using UnityEngine;

public class MoveTitle : MonoBehaviour
{
    public float moveSpeed = 1f; // Speed of movement
    public float moveDuration = 3f; // Duration of each movement
    private float timer = 0f; // Timer to keep track of movement duration
    private bool moveRight = true; // Whether to move right initially

    void Update()
    {
        // Increment the timer
        timer += Time.deltaTime;

        // Check if it's time to change direction
        if (timer >= moveDuration)
        {
            // Reset the timer
            timer = 0f;

            // Change direction
            moveRight = !moveRight;
        }

        // Calculate the direction of movement
        int direction = moveRight ? 1 : -1;

        // Move the title continuously in the specified direction
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);
    }
}