using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ApplicationManager : MonoBehaviour {
	

	public void Quit () 
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}

	public void RecognitionScene ()
	{
		SceneManager.LoadScene ("Recognize");
	}
	public void SetupScene ()
	{
		SceneManager.LoadScene ("Set-up Recognition");
	}
	public void FindPatternScene ()
	{
		SceneManager.LoadScene ("Capture Pattern");
	}

	public void Settings ()
	{
		SceneManager.LoadScene ("Settings");
	}
}
