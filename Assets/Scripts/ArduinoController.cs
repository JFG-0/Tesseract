using UnityEngine;

public class ArduinoController : MonoBehaviour
{
    private SerialManager serialManager;
    private bool ledOn = false;

    void Start()
    {
        serialManager = FindObjectOfType<SerialManager>();
        if (serialManager == null)
        {
            Debug.LogError("No SerialManager found in scene!");
        }
    }

    void Update()
    {
        if (serialManager != null && serialManager.IsOpen)
        {
            // Toggle LED with Space
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!ledOn)
                {
                    serialManager.Send("1");
                    ledOn = true;
                    Debug.Log("LED ON");
                }
                else
                {
                    serialManager.Send("0");
                    ledOn = false;
                    Debug.Log("LED OFF");
                }
            }

            // Optional: Backspace forces OFF
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                serialManager.Send("0");
                ledOn = false;
                Debug.Log("LED OFF (forced)");
            }
        }
        else
        {
            // Helpful warning if you press keys before the port is ready
            if (Input.anyKeyDown)
            {
                Debug.LogWarning("SerialManager not ready — input ignored.");
            }
        }
    }
}