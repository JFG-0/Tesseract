using UnityEngine;
using UnityEngine.SceneManagement;

public class ArduinoInputReader : MonoBehaviour
{
    private SerialManager serialManager;
    private bool inScene1 = true;

    void Start()
    {
        serialManager = FindObjectOfType<SerialManager>();
        if (serialManager != null)
        {
            serialManager.OnMessageReceived += HandleMessage;
        }
        else
        {
            Debug.LogError("No SerialManager found in scene!");
        }
   }


    private void HandleMessage(string feedback)
    {
        Debug.Log("Arduino says: " + feedback);

        if (feedback == "2") // Button pressed
        {
            if (inScene1)
            {
                SceneManager.LoadScene("Scene2");
                inScene1 = false;
                Debug.Log("Switched to Scene2");
            }
            else
            {
                SceneManager.LoadScene("Scene1");
                inScene1 = true;
                Debug.Log("Switched to Scene1");
            }
        }
    }

    void OnDestroy()
    {
        if (serialManager != null)
            serialManager.OnMessageReceived -= HandleMessage;
    }
}