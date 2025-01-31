using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // The Transform the camera should follow
    public Transform target;

    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    private void LateUpdate()
    {
        if (target == null) return;  // No target, do nothing

        // Follow with optional smoothing
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    // Example method to set the target if needed
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}