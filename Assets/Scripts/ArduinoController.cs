using System.IO.Ports;
using UnityEngine;

public class ArduinoController : MonoBehaviour
{
    SerialPort stream = new SerialPort("COM6", 9600); // Replace COM6 with your actual port
    bool ledOn = false; // Track LED state

    void Start()
    {
        stream.Open();
    }

    void Update()
    {
        // Toggle LED with Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!ledOn)
            {
                stream.Write("1"); // Turn LED on
                ledOn = true;
                Debug.Log("LED ON");
            }
            else
            {
                stream.Write("0"); // Turn LED off
                ledOn = false;
                Debug.Log("LED OFF");
            }
        }

        // Optional: still allow Backspace to force OFF
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            stream.Write("0");
            ledOn = false;
            Debug.Log("LED OFF (forced)");
        }
    }

    void OnApplicationQuit()
    {
        if (stream != null && stream.IsOpen)
        {
            stream.Write("0"); // Ensure LED off before quitting
            stream.Close();
            Debug.Log("Sent 0 and closed port.");
        }
    }
}