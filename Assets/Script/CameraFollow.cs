using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The player's transform
    public float smoothSpeed = 0.125f; // The smooth speed of the camera movement
    public Vector2 offset; // The offset distance between the player and camera on the X and Y axes

    private Vector3 basePosition;  // Camera position without the shake
    private float shakeTimeLeft;
    private float shakeDuration;
    private float shakeStrength;

    void Start()
    {
        basePosition = transform.position;
    }

    void LateUpdate()
    {
        if (target != null) // Check if the target is not null
        {
            Vector3 desiredPosition = new Vector3(target.position.x + offset.x, offset.y, basePosition.z);
            basePosition = Vector3.Lerp(basePosition, desiredPosition, smoothSpeed);
        }

        Vector3 shakeOffset = Vector3.zero;
        if (shakeTimeLeft > 0f)
        {
            // Unscaled time, so the shake also plays during the hit-stop (timeScale = 0)
            shakeTimeLeft -= Time.unscaledDeltaTime;
            float strength = shakeStrength * Mathf.Clamp01(shakeTimeLeft / shakeDuration);
            shakeOffset = (Vector3)(Random.insideUnitCircle * strength);
        }

        transform.position = basePosition + shakeOffset;
    }

    public void Shake(float duration, float strength)
    {
        shakeDuration = Mathf.Max(duration, 0.01f);
        shakeTimeLeft = shakeDuration;
        shakeStrength = strength;
    }
}
