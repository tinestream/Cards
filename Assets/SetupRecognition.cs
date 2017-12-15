using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CustomUiElements;
using UnityEngine.SceneManagement;

public class SetupRecognition : MonoBehaviour {
	public RawImage videoImage;
	public RawImage patternImage;
	private WebCamTexture webCamTexture;
	private Texture2D texture;
	private Texture2D texturePat;
	private int w = 400;
	private int h = 300;
	private int first;
	private bool is_recording = false;
	private bool is_recognizing = false;
	private DateTime startTime = DateTime.Now;
	private Pattern lastPattern = null;

	public void PropagatePatternList() {
		Manager mng = GameObject.Find ("Manager").GetComponent<Manager> ();
		int typeind = GameObject.Find ("Type").GetComponent<Dropdown> ().value;
		Dropdown dd = GameObject.Find ("Select Pattern").GetComponent<Dropdown> ();
		dd.ClearOptions ();
		IEnumerator<Pattern> cls;
		if (typeind < mng.Types.Count)
			cls = mng.Types [typeind].GetEnumerator();
		else
			cls = mng.PatternsToRecognize.GetEnumerator();
		if (!cls.MoveNext())
			dd.enabled = false;
		else {
			dd.enabled = true;
			List<string> options = new List<string> (10);
			do {
				options.Add(Pattern.GetName(cls.Current.Id));
			} while (cls.MoveNext ());
			dd.AddOptions (options);
		}
		if (dd.enabled) {
			dd.value = mng.lastPatternInd;
			if (mng.lastPatternInd > 0)
				mng.lastPatternInd = 0;
			OnChoosePattern ();
		}
	}

	private Pattern currentPattern() {
		Manager mng = GameObject.Find ("Manager").GetComponent<Manager> ();
		int typeind = GameObject.Find ("Type").GetComponent<Dropdown> ().value;
		int ind = GameObject.Find ("Select Pattern").GetComponent<Dropdown> ().value;
		if (typeind < mng.Types.Count) {
			PatternClass cls = mng.Types [typeind];
			if (cls.Count > ind)
				return cls [ind];
		} else {
			if (ind < mng.PatternsToRecognize.Count)
				return mng.PatternsToRecognize [ind];
		}
		return null;
	}
	private void drawPattern(Pattern pat) {
		texturePat = new Texture2D (pat.W, pat.H);
		patternImage.texture = texturePat;
		patternImage.material.mainTexture = texturePat;
		ImageProcessor.DrawPattern (pat, texturePat);
		PatternImage pi = null;
		if (pat is PatternImage)
			pi = (PatternImage)pat;
		else if (pat is PatternGroup && (((PatternGroup)pat) [0]) is PatternImage)
			pi = (PatternImage)(((PatternGroup)pat) [0]);
		GameObject.Find ("Ratio Slider").GetComponent<Slider> ().value = (pat.Tol - 0.0f) / 1.0f;	
		if (pi != null) {
			GameObject.Find ("Threshold Slider").GetComponent<Slider> ().value = (pi.Thrs [1] - 0.0f) / 1.0f;
			Debug.Log ("Thres: " + pi.Thrs [1]);
		}
		texturePat.Apply ();
	}

	public void OnChoosePattern() {
		Pattern pat = currentPattern ();
		if (pat != null) {
			lastPattern = pat;
			drawPattern (lastPattern);
		}
	}

