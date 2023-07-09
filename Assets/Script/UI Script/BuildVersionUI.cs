using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuildVersionUI : MonoBehaviour
{
    public TextMeshProUGUI buildNumberText;

    private void Start() {
        // Get the build number
        string buildNumber = Application.version;

        // Set the build number in the TextMeshPro UI element
        buildNumberText.text = "Build Number: " + buildNumber;
    }
}
