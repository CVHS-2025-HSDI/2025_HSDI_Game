using UnityEngine;
using UnityEngine.UI;

public class SingletonManager: MonoBehaviour 
{
    // Singleton instance for global access.
    public static SingletonManager Instance { get; private set; } 

    [Header("Scene References")]
    [Tooltip("Reference to the main camera in the scene.")]
    public Camera mainCamera;

    [Tooltip("Reference to the Gameplay Canvas in the scene.")]
    public Canvas gameplayCanvas;
    
    [Tooltip("Reference to Loading Panel.")]
    public GameObject loadingPanel;

    void Awake()
    {
        // Ensure only one instance exists.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}