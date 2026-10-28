using UnityEngine;

public class BackgroundMovement : MonoBehaviour
{
    public float speed;
    public float time;
    public Transform target;

    private float originalSpeed;
    private bool isSpeedIncreased = false;
    private float speedIncreaseAmount;
    private float speedIncreaseDuration;
    private float speedIncreaseTimer;

    private bool isSlowingDown = false;
    private float slowdownTimer = 6f; // Time after which the background starts slowing down
    private float slowdownDuration = 4f; // Duration over which the background slows down
    private float slowdownStartSpeed;

    void Start()
    {
        originalSpeed = speed;
        slowdownTimer = 6f; // Reset slowdown timer
    }

    void Update()
    {
        if (target == null)
        {
            // Handle case where target is null
        }
        else if (target != null)
        {
            Move();
            UpdateSpeed();
        }

        if (!isSpeedIncreased)
        {
            slowdownTimer -= Time.deltaTime;
            if (slowdownTimer <= 0 && !isSlowingDown)
            {
                StartSlowingDown();
            }
        }
    }

    void Move()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime * time);
    }

    void StartSlowingDown()
    {
        isSlowingDown = true;
        slowdownStartSpeed = speed;
    }

    void UpdateSpeed()
    {
        if (isSpeedIncreased)
        {
            speedIncreaseTimer -= Time.deltaTime;
            if (speedIncreaseTimer <= 0)
            {
                ResetSpeed();
            }
        }

        if (isSlowingDown)
        {
            float t = (slowdownDuration - slowdownTimer) / slowdownDuration;
            speed = Mathf.Lerp(slowdownStartSpeed, originalSpeed, t);
            slowdownTimer -= Time.deltaTime;
            if (slowdownTimer <= 0)
            {
                isSlowingDown = false;
            }
        }
    }

    public void IncreaseSpeed(float amount, float duration)
    {
        if (!isSpeedIncreased)
        {
            isSpeedIncreased = true;
            speedIncreaseAmount = amount;
            speedIncreaseDuration = duration;
            speedIncreaseTimer = duration;
            speed += amount;
            Debug.Log("Background speed increased by " + amount + " for " + duration + " seconds");
        }
    }

    void ResetSpeed()
    {
        isSpeedIncreased = false;
        speed -= speedIncreaseAmount;
        Debug.Log("Background speed reset to normal");
    }
}