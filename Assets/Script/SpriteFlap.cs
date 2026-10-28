using UnityEngine;

// Tiny two-frame animation: swaps the sprite every few hundredths of a second.
// Used for the wings of the flying enemy.
public class SpriteFlap : MonoBehaviour
{
    public Sprite[] frames;
    public float frameSeconds = 0.14f;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private int index;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        timer = Random.Range(0f, frameSeconds); // Birds next to each other do not flap in sync
    }

    private void Update()
    {
        if (spriteRenderer == null || frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= frameSeconds)
        {
            timer -= frameSeconds;
            index = (index + 1) % frames.Length;
            spriteRenderer.sprite = frames[index];
        }
    }
}
