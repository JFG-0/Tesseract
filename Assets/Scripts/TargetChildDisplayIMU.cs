using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System.IO.Ports;

public class TargetChildDisplayIMU : MonoBehaviour
{
    private List<Transform> childList = new List<Transform>();
    private int currentIndex = 0;

    [Header("UI Elements")]
    public TMP_Text statusText;
    public TMP_Text instructionText;

    SerialPort sp;

    // Debounce variables
    private int pendingFace = -1;
    private int stableCount = 0;
    private const int requiredStableReads = 3; // number of consecutive reads before switching

    void Start()
    {
        childList = transform.Cast<Transform>()
                             .OrderBy(t => t.name)
                             .Take(10)
                             .ToList();

        foreach (Transform child in childList)
        {
            child.gameObject.SetActive(false);
        }

        if (childList.Count > 0)
        {
            childList[0].gameObject.SetActive(true);
            currentIndex = 0;
        }

        sp = new SerialPort("COM6", 115200);
        sp.ReadTimeout = 50;
        try { sp.Open(); }
        catch (System.Exception e) { Debug.LogError("Serial open failed: " + e.Message); }
    }

    void Update()
    {
        // --- Keyboard input ---
        for (int i = 0; i <= 6; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                ToggleItem(i);
            }
        }
        if (Input.GetKeyDown(KeyCode.Keypad0)) ToggleItem(0);
        if (Input.GetKeyDown(KeyCode.Keypad1)) ToggleItem(1);
        if (Input.GetKeyDown(KeyCode.Keypad2)) ToggleItem(2);
        if (Input.GetKeyDown(KeyCode.Keypad3)) ToggleItem(3);
        if (Input.GetKeyDown(KeyCode.Keypad4)) ToggleItem(4);
        if (Input.GetKeyDown(KeyCode.Keypad5)) ToggleItem(5);
        if (Input.GetKeyDown(KeyCode.Keypad6)) ToggleItem(6);
        if (Input.GetKeyDown(KeyCode.Keypad7)) ToggleItem(7);
        if (Input.GetKeyDown(KeyCode.Keypad8)) ToggleItem(8);
        if (Input.GetKeyDown(KeyCode.Keypad9)) ToggleItem(9);

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
        if (index < childList.Count)
        {
            childList[currentIndex].gameObject.SetActive(false);
            childList[index].gameObject.SetActive(true);
            currentIndex = index;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (statusText != null)
        {
            statusText.text = $"Active Object: {childList[currentIndex].name} (Index {currentIndex})";
        }
    }

    void OnApplicationQuit()
    {
        if (sp != null && sp.IsOpen) sp.Close();
    }
}