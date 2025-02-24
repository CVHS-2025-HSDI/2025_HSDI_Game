using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LorePanelController : MonoBehaviour
{
    public static LorePanelController Instance;

    [Header("UI Components")]
    public TextMeshProUGUI loreTitleText; // Assign via Inspector (child of the panel)
    public TextMeshProUGUI loreBodyText;  // Assign via Inspector (child of the panel)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}