using UnityEngine;
using System.Net.Sockets;
using System.Text;

public class ArduinoConnector : MonoBehaviour
{
    public string arduinoIP = "172.20.10.5"; // set Arduino’s LAN IP
    public int port = 8888;

    private TcpClient client;
    private NetworkStream stream;

    void Start()
    {
        ConnectToArduino();
    }

    void ConnectToArduino()
    {
        try
        {
            client = new TcpClient(arduinoIP, port);
            stream = client.GetStream();

            Debug.Log("Connected to Arduino!");
            SendMessage("Unity says hello");
        }
        catch (SocketException e)
        {
            Debug.LogError("Connection failed: " + e.Message);
        }
    }

    void SendMessage(string msg)
    {
        if (stream != null)
        {
            byte[] data = Encoding.ASCII.GetBytes(msg);
            stream.Write(data, 0, data.Length);
        }
    }

    void Update()
    {
        if (stream != null && stream.DataAvailable)
        {
            byte[] buffer = new byte[client.Available];
            stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer);
            Debug.Log("Arduino says: " + response);
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}