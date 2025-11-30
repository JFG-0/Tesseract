using UnityEngine;

public class ClickMoveOnObject : MonoBehaviour
{
    private Vector3 startPos;
    private bool isMovingUp = false;
    private bool isMovingDown = false;
    private float speed = 3f; // movement speed

    void Start()
    {
        // Save the starting position
        startPos = transform.position;
    }

    // This is called when you click directly on the object (requires Collider)
    void OnMouseDown()
    {
        if (!isMovingUp && !isMovingDown)
        {
            isMovingUp = true;
        }
    }

    void Update()
    {
        // Move up
        if (isMovingUp)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                                                     startPos + Vector3.up * 3,
                                                     speed * Time.deltaTime);

            if (transform.position == startPos + Vector3.up * 3)
            {
                isMovingUp = false;
                isMovingDown = true;
            }
        }

        // Move back down
        if (isMovingDown)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                                                     startPos,
                                                     speed * Time.deltaTime);

            if (transform.position == startPos)
            {
                isMovingDown = false;
            }
        }
    }
}