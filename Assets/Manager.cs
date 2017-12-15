using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class PatternsStruct {
	private List<PatternImage> patterns;
	private List<PatternClass> types;
	private List<PatternGroup> lowGroups;
	private List<Pattern> patternsToRecognize;
	public List<PatternImage> Patterns {get { return patterns;}}
	public List<PatternClass> Types {get { return types;}}
	public List<PatternGroup> LowGroups {get {return lowGroups;}}
	public List<Pattern> PatternsToRecognize { get { return patternsToRecognize; } }
	public void Clear() {
		patterns.Clear ();
		types.Clear ();
		lowGroups.Clear ();
		patternsToRecognize.Clear ();
	}

	public PatternsStruct() {
		patterns = new List<PatternImage> (PatternBaseGroup.MAX_GROUP);
		types = new List<PatternClass> (PatternBaseGroup.MAX_GROUP);
		lowGroups = new List<PatternGroup> (PatternBaseGroup.MAX_GROUP);
		patternsToRecognize = new List<Pattern> (PatternBaseGroup.MAX_GROUP);
	}
}

public class Manager: MonoBehaviour
{
	PatternsStruct patstr;
	public static Manager manager;
	public List<PatternImage> Patterns {get { return patstr.Patterns;}}
	public List<PatternClass> Types {get { return patstr.Types;}}
	public List<PatternGroup> LowGroups {get {return patstr.LowGroups;}}
	public List<Pattern> PatternsToRecognize { get { return patstr.PatternsToRecognize; } }
	public float captureMin = 0;
	public float captureMax = 0.5f;
	public float minThr = 0.1f;
	public float maxThr = 0.5f;
	public float filterSize = 10f;//1.4f;
	public int selectedDevice = 0;
	public int camMin = 300;
	public int camMax = 200;
	public int lastType;
	public int lastPatternInd;

	public int lastTime = 0;
	public DateTime startTime = DateTime.Now;
	public string spoken = "";

	private void initTypes() {
		PatternClass suit = new PatternClass (1000);
		PatternClass number = new PatternClass (1001);
		Types.Add (suit);
		Types.Add (number);
	}


	public double LastSpanMins() {
		return new TimeSpan (DateTime.Now.Ticks - startTime.Ticks).TotalMinutes;
		//return data.Count == 0 ? 0 : new TimeSpan (DateTime.Now.Ticks - ((PlayerData)data [data.Count - 1]).time.Ticks).TotalMinutes;
	}


	public void Clear()
	{
		patstr.Clear ();
		Save ();
	}


	// Explicit static constructor to tell C# compiler
	// not to mark type as beforefieldinit
	void Awake ()
	{
		if (manager == null) 
		{
			if (!LoadData ()) {
				patstr = new PatternsStruct ();
				initTypes ();
			}

			DontDestroyOnLoad (gameObject);
			manager = this;
		} else if (manager != this) 
		{
			Destroy (gameObject);
		}

	}
	public void PropagateSpoken()
	{
		try {
			if(Application.platform==RuntimePlatform.Android){								
				AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
				spoken=jo.Call<string>("getstr");
			}else{
				//Application.LoadLevel("Scene_03");
			}
		} catch (Exception e) {
			spoken = e.ToString().Substring(0,250);
		}
	}
	public void Recognize()
	{
		try {
			if(Application.platform==RuntimePlatform.Android){								
				AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
				jo.Call("StartActivity0");
			}else{
				//Application.LoadLevel("Scene_03");
			}
		} catch (Exception e) {
			spoken = e.ToString();
		}
	}
	public void Save() 
	{
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.persistentDataPath + "/patterns.dat", FileMode.Create);
		bf.Serialize (file, patstr);
		file.Close ();
		/*if (patternsToRecognize.Count > 0) {
			file = File.Open (Application.persistentDataPath + "/patterns_to_recognize.dat", FileMode.Create);
			bf.Serialize (file, patternsToRecognize);
		} else {
			file = File.Open (Application.persistentDataPath + "/all_types.dat", FileMode.Create);
			bf.Serialize (file, types);
			file.Close ();
		}*/
		/*file = File.Open (Application.persistentDataPath + "/all_patterns.dat", FileMode.Create);
		bf.Serialize (file, patterns);
		file.Close ();
		file = File.Open (Application.persistentDataPath + "/all_groups.dat", FileMode.Create);
		bf.Serialize (file, lowGroups);
		file.Close ();
		file.Close ();*/
	}
	public bool LoadData() 
	{
		Debug.Log (Application.persistentDataPath);
		if (File.Exists (Application.persistentDataPath + "/patterns.dat")) {
			FileStream file;
			BinaryFormatter bf = new BinaryFormatter ();
			file = File.Open (Application.persistentDataPath + "/patterns.dat", FileMode.Open);
			patstr = (PatternsStruct)bf.Deserialize (file);
			Debug.Log ("Number of types (" + Types.Count + "), groups(" + LowGroups.Count + "), patterns: " + Patterns.Count);
		} else
			return false;
		return true;
	}
}


/*
  		if (File.Exists (Application.persistentDataPath + "/patterns_to_recognize.dat")) {
			file = File.Open (Application.persistentDataPath + "/patterns_to_recognize.dat", FileMode.Open);
			patternsToRecognize = (List<Pattern>)bf.Deserialize (file);
			file.Close ();
		} else
			if (File.Exists (Application.persistentDataPath + "/all_types.dat")) {
				file = File.Open (Application.persistentDataPath + "/all_types.dat", FileMode.Open);
				types = (List<PatternClass>)bf.Deserialize (file);
				file.Close ();
			} else
				return false;
		if (patternsToRecognize.Count > 0) {
		}
		for (int i = 0; types.Count; i++) {
			if (types [i] is PatternImage)
				patterns.Add ((PatternImage)types [i]);
			else if (types [i] is PatternGroup)
				lowGroups.Add ((PatternGroup)types [i]);
		}
*/
/*File.Exists (Application.persistentDataPath + "/all_patterns.dat") &&
			File.Exists (Application.persistentDataPath + "/all_groups.dat") &&
			) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/all_patterns.dat", FileMode.Open);
			patterns = (List<PatternImage>)bf.Deserialize (file);
			file.Close ();

			file = File.Open (Application.persistentDataPath + "/all_groups.dat", FileMode.Open);
			lowGroups = (List<PatternGroup>)bf.Deserialize (file);
			file.Close ();*/
