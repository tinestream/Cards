using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CustomUiElements;
using UnityEngine.SceneManagement;

public class CapturePattern : MonoBehaviour {
	public RawImage videoImage;
	public RawImage patternImage;
	private WebCamTexture webCamTexture;
	private Texture2D texture;
	private Texture2D texturePat;
	private int w = 640;
	private int h = 480;
	private int first;
	private bool is_recording = false;
	private DateTime startTime = DateTime.Now;
	private Pattern lastPattern = null;

	void Start () {
		Manager mng = GameObject.Find ("Manager").GetComponent<Manager> ();
		GameObject.Find ("CMinSlider").GetComponent<MinimumSlider> ().value = mng.captureMin;
		GameObject.Find ("CMaxSlider").GetComponent<MaximumSlider> ().value = 1 - mng.captureMax;
		GameObject.Find ("TMinSlider").GetComponent<MinimumSlider> ().value = mng.minThr;
		GameObject.Find ("TMaxSlider").GetComponent<MaximumSlider> ().value = 1 - mng.maxThr;
		GameObject.Find ("Filter Slider").GetComponent<Slider> ().value = mng.filterSize;

		webCamTexture = null;
		WebCamDevice[] wdcs = WebCamTexture.devices;
		for (int n = 0; n < wdcs.Length; ++n) {
			if (wdcs.Length - 1 == n || !wdcs[n].isFrontFacing) {
				webCamTexture = new WebCamTexture(wdcs[n].name);
				break;
			}
		}
		//Debug.Log (wdcs.Length);

		if (webCamTexture == null) {
			webCamTexture = new WebCamTexture ();
		}
		//rawImage.texture = webCamTexture;
		//rawImage.material.mainTexture = webCamTexture;
		webCamTexture.requestedFPS = 10f;//1.0f;
		webCamTexture.requestedHeight = h;
		webCamTexture.requestedWidth = w;
		webCamTexture.Play ();
		first = 0;
	}

	void Update () {
		if (webCamTexture.didUpdateThisFrame) {
			Color[] color = webCamTexture.GetPixels ();
			if (first == 0) {
				w = webCamTexture.width;
				h = webCamTexture.height;
				GameObject.Find ("ResultLabel").GetComponent<Text> ().text = w + " x " + h;
				texture = new Texture2D (w, h);
				videoImage.texture = texture;
				videoImage.material.mainTexture = texture;
			}
			ImageProcessor processor = new ImageProcessor (w, h, webCamTexture, texture);
			Manager manager = GameObject.Find ("Manager").GetComponent<Manager> ();
			int captSize = (int)(90 * (1 - manager.captureMax)) + 10;
			if (is_recording) {
				processor.ProcessImage (captSize, manager.minThr, manager.maxThr, manager.filterSize);
				is_recording = new TimeSpan (DateTime.Now.Ticks - startTime.Ticks).TotalSeconds <= 5;
				lastPattern = processor.FindLowLevelPattern (captSize, (int)(90 * manager.captureMin) + 10, GameObject.Find ("Pattern Name").GetComponent<InputField> ().text);// manager.Types[GameObject.Find ("Type").GetComponent<Dropdown> ().value]
				if (lastPattern != null) {
					texturePat = new Texture2D (lastPattern.W, lastPattern.H);
					patternImage.texture = texturePat;
					patternImage.material.mainTexture = texturePat;
				}
				if (false) {//lastPattern is PatternGroup) {
					PatternGroup group = (PatternGroup)lastPattern;
					string s = "Groups: " + group.Count + "; ";
					for (int i = 0; i < group.Count; i++) {
						BoundingBox box = group.GetBox (i);
						s += box.MinX + ", " + box.MinY + ", " + box.MaxX + ", " + box.MaxY + "; ";
					}
					Debug.Log (s);
				}
			} else {
				texture.SetPixels (webCamTexture.GetPixels ());
				processor.DrawCaptureBox (captSize);
			}
			texture.Apply ();
			if (lastPattern != null) {
				ImageProcessor.DrawPattern (lastPattern, texturePat);
				texturePat.Apply ();
			}
			first++;
		}
	}

	public void OnChangeCMinSlider() {
		float value = GameObject.Find ("CMinSlider").GetComponent<MinimumSlider> ().value;
		GameObject.Find ("Manager").GetComponent<Manager> ().captureMin = value;
	}

	public void OnChangeCMaxSlider() {
		float value = GameObject.Find ("CMaxSlider").GetComponent<MaximumSlider> ().value;
		GameObject.Find ("Manager").GetComponent<Manager> ().captureMax = 1 - value;
	}

	public void OnChangeTMinSlider() {
		float value = GameObject.Find ("TMinSlider").GetComponent<MinimumSlider> ().value;
		GameObject.Find ("Manager").GetComponent<Manager> ().minThr = value;
	}

	public void OnChangeTMaxSlider() {
		float value = GameObject.Find ("TMaxSlider").GetComponent<MaximumSlider> ().value;
		GameObject.Find ("Manager").GetComponent<Manager> ().maxThr = 1 - value;
	}

	public void OnChangeFilterSlider() {
		float value = GameObject.Find ("Filter Slider").GetComponent<Slider> ().value;
		GameObject.Find ("Manager").GetComponent<Manager> ().filterSize = value;
	}

	public void onRecButton() {
		if (GameObject.Find ("Pattern Name").GetComponent<InputField> ().text.Length > 0) {
			is_recording = true;
			startTime = DateTime.Now;
		}
	}

	public void OnPlusButton() {
		if (lastPattern != null) {
			if (lastPattern is PatternImage && ((PatternImage)lastPattern).Id % Pattern.BASE_ID == ImageProcessor.UNKNOWN)
				return;
			if (lastPattern is PatternGroup) {
				PatternGroup group = (PatternGroup)lastPattern;
				for (int i = 0; i < group.Count; i++)
					if (group [i].Id % Pattern.BASE_ID == ImageProcessor.UNKNOWN) {
						Debug.Log ("Unknown id");
						return;
					}
			}
			Manager mng = GameObject.Find ("Manager").GetComponent<Manager> ();
			mng.lastType = GameObject.Find ("Type").GetComponent<Dropdown> ().value;
			PatternClass type = mng.Types [mng.lastType];
			type.add_pattern(lastPattern);
			mng.lastPatternInd = type.Count - 1;
			if (lastPattern is PatternImage)
				mng.Patterns.Add ((PatternImage)lastPattern);
			else if (lastPattern is PatternGroup)
				mng.LowGroups.Add ((PatternGroup)lastPattern);
			lastPattern = null;
			Pattern.IncLastId ();
			mng.Save ();
			SceneManager.LoadScene ("Set-up Recognition");
		}
	}

	public void OnExitButton () {
		SceneManager.LoadScene ("Main Menu");
	}
}
