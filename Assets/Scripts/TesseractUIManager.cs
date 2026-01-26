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
    
    private ProjectInfo currentProject = null;
    
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
    
    void ConfigurePickingModes()
    {
        // Root and background: Ignore (pass clicks through to AR)
        if (root != null)
            root.pickingMode = PickingMode.Ignore;
        
        // All state screens: Ignore by default (AR interactivity)
        if (stateSplash != null) stateSplash.pickingMode = PickingMode.Ignore;
        if (stateNotConnected != null) stateNotConnected.pickingMode = PickingMode.Ignore;
        if (stateConnected != null) stateConnected.pickingMode = PickingMode.Ignore;
        if (stateTracking != null) stateTracking.pickingMode = PickingMode.Ignore;
        if (stateTransition != null) stateTransition.pickingMode = PickingMode.Ignore;
        
        // Blur: Ignore (should not block clicks)
        if (cameraBlur != null) cameraBlur.pickingMode = PickingMode.Ignore;
        
        Debug.Log("[UIManager] Picking modes configured - UI passes through to AR by default");
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
        // Find VuforiaIMUController if not assigned (using non-deprecated method)
        if (imuController == null)
        {
            imuController = FindFirstObjectByType<VuforiaIMUController>();
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
        
        // Configure picking modes for proper event handling
        ConfigurePickingModes();
        
        // CRITICAL FIX: Setup button with proper picking mode
        var projectBtn = stateTracking?.Q<Button>("project-button");
        if (projectBtn != null)
        {
            projectBtn.pickingMode = PickingMode.Position; // Receive clicks
            projectBtn.clicked += OnProjectButtonClicked;
            Debug.Log("✓ Project button found in UXML");
            Debug.Log($"  - Picking mode: {projectBtn.pickingMode}");
            Debug.Log($"  - Enabled: {projectBtn.enabledSelf}");
        }
        else
        {
            Debug.LogError("✗ Project button NOT found in UXML!");
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
                UpdateCurrentProject(face);
                StartCoroutine(FaceChangeTransition());
            }
        }
    }
    
    void UpdateCurrentProject(int faceID)
    {
        // Get the active ImageTarget from VuforiaIMUController
        GameObject activeTarget = null;
        
        switch (faceID)
        {
            case 1: activeTarget = imuController.imageTarget1; break;
            case 2: activeTarget = imuController.imageTarget2; break;
            case 3: activeTarget = imuController.imageTarget3; break;
            case 4: activeTarget = imuController.imageTarget4; break;
            case 5: activeTarget = imuController.imageTarget5; break;
            case 6: activeTarget = imuController.imageTarget6; break;
        }
        
        if (activeTarget != null)
        {
            currentProject = activeTarget.GetComponent<ProjectInfo>();
            
            if (currentProject != null)
            {
                Debug.Log($"[UIManager] Current project: {currentProject.projectName}");
                
                // Optional: Update button text with project name
                UpdateProjectButtonText();
            }
            else
            {
                Debug.LogWarning($"[UIManager] No ProjectInfo on {activeTarget.name}");
            }
        }
    }
    
    void UpdateProjectButtonText()
    {
        if (stateTracking == null || currentProject == null) return;
        
        var projectBtn = stateTracking.Q<Button>("project-button");
        var buttonLabel = projectBtn?.Q<Label>();
        
        if (buttonLabel != null)
        {
            // Option 1: Generic text
            buttonLabel.text = "Let's discuss your project";
            
            // Option 2: Include project name
            // buttonLabel.text = $"Discuss {currentProject.projectName}";
        }
    }
    
    void OnProjectButtonClicked()
    {
        Debug.Log("[UIManager] Project button clicked!");
        
        if (currentProject != null)
        {
            Debug.Log($"[UIManager] Opening URL for: {currentProject.projectName}");
            currentProject.OpenCreatorURL();
        }
        else
        {
            Debug.LogWarning("[UIManager] No active project - checking current face...");
            
            // Fallback: try to get current project now
            UpdateCurrentProjectFromCurrentFace();
            
            if (currentProject != null)
            {
                Debug.Log($"[UIManager] Found project: {currentProject.projectName}, opening URL");
                currentProject.OpenCreatorURL();
            }
            else
            {
                Debug.LogError("[UIManager] Still no project found! Check if ProjectInfo component is attached to ImageTargets");
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
            
            // Update current project when entering tracking state
            if (newState == UIState.Tracking)
            {
                UpdateCurrentProjectFromCurrentFace();
            }
            
            ShowState(newState);
        }
    }
    
    void UpdateCurrentProjectFromCurrentFace()
    {
        // Read current face from IMU controller
        var currentFaceField = imuController.GetType().GetField("currentFaceID",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (currentFaceField != null)
        {
            int face = (int)currentFaceField.GetValue(imuController);
            if (face > 0)
            {
                UpdateCurrentProject(face);
            }
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