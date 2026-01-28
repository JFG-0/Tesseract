using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

/// <summary>
/// AR Object Manipulator that works alongside UI Toolkit
/// Uses Unity's new Input System (required when legacy Input is disabled)
/// Does NOT use IsPointerOverGameObject - that doesn't work with UI Toolkit
/// Instead relies on physics raycast to detect if touch hits this object
/// Requires: Collider on this object or children, PhysicsRaycaster on Camera
/// </summary>
public class ARManipulator : MonoBehaviour
{
    [Header("Movement Settings")]
    public float SpeedRotation = 0.4f;
    public float SpeedZoom = 0.5f;

    [Header("Zoom Limits")]
    public float MinScale = 0.5f;
    public float MaxScale = 3.0f;

    [Header("Animation Link")]
    public Animator AnimatorObject;
    public string AnimationParameterName = "Attiva";

    [Header("Debug")]
    public bool enableDebugLogs = true;

    // Internal state
    private float lastTapTime = 0;
    private float tapThreshold = 0.3f;
    private Quaternion initialRotation;
    
    private bool isDragging = false;
    private Vector2 lastPointerPosition;
    private int activePointerId = -1;

    // For pinch zoom
    private float initialPinchDistance;
    private float initialScale;

    void OnEnable()
    {
        // Enable Enhanced Touch support for new Input System
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Start()
    {
        initialRotation = transform.localRotation;

        // Verify collider exists
        Collider col = GetComponentInChildren<Collider>();
        if (col == null)
        {
            Debug.LogError($"[ARManipulator] {gameObject.name} needs a Collider for touch detection!");
        }
        else if (enableDebugLogs)
        {
            Debug.Log($"[ARManipulator] {gameObject.name} ready with collider: {col.GetType().Name}");
        }

        // Check for PhysicsRaycaster on main camera
        if (Camera.main != null && Camera.main.GetComponent<PhysicsRaycaster>() == null)
        {
            Debug.LogWarning("[ARManipulator] Adding PhysicsRaycaster to Main Camera for 3D touch detection");
            Camera.main.gameObject.AddComponent<PhysicsRaycaster>();
        }
    }

    void Update()
    {
        ProcessTouchInput();
        ProcessMouseInput();
    }

    void ProcessTouchInput()
    {
        var activeTouches = Touch.activeTouches;
        
        if (activeTouches.Count == 0) return;

        // Two-finger pinch zoom
        if (activeTouches.Count == 2)
        {
            HandlePinchZoom(activeTouches[0], activeTouches[1]);
            return;
        }

        // Single finger - rotation/tap
        Touch touch = activeTouches[0];
        Vector2 position = touch.screenPosition;
        int fingerId = touch.touchId;

        switch (touch.phase)
        {
            case TouchPhase.Began:
                TryStartDrag(position, fingerId);
                break;
                
            case TouchPhase.Moved:
                if (isDragging && activePointerId == fingerId)
                {
                    HandleDrag(position);
                }
                break;
                
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (isDragging && activePointerId == fingerId)
                {
                    HandleDragEnd();
                }
                break;
        }
    }

    void ProcessMouseInput()
    {
        // Only process mouse if no touches active (Editor testing)
        if (Touch.activeTouches.Count > 0) return;

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mousePos = mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame)
        {
            TryStartDrag(mousePos, -1);
        }
        else if (mouse.leftButton.isPressed && isDragging)
        {
            HandleDrag(mousePos);
        }
        else if (mouse.leftButton.wasReleasedThisFrame && isDragging)
        {
            HandleDragEnd();
        }

        // Mouse scroll zoom
        float scroll = mouse.scroll.ReadValue().y;
        if (scroll != 0 && IsPointerOverThis(mousePos))
        {
            ApplyZoom(scroll * SpeedZoom * 0.01f);
        }
    }

    void TryStartDrag(Vector2 screenPosition, int pointerId)
    {
        // Only check: Does raycast hit this object?
        // We don't use IsPointerOverGameObject because UI Toolkit breaks it
        if (IsPointerOverThis(screenPosition))
        {
            isDragging = true;
            activePointerId = pointerId;
            lastPointerPosition = screenPosition;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[ARManipulator] Drag started on {gameObject.name} at {screenPosition}");
            }
        }
    }

    void HandleDrag(Vector2 currentPosition)
    {
        Vector2 delta = currentPosition - lastPointerPosition;
        
        // Apply rotation
        float rotX = delta.x * SpeedRotation;
        float rotY = delta.y * SpeedRotation;
        transform.Rotate(rotY, -rotX, 0f, Space.World);
        
        lastPointerPosition = currentPosition;
    }

    void HandleDragEnd()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[ARManipulator] Drag ended on {gameObject.name}");
        }

        // Check for double-tap
        if (Time.time - lastTapTime < tapThreshold)
        {
            TriggerAnimation();
        }
        lastTapTime = Time.time;

        isDragging = false;
        activePointerId = -1;
    }

    void HandlePinchZoom(Touch t0, Touch t1)
    {
        Vector2 pos0 = t0.screenPosition;
        Vector2 pos1 = t1.screenPosition;

        // Check if either finger began on this object
        if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
        {
            if (IsPointerOverThis(pos0) || IsPointerOverThis(pos1))
            {
                initialPinchDistance = Vector2.Distance(pos0, pos1);
                initialScale = transform.localScale.x;
                isDragging = true;
                
                if (enableDebugLogs) Debug.Log($"[ARManipulator] Pinch started on {gameObject.name}");
            }
        }

        if (isDragging && t0.phase == TouchPhase.Moved && t1.phase == TouchPhase.Moved)
        {
            float currentDistance = Vector2.Distance(pos0, pos1);
            if (initialPinchDistance > 0)
            {
                float scaleFactor = currentDistance / initialPinchDistance;
                float newScale = Mathf.Clamp(initialScale * scaleFactor, MinScale, MaxScale);
                transform.localScale = Vector3.one * newScale;
            }
        }

        if (t0.phase == TouchPhase.Ended || t1.phase == TouchPhase.Ended)
        {
            isDragging = false;
        }
    }

    bool IsPointerOverThis(Vector2 screenPosition)
    {
        Camera cam = Camera.main;
        if (cam == null) 
        {
            if (enableDebugLogs) Debug.LogWarning("[ARManipulator] No main camera found!");
            return false;
        }

        Ray ray = cam.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Check if hit this object or any child
            bool isThis = (hit.transform == transform || hit.transform.IsChildOf(transform));
            
            if (enableDebugLogs)
            {
                Debug.Log($"[ARManipulator] Raycast hit: {hit.transform.name}, isThis: {isThis}");
            }
            
            return isThis;
        }
        else if (enableDebugLogs)
        {
            Debug.Log($"[ARManipulator] Raycast hit nothing at {screenPosition}");
        }
        
        return false;
    }

    void ApplyZoom(float amount)
    {
        float currentScale = transform.localScale.x;
        float newScale = Mathf.Clamp(currentScale + amount, MinScale, MaxScale);
        transform.localScale = Vector3.one * newScale;
    }

    void TriggerAnimation()
    {
        if (enableDebugLogs) Debug.Log($"[ARManipulator] Double-tap animation on {gameObject.name}");

        // Reset rotation
        transform.localRotation = initialRotation;

        // Toggle animation
        if (AnimatorObject != null)
        {
            bool currentState = AnimatorObject.GetBool(AnimationParameterName);
            AnimatorObject.SetBool(AnimationParameterName, !currentState);
        }
    }
}