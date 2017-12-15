using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CustomUiElements;

public class Settings : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Dropdown dd = GameObject.Find ("Devices").GetComponent<Dropdown> ();
		dd.ClearOptions ();
		List<string> options = new List<string> (10);
		WebCamDevice[] wdcs = WebCamTexture.devices;
		for (int n = 0; n < wdcs.Length; ++n) 
			if (wdcs[n].isFrontFacing)
				options.Add( "[Front] " + wdcs[n].name);
			else
				options.Add("[Back] " + wdcs[n].name);
		dd.AddOptions (options);
		Manager mng = GameObject.Find ("Manager").GetComponent<Manager> ();
		dd.value = mng.selectedDevice;
		GameObject.Find ("WHMinSlider").GetComponent<MinimumSlider> ().value = mng.camMin;
		GameObject.Find ("WHMaxSlider").GetComponent<MaximumSlider> ().value = mng.camMax;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void onDeviceChange() {
		Dropdown dd = GameObject.Find ("Devices").GetComponent<Dropdown> ();
		Manager mng = GameObject.Find ("Manager").GetComponent<Manager> ();
		mng.selectedDevice = dd.value;
	}

	public void onResolutionChange() {
		Manager mng = GameObject.Find ("Manager").GetComponent<Manager> ();
		mng.camMin = (int) GameObject.Find ("WHMinSlider").GetComponent<MinimumSlider> ().value;
		mng.camMax = (int) GameObject.Find ("WHMaxSlider").GetComponent<MaximumSlider> ().value;
	}
}
