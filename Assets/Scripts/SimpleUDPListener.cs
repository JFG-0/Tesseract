//Test script to confirm whether Unity can receive UDP packets correctly

using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class SimpleUDPListener : MonoBehaviour
{
    [Header("UDP Settings")]
    public int udpPort = 8888;
    
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning = false;
    
    private string lastReceivedData = "";
    private int lastFaceID = 0;
    private int totalPackets = 0;
    private bool newDataAvailable = false;

    void Start()
    {
        Debug.Log("========================================");
        Debug.Log("Simple UDP Listener Started - ENHANCED");
        Debug.Log("========================================");
        
        // Show network info
        Debug.Log("Network Interfaces:");
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Debug.Log($"  IPv4: {ip}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Could not get network info: {e.Message}");
        }
        
        Debug.Log("");
        Debug.Log($"Will listen on UDP Port: {udpPort}");
        Debug.Log($"Waiting for face IDs from ESP32...");
        Debug.Log("");
        
        StartUDPListener();
    }

    void StartUDPListener()
    {
        try
        {
            Debug.Log($"Binding to 0.0.0.0:{udpPort}...");
            
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, udpPort);
            udpClient = new UdpClient();
            
            // Critical socket options
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.ExclusiveAddressUse = false;
            
            udpClient.Client.Bind(endpoint);
            
            Debug.Log($"✓ Bound to port {udpPort}");
            Debug.Log($"  Endpoint: {udpClient.Client.LocalEndPoint}");
            
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
            
            isRunning = true;
            
            Debug.Log("✓ UDP Listener ready!");
            Debug.Log("");
        }
        catch (System.Exception e)
        {
            Debug.LogError("✗ FAILED TO START!");
            Debug.LogError($"Error: {e.Message}");
            Debug.LogError($"Stack: {e.StackTrace}");
        }
    }

    void ReceiveData()
    {
        Debug.Log("[Thread] Receive thread active");
        
        udpClient.Client.ReceiveTimeout = 5000;
        int timeoutCount = 0;
        
        while (isRunning)
        {
            try
            {
                IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                
                if (timeoutCount == 0)
                {
                    Debug.Log("[Thread] Ready to receive packets...");
                }
                
                byte[] data = udpClient.Receive(ref remote);
                
                string text = Encoding.UTF8.GetString(data).Trim();
                
                lock(this)
                {
                    lastReceivedData = text;
                    totalPackets++;
                    newDataAvailable = true;
                    
                    if (int.TryParse(text, out int faceID))
                    {
                        lastFaceID = faceID;
                    }
                }
                
                Debug.Log($"[Thread] *** RECEIVED *** from {remote.Address}:{remote.Port}");
                Debug.Log($"[Thread] Data: \"{text}\" | Packet #{totalPackets}");
                
                timeoutCount = 0;
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.TimedOut)
                {
                    timeoutCount++;
                    if (timeoutCount % 6 == 0) // Every 30 seconds
                    {
                        Debug.Log($"[Thread] Still waiting... ({timeoutCount * 5}s)");
                    }
                }
                else if (isRunning)
                {
                    Debug.LogError($"[Thread] Socket error: {se.Message}");
                }
            }
            catch (System.Exception e)
            {
                if (isRunning)
                {
                    Debug.LogError($"[Thread] Error: {e.Message}");
                }
            }
        }
        
        Debug.Log("[Thread] Stopped");
    }

    void Update()
    {
        if (newDataAvailable)
        {
            lock(this)
            {
                Debug.Log("========================================");
                Debug.Log($"FACE ID: {lastFaceID}");
                Debug.Log($"Raw: \"{lastReceivedData}\"");
                Debug.Log($"Packets: {totalPackets}");
                Debug.Log($"Time: {Time.time:F2}s");
                Debug.Log("========================================");
                
                newDataAvailable = false;
            }
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("Shutting down...");
        isRunning = false;
        
        if (receiveThread != null)
            receiveThread.Abort();
        
        if (udpClient != null)
            udpClient.Close();
    }
}
