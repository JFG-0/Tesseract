using System.IO.Ports;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArduinoInputReader : MonoBehaviour
{
    SerialPort stream = new SerialPort("COM6", 9600); // Replace with your actual port
    bool inScene1 = true; // Track which scene is active

    void Start()
    {
        stream.Open();
        stream.ReadTimeout = 50;
    }

    void Update()
    {
        try
        {
            string feedback = stream.ReadLine();
            if (!string.IsNullOrEmpty(feedback))
            {
                Debug.Log("Arduino says: " + feedback);

                if (feedback.Trim() == "2") // Button pressed
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
        }
        catch (System.TimeoutException)
        {
            // Ignore if no data this frame
        }
    }

    void OnApplicationQuit()
    {
        if (stream != null && stream.IsOpen)
        {
            stream.Close();
        }
    }
}