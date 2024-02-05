using UnityEngine;

public class RotationLocker : MonoBehaviour
{
    private Quaternion initialRotation;

    void Start()
    {
        // Save the initial rotation of the object
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // Lock the rotation to the initial rotation
        transform.rotation = initialRotation;
    }
}
