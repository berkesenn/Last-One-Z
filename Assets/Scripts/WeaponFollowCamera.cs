using UnityEngine;

public class WeaponFollowCamera : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    
    [Header("Settings")]
    public Vector3 positionOffset = new Vector3(0.3f, -0.2f, 0.5f);
    public Vector3 rotationOffset = Vector3.zero;
    public float smoothSpeed = 10f;
    
    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }
    
    void LateUpdate()
    {
        if (cameraTransform == null)
            return;
        
        // Calculate target position and rotation based on camera
        Vector3 targetPosition = cameraTransform.position + cameraTransform.TransformDirection(positionOffset);
        Quaternion targetRotation = cameraTransform.rotation * Quaternion.Euler(rotationOffset);
        
        // Smoothly move weapon to target
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
    }
}
