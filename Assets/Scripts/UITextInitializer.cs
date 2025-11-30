using UnityEngine;
using TMPro; // if using TextMeshPro
using UnityEngine.UI; // if using legacy UI Text

public class UITextInitializer : MonoBehaviour
{
    [Header("UI Texts")]
    public TMP_Text[] tmpTexts;     // assign all TMP_Text elements here
    public Text[] uiTexts;          // assign all legacy Text elements here

    [Header("Default Placeholder")]
    public string defaultText = "Default text";

    void Awake()
    {
        // Set all placeholders before scripts kick in
        foreach (var t in tmpTexts)
        {
            if (t != null) t.text = defaultText;
        }

        foreach (var t in uiTexts)
        {
            if (t != null) t.text = defaultText;
        }
    }

    // Call this from other scripts to overwrite placeholder
    public void UpdateText(TMP_Text tmp, string newText)
    {
        if (tmp != null) tmp.text = newText;
    }

    public void UpdateText(Text ui, string newText)
    {
        if (ui != null) ui.text = newText;
    }
}