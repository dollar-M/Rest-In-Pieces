/*
Implementation:
    1. Attach this script to the player GameObject.
    2. Ensure the player has a Collider2D component with "Is Trigger" unchecked for collision detection.
    3. Tag all obstacle GameObjects with the tag "Obstacle".
    4. When the player collides with an obstacle, the level will reset after a short delay
*/

using UnityEngine;
using UnityEngine.SceneManagement;

public class ObstacleCollisionDetector : MonoBehaviour
{
    [SerializeField] private float delay = 1f; // Delay before resetting the level

    private void OnCollisionEnter2D(Collision2D collisoin)
    {
        /*if (collisoin.gameObject.CompareTag("Obstacle")) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);//Reloads the current scene, effectively resetting the level
        }*/

        //For use when death animation is added, to delay the reset of the level until after the animation has played
        
        if (collisoin.gameObject.CompareTag("Obstacle"))
        {
            Invoke(nameof(ResetLevel), delay);
        }
    }

    private void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
