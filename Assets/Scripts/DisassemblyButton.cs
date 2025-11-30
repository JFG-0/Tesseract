using UnityEngine;
using UnityEngine.UI;

public class DisassemblyButton : MonoBehaviour
{
    [Header("UI Button")]
    public Button uiButton; // assign your UI button in Inspector

    [Header("Target Object")]
    public GameObject activeObject; // assign or set dynamically

    void Start()
    {
        if (uiButton != null)
        {
            // Link the button click to our handler
            uiButton.onClick.AddListener(OnButtonClick);
        }
    }

    void OnButtonClick()
    {
        if (activeObject != null)
        {
            DisassemblyController controller = activeObject.GetComponent<DisassemblyController>();
            if (controller != null)
            {
                controller.TriggerDisassembly();
            }
            else
            {
                Debug.LogWarning("Active object has no DisassemblyController attached.");
            }
        }
        else
        {
            Debug.LogWarning("No active object assigned to DisassemblyButton.");
        }
    }
}
