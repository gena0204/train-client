using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_21 : GameBase {

	class WordInfo {
		public string key;
		public List<string> words = new List<string>();
	}

	private Text[] texts = new Text[2];
	private Image[] textBGImages = new Image[2];
	private GameObject[] textQMarks = new GameObject[2];
	
	private Transform[] bubbles = new Transform[3];
	private Animation[] bubbleAnims = new Animation[3];
	private Text[] bubbleTexts = new Text[3];
	private Image[] bubbleImages = new Image[3];
	private Button[] bubbleButtons = new Button[3];

	private Color[] colors = new Color[2];
	private Sprite[] sprites = new Sprite[2];
	private Vector2Int xRange = new Vector2Int();

	private List<WordInfo> wordList = new List<WordInfo>();
	private List<string> questionHistoryList = new List<string>();

	private int topIndex = 0;
	private int answerIndex = 0;

	public Game_21() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		string[] names = new string[] {
			"game2_7_bubble_blue", "game2_7_bubble_green"
		};
		for (int i = 0; i < 2; i++) {
			sprites[i] = Resources.Load<Sprite>("Sprites/" + names[i]);
			texts[i] = transform.Find("Text_" + (i+1)).GetComponent<Text>();
			textBGImages[i] = transform.Find("Image_Top_" + (i+1)).GetComponent<Image>();
			textQMarks[i] = textBGImages[i].transform.Find("Image").gameObject;
		}
		for (int i = 0; i < 3; i++) {
			int index = i;
			bubbles[i] = transform.Find("Panel_" + (i+1));
			bubbleAnims[i] = bubbles[i].Find("Button").GetComponent<Animation>();
			bubbleImages[i] = bubbleAnims[i].transform.GetComponent<Image>();
			bubbleTexts[i] = bubbleAnims[i].transform.Find("Text").GetComponent<Text>();
			bubbleButtons[i] = bubbleAnims[i].GetComponent<Button>();
			bubbleButtons[i].onClick.AddListener(delegate() {
				audioManager.PlaySound((int)Define.Sound.Click);
				Answer(index);
			});
		}

		colors[0] = new Color(60/255.0f, 204/255.0f, 251/255.0f);
		colors[1] = new Color(249/255.0f, 83/255.0f, 96/255.0f);

		xRange.x = (int)bubbles[0].position.x;
		xRange.y = (int)bubbles[2].position.x;

		var gameData = SystemManager.Instance.GetGameData(UserInfo.Instance.Room.CurrentGameIndex);
		var words = new JSONObject(gameData.dataJSON);
		if (words == null || words.keys.Count == 0) {
			MessagePanel.ShowMessage(Lang.Instance.getString("no_gamedata"), delegate() {
                HomePanel.panelIndex = 1; // 回訓練頁面
				Game.self.Exit();
            });
			return;
		}
		foreach (var key in words.keys) {
			var info = new WordInfo();
			info.key = key;
			foreach (var word in words[key].list) {
				info.words.Add(word.str);
			}
			wordList.Add(info);
		}
		
		CreateQuestion();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();

		question = "";
		reaction = "";

		foreach (var btn in bubbleButtons) {
			btn.interactable = true;
		}

		topIndex = rand.Next(2);
		answerIndex = rand.Next(3);

		// 12/18: 題目不可重複
		var keyIndex = 0;
		var wordIndex = 0;
		do {
			keyIndex = rand.Next(wordList.Count);
			wordIndex = rand.Next(wordList[keyIndex].words.Count);
		} while (questionHistoryList.Contains(keyIndex + "-" + wordIndex));
		questionHistoryList.Add(keyIndex + "-" + wordIndex);

		var otherKeyIndex = 0;
		var otherWordIndex = 0;
		var textList = new List<string>();

		switch (topIndex) {
			case 0: // 量詞
				question = wordList[keyIndex].words[wordIndex];
				texts[0].text = "";
				texts[1].text = question;
				texts[1].color = Color.white;
				textBGImages[0].gameObject.SetActive(true);
				textBGImages[1].gameObject.SetActive(false);
				textQMarks[0].SetActive(true);

				for (int i = 0; i < 3; i++) {
					if (i == answerIndex) {
						bubbleTexts[i].text = wordList[keyIndex].key;
					} else {
						do {
							do {
								otherKeyIndex = rand.Next(wordList.Count);
							} while (otherKeyIndex == keyIndex || wordList[otherKeyIndex].words.Contains(question));
							bubbleTexts[i].text = wordList[otherKeyIndex].key;
						} while (textList.Contains(bubbleTexts[i].text));
					}
					textList.Add(bubbleTexts[i].text);
				}
				break;

			case 1: // 字詞
				question = wordList[keyIndex].key;
				texts[0].text = question;
				texts[1].text = "";
				texts[0].color = Color.white;
				textBGImages[0].gameObject.SetActive(false);
				textBGImages[1].gameObject.SetActive(true);
				textQMarks[1].SetActive(true);

				for (int i = 0; i < 3; i++) {
					if (i == answerIndex) {
						bubbleTexts[i].text = wordList[keyIndex].words[wordIndex];
					} else {
						do {
							do {
								otherKeyIndex = rand.Next(wordList.Count);
							} while (otherKeyIndex == keyIndex);
							otherWordIndex = rand.Next(wordList[otherKeyIndex].words.Count);
							bubbleTexts[i].text = wordList[otherKeyIndex].words[otherWordIndex];
						} while (textList.Contains(bubbleTexts[i].text) || wordList[keyIndex].words.Contains(bubbleTexts[i].text));
					}
					textList.Add(bubbleTexts[i].text);
				}
				break;
		}

		for (int i = 0; i < 3; i++) {
			var pos = bubbles[i].position;
			pos.x = rand.Next(xRange.x, xRange.y);
			bubbles[i].position = pos;

			bubbleImages[i].sprite = sprites[topIndex];

			bubbleAnims[i].transform.localPosition = Vector3.zero;
			Utils.Instance.DelayPlayAnimation(rand.Next(10)/10.0f, bubbleAnims[i], null, 0, "bubble");
		}
		
		type = topIndex == 0 ? "c" : "n"; // 類型-c:量詞 n:字詞 
	}

	private void Answer(int index) {
		if (reaction != "") reaction += ",";
		reaction += bubbleTexts[index].text;

		textQMarks[topIndex].SetActive(false);
		texts[topIndex].text = bubbleTexts[index].text;

		if (answerIndex == index) {
			Game.self.Right();
			SaveQuestion();

			foreach (var btn in bubbleButtons) {
				btn.interactable = false;
			}

			texts[topIndex].color = colors[0];
			Utils.Instance.StartDelayProcess(0.5f, delegate() {
				CreateQuestion();
			});
		} else {
			Game.self.Wrong();
			texts[topIndex].color = colors[1];
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
		if (reaction == "") {
			Game.self.Next(true, false);
		}
	}
}
