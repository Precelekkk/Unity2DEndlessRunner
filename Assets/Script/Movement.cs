using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Running")]
    public float speed;                  // Starting speed
    public float time;
    public float acceleration = 0.12f;   // How much faster the cat gets every second
    public float maxSpeed = 18f;         // Speed cap for the ramp (power-ups can still go above it)

    [Header("Jumping")]
    // Space #1 on the ground = jump, Space #2 in the air = second jump,
    // Space #3 in the air = slam down to the ground (dodge something / land faster)
    public float jumpForce;
    public float airJumpForce = 3f;      // Force of the second jump
    public float fastFallSpeed = 15f;    // Downward speed of the slam
    public float coyoteTime = 0.1f;      // The ground jump still works this long after running off an edge
    public AudioClip jumpSound;

    public GroundCheck groundCheck;      // Feet trigger - used together with the body contacts below

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Animator animator; // Reference to the Animator component

    private ContactFilter2D groundFilter;
    private bool isGrounded;
    private int jumpsUsed;               // 0 = on the ground, 1 = jumped, 2 = second jump, 3 = slammed
    private float lastGroundedTime = -1f;

    private float baseSpeed;             // Ramps up over time
    private float boostAmount;           // Extra speed from the speed power-up
    private bool isSpeedIncreased = false;
    private float speedIncreaseTimer;

    public float CurrentSpeed => baseSpeed + boostAmount;

    // Speed power-up state, shown by SpeedBoostHud
    public bool IsBoosted => isSpeedIncreased;
    public float BoostTimeLeft => isSpeedIncreased ? Mathf.Max(0f, speedIncreaseTimer) : 0f;
    public float BoostDuration { get; private set; }

    // 1 at the start, 2 at double the starting speed - used for score and spawning
    public float SpeedMultiplier => speed > 0f ? CurrentSpeed / speed : 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>(); // Assign the Animator component
        baseSpeed = speed;

        // Only contacts that push the cat UP count as ground (not walls / ceilings)
        groundFilter = new ContactFilter2D();
        groundFilter.SetNormalAngle(45f, 135f);
    }

    void Update()
    {
        UpdateGrounded();

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

    void UpdateGrounded()
    {
        bool touchingGround = rb.IsTouching(groundFilter) || (groundCheck != null && groundCheck.IsGrounded);

        // Right after a jump the cat still touches the ground for a frame,
        // so it only counts as landed when it is not moving up
        isGrounded = touchingGround && rb.velocity.y <= 0.01f;

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            jumpsUsed = 0;
        }
    }

    void Move()
    {
        transform.Translate(Vector2.right * CurrentSpeed * Time.deltaTime * time);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool canGroundJump = jumpsUsed == 0 && Time.time - lastGroundedTime <= coyoteTime;

            if (canGroundJump)
            {
                DoJump(jumpForce);
                jumpsUsed = 1;
            }
            else if (jumpsUsed <= 1)
            {
                // Second jump (also used when the cat ran off an edge without jumping)
                DoJump(airJumpForce);
                jumpsUsed = 2;
            }
            else if (jumpsUsed == 2)
            {
                Slam();
                jumpsUsed = 3;
            }
        }

        // S / Down arrow = slam at any time in the air
        if (!isGrounded && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)))
        {
            Slam();
            jumpsUsed = 3;
        }

        if (animator != null) // Ensure animator is not null before accessing it
        {
            animator.SetBool("Jump", !isGrounded);
        }
    }

    void DoJump(float force)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f); // Same height no matter if the cat was falling
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        PlayJumpSound();
    }

    void Slam()
    {
        rb.velocity = new Vector2(rb.velocity.x, -fastFallSpeed);
        AudioManager.PlaySfx(RetroSfx.Slam);
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

    // Picking up another boost while boosted refills the timer
    public void IncreaseSpeed(float amount, float duration)
    {
        isSpeedIncreased = true;
        boostAmount = Mathf.Max(boostAmount, amount);
        speedIncreaseTimer = duration;
        BoostDuration = duration;
    }

    void ResetSpeed()
    {
        isSpeedIncreased = false;
        boostAmount = 0f;
        speedIncreaseTimer = 0f;
    }
}
