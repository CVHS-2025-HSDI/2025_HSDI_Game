using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private static CameraFollow instance;
    
    // The Transform the camera should follow
    public Transform target;

    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Enforce a fixed z position of -10 directly
        smoothedPosition.z = -10f;
        transform.position = smoothedPosition;
    }

    // Example method to set the target if needed
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}