using UnityEngine;

public class LRKill : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float speed = 3f;
    [SerializeField] private int startDirection = 1;

    private float halfWidth;
    private int currentDirection;
    private Vector2 movement;

    private void Start()
    {
        halfWidth = spriteRenderer.bounds.extents.x;
        currentDirection = startDirection;
    }

    private void FixedUpdate()
    {
        SetDirection();

        movement.x = speed * currentDirection;
        movement.y = rigidBody.linearVelocityY;
        rigidBody.linearVelocity = movement;
    }

    private void SetDirection()
    {
        bool hitRight = Physics2D.Raycast(
            transform.position,
            Vector2.right,
            halfWidth + 0.1f,
            LayerMask.GetMask("KillBlockLR")
        );

        bool hitLeft = Physics2D.Raycast(
            transform.position,
            Vector2.left,
            halfWidth + 0.1f,
            LayerMask.GetMask("KillBlockLR")
        );

        if (hitRight && currentDirection > 0)
        {
            currentDirection = -1;
        }
        else if (hitLeft && currentDirection < 0)
        {
            currentDirection = 1;
        }

        // Debug.DrawRay(transform.position, Vector2.right * (halfWidth + 0.1f), Color.red);
        // Debug.DrawRay(transform.position, Vector2.left * (halfWidth + 0.1f), Color.red);
    }
}
