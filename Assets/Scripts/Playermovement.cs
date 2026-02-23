// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class NewMonoBehaviourScript : MonoBehaviour
// {
//     public float speed = 5f; // Speed of the player movement
//     private Vector2 movement; // Variable to store movement input
//     private Animator animator; 
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//          animator = GetComponent<Animator>();
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         float input =Input.GetAxisRaw("Horizontal"); // Get horizontal input on keys
//         movement.x = input * Time.deltaTime * speed; // only move horizontally, multiply by deltaTime for frame rate independence
//         transform.Translate(movement); // Move the player based on the movement vector
//         if (input != 0) //no input from key
//         {
//             animator.SetBool("isRunning", true); 
//         }
//         else
//         {
//             animator.SetBool("isRunning", false); 
//         }
//     }
// }

using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D parentRb;

    void Awake()
    {
        animator = GetComponent<Animator>();
        parentRb = transform.parent.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Use parent's velocity to set animation
        bool isRunning = parentRb.linearVelocity.magnitude > 0.1f;
        animator.SetBool("isRunning", isRunning);
    }
}