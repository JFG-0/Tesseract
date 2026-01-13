using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Vuforia;

public class VuforiaIMUController : MonoBehaviour
{
    [Header("UDP Settings")]
    public int udpPort = 8888;
    
    [Header("Scene Container")]
    public GameObject sceneContainer;
    
    [Header("Vuforia Image Targets (assign in order 1-6)")]
    public GameObject imageTarget1;
    public GameObject imageTarget2;
    public GameObject imageTarget3;
    public GameObject imageTarget4;
    public GameObject imageTarget5;
    public GameObject imageTarget6;
    
    [Header("Debug UI (optional)")]
    public UnityEngine.UI.Text debugText;
    
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning = false;
    
    private GameObject[] imageTargets;
    private int currentFaceID = 0;
    private int lastActiveFaceID = -1;
    private int totalPackets = 0;
    private bool newDataAvailable = false;
    
    void Start()
    {
        Debug.Log("========================================");
        Debug.Log("Vuforia IMU Controller Started");
        Debug.Log("========================================");
        
        // Build array from individual assignments
        imageTargets = new GameObject[6];
        imageTargets[0] = imageTarget1;
        imageTargets[1] = imageTarget2;
        imageTargets[2] = imageTarget3;
        imageTargets[3] = imageTarget4;
        imageTargets[4] = imageTarget5;
        imageTargets[5] = imageTarget6;
        
        // Verify all targets are assigned
        for (int i = 0; i < 6; i++)
        {
            if (imageTargets[i] == null)
            {
                Debug.LogError($"⚠️ ImageTarget {i + 1} NOT assigned!");
            }
            else
            {
                Debug.Log($"✓ ImageTarget {i + 1}: {imageTargets[i].name}");
            }
        }
        
        // Keep scene container active
        if (sceneContainer != null)
        {
            sceneContainer.SetActive(true);
            Debug.Log($"✓ Scene Container: {sceneContainer.name} (always active)");
        }
        
        // Deactivate all image targets initially
        DeactivateAllTargets();
        Debug.Log("All Image Targets deactivated initially");
        
        Debug.Log("");
        StartUDPListener();
    }

    void StartUDPListener()
    {
        try
        {
            Debug.Log($"Binding to UDP port {udpPort}...");
            
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, udpPort);
            udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.Bind(endpoint);
            
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
            
            isRunning = true;
            
            Debug.Log("✓ UDP Listener active on port 8888");
            Debug.Log("✓ Waiting for IMU cube rotations...");
            Debug.Log("");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ UDP setup failed: {e.Message}");
        }
    }

    void ReceiveData()
    {
        Debug.Log("[Thread] Listening for face IDs from ESP32...");
        
        udpClient.Client.ReceiveTimeout = 5000;
        
        while (isRunning)
        {
            try
            {
                IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpClient.Receive(ref remote);
                string text = Encoding.UTF8.GetString(data).Trim();
                
                lock(this)
                {
                    if (int.TryParse(text, out int faceID))
                    {
                        if (faceID >= 1 && faceID <= 6)
                        {
                            currentFaceID = faceID;
                            newDataAvailable = true;
                            totalPackets++;
                        }
                    }
                }
            }
            catch (SocketException) { }
            catch (System.Exception e)
            {
                if (isRunning)
                {
                    Debug.LogError($"[Thread] Error: {e.Message}");
                }
            }
        }
    }

    void Update()
    {
        if (newDataAvailable)
        {
            lock(this)
            {
                // Only switch if face actually changed
                if (currentFaceID != lastActiveFaceID)
                {
                    SwitchToFace(currentFaceID);
                    lastActiveFaceID = currentFaceID;
                    
                    Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    Debug.Log($"🎲 CUBE ROTATED TO FACE: {currentFaceID}");
                    Debug.Log($"✓ Active: {imageTargets[currentFaceID - 1].name}");
                    Debug.Log($"📦 Total Packets: {totalPackets}");
                    Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                }
                
                newDataAvailable = false;
            }
        }
        
        // Update debug UI
        if (debugText != null)
        {
            debugText.text = $"Current Face: {currentFaceID}\n" +
                           $"Active: ImageTarget_{currentFaceID}\n" +
                           $"Packets: {totalPackets}";
        }
    }

    void SwitchToFace(int faceID)
    {
        Debug.Log($"[Switch] Changing to Face {faceID}...");
        
        // Step 1: Deactivate ALL targets
        for (int i = 0; i < imageTargets.Length; i++)
        {
            if (imageTargets[i] != null)
            {
                imageTargets[i].SetActive(false);
                
                // Disable Vuforia tracking
                var trackable = imageTargets[i].GetComponent<ObserverBehaviour>();
                if (trackable != null)
                {
                    trackable.enabled = false;
                }
            }
        }
        
        Debug.Log("[Switch] All targets deactivated");
        
        // Step 2: Small delay for Vuforia to process
        System.Threading.Thread.Sleep(50);
        
        // Step 3: Activate ONLY the selected target
        int index = faceID - 1; // Face 1 = index 0
        if (index >= 0 && index < imageTargets.Length && imageTargets[index] != null)
        {
            imageTargets[index].SetActive(true);
            
            // Re-enable Vuforia tracking
            var trackable = imageTargets[index].GetComponent<ObserverBehaviour>();
            if (trackable != null)
            {
                trackable.enabled = true;
            }
            
            Debug.Log($"[Switch] ✓ {imageTargets[index].name} activated and tracking enabled");
        }
        else
        {
            Debug.LogError($"[Switch] ❌ Invalid face index: {index}");
        }
    }

    void DeactivateAllTargets()
    {
        for (int i = 0; i < imageTargets.Length; i++)
        {
            if (imageTargets[i] != null)
            {
                imageTargets[i].SetActive(false);
            }
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("Shutting down UDP listener...");
        isRunning = false;
        
        if (receiveThread != null)
            receiveThread.Abort();
        
        if (udpClient != null)
            udpClient.Close();
    }
}
