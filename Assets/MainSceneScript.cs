using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainSceneScript : MonoBehaviour {


	// Use this for initialization
	void Start () {
		Manager mng = GameObject.Find ("Manager").GetComponent<Manager> ();
		/*GameObject.Find ("ResultLabel").GetComponent<Text> ().text = "Last ( " + mng.sign + "): " + mng.lastCorrect + "/" + mng.lastWrong + " (" + mng.lastTime + ":00)";
		GameObject.Find ("TimeSlider").GetComponent<Slider> ().value = mng.time;
		GameObject.Find ("NumSlider").GetComponent<Slider> ().value = mng.num;
		GameObject.Find ("MaxSlider").GetComponent<Slider> ().value = mng.max1 - 1;*/
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnChangeTime() {
		int value = (int) GameObject.Find ("TimeSlider").GetComponent<Slider> ().value;
		if (value == 11)
			value = 60;
		//GameObject.Find ("Manager").GetComponent<Manager> ().time = value;
		GameObject.Find ("TimeLabel").GetComponent<Text> ().text = (value < 10 ? "0" : "") + value + ":" + "00";
		//
	}

	public void OnChangeNum() {
		int value = (int) GameObject.Find ("NumSlider").GetComponent<Slider> ().value;
		//GameObject.Find ("Manager").GetComponent<Manager> ().num = value;
		GameObject.Find ("NumLabel").GetComponent<Text> ().text = "" + value;
		//
	}

	public void OnChangeMax() {
		int value = (int) GameObject.Find ("MaxSlider").GetComponent<Slider> ().value;
		//GameObject.Find ("Manager").GetComponent<Manager> ().SetMax (value);
		GameObject.Find ("MaxLabel").GetComponent<Text> ().text = "" + value;
		//
	}

	public void OnPlusButtonClick() {
		//GameObject.Find ("Manager").GetComponent<Manager> ().sign = '+';
		SceneManager.LoadScene ("scene_numbers");
	}

	public void OnMinusButtonClick() {
		//GameObject.Find ("Manager").GetComponent<Manager> ().sign = '-';
		SceneManager.LoadScene ("scene_numbers");
	}

	public void OnExit() {
		Application.Quit ();
	}
}
