using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
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
    private PlayerStage lastStage;

    // Our commonly used variables

    [Header("Player form")]
    public PlayerStage currentStage;

    // We can start with 5, we can always change it
    public float moveSpeed = 5f;

    [Header("Phasing")]
    public KeyCode phaseKeyBind = KeyCode.E;
    public float phaseDuration = 4f;
    public float phaseCooldown = 7f;
    public bool phaseIsOnCooldown;

    private bool playerCanPhase = false;
    private bool isPhasing = false;
    private float currentPhasingTimer;
    private float phaseCooldownTimer;

    [Header("Jumping")]
    public KeyCode jumpKeyBind = KeyCode.Space;
    public float jumpForce = 8f;

    private bool playerCanJump = false;
    private bool playerCanDoubleJump = false;
    private bool doubleJumpAvailable = false;

    [Header("Grappling Hook")]
    public KeyCode grappleBind = KeyCode.F;
    public float grappleDistance = 10f;
    public DistanceJoint2D grappleJoint;
    public LineRenderer grappleLine;
    public bool isGrappling = false;
    public bool grappledToRoof = false;
    public bool grappledToWall = false;
    public bool playerCanGrapple = false;

    private Vector2 grapplePoint;
    private Vector2 grappleDirection;

    [Header("General stuff")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool onGround = true;

    public TMP_Text DebugText;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        ChangePlayerStage(currentStage);
        lastStage = currentStage;

        if (grappleJoint != null)
        {
            grappleJoint.enabled = false;
        }

        if (grappleLine != null)
        {
            grappleLine.positionCount = 2;
            grappleLine.enabled = false;
        }
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        // If current stage was changed in the inspector, update the stage settings
        if (currentStage != lastStage)
        {
            ChangePlayerStage(currentStage);
            lastStage = currentStage;
        }

        // Check if the player is on the ground
        if (groundCheck != null)
        {
            onGround = CheckIfGrounded();
        }

        // Reset double jump when player lands
        if (onGround)
        {
            doubleJumpAvailable = true;
        }

        // Check if the player is a ghost, check if they click the phasing keybind
        if (GetInput(phaseKeyBind) && currentStage == PlayerStage.Ghost && !isPhasing && !phaseIsOnCooldown)
        {
            StartPhasing();
        }

        // Normal jump from the ground
        if (GetInput(jumpKeyBind) && playerCanJump && onGround && !isGrappling)
        {
            Jump();
        }
        // Double jump in the air
        else if (GetInput(jumpKeyBind) && playerCanJump && playerCanDoubleJump && !onGround && doubleJumpAvailable && !isGrappling)
        {
            Jump();
            doubleJumpAvailable = false;
        }

        // Check if player clicks the grapple bind
        if (GetInput(grappleBind) && playerCanGrapple)
        {
            if (!isGrappling)
            {
                StartGrapple();
            }
            else
            {
                StopGrapple();
            }
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

        // If the phasing is on cooldown, count the timer down
        if (phaseIsOnCooldown && !isPhasing)
        {
            phaseCooldownTimer -= Time.deltaTime;

            // if cooldown is finished
            if (phaseCooldownTimer <= 0f)
            {
                phaseCooldownTimer = 0f;
                phaseIsOnCooldown = false;
            }
        }

        UpdateGrappleLine();
        UpdateDebugText();
    }

    void FixedUpdate()
    {
        // If grappled to a wall, player can not move
        if (isGrappling && grappledToWall)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        // If grappled to a roof, player can move side to side
        if (isGrappling && grappledToRoof)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    private void UpdateDebugText()
    {
        if (DebugText == null)
            return;

        DebugText.text =
            "Current Stage: " + currentStage +
            "\nCan Jump: " + playerCanJump +
            "\nCan Double Jump: " + playerCanDoubleJump +
            "\nDouble Jump Available: " + doubleJumpAvailable +
            "\nCan Grapple: " + playerCanGrapple +
            "\nIs Phasing: " + isPhasing +
            "\nPhase Timer: " + currentPhasingTimer +
            "\nPhase Cooldown Timer: " + phaseCooldownTimer +
            "\nPhase On Cooldown: " + phaseIsOnCooldown +
            "\nPlayer on ground: " + onGround +
            "\nIs Grappling: " + isGrappling +
            "\nGrappled To Roof: " + grappledToRoof +
            "\nGrappled To Wall: " + grappledToWall +
            "\nPhaseable Objects Count: " + ObjectProperties.phaseableObjects.Count;
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
        playerCanDoubleJump = false;
        playerCanGrapple = false;

        // Reset double jump state
        doubleJumpAvailable = false;

        // If we are not ghost anymore, stop phasing
        if (currentStage != PlayerStage.Ghost)
        {
            StopPhasing();
        }

        // Set what this stage is allowed to do
        switch (currentStage)
        {
            case PlayerStage.Ghost:
                playerCanPhase = true;
                break;

            case PlayerStage.Leg:
                playerCanJump = true;
                break;

            case PlayerStage.Arm:
                playerCanGrapple = true;
                playerCanJump = true;
                break;

            case PlayerStage.Torso:
                playerCanJump = true;
                playerCanDoubleJump = true;
                doubleJumpAvailable = true;
                playerCanGrapple = true;
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

        // Set the phase on cooldown
        phaseIsOnCooldown = true;
        phaseCooldownTimer = phaseCooldown;
    }

    void StartGrapple()
    {
        // If the grapple joint is missing, do nothing
        if (grappleJoint == null)
            return;

        if (Camera.main == null)
            return;

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f;

        // Fire the grapple as a straight line in the direction of the mouse
        grappleDirection = ((Vector2)mouseWorldPosition - (Vector2)transform.position).normalized;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, grappleDirection, grappleDistance);

        RaycastHit2D validHit = default;
        bool foundValidHit = false;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (hit.collider == playerCollider)
                continue;

            if (hit.collider.transform.root == transform.root)
                continue;

            validHit = hit;
            foundValidHit = true;
            break;
        }

        // If we hit nothing, do nothing
        if (!foundValidHit)
            return;

        isGrappling = true;
        grapplePoint = validHit.point;

        grappleJoint.enabled = true;
        grappleJoint.connectedAnchor = grapplePoint;
        grappleJoint.autoConfigureDistance = false;
        grappleJoint.distance = Vector2.Distance(transform.position, grapplePoint);

        // Reset grapple type first
        grappledToRoof = false;
        grappledToWall = false;

        // Roof = surface facing downward
        if (validHit.normal.y < -0.7f)
        {
            grappledToRoof = true;
        }
        // Wall = surface facing left or right
        else if (Mathf.Abs(validHit.normal.x) > 0.7f)
        {
            grappledToWall = true;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            // If it is not really a roof or wall, just retract immediately
            StopGrapple();
        }
    }

    void StopGrapple()
    {
        isGrappling = false;
        grappledToRoof = false;
        grappledToWall = false;

        if (grappleJoint != null)
        {
            grappleJoint.enabled = false;
        }

        if (grappleLine != null)
        {
            grappleLine.enabled = false;
        }
    }

    void UpdateGrappleLine()
    {
        if (grappleLine == null)
            return;

        if (!isGrappling)
        {
            grappleLine.enabled = false;
            return;
        }

        grappleLine.enabled = true;
        grappleLine.SetPosition(0, transform.position);
        grappleLine.SetPosition(1, grapplePoint);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Hit: " + collision.gameObject.name);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log("Still touching: " + collision.gameObject.name);
    }

    void SetPhaseCollision(bool ignoreCollision)
    {
        // Go through every phaseable object in the list
        foreach (Collider2D objectCollider in GetAllPhaseableColliders())
        {
            if (objectCollider == null || playerCollider == null)
                continue;

            Physics2D.IgnoreCollision(playerCollider, objectCollider, ignoreCollision);
        }
    }

    // This checks if the player is grounded on any solid collider
    bool CheckIfGrounded()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            // Ignore the player collider
            if (hit == playerCollider)
                continue;

            // Ignore colliders that belong to the player or the player's children
            if (hit.transform.root == transform.root)
                continue;

            // Ignore trigger colliders
            if (hit.isTrigger)
                continue;

            return true;
        }

        return false;
    }

    List<Collider2D> GetAllPhaseableColliders()
    {
        List<Collider2D> colliders = new List<Collider2D>();

        foreach (ObjectProperties obj in ObjectProperties.phaseableObjects)
        {
            if (obj == null)
                continue;

            if (!obj.isPhaseable)
                continue;

            Collider2D[] objectColliders = obj.GetComponentsInChildren<Collider2D>(true);

            foreach (Collider2D objectCollider in objectColliders)
            {
                if (objectCollider == null)
                    continue;

                if (objectCollider == playerCollider)
                    continue;

                if (objectCollider.transform.root == transform.root)
                    continue;

                colliders.Add(objectCollider);
            }
        }

        return colliders;
    }

    // This is just to save writing some shit
    private bool GetInput(KeyCode key)
    {
        if (Input.GetKeyDown(key))
            return true;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        // Draw the ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}