using UnityEngine;
using TMPro; // for TextMeshPro

public class DisassemblyController : MonoBehaviour
{
    private Animator animator;
    private string objectName;

    [Header("UI Elements")]
    public TMP_Text statusText; // Drag your UI text here in Inspector

    void Start()
    {
        // Cache Animator and object name
        animator = GetComponent<Animator>();
        objectName = gameObject.name;
    }

    // Trigger disassembly animation
    public void TriggerDisassembly()
    {
        if (animator == null)
        {
            Debug.LogWarning($"No Animator found on {objectName}");
            return;
        }

        // Build animation trigger name based on object
        string triggerName = objectName + "_Disassembly";

        // Set trigger on Animator
        animator.SetTrigger(triggerName);

        // Update UI text
        if (statusText != null)
        {
            statusText.text = $"{objectName} is disassembling...";
        }
    }
}