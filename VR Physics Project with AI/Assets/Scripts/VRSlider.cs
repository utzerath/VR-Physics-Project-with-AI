using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRSlider : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Vector3 initialLocalPosition;
    private float minX, maxX;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        initialLocalPosition = transform.localPosition;
        minX = -3.36f;
        maxX = -2.514f;
    }

    void Update()
    {
        if (grabInteractable.isSelected)
        {
            Vector3 localPosition = transform.localPosition;
            localPosition.x = Mathf.Clamp(localPosition.x, minX, maxX);
            localPosition.y = initialLocalPosition.y;
            localPosition.z = initialLocalPosition.z;
            transform.localPosition = localPosition;
        }
    }
}
