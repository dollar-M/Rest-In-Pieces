using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    /*
     * boxcollider2d + isPhaseable on the objectproperties script
     */

    public enum PlayerStage
    {
        Ghost,
        Leg,
        Arm,
        Torso,
        Head
    }

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private float moveInput;

    // Our commonly used variables

    [Header("Player form")]
    public PlayerStage currentStage = PlayerStage.Ghost;

    // We can start with 5, we can always change it
    public float moveSpeed = 5f;


    [Header("Phasing")]
    public KeyCode phaseKeyBind = KeyCode.E;
    public float phaseDuration = 4f;
    public bool playerCanPhase = false;

    private bool isPhasing = false;
    private float currentPhasingTimer;

    [Header("Jumping")]
    public KeyCode jumpKeyBind = KeyCode.Space;
    public float jumpForce = 8f;
    public bool playerCanJump = false;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool onGround = true;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        ChangePlayerStage(currentStage);
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        // Check if the player is on the ground
        if (groundCheck != null)
        {
            onGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Check if the player is a ghost, check if they click the phasing keybind
        if (GetInput(phaseKeyBind) && currentStage == PlayerStage.Ghost && !isPhasing)
        {
            StartPhasing();
        }

        Debug.Log(onGround.ToString());
        if (GetInput(jumpKeyBind) && currentStage == PlayerStage.Leg && playerCanJump && onGround)
        {
            Debug.Log("JUMP");
            Jump();
        }

        // If player is currently phasing, count the timer down
        if (isPhasing)
        {
            currentPhasingTimer -= Time.deltaTime;

            if (currentPhasingTimer <= 0f)
            {
                StopPhasing();
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        // Do all the stuff to make the player jump
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    // This just changes the player stages
    public void ChangePlayerStage(PlayerStage stage)
    {
        currentStage = stage;

        // Reset what the player can do
        playerCanPhase = false;
        playerCanJump = false;

        // If we're not ghost anymore, stop phasing
        if (currentStage != PlayerStage.Ghost)
        {
            StopPhasing();
        }

        // Set what this stage is allowed to do
        switch (currentStage)
        {
            case PlayerStage.Ghost:
                playerCanPhase = true;
                rb.gravityScale = 3f;
                break;

            case PlayerStage.Leg:
                playerCanJump = true;
                rb.gravityScale = 3f;
                break;
        }
    }

    void StartPhasing()
    {
        // Do all the stuff to start phasing
        isPhasing = true;
        currentPhasingTimer = phaseDuration;
        playerCanPhase = true;

        SetPhaseCollision(true);
    }

    void StopPhasing()
    {
        // Do all the stuff to stop phasing
        isPhasing = false;
        currentPhasingTimer = 0f;
        playerCanPhase = false;

        SetPhaseCollision(false);
    }

    void SetPhaseCollision(bool ignoreCollision)
    {
        // Go through every phaseable object in the list
        foreach (ObjectProperties obj in ObjectProperties.phaseableObjects)
        {
            if (obj == null)
                continue;

            Collider2D objectCollider = obj.GetComponent<Collider2D>();

            if (objectCollider != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, objectCollider, ignoreCollision);
            }
        }
    }

    // This is just to save writing some shit
    private bool GetInput(KeyCode key)
    {
        if (Input.GetKeyDown(key))
            return true;
        return false;
    }
}