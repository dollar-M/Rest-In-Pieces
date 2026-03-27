using UnityEngine;

public class StageCrystal : MonoBehaviour
{
    [Header("Stage to Unlock")]
    public PlayerMovement.PlayerStage stageToGive;

    public ParticleSystem collectEffect; // optional sparkle
    public GameObject LegText;
    public GameObject ArmText;
    public GameObject TorsoText;
    void Start()
    {
        LegText.SetActive(false);
        ArmText.SetActive(false);
        TorsoText.SetActive(false);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            Debug.Log("Player collected a stage crystal: " + stageToGive);
            player.ChangePlayerStage(stageToGive); // change stage
            if(stageToGive == PlayerMovement.PlayerStage.Leg)
            {
                LegText.SetActive(true);
            }
            else if(stageToGive == PlayerMovement.PlayerStage.Arm)
            {
                ArmText.SetActive(true);
            }
            else if(stageToGive == PlayerMovement.PlayerStage.Torso)
            {
                TorsoText.SetActive(true);
            }
            if(collectEffect != null)
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            Destroy(gameObject); // remove the crystal
        }
    }
}
