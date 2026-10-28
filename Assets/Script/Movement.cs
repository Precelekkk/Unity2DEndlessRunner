using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed;
    public float time;
    public float jumpForce;
    public float fastFallSpeed = 15f; // Downward speed when Space is pressed again in the air
    public AudioClip jumpSound;
    private float MaxJumps = 1;
    private float JumpNumber;
    private bool isGrounded;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Animator animator; // Reference to the Animator component
    public GroundCheck groundCheck;

    private bool isSpeedIncreased = false;
    private float speedIncreaseAmount;
    private float speedIncreaseDuration;
    private float speedIncreaseTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>(); // Assign the Animator component
    }

    void Update()
    {
        isGrounded = groundCheck.IsGrounded;
        Move();
        Jump();

        if (isSpeedIncreased)
        {
            speedIncreaseTimer -= Time.deltaTime;
            if (speedIncreaseTimer <= 0)
            {
                ResetSpeed();
            }
        }
    }

    void Move()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime * time);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (JumpNumber < MaxJumps)
            {
                JumpNumber++;
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); // Apply the jump force
                PlayJumpSound();
            }
            else if (!isGrounded)
            {
                // Second press in the air = fast fall (used to teleport the cat to y = 0)
                rb.velocity = new Vector2(rb.velocity.x, -fastFallSpeed);
            }
        }

        if (isGrounded)
        {
            JumpNumber = 0;
        }

        if (animator != null) // Ensure animator is not null before accessing it
        {
            animator.SetBool("Jump", !isGrounded);
        }
    }

    void PlayJumpSound()
    {
        if (jumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
        else
        {
            Debug.LogWarning("Jump sound or AudioSource not assigned");
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
            Debug.Log("Player speed increased by " + amount + " for " + duration + " seconds");
        }
    }

    void ResetSpeed()
    {
        isSpeedIncreased = false;
        speed -= speedIncreaseAmount;
        Debug.Log("Player speed reset to normal");
    }
}