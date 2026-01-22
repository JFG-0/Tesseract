using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Handles screen orientation changes and updates UI classes accordingly
/// Attach this to the same GameObject as TesseractUIManager
/// </summary>
public class OrientationManager : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    private ScreenOrientation lastOrientation;
    
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        lastOrientation = Screen.orientation;
        
        // Enable auto-rotation for Android
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.AutoRotation;
        
        ApplyOrientationClass();
    }
    
    void Update()
    {
        // Check if orientation changed
        if (Screen.orientation != lastOrientation)
        {
            lastOrientation = Screen.orientation;
            ApplyOrientationClass();
        }
    }
    
    void ApplyOrientationClass()
    {
        // Remove previous orientation classes
        root.RemoveFromClassList("portrait");
        root.RemoveFromClassList("landscape");
        
        // Determine current orientation
        bool isLandscape = Screen.width > Screen.height;
        
        if (isLandscape)
        {
            root.AddToClassList("landscape");
            Debug.Log("OrientationManager: Switched to LANDSCAPE");
        }
        else
        {
            root.AddToClassList("portrait");
            Debug.Log("OrientationManager: Switched to PORTRAIT");
        }
    }
}