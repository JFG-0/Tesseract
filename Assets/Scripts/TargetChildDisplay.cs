using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI; // Needed for UI Text
using TMPro; // add this at the top


public class TargetChildDisplay : MonoBehaviour
{
    private List<Transform> childList = new List<Transform>();
    private int currentIndex = 0;

    [Header("UI Elements")]
    public TMP_Text statusText; // Drag a Text UI element here in Inspector
    public TMP_Text instructionText; // Optional: another text for instructions



    void Start()
    {
        // Collect children and sort alphabetically by name
        childList = transform.Cast<Transform>()
                             .OrderBy(t => t.name)
                             .Take(10) // only first 10
                             .ToList();

        // Hide all children initially
        foreach (Transform child in childList)
        {
            child.gameObject.SetActive(false);
        }

        // Show default item (index 0)
        if (childList.Count > 0)
        {
            childList[0].gameObject.SetActive(true);
            currentIndex = 0;
        }
    }

    void Update()
    {
        // Check numeric key input (0–9)
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                ToggleItem(i);
            }
        }
        // Numpad keys
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
    }

    private void ToggleItem(int index)
    {
        if (index < childList.Count)
        {
            // Hide current
            childList[currentIndex].gameObject.SetActive(false);

            // Show new
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

}