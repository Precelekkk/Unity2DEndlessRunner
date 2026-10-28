using UnityEngine;
using System.Collections;

public class SpeedPowerUpScript : MonoBehaviour
{
    public float playerSpeedIncreaseAmount = 8f;
    public float backgroundSpeedIncreaseAmount = 8f;
    public float duration = 10f;
    public ParticleSystem speedParticles; // Public reference to the particle system

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Check if the collider is the player
        {
            Movement playerMovement = collision.GetComponent<Movement>(); // Get the Movement component of the player
            if (playerMovement != null)
            {
                playerMovement.IncreaseSpeed(playerSpeedIncreaseAmount, duration); // Increase player speed by specified amount for duration

                // Turn on the particle system if available
                if (speedParticles != null)
                {
                    speedParticles.gameObject.SetActive(true); // Enable the particle system GameObject
                    speedParticles.Play(); // Start playing the particle system

                    // Start coroutine to disable the particle system after the duration
                    StartCoroutine(DisableParticlesAfterDuration(duration));
                    Debug.Log("Coroutine started to disable particles after " + duration + " seconds");
                }

                // Now, find and increase the background speed
                BackgroundMovement backgroundMovement = FindObjectOfType<BackgroundMovement>();
                if (backgroundMovement != null)
                {
                    backgroundMovement.IncreaseSpeed(backgroundSpeedIncreaseAmount, duration);
                }

                Destroy(gameObject); // Destroy the SpeedPowerUp object
            }
        }
    }

    private IEnumerator DisableParticlesAfterDuration(float duration)
    {
        if (duration > 0)
        {
            Debug.Log("Disabling particles after " + duration + " seconds...");
            yield return new WaitForSeconds(duration);
        }
        else
        {
            Debug.Log("Duration is 0. Disabling particles immediately...");
        }
        
        // Disable the particle system after the specified duration
        if (speedParticles != null)
        {
            speedParticles.Stop(); // Stop playing the particle system
            speedParticles.gameObject.SetActive(false); // Disable the particle system GameObject
            Debug.Log("Particles disabled.");
        }
    }
}