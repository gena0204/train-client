using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SystemManager : Singleton<SystemManager> {

    private Dictionary<string, string> valueDict = new Dictionary<string, string>();

	private GameData[] gameDatas = new GameData[14];
	private List<string> sloganList = new List<string>();

	private System.Random rand = new System.Random();


	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Release() {
		valueDict.Clear();
	}

    //----------------------------------------
    // System Data
    //----------------------------------------
    public bool HasValue(string key) {
		return valueDict.ContainsKey(key);
	}

    public void SetValue(string key, string value) {
		valueDict.Add(key, value);
	}

    public string GetValue(string key) {
		return valueDict.ContainsKey(key) ? valueDict[key] : null;
	}

    public int GetInt(string key, int defaultValue = 0) {
		return valueDict.ContainsKey(key) ? int.Parse(valueDict[key]) : 0;
	}

	public float GetFloat(string key, float defaultValue = 0) {
		return valueDict.ContainsKey(key) ? float.Parse(valueDict[key]) : 0;
	}

	//----------------------------------------
    // Game Data
    //----------------------------------------
	public void SetGameData(GameData data) {
		if (data.index < 0 || data.index >= gameDatas.Count()) {
			return;
		}
		gameDatas[data.index] = data;
	}

	public GameData GetGameData(int index) {
		if (index < 0 || index >= gameDatas.Count()) {
			return new GameData();
		}
		return gameDatas[index];
	}

	//----------------------------------------
    // Slogan
    //----------------------------------------
	public void AddSlogan(string slogan) {
		sloganList.Add(slogan);
	}

	public string GetSlogan() {
		if (sloganList.Count == 0) {
			return "";
		}
		return sloganList[rand.Next(sloganList.Count)];
	}
}

public class GameData {
    public int index 		= 0;
    public string caption 	= "";
    public int level;
    public string[] colors;
    public string[] texts;
	public string[] images;
}
