using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Security.Cryptography;
using UnityEngine;

public class PlaceHolderMovement : MonoBehaviour
{
    private Rigidbody2D body;
    [SerializeField] private float speed;
    [SerializeField] private float jumpVelocity;
    
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

    }

    private void Update()
    {
        //Horizontal movement
        body.linearVelocity = new UnityEngine.Vector2(Input.GetAxis("Horizontal") * speed, body.linearVelocity.y);

        //Jumping
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W))
        {
            body.linearVelocity = new UnityEngine.Vector2(body.linearVelocity.x, jumpVelocity);
        }
    }
    
}
