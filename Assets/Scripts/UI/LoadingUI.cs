using UnityEngine;

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance;
    public GameObject loadingPanel;

    void Awake()
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

    public void ShowLoading(string message = "")
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        // Optionally update a child Text component with the message; not sure if we can implement
    }

    public void HideLoading()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
}