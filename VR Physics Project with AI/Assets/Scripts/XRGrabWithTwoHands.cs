using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class XRGrabWithTwoHands : MonoBehaviour
{
    public GameObject leftControllerGameObject;  // Assign in the inspector
    public GameObject rightControllerGameObject; // Assign in the inspector

    private InputDevice leftHandDevice;
    private InputDevice rightHandDevice;

    private bool isLeftHandGrabbing = false;
    private bool isRightHandGrabbing = false;
    private GameObject parentWhenGrabbed = null;

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

        CheckForGrabOrRelease(leftHandDevice, ref isLeftHandGrabbing, leftControllerGameObject);
        CheckForGrabOrRelease(rightHandDevice, ref isRightHandGrabbing, rightControllerGameObject);

        if (isLeftHandGrabbing && isRightHandGrabbing && parentWhenGrabbed == null)
        {
            // Parent to the first hand that grabbed
            parentWhenGrabbed = isLeftHandGrabbing ? leftControllerGameObject : rightControllerGameObject;
            transform.SetParent(parentWhenGrabbed.transform);
        }
        else if ((!isLeftHandGrabbing || !isRightHandGrabbing) && parentWhenGrabbed != null)
        {
            // Release the object if one of the hands releases the grab
            transform.SetParent(null);
            parentWhenGrabbed = null;
        }
    }

    void CheckForGrabOrRelease(InputDevice device, ref bool isHandGrabbing, GameObject controllerGameObject)
    {
        if (device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripValue))
        {
            if (gripValue && !isHandGrabbing)
            {
                isHandGrabbing = true;
            }
            else if (!gripValue && isHandGrabbing)
            {
                isHandGrabbing = false;
            }
        }
    }
}
