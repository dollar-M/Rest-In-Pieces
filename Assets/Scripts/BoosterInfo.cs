//Does not do anything exept store information about the booster.

using UnityEngine;

public class BoosterInfo : MonoBehaviour
{
    [Header("Booster Settings")]
    public float boostForce = 20f;
    public Vector2 boostDirection = Vector2.right; // Local direction

    public Vector2 GetBoost()
    {
        return boostDirection.normalized * boostForce;
    }
}

