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

    // Step 2: expose readiness flag
    public bool IsOpen => stream != null && stream.IsOpen;

    void Awake()
    {
        // Step 1: make persistent across scene loads
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        try
        {
            stream = new SerialPort(portName, baudRate);
            stream.ReadTimeout = readTimeout;

            // Allow Arduino reset before opening
            System.Threading.Thread.Sleep(2000);

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
        if (IsOpen)
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
        if (IsOpen)
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
        else
        {
            Debug.LogWarning("Attempted to send but port is not open.");
        }
    }

    void OnApplicationQuit()
    {
        if (IsOpen)
        {
            stream.Close();
            Debug.Log("Closed serial port.");
        }
    }
}