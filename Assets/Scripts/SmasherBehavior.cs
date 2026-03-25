/*
Implementation:
    1. Attach this script to the smasher GameObject.
    2. Set the topPoint and bottomPoint Transforms to define the movement range of the smasher.
        These can be empty GameObjects placed at the desired top and bottom positions in the scene, but DO NOT make them children of the smasher, as that will cause issues with the movement logic.
    3. Adjust the downSpeed, upSpeed, waitAtTop, and waitAtBottom variables to customize the smasher's behavior.
    4. The smasher will continuously move between the top and bottom points, pausing at each end for the specified wait times.
*/

using UnityEngine;

public class SmasherBehavior : MonoBehaviour
{
    public Transform topPoint;
    public Transform bottomPoint;

    public float downSpeed = 10f;
    public float upSpeed = 3f;

    public float waitAtTop = 1f;
    public float waitAtBottom = 0.5f;

    private bool goingDown = true;
    private float waitTimer = 0f;

    void Start()
    {
        transform.position = topPoint.position;
        waitTimer = waitAtTop;
    }

    void Update()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        if (goingDown)
        {
            MoveAndCheck(bottomPoint, downSpeed, waitAtBottom, false);
        }
        else
        {
            MoveAndCheck(topPoint, upSpeed, waitAtTop, true);
        }
    }

    void MoveAndCheck(Transform target, float speed, float waitTime, bool nextGoingDown)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // Arrival check with snap
        if (Vector3.SqrMagnitude(transform.position - target.position) < 0.0001f)
        {
            transform.position = target.position;
            goingDown = nextGoingDown;
            waitTimer = waitTime;
        }
    }
}
