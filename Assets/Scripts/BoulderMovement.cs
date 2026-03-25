/*
Implementation:
    1. Attach this script to the boulder GameObject.
    2. Ensure the boulder has a Rigidbody2D component for physics interactions.
    3. Create a Booster object in the scene with a BoosterInfo component that defines the boost force and direction.
    4. When the boulder enters the trigger area of the Booster, it will receive an boost in the specified direction and magnitude.
*/

using UnityEngine;

public class BoulderMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private bool VelocityReset = true; // Option to reset velocity before applying boost

    // Prevents repeated boosting while inside the same booster
    private BoosterInfo lastBooster = null;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BoosterInfo booster = collision.GetComponent<BoosterInfo>();

        if (booster != null && booster != lastBooster)
        {
            lastBooster = booster;

            if(VelocityReset){
                rb.linearVelocity = Vector2.zero; // Optional: reset current velocity before applying boost
            }
            Vector2 boost = booster.GetBoost();
            rb.AddForce(boost, ForceMode2D.Impulse);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Reset so the same booster can fire again next time
        if (collision.GetComponent<BoosterInfo>() == lastBooster)
        {
            lastBooster = null;
        }
    }
}


