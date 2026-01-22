using UnityEngine;

/// <summary>
/// Simple component to hold project metadata for each ImageTarget
/// Attach this to each ImageTarget GameObject
/// </summary>
public class ProjectInfo : MonoBehaviour
{
    [Header("Project Details")]
    [Tooltip("Name of the project (for display)")]
    public string projectName = "My Project";
    
    [Tooltip("URL to creator's social media or portfolio")]
    public string creatorURL = "https://www.behance.net/yourprofile";
    
    [Header("Optional Info")]
    [Tooltip("Creator name")]
    public string creatorName = "";
    
    [Tooltip("Project description")]
    [TextArea(3, 5)]
    public string description = "";
    
    /// <summary>
    /// Opens the creator's URL in default browser
    /// </summary>
    public void OpenCreatorURL()
    {
        if (!string.IsNullOrEmpty(creatorURL))
        {
            Debug.Log($"Opening URL for {projectName}: {creatorURL}");
            Application.OpenURL(creatorURL);
        }
        else
        {
            Debug.LogWarning($"No URL set for project: {projectName}");
        }
    }
}