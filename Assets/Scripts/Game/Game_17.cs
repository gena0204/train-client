using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_17 : GameBase {

	private static object _lock = new object();

	private Transform textPanel;
	private GameObject wordPrefab;
	private Vector3[] positions = new Vector3[4];
	private Queue<GameObject> questionObjQ = new Queue<GameObject>();
	private Queue<GameObject> objQ = new Queue<GameObject>();
	private int answerIndex = -1;
	private int answerLastIndex;

	private List<List<string>[]> wordList = new List<List<string>[]>();

	private int[] questionIndexs = new int[3]; // 組別 近/反義 題庫
	private string lastQuestion = "";

	private string[] answerCodes = new string[] {"S", "N", "D"}; // 類型/反應- S(相似), D(相反), N(無關)
	


	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		textPanel = transform.Find("Panel");
		wordPrefab = Resources.Load<GameObject>("Prefabs/Text_Word");

		for (int i = 0; i < 4; i++) {
			positions[i] = transform.Find("Text_" + i).position;
		}

		for (int i = 0; i < 3; i++) {
			int index = i;
			transform.Find("Button_" + (i+1)).gameObject.GetComponent<Button>().onClick.AddListener(delegate() {
				audioManager.PlaySound((int)Define.Sound.Click);
				Answer(index);
			});
		}

		var lang = PlayerPrefs.GetString(Define.PP_Language, "Chinese") == "Chinese" ? 0 : 1;

		var gameData = SystemManager.Instance.GetGameData(UserInfo.Instance.Room.CurrentGameIndex);
		var words = new JSONObject(gameData.dataJSON);
		if (words == null || !words.IsArray || words.list.Count <= lang) {
			MessagePanel.ShowMessage(Lang.Instance.getString("no_gamedata"), delegate() {
                HomePanel.panelIndex = 1; // 回訓練頁面
				Game.self.Exit();
            });
			return;
		}
		foreach (var w in words.list[lang].list) {
			var list = new List<string>[2];
			for (int i = 0; i < 2; i++) {
				list[i] = new List<string>();
				foreach (var word in w[i].list) {
					list[i].Add(word.str);
				}
			}			
			wordList.Add(list);
		}

		CreateQuestion();
		type = "*";
		reaction = "*";
		Game.self.Next(true, false);

		Utils.Instance.StartDelayProcess(0.5f, delegate() {
			var go = questionObjQ.Peek();
			Utils.Instance.MoveOverSeconds(go, positions[2], 0.5f);
			CreateQuestion();
		});
	}

	// Update is called once per frame
	void Update () {
	}

	private GameObject GenerateObject() {
		lock (_lock) {
			GameObject go;
			if (objQ.Count == 0) {
				go = Instantiate<GameObject>(wordPrefab);
				go.transform.SetParent(textPanel);
				go.transform.localScale = Vector3.one;
				
			} else {
				go = objQ.Dequeue();
			}
			go.transform.position = positions[0];
			go.GetComponent<CanvasGroup>().alpha = 1;
			go.SetActive(true);
			return go;
		}
	}

	private void RecycleObject(GameObject go) {
		lock (_lock) {
			if (go && !objQ.Contains(go)) {
				go.SetActive(false);
				objQ.Enqueue(go);
			}
		}
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();

		// question = "";
		reaction = "";

		var go = GenerateObject();
		questionObjQ.Enqueue(go);
		
		if (questionObjQ.Count == 0) {
			questionIndexs[0] = rand.Next(wordList.Count);
			questionIndexs[1] = rand.Next(2);
			questionIndexs[2] = rand.Next(wordList[questionIndexs[0]][questionIndexs[1]].Count);
		} else {
			int index;
			string word;

			answerLastIndex = answerIndex;
			answerIndex = rand.Next(3);

			// 由於部分詞組受限於選擇條件只能列舉兩個詞，當同一組別只有兩個詞
			// 遇到”詞1 詞2 詞1 “的當機狀況時，改為選擇相反或無關的詞（相反和無關的機率各1/2）
			if (answerIndex == 0 && answerLastIndex == answerIndex && wordList[questionIndexs[0]][questionIndexs[1]].Count < 3) {
				answerIndex = rand.Next(2) + 1;
			}

			switch (answerIndex) {
				case 0: // 相似
					do {
						index = rand.Next(wordList[questionIndexs[0]][questionIndexs[1]].Count);
						word = wordList[questionIndexs[0]][questionIndexs[1]][index];
					} while (word == question || word == lastQuestion);
					questionIndexs[2] = index;
					break;

				case 1: // 無關
					do {
						index = rand.Next(wordList.Count);
					} while (index == questionIndexs[0]);
					questionIndexs[0] = index;
					questionIndexs[1] = rand.Next(2);
					do {
						index = rand.Next((wordList[questionIndexs[0]][questionIndexs[1]].Count));
						word = wordList[questionIndexs[0]][questionIndexs[1]][index];
					} while (word == lastQuestion);
					questionIndexs[2] = index;
					break;

				case 2: // 相反
					questionIndexs[1] = (questionIndexs[1] + 1) % 2;
					do {
						index = rand.Next(wordList[questionIndexs[0]][questionIndexs[1]].Count);
						word = wordList[questionIndexs[0]][questionIndexs[1]][index];
					} while (word == lastQuestion);
					questionIndexs[2] = index;
					break;
			}
		}

		type = answerCodes[answerIndex];
		lastQuestion = question;
		question = wordList[questionIndexs[0]][questionIndexs[1]][questionIndexs[2]];
		go.GetComponent<Text>().text = question;

		Utils.Instance.MoveOverSeconds(go, positions[1], 0.5f);
	}

	private void Answer(int index) {
		if (questionObjQ.Count < 2) return;

		reaction += answerCodes[index];

		if (answerIndex == index) {
			var go = questionObjQ.Dequeue();
			var status = 0;
			Utils.Instance.MoveOverSeconds(go, positions[3], 0.5f, delegate() {
				status++;
				if (status == 2) RecycleObject(go);
			});
			Utils.Instance.PlayAnimation(go.GetComponent<Animation>(), "card_fadeout", delegate() {
				status++;
				if (status == 2) RecycleObject(go);
			});

			Utils.Instance.MoveOverSeconds(questionObjQ.Peek(), positions[2], 0.5f);

			Game.self.Right();
			SaveQuestion();
			CreateQuestion();
		} else {
			Game.self.Wrong();
		}
	}

	public override JSONObject CreateHistory() {
		var json = new JSONObject();
		json.AddField("level",   		level); // 難度
		json.AddField("level_value",   	levelValue); // 難度
		json.AddField("type",   		type); // 類型
		json.AddField("question",   	question); // 題目
		json.AddField("reaction",   	reaction); // 反應
		return json;
	}

	public override void GameOver() {
		if (reaction != "") {
			Game.self.Next(true);
		}
	}
}
