using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SingletonManager : MonoBehaviour 
{
    // Singleton instance for global access.
    public static SingletonManager Instance { get; private set; } 

    [Header("Scene References")]
    [Tooltip("Reference to the main camera in the scene.")]
    public Camera mainCamera;
    
    [Tooltip("Reference to the bedroom cutscene camera.")]
    public Camera bedroomCamera;

    [Tooltip("Reference to the Gameplay Canvas in the scene.")]
    public Canvas gameplayCanvas;

    [Tooltip("Reference to the CutsceneCanvas.")]
    public Canvas cutsceneCanvas;
    
    [Tooltip("Reference to Loading Panel.")]
    public GameObject loadingPanel;
    
    [Tooltip("Reference to Lore Panel.")]
    public GameObject lorePanel;

    [Header("UI References")]
    [Tooltip("Game Over Panel under the main canvas.")]
    public GameObject gameOverPanel;
    
    [Tooltip("Text that displays 'You Died'.")]
    public GameObject youDiedText;
    
    [Tooltip("Restart button.")]
    public GameObject restartButton;
    
    [Tooltip("Quit To Menu button.")]
    public GameObject quitToMenuButton;
    
    [Tooltip("Quit button.")]
    public GameObject quitButton;
    
    [Tooltip("Final Level.")]
    public GameObject finalLevel;
    
    [Tooltip("Final Floor.")]
    public GameObject finalFloor;
    
    [Tooltip("Toolbar under GameplayCanvas.")]
    public GameObject toolbar;
    
    [Tooltip("ShowMainInventory button under GameplayCanvas.")]
    public GameObject invButton;
    
    [Tooltip("ShowCharacter button under GameplayCanvas.")]
    public GameObject showCharacter;
    
    [Tooltip("Event System")]
    public GameObject eventSystem;
    
    [Tooltip("XP Text")]
    public GameObject xpText;
    
    [Tooltip("XP Text Prefab")]
    public GameObject XPTextPrefab;

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