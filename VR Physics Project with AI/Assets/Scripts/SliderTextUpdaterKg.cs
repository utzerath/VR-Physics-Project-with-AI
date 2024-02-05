using UnityEngine;
using TMPro; // Required for TextMeshPro
using UnityEngine.UI; // Required for UI elements like the Slider

public class SliderTextUpdaterKg : MonoBehaviour
{
    public Slider slider; // Reference to the UI Slider
    public TextMeshProUGUI tmpText; // Reference to the TextMeshProUGUI

    void Update()
    {
        // Update the TMP text to display the slider's value
        tmpText.text = "Weight: " + slider.value.ToString("F2") + " kg"; // "F2" formats the number with 2 decimal places
    }
}
