using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OnClick : MonoBehaviour {
	
	private int num1 = 1;
	private int num2 = 2;
	private int correct = 0;
	private int wrong = 0;
	private int curRes = 0;
	private static int[] _clear = { 0, 0, 0, 1,4,0,1,1,9,8,3,0,0,0 };
	private int correctClear = 0;

	private void UpdateNums(Manager mng)
	{
		GameObject.Find ("CorrectRes").GetComponent<Text> ().text = correct + "";
		GameObject.Find ("WrongRes").GetComponent<Text> ().text = wrong + "";
	}

	// Use this for initialization
	void Start () {
		Manager mng = GameObject.Find ("Manager").GetComponent<Manager> ();
		mng.startTime = System.DateTime.Now;
		UpdateNums (mng);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	private void exitGame(Manager mng) {
		mng.lastTime = (int) mng.LastSpanMins ();
		//			correct = 0;
		//			wrong = 0;
		SceneManager.LoadScene ("scene_main");
	}
	public void onExitGame() {
		Manager mng = GameObject.Find ("Manager").GetComponent<Manager> ();
		exitGame (mng);
	}
	private void checkGame(Manager mng) {
		/*if (mng.LastSpanMins() >= mng.time || correct + wrong >= mng.num) {
			exitGame (mng);
		}*/

	}
	void OnGUI() {
//		GameObject.Find ("AllRes").GetComponent<Text> ().text = System.DateTime.Now.ToString();
		Manager mng = GameObject.Find ("Manager").GetComponent<Manager>();
		string spoken0 = mng.spoken;
		mng.PropagateSpoken ();
		if (mng.spoken.Length > 0 && !spoken0.Equals (mng.spoken)) {
			GameObject.Find ("AllRes").GetComponent<Text> ().text = mng.spoken;	
			int ans = 0;
			if (System.Int32.TryParse (mng.spoken, out ans)) {
				onClick (ans);
				mng.spoken = "";
			}
		}
		checkGame (mng);
	}

	private int performOP(char sign, int num1, int num2) {
		return sign == '+' ? num1 + num2 : sign == '-' ? num1 - num2 : -1;
	}

	private bool checkOverfill(int curRes, char sign) {
		return curRes >= performOP(sign, num1, num2) || curRes * 10 > performOP(sign, num1, num2);
	}

	private bool checkCorrect(int curRes, char sign) {
		return performOP(sign, num1, num2) == curRes;
	}

	public void onClick(int ans) {
		Manager mng = GameObject.Find ("Manager").GetComponent<Manager>();
		curRes = curRes * 10 + ans;
		//gameObject.GetComponent<Text>().text = "" + num1 + mng.sign + num2 + "=" + curRes;

		if (ans == _clear [correctClear]) 
		{
			if (correctClear > 1)
				GameObject.Find ("AllRes").GetComponent<Text> ().text += "" + ans;
			correctClear++;
			if (correctClear == 14) {
				mng.Clear ();
				correctClear = 0;
			}
		} else
			correctClear = 0;
	}

	public void onApprove() {
		Manager mng = GameObject.Find ("Manager").GetComponent<Manager>();
	}
}
