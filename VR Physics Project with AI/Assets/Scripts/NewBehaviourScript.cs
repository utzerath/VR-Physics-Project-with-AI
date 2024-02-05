using UnityEngine;
using UnityEngine.UI; // For the Slider
using TMPro; // Required for TextMeshPro elements

public class SliderValueDisplay : MonoBehaviour
{
    public Slider slider; // Reference to the Slider
    public TextMeshProUGUI displayText; // Reference to the TextMeshProUGUI element

    void Update()
    {
        if (slider != null && displayText != null)
        {
            // Update the text to show the slider's value followed by 'N'
            displayText.text = "Slider Value: " + slider.value.ToString("F2") + "N"; // Adds 'N' after the number
        }
    }
}
