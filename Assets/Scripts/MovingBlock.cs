using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class MovingBlock : MonoBehaviour
{
    public enum MovementMode
    {
        Loop,
        PingPong
    }

    [Header("Path")]
    public Transform[] pathPoints;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float waitTimeAtPoint = 0.2f;
    public MovementMode movementMode = MovementMode.Loop;
    public bool startMovingOnPlay = true;

    [Header("Debug")]
    public bool drawPathGizmos = true;
    public float pointGizmoRadius = 0.15f;

    public Vector2 DeltaThisFixedStep { get; private set; }

    private Rigidbody2D rb;
    private Vector3[] cachedWorldPoints;
    private int targetPointIndex = 0;
    private int pingPongDirection = 1;
    private float waitTimer = 0f;
    private bool isMoving = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {
        if (pathPoints == null || pathPoints.Length == 0)
        {
            Debug.LogWarning($"{name}: No path points assigned.");
            enabled = false;
            return;
        }

        cachedWorldPoints = new Vector3[pathPoints.Length];

        for (int i = 0; i < pathPoints.Length; i++)
        {
            if (pathPoints[i] == null)
            {
                Debug.LogWarning($"{name}: Path point at index {i} is null.");
                enabled = false;
                return;
            }

            cachedWorldPoints[i] = pathPoints[i].position;
        }

        rb.position = cachedWorldPoints[0];
        transform.position = cachedWorldPoints[0];

        isMoving = startMovingOnPlay;
        targetPointIndex = cachedWorldPoints.Length > 1 ? 1 : 0;
    }

    void FixedUpdate()
    {
        Vector2 startPosition = rb.position;
        Vector2 endPosition = startPosition;

        if (isMoving && cachedWorldPoints != null && cachedWorldPoints.Length > 1)
        {
            if (waitTimer > 0f)
            {
                waitTimer -= Time.fixedDeltaTime;
            }
            else
            {
                Vector2 targetPosition = cachedWorldPoints[targetPointIndex];

                endPosition = Vector2.MoveTowards(
                    startPosition,
                    targetPosition,
                    moveSpeed * Time.fixedDeltaTime
                );

                rb.MovePosition(endPosition);

                if (Vector2.Distance(endPosition, targetPosition) <= 0.01f)
                {
                    endPosition = targetPosition;
                    rb.MovePosition(endPosition);
                    waitTimer = waitTimeAtPoint;
                    AdvanceTargetIndex();
                }
            }
        }

        DeltaThisFixedStep = endPosition - startPosition;
    }

    void AdvanceTargetIndex()
    {
        if (cachedWorldPoints.Length <= 1)
        {
            targetPointIndex = 0;
            return;
        }

        if (movementMode == MovementMode.Loop)
        {
            targetPointIndex++;
            if (targetPointIndex >= cachedWorldPoints.Length)
                targetPointIndex = 0;
        }
        else
        {
            targetPointIndex += pingPongDirection;

            if (targetPointIndex >= cachedWorldPoints.Length)
            {
                pingPongDirection = -1;
                targetPointIndex = cachedWorldPoints.Length - 2;
            }
            else if (targetPointIndex < 0)
            {
                pingPongDirection = 1;
                targetPointIndex = 1;
            }
        }
    }

    public void StartMoving()
    {
        isMoving = true;
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    void OnDrawGizmos()
    {
        if (!drawPathGizmos || pathPoints == null || pathPoints.Length == 0)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < pathPoints.Length; i++)
        {
            if (pathPoints[i] == null)
                continue;

            Gizmos.DrawSphere(pathPoints[i].position, pointGizmoRadius);

            if (i < pathPoints.Length - 1 && pathPoints[i + 1] != null)
            {
                Gizmos.DrawLine(pathPoints[i].position, pathPoints[i + 1].position);
            }
        }

        if (movementMode == MovementMode.Loop && pathPoints.Length > 1)
        {
            if (pathPoints[0] != null && pathPoints[pathPoints.Length - 1] != null)
            {
                Gizmos.DrawLine(pathPoints[pathPoints.Length - 1].position, pathPoints[0].position);
            }
        }
    }
}