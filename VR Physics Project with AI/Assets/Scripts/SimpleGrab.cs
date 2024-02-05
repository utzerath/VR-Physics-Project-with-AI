using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XRGrab : MonoBehaviour
{
    public GameObject rightControllerGameObject; // Assign this in the inspector
    private InputDevice targetDevice;
    private bool isHeld = false;

    private void Start()
    {
        // Initialize the targetDevice. In this case, let's assume it's the right hand.
        TryInitialize();
    }

    void TryInitialize()
    {
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        List<InputDevice> devices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);

        if (devices.Count > 0)
        {
            targetDevice = devices[0];
        }
    }

    private void Update()
    {
        if (!targetDevice.isValid)
        {
            TryInitialize();
        }

        if (targetDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripValue))
        {
            if (gripValue && !isHeld)
            {
                GrabObject();
            }
            else if (!gripValue && isHeld)
            {
                ReleaseObject();
            }
        }
    }

    private void GrabObject()
    {
        // Logic to attach the object to the controller
        if (rightControllerGameObject != null)
        {
            transform.SetParent(rightControllerGameObject.transform);
            isHeld = true;
        }
    }

    private void ReleaseObject()
    {
        // Logic to detach the object from the controller
        transform.SetParent(null); // or original parent if applicable
        isHeld = false;
    }
}
