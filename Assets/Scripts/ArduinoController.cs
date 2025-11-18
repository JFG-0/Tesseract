using System.IO.Ports;
using UnityEngine;

public class ArduinoController : MonoBehaviour
{
    SerialPort stream = new SerialPort("COM6", 9600); // Replace COM4 with your actual port

    void Start()
    {
        stream.Open();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            stream.Write("1"); // Turn LED on
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            stream.Write("0"); // Turn LED off
        }
    }

    void OnApplicationQuit()
    {
        stream.Close();
    }
}