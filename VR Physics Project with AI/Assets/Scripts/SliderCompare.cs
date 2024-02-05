using UnityEngine;
using UnityEngine.UI; // This is needed for UI elements like Sliders

public class SliderCompare : MonoBehaviour
{
    public Slider leftSlider;  // Assign in inspector
    public Slider rightSlider; // Assign in inspector

    // Update is called once per frame
    void Update()
    {
        CheckSliderValues();
    }

    void CheckSliderValues()
    {
        if (leftSlider.value > rightSlider.value * 10)
        {
            // Action to take when left slider value is more than 10 times the right slider value
            TriggerAction();
        }
    }

    void TriggerAction()
    {
        // Replace this with the action you want to take
        Debug.Log("Left slider value is greater than 10 times the right slider value");
    }
}
