using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine.XR;

public class XRRayInteractorUIController : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public InputHelpers.Button selectButton = InputHelpers.Button.Trigger;
    public float selectActivationThreshold = 0.1f;
    private XRController xrController;

    void Awake()
    {
        if (!rayInteractor)
        {
            rayInteractor = GetComponent<XRRayInteractor>();
        }
        xrController = rayInteractor.GetComponent<XRController>();
    }

    void Update()
    {
        if (IsSelectButtonDown())
        {
            if (rayInteractor.TryGetCurrentUIRaycastResult(out var result))
            {
                XRBaseInteractable interactable = result.gameObject.GetComponent<XRBaseInteractable>();
                if (interactable)
                {
                    SelectUIElement(interactable);
                }
            }
        }
    }

    bool IsSelectButtonDown()
    {
        if (xrController)
        {
            xrController.inputDevice.IsPressed(selectButton, out bool isPressed, selectActivationThreshold);
            return isPressed;
        }
        return false;
    }

    void SelectUIElement(XRBaseInteractable interactable)
    {
        var selectable = interactable.GetComponent<Selectable>();
        if (selectable)
        {
            selectable.Select();
            if (selectable is Button button)
            {
                button.onClick.Invoke();
            }
        }
    }
}
