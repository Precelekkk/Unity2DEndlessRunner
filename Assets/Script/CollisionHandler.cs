using UnityEngine;

// Handles solid (non-trigger) obstacles, e.g. the flying Circle enemy.
// Trigger obstacles are handled in PlayerController.OnTriggerEnter2D.
public class CollisionHandler : MonoBehaviour
{
    public PlayerController playerController; // Reference to the PlayerController script

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") && playerController != null)
        {
            playerController.DestroyPlayer();
        }
    }
}
