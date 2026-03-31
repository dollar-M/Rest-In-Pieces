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
        Debug.Log("Entered trigger with: " + collision.name);
        BoosterInfo booster = collision.GetComponent<BoosterInfo>();

        if (booster != null && booster != lastBooster)
        {
            lastBooster = booster;

            if (VelocityReset)
            {
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

