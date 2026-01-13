using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TargetChildDisplayIMU_UDP : MonoBehaviour
{
    [Header("Child Objects (assign manually, indices 0-6)")]
    public List<GameObject> childList = new List<GameObject>();
    
    [Header("UI Elements")]
    public TMP_Text statusText;
    public TMP_Text instructionText;
    public TMP_Text debugText;  // For connection status
    
    [Header("UDP Settings")]
    public int udpPort = 8888;
    
    private UdpClient udpClient;
    private Thread receiveThread;
    private int currentIndex = 0;
    
    // Debounce variables
    private int pendingFace = -1;
    private int stableCount = 0;
    private const int requiredStableReads = 3;
    
    // Change detection
    private int lastFace = -1;
    
    // Thread-safe data
    private int receivedFace = -1;
    private bool newFaceAvailable = false;
    private int packetsReceived = 0;
    private string myIP = "";

    void Start()
    {
        Debug.Log("=== IMU Face Display - UDP Mode ===");
        
        // Get local IP
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    myIP = ip.ToString();
                    Debug.Log("My IP: " + myIP);
                    break;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Could not get IP: " + e.Message);
        }
        
        // Hide all children initially
        foreach (GameObject child in childList)
        {
            if (child != null)
                child.SetActive(false);
        }
        
        // Show default (index 0)
        if (childList.Count > 0 && childList[0] != null)
        {
            childList[0].SetActive(true);
            currentIndex = 0;
        }
        
        // Start UDP receiver
        try
        {
            udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, udpPort));
            Debug.Log("UDP listening on port " + udpPort);
            
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
            
            Debug.Log("UDP receiver started");
        }
        catch (System.Exception e)
        {
            Debug.LogError("UDP setup failed: " + e.Message);
        }
        
        UpdateUI();
    }

    void ReceiveData()
    {
        Debug.Log("[Thread] UDP receiver running...");
        
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpClient.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data).Trim();
                
                Debug.Log("Received: " + text + " from " + anyIP.Address);
                
                int face;
                if (int.TryParse(text, out face))
                {
                    if (face >= 0 && face <= 6)
                    {
                        lock(this)
                        {
                            receivedFace = face;
                            newFaceAvailable = true;
                            packetsReceived++;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Thread] Error: " + e.Message);
            }
        }
    }

    void Update()
    {
        // Process received face data with debouncing
        if (newFaceAvailable)
        {
            int face;
            lock(this)
            {
                face = receivedFace;
                newFaceAvailable = false;
            }
            
            ProcessFace(face);
        }
        
        // Keyboard input for testing
        for (int i = 0; i <= 6; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                ToggleItem(i);
                LogChange(i);
            }
        }
        
        // Update debug UI
        UpdateDebugUI();
    }
    
    void ProcessFace(int face)
    {
        if (face >= 1 && face <= 6)
        {
            if (face == pendingFace)
            {
                stableCount++;
                if (stableCount >= requiredStableReads && face != currentIndex)
                {
                    ToggleItem(face);
                    LogChange(face);
                    stableCount = 0;
                }
            }
            else
            {
                pendingFace = face;
                stableCount = 1;
            }
        }
        else if (face == 0)
        {
            // Face 0 = undefined orientation, keep current
            Debug.Log("Undefined orientation (0)");
        }
    }

    private void ToggleItem(int index)
    {
        if (index < childList.Count && childList[index] != null)
        {
            // Hide current
            if (childList[currentIndex] != null)
                childList[currentIndex].SetActive(false);
            
            // Show new
            childList[index].SetActive(true);
            currentIndex = index;
            UpdateUI();
        }
    }

    private void LogChange(int face)
    {
        if (face != lastFace)
        {
            Debug.Log($"Face changed to: {face}");
            lastFace = face;
        }
    }

    private void UpdateUI()
    {
        if (statusText != null && currentIndex < childList.Count && childList[currentIndex] != null)
        {
            statusText.text = $"Active: {childList[currentIndex].name} (Face {currentIndex})";
        }
    }
    
    private void UpdateDebugUI()
    {
        if (debugText != null)
        {
            debugText.text = $"IP: {myIP}\nPort: {udpPort}\nPackets: {packetsReceived}\nCurrent Face: {currentIndex}";
        }
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null)
            receiveThread.Abort();
        if (udpClient != null)
            udpClient.Close();
    }
}