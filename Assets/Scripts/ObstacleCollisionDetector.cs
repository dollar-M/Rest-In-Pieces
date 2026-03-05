using UnityEngine;
using UnityEngine.SceneManagement;

public class ObstacleCollisionDetector : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collisoin)
    {
        //[SerializeField] private float delay = 1f; // Delay before resetting the level
        if (collisoin.gameObject.CompareTag("Obstacle")) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);//Reloads the current scene, effectively resetting the level
        }

        //For use when death animation is added, to delay the reset of the level until after the animation has played
        /*
        if (other.CompareTag("Obstacle"))
        {
            Invoke(nameof(ResetLevel), delay);
    }

    void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }*/
    }
}
