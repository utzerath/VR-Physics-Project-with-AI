using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class YZAxisLock : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Lock the Y and Z position movement and rotation along X and Z axes
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ
                         | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }
}