	void Start () {
		Manager mng = GameObject.Find ("Manager").GetComponent<Manager> ();
		if (mng.lastType > 0)
			GameObject.Find ("Type").GetComponent<Dropdown> ().value = mng.lastType;
		else
			GameObject.Find ("Type").GetComponent<Dropdown> ().value = mng.Types.Count;
		PropagatePatternList ();
		GameObject.Find ("CMinSlider").GetComponent<MinimumSlider> ().value = mng.captureMin;
		GameObject.Find ("CMaxSlider").GetComponent<MaximumSlider> ().value = 1 - mng.captureMax;

		webCamTexture = null;
		WebCamDevice[] wdcs = WebCamTexture.devices;
		for (int n = 0; n < wdcs.Length; ++n) {
			if (wdcs[n].isFrontFacing) {
				webCamTexture = new WebCamTexture(wdcs[n].name);
			}
		}
		Debug.Log (wdcs.Length);

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
		//Debug.Log ("Types: " + mng.Types [0].Id + " " + mng.Types [1].Id + "; " + ((PatternGroup)mng.PatternsToRecognize [0]) [0].Id + ", " + ((PatternGroup)mng.PatternsToRecognize [0]) [1].Id);
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
			if (is_recording || is_recognizing) {
				bool flag = new TimeSpan (DateTime.Now.Ticks - startTime.Ticks).TotalSeconds <= 5;
				List<int> indst = new List<int> (manager.Patterns.Count + 1);
				List<RecognizedPatternImage> recognized = new List<RecognizedPatternImage> (1000);
				Pattern pat = currentPattern();
				if (pat is PatternGroup)
					pat = ((PatternGroup) pat)[0];
				if (!(pat is PatternImage) || is_recognizing)
					pat = null;
				processor.ProcessImage (captSize, manager.minThr, manager.maxThr, manager.filterSize);
				processor.RecognizePatterns ((int)(90 * manager.captureMin) + 10, captSize, manager.Patterns, indst, recognized, (PatternImage) pat);
				List<int> indstgr = new List<int> (manager.LowGroups.Count + 1);
				List<RecognizedPatternGroup> recognizedgr = new List<RecognizedPatternGroup> (1000);
				processor.RecognizeGroups ((int)(90 * manager.captureMin) + 10, captSize, manager.LowGroups, indstgr, recognizedgr);
				if (is_recording) {
					is_recording = flag;
					Pattern lastPattern1 = processor.FindPatternToRecognize (captSize, indst, recognized, indstgr, recognizedgr, 0, true);
					if (lastPattern1 != null) {
						lastPattern = lastPattern1;
						drawPattern (lastPattern);
					}
				} else {
					is_recognizing = flag;
					List<int> iall = new List<int> (manager.Patterns.Count + 1);
					List<RecognizedPattern> all = new List<RecognizedPattern> (1000);
					processor.RecognizeAll (captSize, indst, recognized, indstgr, recognizedgr, manager.PatternsToRecognize, iall, all);
				}
			} else {
				texture.SetPixels (webCamTexture.GetPixels ());
				processor.DrawCaptureBox (captSize);
			}
			texture.Apply ();

			first++;
		}
	}

	public void OnChangeCMinSlider() {
		float value = GameObject.Find ("CMinSlider").GetComponent<MinimumSlider> ().value;
		GameObject.Find ("Manager").GetComponent<Manager> ().captureMin = value;
	}

	public void OnChangeCMaxSlider() {
		float value = GameObject.Find ("CMaxSlider").GetComponent<MaximumSlider> ().value;
		GameObject.Find ("Manager").GetComponent<Manager> ().captureMax = value;
	}

	public void OnChangeRatioSlider() {
		float value = GameObject.Find ("Ratio Slider").GetComponent<Slider> ().value;
		Pattern pat = lastPattern;//currentPattern();
		if (pat != null) {
			pat.Tol = value;
			if (pat is PatternGroup) {
				PatternGroup pg = (PatternGroup)pat;
				for (int i = 0; i < pg.Count; i++)
					if (pg [i] is PatternImage)
						pg [i].Tol = value;
			}
		}
	}

	public void OnChangeThresholdSlider() {
		float value = GameObject.Find ("Threshold Slider").GetComponent<Slider> ().value;
		Pattern pat = lastPattern;//currentPattern();
		if (pat is PatternImage)
			((PatternImage)pat).Thrs [1] = value;
		else if (pat is PatternGroup) {
			PatternGroup pg = (PatternGroup)pat;
			for (int i = 0; i < pg.Count; i++)
				if (pg [i] is PatternImage)
					((PatternImage)(pg [i])).Thrs [1] = value;
		}
	}

	public void OnRecButton() {
		is_recording = true;
		is_recognizing = false;
		startTime = DateTime.Now;
	}

	public void OnPlayButton() {
		is_recording = false;
		is_recognizing = true;
		startTime = DateTime.Now;
	}

	public void OnPlusButton() {
		if (lastPattern != null) {
			Manager mng = GameObject.Find ("Manager").GetComponent<Manager> ();
			mng.PatternsToRecognize.Add (lastPattern);
			lastPattern = null;
			Pattern.IncLastId ();
			PropagatePatternList ();
			mng.Save ();
		}
	}

	public void OnExitButton () {
		SceneManager.LoadScene ("Main Menu");
	}
}
