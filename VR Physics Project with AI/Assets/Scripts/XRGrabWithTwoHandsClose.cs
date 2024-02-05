using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class XRGrabWithTwoHandsClose : MonoBehaviour
{
    public GameObject leftControllerGameObject;  // Assign in the inspector
    public GameObject rightControllerGameObject; // Assign in the inspector
    public float grabDistance = 1.0f; // Maximum distance to grab the object

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

        UpdateParenting();
    }

    void CheckForGrabOrRelease(InputDevice device, ref bool isHandGrabbing, GameObject controllerGameObject)
    {
        if (device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripValue))
        {
            if (gripValue && !isHandGrabbing && IsWithinGrabDistance(controllerGameObject))
            {
                isHandGrabbing = true;
            }
            else if (!gripValue && isHandGrabbing)
            {
                isHandGrabbing = false;
            }
        }
    }

    bool IsWithinGrabDistance(GameObject controller)
    {
        float distance = Vector3.Distance(controller.transform.position, transform.position);
        return distance <= grabDistance;
    }

    void UpdateParenting()
    {
        if (isLeftHandGrabbing && isRightHandGrabbing && parentWhenGrabbed == null)
        {
            parentWhenGrabbed = isLeftHandGrabbing ? leftControllerGameObject : rightControllerGameObject;
            transform.SetParent(parentWhenGrabbed.transform);
        }
        else if ((!isLeftHandGrabbing || !isRightHandGrabbing) && parentWhenGrabbed != null)
        {
            transform.SetParent(null);
            parentWhenGrabbed = null;
        }
    }
}
