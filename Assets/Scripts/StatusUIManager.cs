using UnityEngine;
using TMPro;

public class StatusUIManager : MonoBehaviour
{
    [Header("UI Text")]
    public TMP_Text statusText;

    public void SetMessage(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
