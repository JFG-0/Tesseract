using System;
using System.IO.Ports;
using UnityEngine;

public class SerialManager : MonoBehaviour
{
    private SerialPort stream;

    [SerializeField] private string portName = "COM6";
    [SerializeField] private int baudRate = 9600;
    [SerializeField] private int readTimeout = 50;

    // Event for incoming messages
    public event Action<string> OnMessageReceived;

    void Start()
    {
        try
        {
            stream = new SerialPort(portName, baudRate);
            stream.ReadTimeout = readTimeout;
            System.Threading.Thread.Sleep(2000); // let Arduino reset
            stream.Open();
            Debug.Log($"Serial port {portName} opened at {baudRate} baud.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to open serial port: " + e.Message);
        }
    }

    void Update()
    {
        if (stream != null && stream.IsOpen)
        {
            try
            {
                while (stream.BytesToRead > 0)
                {
                    string feedback = stream.ReadLine();
                    OnMessageReceived?.Invoke(feedback.Trim());
                }
            }
            catch (TimeoutException) { }
            catch (Exception e)
            {
                Debug.LogWarning("Serial read failed: " + e.Message);
            }
        }
    }

    public void Send(string message)
    {
        if (stream != null && stream.IsOpen)
        {
            try
            {
                stream.Write(message);
                Debug.Log("Sent: " + message);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Serial write failed: " + e.Message);
            }
        }
    }

    void OnApplicationQuit()
    {
        if (stream != null && stream.IsOpen)
        {
            stream.Close();
            Debug.Log("Closed serial port.");
        }
    }
}