using UnityEngine;

// Keeps an object (background, ground) next to the player on the X axis.
// The background used to move at its own speed,
// which drifted away from the cat once the game started to speed up.
public class FollowPlayerX : MonoBehaviour
{
    public Transform target;
    [Range(0f, 0.2f)]
    public float parallax = 0f; // 0 = glued to the player, a bit more = background slowly slides back
    public float stepSize = 0f; // 0 = move every frame; more = jump only after the player moved this far (ground collider)

    private float offsetX;
    private float targetStartX;

    void Start()
    {
        if (target != null)
        {
            offsetX = transform.position.x - target.position.x;
            targetStartX = target.position.x;
        }
    }

    void LateUpdate()
    {
        if (target == null) return; // Player is dead - leave the background where it is

        float travelled = target.position.x - targetStartX;
        Vector3 position = transform.position;
        float desiredX = target.position.x + offsetX - travelled * parallax;
        if (stepSize > 0f && Mathf.Abs(desiredX - position.x) < stepSize) return;

        position.x = desiredX;
        transform.position = position;
    }
}
