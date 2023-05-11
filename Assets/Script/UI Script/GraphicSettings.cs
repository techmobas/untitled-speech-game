using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphicSettings : MonoBehaviour
{
	public Toggle fullScreenToggle, vSyncToggle;
	public TMP_Dropdown resDropdown;

	public List<ResItem> resolutions = new List<ResItem>();
	private int selectedRes;

	public TextMeshProUGUI micText;

	void Start() {
		fullScreenToggle.isOn = Screen.fullScreen;
		if (QualitySettings.vSyncCount == 0) {
			vSyncToggle.isOn = false;
		}

		else {
			vSyncToggle.isOn = true;
		}

		bool foundRes = false;
		for (int i = 0; i < resolutions.Count; i++) {
			if (Screen.width == resolutions[i].horizontal && Screen.height == resolutions[i].vertical) {
				foundRes = true;
				selectedRes = i;
				UpdateResText();
			}
		}

		if (!foundRes) {
			ResItem newRes = new ResItem();
			newRes.horizontal = Screen.width;
			newRes.vertical = Screen.height;

			resolutions.Add(newRes);
			selectedRes = resolutions.Count - 1;

			UpdateResText();
		}


		string[] devices = Microphone.devices;
		if (devices.Length > 0) {
			micText.text = "Microphone: " + devices[0];
		}
		else {
			micText.text = "No microphones found";
		}

		// Set up the resolution dropdown
		resDropdown.ClearOptions();
		List<string> resOptions = new List<string>();
		foreach (ResItem res in resolutions) {
			resOptions.Add(res.horizontal + " x " + res.vertical);
		}
		resDropdown.AddOptions(resOptions);
		resDropdown.value = selectedRes;
	}

	public void ResDropdownChanged(int index) {
		selectedRes = index;
		UpdateResText();
	}

	public void UpdateResText() {
		resDropdown.value = selectedRes;
	}

	public void ApplyGraphic() {
		Screen.fullScreen = fullScreenToggle.isOn;
		if (vSyncToggle.isOn) {
			QualitySettings.vSyncCount = 1;
		}
		else {
			QualitySettings.vSyncCount = 0;
		}

		Screen.SetResolution(resolutions[selectedRes].horizontal, resolutions[selectedRes].vertical, fullScreenToggle.isOn);
	}
}

[System.Serializable]
public class ResItem {
	public int horizontal, vertical;
}