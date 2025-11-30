using UnityEngine;
using UnityEngine.UI;

public class ClickMoveButton : MonoBehaviour
{
    [Header("UI Button")]
    public Button uiButton; // assign your UI button in Inspector

    [Header("Target Object")]
    public GameObject targetObject; // assign the object you want to move

    private Vector3 startPos;
    private bool isMovingUp = false;
    private bool isMovingDown = false;
    private float speed = 3f;

    void Start()
    {
        if (targetObject != null)
        {
            startPos = targetObject.transform.position;
        }

        if (uiButton != null)
        {
            // Link the button click to our handler
            uiButton.onClick.AddListener(OnButtonClick);
        }
    }

    void OnButtonClick()
    {
        if (targetObject != null && !isMovingUp && !isMovingDown)
        {
            isMovingUp = true;
        }
    }

    void Update()
    {
        if (targetObject == null) return;

        // Move up
        if (isMovingUp)
        {
            targetObject.transform.position = Vector3.MoveTowards(
                targetObject.transform.position,
                startPos + Vector3.up * 3,
                speed * Time.deltaTime
            );

            if (targetObject.transform.position == startPos + Vector3.up * 3)
            {
                isMovingUp = false;
                isMovingDown = true;
            }
        }

        // Move back down
        if (isMovingDown)
        {
            targetObject.transform.position = Vector3.MoveTowards(
                targetObject.transform.position,
                startPos,
                speed * Time.deltaTime
            );

            if (targetObject.transform.position == startPos)
            {
                isMovingDown = false;
            }
        }
    }
}