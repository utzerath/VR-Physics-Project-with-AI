using UnityEngine;
using UnityEngine.UI; // Required for the Slider component

public class LockUnlockObject : MonoBehaviour
{
    public GameObject objectToControl;
    public Slider leftSlider;
    public Slider rightSlider;

    private bool isLocked = true;
    private Vector3 originalPosition;

    void Start()
    {
        if (objectToControl != null)
        {
            originalPosition = objectToControl.transform.position;
        }
    }

    void Update()
    {
        // Check if the condition to unlock is met
        if (leftSlider.value > rightSlider.value * 9.8f && isLocked)
        {
            isLocked = false;
            // Optional: Do something when the object is unlocked
        }
        else if (leftSlider.value <= rightSlider.value * 9.8f && !isLocked)
        {
            // This condition will lock the object again and reset it if the sliders are adjusted
            isLocked = true;
            ResetObjectPosition();
        }

        // Lock or unlock the object based on `isLocked` state
        if (isLocked)
        {
            // Lock the object's position
            LockObjectPosition();
        }
        else
        {
            // Object is unlocked and can move freely
            // You can add logic here for when the object is unlocked
        }
    }

    private void LockObjectPosition()
    {
        if (objectToControl != null)
        {
            objectToControl.transform.position = originalPosition;
        }
    }

    private void ResetObjectPosition()
    {
        // Reset the object's position to its original state
        if (objectToControl != null)
        {
            objectToControl.transform.position = originalPosition;
        }
    }
}
