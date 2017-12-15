using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CustomUiElements;
using UnityEngine.SceneManagement;

public class Recognitio : MonoBehaviour {
	public RawImage videoImage;
	private WebCamTexture webCamTexture;
	private Texture2D texture;
	private bool is_recognizing = false;
	private DateTime startTime = DateTime.Now;
	private int first;
	private int w;
	private int h;
	private List<List<int>> indcs = new List<List<int>>(100);
	private List<float> scores = new List<float>(100);
	private List<string> names = new List<string>(100);

	// Use this for initialization
	void Start () {
		Manager mng = GameObject.Find ("Manager").GetComponent<Manager> ();
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
	}
	
	// Update is called once per frame
	void Update () {
		if (webCamTexture.didUpdateThisFrame) {
			Color[] color = webCamTexture.GetPixels ();
			if (first == 0) {
				w = webCamTexture.width;
				h = webCamTexture.height;
				texture = new Texture2D (w, h);
				videoImage.texture = texture;
				videoImage.material.mainTexture = texture;
			}
			ImageProcessor processor = new ImageProcessor (w, h, webCamTexture, texture);
			Manager manager = GameObject.Find ("Manager").GetComponent<Manager> ();
			int captSize = (int)(90 * (1 - manager.captureMax)) + 10;
			bool showall = GameObject.Find ("Video Toggle").GetComponent<Toggle> ().isOn;
			int minDrawSize = 0;
			if (!showall) {
				minDrawSize = captSize;
			}
			if (is_recognizing) {
				bool flag = new TimeSpan (DateTime.Now.Ticks - startTime.Ticks).TotalSeconds <= 5;
				List<int> indst = new List<int> (manager.Patterns.Count + 1);
				List<RecognizedPatternImage> recognized = new List<RecognizedPatternImage> (1000);
				processor.ProcessImage (captSize, manager.minThr, manager.maxThr, manager.filterSize, minDrawSize);
				processor.RecognizePatterns ((int)(90 * manager.captureMin) + 10, captSize, manager.Patterns, indst, recognized, null);
				List<int> indstgr = new List<int> (manager.LowGroups.Count + 1);
				List<RecognizedPatternGroup> recognizedgr = new List<RecognizedPatternGroup> (1000);
				processor.RecognizeGroups ((int)(90 * manager.captureMin) + 10, captSize, manager.LowGroups, indstgr, recognizedgr);
				is_recognizing = flag;
				List<int> iall = new List<int> (manager.Patterns.Count + 1);
				List<RecognizedPattern> all = new List<RecognizedPattern> (1000);
				processor.RecognizeAll (captSize, indst, recognized, indstgr, recognizedgr, manager.PatternsToRecognize, iall, all);
				if (all.Count > 0) {
					for (int i = 0; i < all.Count; i++) {
						RecognizedPattern rp = all [i];
						int rpc = 1;
						RecognizedPatternGroup rpg = null;
						if (rp is RecognizedPatternGroup) {
							rpg = (RecognizedPatternGroup)rp;
							rpc = rpg.Count;
						}
						bool hit = false;
						for (int j = 0; j < scores.Count; j++) {
							if (indcs [j].Count == rpc) {
								hit = true;
								if (rpg == null)
									hit = rp.Pattern.Id == indcs [j] [0];
								else
									for (int k = 0; k < rpc; k++)
										if (rpg [k].Pattern.Id != indcs [j] [k]) {
											hit = false;
											break;
										}
							}
							if (hit) {
								scores [j] += (1 - rp.Score);
								break;
							} 
						}
						if (!hit) {
							scores.Add (1 - rp.Score);
							indcs.Add (new List<int> (rpc));
							String name = "";
							if (rpg == null) {
								indcs [scores.Count - 1].Add (rp.Pattern.Id);
								name = Pattern.GetName (rp.Pattern.Id);
							} else
								for (int k = 0; k < rpc; k++) {
									indcs [scores.Count - 1].Add (rpg [k].Pattern.Id);
									name += Pattern.GetName (rpg [k].Pattern.Id);
								}
							names.Add (name);
						}
					}
					int maxi = 0;
					for (int i = 1; i < scores.Count; i++) {
						if (scores [i] > scores [maxi])
							maxi = i;
					}
					GameObject.Find ("AnswerText").GetComponent<Text> ().text = names [maxi];
				}
			} else if (showall) {
				texture.SetPixels (webCamTexture.GetPixels ());
				processor.DrawCaptureBox (captSize);
			}
			texture.Apply ();

			first++;
		}
	}
	public void OnPlayButton() {
		is_recognizing = true;
		startTime = DateTime.Now;
		scores.Clear ();
		indcs.Clear ();
	}

	public void OnExitButton () {
		SceneManager.LoadScene ("Main Menu");
	}
	public void OnChangeCMinSlider() {
		float value = GameObject.Find ("CMinSlider").GetComponent<MinimumSlider> ().value;
		GameObject.Find ("Manager").GetComponent<Manager> ().captureMin = value;
	}

	public void OnChangeCMaxSlider() {
		float value = GameObject.Find ("CMaxSlider").GetComponent<MaximumSlider> ().value;
		GameObject.Find ("Manager").GetComponent<Manager> ().captureMax = value;
	}
}
