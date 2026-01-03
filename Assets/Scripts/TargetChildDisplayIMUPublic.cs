using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO.Ports;

public class TargetChildDisplayIMUPublic : MonoBehaviour
{
    [Header("Child Objects (assign manually in Inspector, 1–6)")]
    public List<GameObject> childList = new List<GameObject>(); // assign in Inspector

    private int currentIndex = 0;

    [Header("UI Elements")]
    public TMP_Text statusText;
    public TMP_Text instructionText;

    SerialPort sp;

    // Debounce variables
    private int pendingFace = -1;
    private int stableCount = 0;
    private const int requiredStableReads = 3; // number of consecutive reads before switching

    // Change detection
    private int lastFace = -1; // track last confirmed face

    void Start()
    {
        // Hide all children initially
        foreach (GameObject child in childList)
        {
            if (child != null)
                child.SetActive(false);
        }

        // Show default item (index 0)
        if (childList.Count > 0 && childList[0] != null)
        {
            childList[0].SetActive(true);
            currentIndex = 0;
        }

        // Open Arduino serial port
        sp = new SerialPort("COM6", 115200);
        sp.ReadTimeout = 50;
        try { sp.Open(); }
        catch (System.Exception e) { Debug.LogError("Serial open failed: " + e.Message); }
    }

    void Update()
    {
        // --- Keyboard input (for testing) ---
        for (int i = 0; i <= 6; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                ToggleItem(i);
                LogChange(i);
            }
        }
        if (Input.GetKeyDown(KeyCode.Keypad0)) { ToggleItem(0); LogChange(0); }
        if (Input.GetKeyDown(KeyCode.Keypad1)) { ToggleItem(1); LogChange(1); }
        if (Input.GetKeyDown(KeyCode.Keypad2)) { ToggleItem(2); LogChange(2); }
        if (Input.GetKeyDown(KeyCode.Keypad3)) { ToggleItem(3); LogChange(3); }
        if (Input.GetKeyDown(KeyCode.Keypad4)) { ToggleItem(4); LogChange(4); }
        if (Input.GetKeyDown(KeyCode.Keypad5)) { ToggleItem(5); LogChange(5); }
        if (Input.GetKeyDown(KeyCode.Keypad6)) { ToggleItem(6); LogChange(6); }

        // --- Arduino serial input with debounce ---
        if (sp != null && sp.IsOpen)
        {
            try
            {
                string line = sp.ReadLine().Trim();
                int face;
                if (int.TryParse(line, out face))
                {
                    if (face >= 1 && face <= 6)
                    {
                        if (face == pendingFace)
                        {
                            stableCount++;
                            if (stableCount >= requiredStableReads && face != currentIndex)
                            {
                                ToggleItem(face);

                                // Log only when value changes
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
                }
            }
            catch (System.TimeoutException) { }
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
            Debug.Log($"Arduino face changed to: {face}");
            lastFace = face;
        }
    }

    private void UpdateUI()
    {
        if (statusText != null && currentIndex < childList.Count && childList[currentIndex] != null)
        {
            statusText.text = $"Active Object: {childList[currentIndex].name} (Index {currentIndex})";
        }
    }

    void OnApplicationQuit()
    {
        if (sp != null && sp.IsOpen) sp.Close();
    }
}