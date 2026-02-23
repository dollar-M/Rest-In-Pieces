// using UnityEngine;

// public class SwitchCHara : MonoBehaviour
// {
//     public GameObject ghost;
//     public GameObject fox;

//     private GameObject currentCharacter;

//     void Start()
//     {
//         ghost.SetActive(true);
//         fox.SetActive(false);
//         currentCharacter = ghost;
//     }

//     public void SwitchCharacter()
//     {
//         if (currentCharacter == ghost)
//         {
//             ghost.SetActive(false);
//             fox.SetActive(true);
//             currentCharacter = fox;
//         }
//         else
//         {
//             fox.SetActive(false);
//             ghost.SetActive(true);
//             currentCharacter = ghost;
//         }
//     }

//     void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("Crystal"))
//         {
//             SwitchCharacter();
//             Destroy(other.gameObject);
//         }
//         Debug.Log("Touched: " + other.name);
//     }

// }

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Characters")]
    public GameObject ghost;
    public GameObject fox;

    private GameObject currentCharacter;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Start with Ghost active
        ghost.SetActive(true);
        fox.SetActive(false);
        currentCharacter = ghost;
    }

    void FixedUpdate()
    {
        // Read input
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(moveX, moveY).normalized;

        // Move the parent Rigidbody2D
        rb.linearVelocity = movement * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Crystal"))
        {
            SwitchCharacter();
            Destroy(other.gameObject);
            Debug.Log("Touched Crystal! Switching characters.");
        }
    }

    public void SwitchCharacter()
    {
        if (currentCharacter == ghost)
        {
            ghost.SetActive(false);
            fox.SetActive(true);
            currentCharacter = fox;
        }
        else
        {
            fox.SetActive(false);
            ghost.SetActive(true);
            currentCharacter = ghost;
        }
    }
}