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

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private PhysicsMaterial2D runtimeNoFrictionMaterial;

    private float moveInput;
    private PlayerStage lastStage;
    private float normalLinearDamping;
    private float normalGravityScale;

    [Header("Player Form")]
    public PlayerStage currentStage;

    [Header("Movement")]
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
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.12f;

    private bool playerCanJump = false;
    private bool playerCanDoubleJump = false;
    private bool doubleJumpAvailable = false;
    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;

    [Header("Grappling Hook")]
    public KeyCode grappleBind = KeyCode.C;
    public float grappleDistance = 10f;
    public float minimumGrappleDistance = 0.2f;

    [Tooltip("Lower feels looser / more natural while grappling.")]
    public float grappleLinearDamping = 0f;

    [Tooltip("Extra rope length added when grappling a moving rigidbody so you dangle instead of getting dragged sideways.")]
    public float movingBodyGrappleSlack = 1f;

    [Tooltip("Maximum final grapple length after slack is applied.")]
    public float maxFinalGrappleDistance = 1.75f;

    public DistanceJoint2D grappleJoint;
    public LineRenderer grappleLine;

    public bool isGrappling = false;
    public bool playerCanGrapple = false;

    private Vector2 grapplePoint;
    private Rigidbody2D grappledBody;
    private float currentGrappleDistance;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private bool onGround = true;
    private MovingBlock groundedMovingBlock;

    [Header("Wall Check")]
    public float wallCheckDistance = 0.08f;

    [Range(0.1f, 1f)]
    public float wallCheckHeightMultiplier = 0.8f;

    public float wallCheckVerticalInset = 0.08f;

    private bool touchingWallLeft;
    private bool touchingWallRight;

    [Header("Moving Platform Carry")]
    [Tooltip("How much of the platform's horizontal movement gets added to the player while grounded.")]
    public float groundedPlatformCarryMultiplier = 1f;

    [Header("Debug")]
    public TMP_Text DebugText;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        normalLinearDamping = rb.linearDamping;
        normalGravityScale = rb.gravityScale;

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CreateAndApplyNoFrictionMaterial();

        ChangePlayerStage(currentStage);
        lastStage = currentStage;

        if (grappleJoint != null)
        {
            grappleJoint.enabled = false;
            grappleJoint.autoConfigureDistance = false;
            grappleJoint.autoConfigureConnectedAnchor = false;
            grappleJoint.maxDistanceOnly = true;
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

        if (GetInput(jumpKeyBind))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        onGround = CheckIfGrounded(out groundedMovingBlock);
        UpdateWallContacts();

        if (onGround)
        {
            coyoteTimer = coyoteTime;
            doubleJumpAvailable = true;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", isGrappling ? 0f : Mathf.Abs(moveInput));
            animator.SetBool("OnGround", onGround);
            animator.SetInteger("Stage", (int)currentStage);
        }

        if (currentStage != lastStage)
        {
            ChangePlayerStage(currentStage);
            lastStage = currentStage;
        }

        if (GetInput(phaseKeyBind) && currentStage == PlayerStage.Ghost && !isPhasing && !phaseIsOnCooldown)
        {
            StartPhasing();
        }

        if (!isGrappling)
        {
            bool bufferedJump = jumpBufferTimer > 0f;

            if (bufferedJump && playerCanJump && coyoteTimer > 0f)
            {
                Jump();
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
            }
            else if (bufferedJump && playerCanJump && playerCanDoubleJump && !onGround && doubleJumpAvailable)
            {
                Jump();
                doubleJumpAvailable = false;
                jumpBufferTimer = 0f;
            }
        }

        if (GetInput(grappleBind) && playerCanGrapple)
        {
            if (!isGrappling)
            {
                StartGrapple();

                if (animator != null)
                    animator.SetBool("Grapple", true);
            }
            else
            {
                StopGrapple();

                if (animator != null)
                    animator.SetBool("Grapple", false);
            }
        }

        if (isPhasing)
        {
            currentPhasingTimer -= Time.deltaTime;

            if (currentPhasingTimer <= 0f)
            {
                StopPhasing();
            }
        }

        if (phaseIsOnCooldown && !isPhasing)
        {
            phaseCooldownTimer -= Time.deltaTime;

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
        if (isGrappling)
        {
            // Let gravity + the joint handle grapple motion naturally.
            return;
        }

        float platformCarryX = 0f;

        if (onGround && groundedMovingBlock != null)
        {
            platformCarryX = groundedMovingBlock.DeltaThisFixedStep.x / Time.fixedDeltaTime;
            platformCarryX *= groundedPlatformCarryMultiplier;
        }

        float targetXVelocity = (moveInput * moveSpeed) + platformCarryX;

        bool pushingIntoLeftWall = !onGround && touchingWallLeft && moveInput < 0f;
        bool pushingIntoRightWall = !onGround && touchingWallRight && moveInput > 0f;

        if (pushingIntoLeftWall || pushingIntoRightWall)
        {
            targetXVelocity = platformCarryX;

            if ((pushingIntoLeftWall && rb.linearVelocity.x < platformCarryX) ||
                (pushingIntoRightWall && rb.linearVelocity.x > platformCarryX))
            {
                rb.linearVelocity = new Vector2(targetXVelocity, rb.linearVelocity.y);
                return;
            }
        }

        rb.linearVelocity = new Vector2(targetXVelocity, rb.linearVelocity.y);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    public void ChangePlayerStage(PlayerStage stage)
    {
        currentStage = stage;

        playerCanPhase = false;
        playerCanJump = false;
        playerCanDoubleJump = false;
        playerCanGrapple = false;
        doubleJumpAvailable = false;

        if (currentStage != PlayerStage.Ghost)
        {
            StopPhasing();
        }

        if (currentStage != PlayerStage.Arm && currentStage != PlayerStage.Torso && isGrappling)
        {
            StopGrapple();
        }

        switch (currentStage)
        {
            case PlayerStage.Ghost:
                playerCanPhase = true;
                break;

            case PlayerStage.Leg:
                playerCanJump = true;
                break;

            case PlayerStage.Arm:
                playerCanJump = true;
                playerCanGrapple = true;
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
        isPhasing = true;
        currentPhasingTimer = phaseDuration;
        playerCanPhase = true;
        SetPhaseCollision(true);
    }

    void StopPhasing()
    {
        if (!isPhasing && currentPhasingTimer <= 0f)
            return;

        isPhasing = false;
        currentPhasingTimer = 0f;
        playerCanPhase = false;

        SetPhaseCollision(false);

        phaseIsOnCooldown = true;
        phaseCooldownTimer = phaseCooldown;
    }

    void StartGrapple()
    {
        if (grappleJoint == null || rb == null || playerCollider == null)
            return;

        if (Camera.main == null)
            return;

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f;

        Vector2 direction = ((Vector2)mouseWorldPosition - (Vector2)transform.position).normalized;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, grappleDistance);

        RaycastHit2D validHit = default;
        bool foundValidHit = false;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (hit.collider == playerCollider)
                continue;

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            if (hit.collider.isTrigger)
                continue;

            if (hit.collider.gameObject.name.Contains("tunnel"))
                continue;

            validHit = hit;
            foundValidHit = true;
            break;
        }

        if (!foundValidHit)
            return;

        float distanceToPoint = Vector2.Distance(transform.position, validHit.point);

        if (distanceToPoint < minimumGrappleDistance)
            return;

        isGrappling = true;
        grapplePoint = validHit.point;
        grappledBody = validHit.rigidbody;

        bool targetIsMovingBody =
            grappledBody != null &&
            grappledBody.bodyType != RigidbodyType2D.Static;

        rb.gravityScale = normalGravityScale;
        rb.linearDamping = grappleLinearDamping;

        currentGrappleDistance = distanceToPoint;

        if (targetIsMovingBody)
        {
            currentGrappleDistance += movingBodyGrappleSlack;
        }

        currentGrappleDistance = Mathf.Clamp(
            currentGrappleDistance,
            minimumGrappleDistance,
            maxFinalGrappleDistance
        );

        grappleJoint.enabled = true;
        grappleJoint.maxDistanceOnly = true;
        grappleJoint.distance = currentGrappleDistance;

        if (grappledBody != null)
        {
            grappleJoint.connectedBody = grappledBody;
            grappleJoint.connectedAnchor = grappledBody.transform.InverseTransformPoint(validHit.point);
        }
        else
        {
            grappleJoint.connectedBody = null;
            grappleJoint.connectedAnchor = validHit.point;
        }
    }

    void StopGrapple()
    {
        isGrappling = false;
        grappledBody = null;
        currentGrappleDistance = 0f;

        if (grappleJoint != null)
        {
            grappleJoint.enabled = false;
            grappleJoint.connectedBody = null;
        }

        if (grappleLine != null)
        {
            grappleLine.enabled = false;
        }

        if (rb != null)
        {
            rb.linearDamping = normalLinearDamping;
            rb.gravityScale = normalGravityScale;
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

        Vector2 endPoint = grapplePoint;

        if (grappleJoint != null && grappleJoint.connectedBody != null)
        {
            endPoint = grappleJoint.connectedBody.transform.TransformPoint(grappleJoint.connectedAnchor);
            grapplePoint = endPoint;
        }
        else if (grappleJoint != null)
        {
            endPoint = grappleJoint.connectedAnchor;
            grapplePoint = endPoint;
        }

        grappleLine.enabled = true;
        grappleLine.SetPosition(0, transform.position);
        grappleLine.SetPosition(1, endPoint);
    }

    void CreateAndApplyNoFrictionMaterial()
    {
        if (playerCollider == null)
            return;

        runtimeNoFrictionMaterial = new PhysicsMaterial2D("PlayerNoFrictionRuntime");
        runtimeNoFrictionMaterial.friction = 0f;
        runtimeNoFrictionMaterial.bounciness = 0f;
        playerCollider.sharedMaterial = runtimeNoFrictionMaterial;
    }

    void SetPhaseCollision(bool ignoreCollision)
    {
        foreach (Collider2D objectCollider in GetAllPhaseableColliders())
        {
            if (objectCollider == null || playerCollider == null)
                continue;

            Physics2D.IgnoreCollision(playerCollider, objectCollider, ignoreCollision);
        }
    }

    bool CheckIfGrounded(out MovingBlock movingBlockUnderPlayer)
    {
        movingBlockUnderPlayer = null;

        if (playerCollider == null)
            return false;

        Vector2 checkCenter;
        float checkRadius;

        if (groundCheck != null)
        {
            checkCenter = groundCheck.position;
            checkRadius = groundCheckRadius;
        }
        else
        {
            Bounds bounds = playerCollider.bounds;
            checkCenter = new Vector2(bounds.center.x, bounds.min.y - 0.05f);
            checkRadius = 0.12f;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(checkCenter, checkRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            if (hit == playerCollider)
                continue;

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            if (hit.isTrigger)
                continue;

            movingBlockUnderPlayer = hit.GetComponentInParent<MovingBlock>();
            return true;
        }

        return false;
    }

    void UpdateWallContacts()
    {
        touchingWallLeft = CheckWallSide(Vector2.left);
        touchingWallRight = CheckWallSide(Vector2.right);
    }

    bool CheckWallSide(Vector2 direction)
    {
        if (playerCollider == null)
            return false;

        Bounds bounds = playerCollider.bounds;

        float boxWidth = wallCheckDistance;
        float boxHeight = bounds.size.y * wallCheckHeightMultiplier;
        Vector2 boxSize = new Vector2(boxWidth, boxHeight);

        float xOffset = bounds.extents.x + (boxWidth * 0.5f);
        float yOffset = wallCheckVerticalInset;

        Vector2 checkCenter = new Vector2(
            bounds.center.x + (direction.x * xOffset),
            bounds.center.y + yOffset
        );

        Collider2D[] hits = Physics2D.OverlapBoxAll(checkCenter, boxSize, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            if (hit == playerCollider)
                continue;

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

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

                if (objectCollider.transform == transform || objectCollider.transform.IsChildOf(transform))
                    continue;

                colliders.Add(objectCollider);
            }
        }

        return colliders;
    }

    private bool GetInput(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }

    void UpdateDebugText()
    {
        if (DebugText == null)
            return;

        DebugText.text =
            "Current Stage: " + currentStage +
            "\nOn Ground: " + onGround +
            "\nGrounded Moving Block: " + (groundedMovingBlock != null ? groundedMovingBlock.name : "None") +
            "\nIs Grappling: " + isGrappling +
            "\nGrappled Body: " + (grappledBody != null ? grappledBody.name : "None") +
            "\nCurrent Grapple Distance: " + currentGrappleDistance +
            "\nVelocity: " + rb.linearVelocity +
            "\nGrapple Point: " + grapplePoint;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        if (playerCollider != null && groundCheck == null)
        {
            Bounds bounds = playerCollider.bounds;
            Gizmos.DrawWireSphere(new Vector2(bounds.center.x, bounds.min.y - 0.05f), 0.12f);
        }
        else if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (playerCollider != null)
        {
            Bounds bounds = playerCollider.bounds;

            float boxWidth = wallCheckDistance;
            float boxHeight = bounds.size.y * wallCheckHeightMultiplier;
            float xOffset = bounds.extents.x + (boxWidth * 0.5f);

            Vector2 size = new Vector2(boxWidth, boxHeight);
            Vector2 leftCenter = new Vector2(bounds.center.x - xOffset, bounds.center.y + wallCheckVerticalInset);
            Vector2 rightCenter = new Vector2(bounds.center.x + xOffset, bounds.center.y + wallCheckVerticalInset);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(leftCenter, size);
            Gizmos.DrawWireCube(rightCenter, size);
        }
    }
}