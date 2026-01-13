using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Vuforia;

public class TargetChildDisplayIMUPublic : MonoBehaviour
{
    [Header("Child Objects (assign manually in Inspector, 1–6)")]
    public List<GameObject> childList = new List<GameObject>();

    private int currentIndex = 0;

    [Header("UI Elements")]
    public TMP_Text statusText;
    public TMP_Text instructionText;

    // UDP
    private UdpClient udpClient;
    private Thread udpThread;
    private int udpPort = 8888;
    private volatile int latestFace = -1;

    // Debounce
    private int pendingFace = -1;
    private int stableCount = 0;
    private const int requiredStableReads = 3;

    private int lastFace = -1;

    void Start()
    {
        // Hide all children
        foreach (GameObject child in childList)
            if (child != null) child.SetActive(false);

        // Show default
        if (childList.Count > 0 && childList[0] != null)
        {
            childList[0].SetActive(true);
            currentIndex = 0;
        }

        // Start UDP listener
        udpClient = new UdpClient(udpPort);
        udpThread = new Thread(ReceiveUDP);
        udpThread.IsBackground = true;
        udpThread.Start();

        Debug.Log("UDP listener started on port " + udpPort);

        // Check Vuforia
        if (VuforiaBehaviour.Instance == null)
            Debug.LogError("VuforiaBehaviour not found! Make sure ARCamera is in the scene.");
    }

    void ReceiveUDP()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, udpPort);

        while (true)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEP);
                string msg = Encoding.UTF8.GetString(data).Trim();

                if (int.TryParse(msg, out int face))
                {
                    latestFace = face;
                }
            }
            catch { }
        }
    }

    void Update()
    {
        // Keyboard testing
        for (int i = 0; i <= 6; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                ToggleItem(i);
                LogChange(i);
            }
        }

        // UDP input with debounce
        if (latestFace >= 1 && latestFace <= 6)
        {
            int face = latestFace;

            if (face == pendingFace)
            {
                stableCount++;

                if (stableCount >= requiredStableReads && face != currentIndex)
                {
                    ToggleItem(face);
                    LogChange(face);
                    ResetVuforiaTracking();
                    stableCount = 0;
                }
            }
            else
            {
                pendingFace = face;
                stableCount = 1;
            }
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

            // 🔥 Print active scene index in console
            Debug.Log($"Active Scene Index: {currentIndex}");
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
        if (statusText != null && currentIndex < childList.Count)
        {
            statusText.text = $"Active Object: {childList[currentIndex].name} (Index {currentIndex})";
        }
    }

    // -------------------------------
    // 🔥 VUFORIA RESET HOOK
    // -------------------------------
    private void ResetVuforiaTracking()
    {
    var observers = FindObjectsByType<ObserverBehaviour>(FindObjectsSortMode.None);

    foreach (var obs in observers)
    {
        obs.enabled = false;
        obs.enabled = true;
    }

    Debug.Log("Vuforia tracking reset");
    }


    void OnApplicationQuit()
    {
        udpThread?.Abort();
        udpClient?.Close();
    }
}