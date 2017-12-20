using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBase : MonoBehaviour {

	protected System.Random rand = new System.Random();

	protected int levelCondition = 0;

	protected int level = 0;
	protected int levelValue = 0;
	protected string type = ""; // 類型 
	protected string question = ""; // 題目
	protected string reaction = ""; // 反應 

	public string Reaction {
		get { return reaction; }
	}

	public GameBase() {
		Game.SetGame(this);
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	protected virtual void CreateQuestion() {
		Game.self.StartQuestion();
	}

	protected virtual void SaveQuestion() {
		Game.self.Next();
	}

	public virtual JSONObject CreateHistory() {
		var json = new JSONObject();
		json.AddField("level",   		level); // 難度
		json.AddField("level_value",   	levelValue); // 難度
		json.AddField("type",   		type); // 類型 
		json.AddField("question",   	question); // 題目
		json.AddField("reaction",   	reaction); // 反應
		return json;
	}

	public virtual void GameOver() {

	}
}
