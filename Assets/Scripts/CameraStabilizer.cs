using UnityEngine;

public class CameraStabilizer : MonoBehaviour
{
    [Header("Stabilization Settings")]
    public bool stabilizeY = true;
    public bool stabilizeRotation = false;
    
    [Header("Smoothing")]
    public float positionSmoothing = 10f;
    
    private Vector3 lastPosition;
    private float fixedY;
    
    void Start()
    {
        lastPosition = transform.position;
        fixedY = transform.localPosition.y;
    }
    
    void LateUpdate()
    {
        if (stabilizeY)
        {
            // Keep Y position fixed relative to parent
            Vector3 localPos = transform.localPosition;
            localPos.y = Mathf.Lerp(localPos.y, fixedY, Time.deltaTime * positionSmoothing);
            transform.localPosition = localPos;
        }
        
        if (stabilizeRotation)
        {
            // Keep camera rotation stable (only affected by mouse look)
            Vector3 localRot = transform.localEulerAngles;
            localRot.y = 0; // No side rotation from character
            localRot.z = 0; // No roll
            transform.localEulerAngles = localRot;
        }
    }
}
