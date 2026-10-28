using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed;
    public float time;
    public float jumpForce;
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
    animator.SetBool("Jump", true);
    if (Input.GetKeyDown(KeyCode.Space))
    {
        if (animator != null) // Ensure animator is not null before accessing it
            
        
        if (JumpNumber < MaxJumps)
        {
            JumpNumber++;
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); // Apply the jump force
            PlayJumpSound();
        }
        else if (JumpNumber >= MaxJumps && !isGrounded)
        {
            Vector3 newPosition = transform.position;
            newPosition.y = 0;
            transform.position = newPosition;
            JumpNumber = 0;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }

    if (isGrounded)
    {
        if (animator != null) // Ensure animator is not null before accessing it
            animator.SetBool("Jump", false);
        
        JumpNumber = 0;
    }
}

    void PlayJumpSound()
    {
        if (jumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpSound);
            Debug.Log("Jump sound played");
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