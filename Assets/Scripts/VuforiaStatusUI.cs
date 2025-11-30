using UnityEngine;
using Vuforia;

public class VuforiaStatusUI : MonoBehaviour
{
    public StatusUIManager uiManager;

    void Start()
    {
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
        VuforiaBehaviour.Instance.World.OnObserverCreated += OnObserverCreated;
        VuforiaBehaviour.Instance.World.OnObserverDestroyed += OnObserverDestroyed;
    }

    private void OnVuforiaStarted()
    {
        uiManager?.SetMessage("Looking for objects...");
    }

    private void OnObserverCreated(ObserverBehaviour observer)
    {
        // Instead of writing the whole text, just notify
        uiManager?.SetMessage($"Found {observer.TargetName}");
    }

    private void OnObserverDestroyed(ObserverBehaviour observer)
    {
        uiManager?.SetMessage("No object found");
    }
}
