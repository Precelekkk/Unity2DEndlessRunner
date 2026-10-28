using UnityEngine;
using System.Collections;

public class SpeedPowerUpScript : MonoBehaviour
{
    public float playerSpeedIncreaseAmount = 8f;
    public float duration = 10f;
    public ParticleSystem speedParticles; // Public reference to the particle system

    [Header("Look")]
    public float iconSize = 1.2f;    // World units (floating up and down: FloatBob on the prefab)

    private void Awake()
    {
        // The prefab's sprite is missing from the project - spawned copies were invisible
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.sprite = PowerUpArt.Speed;
            sprite.color = Color.white;
        }
        transform.localScale = new Vector3(iconSize, iconSize, 1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Check if the collider is the player
        {
            Movement playerMovement = collision.GetComponent<Movement>(); // Get the Movement component of the player
            if (playerMovement != null)
            {
                playerMovement.IncreaseSpeed(playerSpeedIncreaseAmount, duration); // Increase player speed by specified amount for duration
                AudioManager.PlaySfx(RetroSfx.PowerUp);

                // Turn on the particle system if available
                if (speedParticles != null)
                {
                    speedParticles.gameObject.SetActive(true); // Enable the particle system GameObject
                    speedParticles.Play(); // Start playing the particle system

                    // Run it on the player: this power-up destroys itself below, which would stop its own coroutines
                    playerMovement.StartCoroutine(DisableParticlesWhenBoostEnds(playerMovement, speedParticles));
                }

                // The background follows the player, so it speeds up together with the cat

                Destroy(gameObject); // Destroy the SpeedPowerUp object
            }
        }
    }

    // Waits for the boost itself instead of a fixed time, so picking up a second
    // power-up (which refills the boost) keeps the particles on as well
    private static IEnumerator DisableParticlesWhenBoostEnds(Movement movement, ParticleSystem particles)
    {
        yield return new WaitWhile(() => movement != null && movement.IsBoosted);

        if (particles != null)
        {
            particles.Stop(); // Stop playing the particle system
            particles.gameObject.SetActive(false); // Disable the particle system GameObject
        }
    }
}
