using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Running")]
    public float speed;                  // Starting speed
    public float time;
    public float acceleration = 0.12f;   // How much faster the cat gets every second
    public float maxSpeed = 18f;         // Speed cap for the ramp (power-ups can still go above it)

    [Header("Jumping")]
    public float jumpForce;
    public float fastFallSpeed = 15f;    // Downward speed when S / Down is pressed in the air
    public float coyoteTime = 0.1f;      // Can still jump this long after running off an edge
    public float jumpBufferTime = 0.12f; // Space pressed this long before landing still jumps
    [Range(0f, 1f)]
    public float jumpCutMultiplier = 0.5f; // Releasing Space early cuts the upward speed (short hop)
    public AudioClip jumpSound;

    public GroundCheck groundCheck;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Animator animator; // Reference to the Animator component

    private bool isGrounded;
    private bool hasJumped;              // Jumped and not landed yet
    private float lastGroundedTime = -1f;
    private float lastJumpPressedTime = -1f;

    private float baseSpeed;             // Ramps up over time
    private float boostAmount;           // Extra speed from the speed power-up
    private bool isSpeedIncreased = false;
    private float speedIncreaseTimer;

    public float CurrentSpeed => baseSpeed + boostAmount;

    // 1 at the start, 2 at double the starting speed - used for score and effects
    public float SpeedMultiplier => speed > 0f ? CurrentSpeed / speed : 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>(); // Assign the Animator component
        baseSpeed = speed;
    }

    void Update()
    {
        isGrounded = groundCheck.IsGrounded;

        baseSpeed = Mathf.Min(baseSpeed + acceleration * Time.deltaTime, Mathf.Max(maxSpeed, speed));

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
        transform.Translate(Vector2.right * CurrentSpeed * Time.deltaTime * time);
    }

    void Jump()
    {
        // The ground trigger stays on for a moment after take-off, so only count
        // it as a landing when the cat is not moving up anymore
        if (isGrounded && rb.velocity.y <= 0.01f)
        {
            lastGroundedTime = Time.time;
            hasJumped = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            lastJumpPressedTime = Time.time;
        }

        bool jumpBuffered = Time.time - lastJumpPressedTime <= jumpBufferTime;
        bool canJump = !hasJumped && Time.time - lastGroundedTime <= coyoteTime;

        if (jumpBuffered && canJump)
        {
            hasJumped = true;
            lastJumpPressedTime = -1f;
            rb.velocity = new Vector2(rb.velocity.x, 0f); // Same jump height no matter if falling off an edge
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); // Apply the jump force
            PlayJumpSound();
        }

        // Short hop: let go of Space while going up
        if (Input.GetKeyUp(KeyCode.Space) && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }

        // Fast fall
        if (!isGrounded && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)))
        {
            rb.velocity = new Vector2(rb.velocity.x, -fastFallSpeed);
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
            boostAmount = amount;
            speedIncreaseTimer = duration;
        }
    }

    void ResetSpeed()
    {
        isSpeedIncreased = false;
        boostAmount = 0f;
    }
}
