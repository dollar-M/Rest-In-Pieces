using UnityEngine;

public class UPKill : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float speed = 2f;
    [SerializeField] private int startDirection = 1;

    private float halfHeight;
    private int currentDirection;
    private Vector2 movement;

    private void Start()
    {
        halfHeight = spriteRenderer.bounds.extents.y;
        currentDirection = startDirection;
    }

    private void FixedUpdate()
    {
        SetDirection();

        movement.x = rigidBody.linearVelocityX;
        movement.y = speed * currentDirection;
        rigidBody.linearVelocity = movement;
    }

    private void SetDirection()
    {
        bool hitUp = Physics2D.Raycast(
            transform.position,
            Vector2.up,
            halfHeight + 0.1f,
            LayerMask.GetMask("KillBlockLR")
        );

        bool hitDown = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            halfHeight + 0.1f,
            LayerMask.GetMask("KillBlockLR")
        );

        if (hitUp && currentDirection > 0)
        {
            currentDirection = -1;
        }
        else if (hitDown && currentDirection < 0)
        {
            currentDirection = 1;
        }
    }
    
}
