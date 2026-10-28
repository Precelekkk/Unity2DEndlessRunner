using UnityEngine;

// Makes a pick-up float gently up and down, so it reads as "collect me".
// Used by the speed power-up and the +20 points pick-up.
public class FloatBob : MonoBehaviour
{
    public float height = 0.12f;  // How far it moves up and down
    public float speed = 3f;

    private Vector3 startPosition;
    private float offset;

    private void Start()
    {
        startPosition = transform.position;
        offset = Random.Range(0f, Mathf.PI * 2f); // Pick-ups next to each other do not move in sync
    }

    private void Update()
    {
        transform.position = startPosition + Vector3.up * Mathf.Sin(Time.time * speed + offset) * height;
    }
}
