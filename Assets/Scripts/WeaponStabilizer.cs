using UnityEngine;

public class WeaponStabilizer : MonoBehaviour
{
    [Header("Stabilization")]
    public bool lockRotation = true;
    public Vector3 fixedRotation = Vector3.zero;
    
    private Quaternion initialLocalRotation;
    
    void Start()
    {
        initialLocalRotation = transform.localRotation;
    }
    
    void LateUpdate()
    {
        if (lockRotation)
        {
            // Keep weapon at fixed rotation relative to camera
            transform.localRotation = Quaternion.Euler(fixedRotation);
        }
    }
}
