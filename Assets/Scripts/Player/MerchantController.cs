using UnityEngine;

public class MerchantController : MonoBehaviour
{
    private static MerchantController _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Keep merchant between scene loads
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }
}