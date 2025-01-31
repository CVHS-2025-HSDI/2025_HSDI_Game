using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static PlayerController _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Keep player between scene loads
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }
}