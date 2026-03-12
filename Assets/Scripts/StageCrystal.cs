using UnityEngine;

public class StageCrystal : MonoBehaviour
{
    [Header("Stage to Unlock")]
    public PlayerMovement.PlayerStage stageToGive;

    public ParticleSystem collectEffect; // optional sparkle

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            Debug.Log("Player collected a stage crystal: " + stageToGive);
            player.ChangePlayerStage(stageToGive); // change stage
            if(collectEffect != null)
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            Destroy(gameObject); // remove the crystal
        }
    }
}
