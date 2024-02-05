using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class XRGrabWithTwoHandsProximity : MonoBehaviour
{
    public GameObject leftControllerGameObject;  // Assign in the inspector
    public GameObject rightControllerGameObject; // Assign in the inspector

    private InputDevice leftHandDevice;
    private InputDevice rightHandDevice;

    private bool isLeftHandGrabbing = false;
    private bool isRightHandGrabbing = false;
    private bool isLeftHandClose = false;
    private bool isRightHandClose = false;

    void Start()
    {
        InitializeControllers();
    }

    void InitializeControllers()
    {
        leftHandDevice = GetDevice(InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller);
        rightHandDevice = GetDevice(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller);
    }

    InputDevice GetDevice(InputDeviceCharacteristics characteristics)
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(characteristics, devices);

        if (devices.Count > 0)
        {
            return devices[0];
        }

        return new InputDevice(); // Return an invalid device if not found
    }

    void Update()
    {
        if (!leftHandDevice.isValid || !rightHandDevice.isValid)
        {
            InitializeControllers();
        }

        CheckForGrabOrRelease(leftHandDevice, ref isLeftHandGrabbing, isLeftHandClose);
        CheckForGrabOrRelease(rightHandDevice, ref isRightHandGrabbing, isRightHandClose);

        if (isLeftHandGrabbing && isRightHandGrabbing)
        {
            if (!isLeftHandClose || !isRightHandClose)
            {
                // Release the object if either hand is no longer close
                transform.SetParent(null);
            }
        }
        else
        {
            if (isLeftHandGrabbing || isRightHandGrabbing)
            {
                // If one hand is grabbing but the other isn't, release the object
                transform.SetParent(null);
            }
        }
    }

    void CheckForGrabOrRelease(InputDevice device, ref bool isHandGrabbing, bool isHandClose)
    {
        if (device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripValue))
        {
            if (gripValue && isHandClose)
            {
                isHandGrabbing = true;
                transform.SetParent(device == leftHandDevice ? leftControllerGameObject.transform : rightControllerGameObject.transform);
            }
            else if (!gripValue)
            {
                isHandGrabbing = false;
                if (transform.parent == (device == leftHandDevice ? leftControllerGameObject.transform : rightControllerGameObject.transform))
                {
                    transform.SetParent(null);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == leftControllerGameObject)
        {
            isLeftHandClose = true;
        }
        else if (other.gameObject == rightControllerGameObject)
        {
            isRightHandClose = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == leftControllerGameObject)
        {
            isLeftHandClose = false;
            isLeftHandGrabbing = false;
        }
        else if (other.gameObject == rightControllerGameObject)
        {
            isRightHandClose = false;
            isRightHandGrabbing = false;
        }
    }
}
