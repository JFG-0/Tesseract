using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

/// <summary>
/// UI Manager that listens to Vuforia's native Debug.Log output
/// Does NOT need custom VuforiaRenderTracker - uses existing Vuforia logs
/// Reads from VuforiaIMUController only
/// </summary>
public class TesseractUIManager : MonoBehaviour
{
    [Header("Backend Reference - READ ONLY")]
    public VuforiaIMUController imuController;
    
    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement cameraBlur;
    
    // State screens
    private VisualElement stateSplash;
    private VisualElement stateNotConnected;
    private VisualElement stateConnected;
    private VisualElement stateTracking;
    private VisualElement stateTransition;
    
    // State tracking
    private enum UIState { Splash, NotConnected, Connected, Tracking, Transition }
    private UIState currentState = UIState.Splash;
    
    private bool isUDPConnected = false;
    private bool isTargetTracked = false;
    private int lastKnownFace = 0;
    private int lastPacketCount = 0;
    private float connectionCheckTime = 0f;
    
    void Awake()
    {
        // Subscribe to Unity's log messages
        Application.logMessageReceived += OnLogMessageReceived;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from log messages
        Application.logMessageReceived -= OnLogMessageReceived;
    }
    
    void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        // Listen for Vuforia's native tracking status messages
        if (logString.Contains("TRACKED -- NORMAL"))
        {
            isTargetTracked = true;
        }
        else if (logString.Contains("NO_POSE -- NOT_OBSERVED") || 
                 logString.Contains("NO_POSE -- LOST"))
        {
            isTargetTracked = false;
        }
    }
    
    void Start()
    {
        // Find VuforiaIMUController if not assigned
        if (imuController == null)
        {
            imuController = FindObjectOfType<VuforiaIMUController>();
            if (imuController == null)
            {
                Debug.LogError("TesseractUIManager: Cannot find VuforiaIMUController!");
                return;
            }
        }
        
        // Get UI Document
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        
        // Cache all state screens from UXML
        cameraBlur = root.Q<VisualElement>("camera-blur");
        stateSplash = root.Q<VisualElement>("state-splash");
        stateNotConnected = root.Q<VisualElement>("state-not-connected");
        stateConnected = root.Q<VisualElement>("state-connected");
        stateTracking = root.Q<VisualElement>("state-tracking");
        stateTransition = root.Q<VisualElement>("state-transition");
        
        // Setup project button
        var projectBtn = stateTracking?.Q<Button>("project-button");
        if (projectBtn != null)
        {
            projectBtn.clicked += () => Application.OpenURL("https://www.behance.net/yourprofile");
        }
        
        // Start with splash
        StartCoroutine(SplashSequence());
        
        Debug.Log("TesseractUIManager: Initialized - Listening to Vuforia logs");
    }

    IEnumerator SplashSequence()
    {
        // Show splash with blur for 2 seconds
        ShowState(UIState.Splash);
        ShowBlur();
        
        yield return new WaitForSeconds(2f);
        
        // Remove subtitle
        var subtitle = stateSplash?.Q<Label>("splash-subtitle");
        if (subtitle != null)
            subtitle.style.display = DisplayStyle.None;
        
        // Hold for 1 more second (total 3 seconds as per Excel)
        yield return new WaitForSeconds(1f);
        
        // Transition to appropriate state
        HideBlur();
        UpdateState();
    }

    void Update()
    {
        if (currentState == UIState.Splash || currentState == UIState.Transition)
            return;
        
        // Read backend state every frame
        ReadBackendState();
        
        // Update UI state based on backend
        UpdateState();
    }

    void ReadBackendState()
    {
        // Check UDP connection via packet count
        var totalPacketsField = imuController.GetType().GetField("totalPackets",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (totalPacketsField != null)
        {
            int currentPackets = (int)totalPacketsField.GetValue(imuController);
            
            // UDP is connected if we're receiving packets
            if (currentPackets > lastPacketCount)
            {
                isUDPConnected = true;
                lastPacketCount = currentPackets;
                connectionCheckTime = Time.time;
            }
            else if (Time.time - connectionCheckTime > 3f)
            {
                // No packets for 3 seconds = disconnected
                isUDPConnected = false;
            }
        }
        
        // Check current face
        var currentFaceField = imuController.GetType().GetField("currentFaceID",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (currentFaceField != null)
        {
            int face = (int)currentFaceField.GetValue(imuController);
            
            // Face changed - trigger transition
            if (face != lastKnownFace && face > 0)
            {
                lastKnownFace = face;
                StartCoroutine(FaceChangeTransition());
            }
        }
    }

    void UpdateState()
    {
        UIState newState;
        
        // Determine state based on backend
        if (!isUDPConnected)
        {
            newState = UIState.NotConnected;
        }
        else if (!isTargetTracked)
        {
            newState = UIState.Connected;
        }
        else
        {
            newState = UIState.Tracking;
        }
        
        // Only change if different
        if (newState != currentState)
        {
            Debug.Log($"[UIManager] State change: {currentState} → {newState}");
            ShowState(newState);
        }
    }

    IEnumerator FaceChangeTransition()
    {
        Debug.Log($"[UIManager] Face changed to {lastKnownFace} - starting transition");
        
        // Show transition state with blur
        ShowState(UIState.Transition);
        ShowBlur();
        
        // Wait 2 seconds
        yield return new WaitForSeconds(2f);
        
        // Return to appropriate state
        HideBlur();
        UpdateState();
    }

    void ShowState(UIState state)
    {
        currentState = state;
        
        // Hide all states
        HideAllStates();
        
        // Show requested state
        switch (state)
        {
            case UIState.Splash:
                ShowElement(stateSplash);
                break;
                
            case UIState.NotConnected:
                ShowElement(stateNotConnected);
                HideBlur();
                break;
                
            case UIState.Connected:
                ShowElement(stateConnected);
                HideBlur();
                break;
                
            case UIState.Tracking:
                ShowElement(stateTracking);
                HideBlur();
                break;
                
            case UIState.Transition:
                ShowElement(stateTransition);
                ShowBlur();
                break;
        }
    }

    void HideAllStates()
    {
        HideElement(stateSplash);
        HideElement(stateNotConnected);
        HideElement(stateConnected);
        HideElement(stateTracking);
        HideElement(stateTransition);
    }

    void ShowElement(VisualElement element)
    {
        if (element != null)
        {
            element.RemoveFromClassList("hidden");
            element.style.display = DisplayStyle.Flex;
        }
    }

    void HideElement(VisualElement element)
    {
        if (element != null)
        {
            element.AddToClassList("hidden");
        }
    }

    void ShowBlur()
    {
        if (cameraBlur != null)
        {
            cameraBlur.RemoveFromClassList("hidden");
            cameraBlur.style.display = DisplayStyle.Flex;
        }
    }

    void HideBlur()
    {
        if (cameraBlur != null)
        {
            cameraBlur.AddToClassList("hidden");
        }
    }
}