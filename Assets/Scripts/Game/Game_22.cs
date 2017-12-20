using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Game_22 : GameBase {

	class WordInfo {
		public string word;
		public string[] chars = new string[4];
		public string desc;
		public WordInfo(string w, string d) {
			word = w; desc = d;
			var c = word.ToCharArray();
			for (int i = 0; i < 4; i++) {
				chars[i] = c[i].ToString();
			}
		}
	}

	private Color[] colors = new Color[2];
	private Color[] bgColors = new Color[2];

	private Text descText;
	private Text[] topTexts = new Text[4];
	private Text[] boxTexts = new Text[15];
	private Button[] topButtons = new Button[4];
	private Button[] boxButtons = new Button[15];
	private Button tipButton;

	private List<WordInfo>[] wordLists = new List<WordInfo>[3];

	private int[] boxIndexs;
	private int currentTopIndex = 0;
	private string boxChars = "";
	private int typeIndex = 0;

	private List<string> questionIndexList = new List<string>();
	private int currentQuestionIndex = 0;

	private int[] userAnswerIndexs = new int[4];
	private int[] answerIndexs = new int[4];
	private List<int[]> answerSameIndexList = new List<int[]>();
	private string tips = "";

	private string[] typeCodes = new string[] {"H", "M", "L"}; // 類型- H-高頻 M-中頻 L-低頻 


	public Game_22() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		// color
		var hexs = new string[] { "#0A113D", "#007BFF" };
		for (int i = 0; i < 2; i++) {
			colors[i] = new Color();
			ColorUtility.TryParseHtmlString(hexs[i], out colors[i]);
		}
		hexs = new string[] { "#AEE0FF", "#F3E43A" };
		for (int i = 0; i < 2; i++) {
			bgColors[i] = new Color();
			ColorUtility.TryParseHtmlString(hexs[i], out bgColors[i]);
		}

		descText = transform.Find("Text_Desc").GetComponent<Text>();
		for (int i = 0; i < 4; i++) {
			int index = i;
			var btn = transform.Find("Button_" + (i+1));
			topTexts[i] = btn.Find("Text").GetComponent<Text>();
			topButtons[i] = btn.GetComponent<Button>();
			topButtons[i].onClick.AddListener(delegate() {
				if (topTexts[index].text == "" || topTexts[index].color == colors[1]) {
					return;
				}
				audioManager.PlaySound((int)Define.Sound.Click);
				boxTexts[userAnswerIndexs[index]].text = topTexts[index].text;
				topTexts[index].text = "";
				userAnswerIndexs[index] = -1;

				for (int j = 0; j < 4; j++) {
					if (userAnswerIndexs[j] == -1) {
						currentTopIndex = j;
						break;
					}
				}
			});
		}
		for (int i = 0; i < 15; i++) {
			int index = i;
			var btn = transform.Find("Panel/Button_" + (i+1));
			boxTexts[i] = btn.Find("Text").GetComponent<Text>();
			boxButtons[i] = btn.GetComponent<Button>();
			boxButtons[i].onClick.AddListener(delegate() {
				if (boxTexts[index].text == "" || currentTopIndex == -1) {
					return;
				}
				audioManager.PlaySound((int)Define.Sound.Click);
				Answer(index);
			});
		}

		tipButton = transform.Find("Button_Tip").GetComponent<Button>();
		tipButton.onClick.AddListener(delegate() {
			if (tips.Length >= 2) {
				return;
			}
			audioManager.PlaySound((int)Define.Sound.Click);

			// 清空錯字
			for (int i = 0; i < 4; i++) {
				if (userAnswerIndexs[i] != -1 && userAnswerIndexs[i] != answerIndexs[i]) {
					boxTexts[userAnswerIndexs[i]].text = topTexts[i].text;
					topTexts[i].text = "";
					userAnswerIndexs[i] = -1;
				}
			}

			int index = 0; // 顯示成語的第1個字(藍字) 
			if (tips.Length > 0) { // 隨機顯示第1個以外的字(藍字)
				// 如果只剩一個空格, 隨機其他已填寫的格子
				int emptyIndex = 0;
				for (int i = 1; i < 4; i++) {
					if (userAnswerIndexs[i] == -1) {
						emptyIndex = emptyIndex > 0 ? 0 : i;
					}
				}
				do {
					index = rand.Next(1, 4);
				} while (index == emptyIndex);
				// index = rand.Next(1, 4);
			}
			topTexts[index].color = colors[1];
			if (userAnswerIndexs[index] != answerIndexs[index]) {
				topTexts[index].text = boxTexts[answerIndexs[index]].text;
				boxTexts[answerIndexs[index]].text = "";
				userAnswerIndexs[index] = answerIndexs[index];
			}
			tips += (index+1);

			if (tips.Length >= 2) {
				tipButton.interactable = false;
			}

			for (int i = 0; i < 4; i++) {
				if (userAnswerIndexs[i] == -1) {
					currentTopIndex = i;
					return;
				}
			}

			// 無空格, 直接答對
			Answer(-1);
		});
		
		var gameData = SystemManager.Instance.GetGameData(UserInfo.Instance.Room.CurrentGameIndex);
		var words = new JSONObject(gameData.dataJSON);
		if (words == null || !words.IsArray || words.list.Count == 0) {
			MessagePanel.ShowMessage(Lang.Instance.getString("no_gamedata"), delegate() {
                HomePanel.panelIndex = 1; // 回訓練頁面
				Game.self.Exit();
            });
			return;
		}
		for (int i = 0; i < 3; i++) {
			var list = new List<WordInfo>();
			var index = 0;
			foreach (var key in words.list[i].keys) {
				list.Add(new WordInfo(key, words.list[i][key].str));
				questionIndexList.Add(i + "-" + index);
				index++;
			}
			wordLists[i] = list;
		}

		boxIndexs = Enumerable.Range(0, 15).ToArray();
		questionIndexList = questionIndexList.OrderBy(n => System.Guid.NewGuid()).ToList();

		CreateQuestion();
	}
	
	// Update is called once per frame
	void Update () {
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();

		question = "";
		reaction = "";
		tips = "";
		boxChars = "";
		userAnswerIndexs = Enumerable.Repeat(-1, 4).ToArray();
		answerSameIndexList.Clear();
		currentTopIndex = 0;

		tipButton.interactable = true;
		foreach (var btn in topButtons) {
			btn.interactable = true;
			btn.GetComponent<Image>().color = bgColors[0];
		}
		foreach (var btn in boxButtons) {
			btn.interactable = true;
		}

		// 不重複題目
		// int tIndex, wordIndex;
		// do {
		// 	tIndex = rand.Next(3);
		// 	wordIndex = rand.Next(wordLists[tIndex].Count);
		// } while (lastQuestion == (tIndex+""+wordIndex));
		// typeIndex = tIndex;
		// lastQuestion = tIndex+""+wordIndex;
		var numbers = questionIndexList[currentQuestionIndex].Split('-').Select(Int32.Parse).ToArray();
		typeIndex = numbers[0];
		int wordIndex = numbers[1];
		currentQuestionIndex = currentQuestionIndex >= questionIndexList.Count ? 0 : (currentQuestionIndex+1);

		boxIndexs = boxIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
		var info = wordLists[typeIndex][wordIndex];
		var otherChar = "";

		for (int i = 0; i < 4; i++) {
			answerIndexs[i] = boxIndexs[i];
			boxTexts[boxIndexs[i]].text = info.chars[i];
			boxChars += info.chars[i];
			topTexts[i].text = "";
			topTexts[i].color = colors[0];

			// 找出成語中相同文字
			int sameIndex = 0;
			for (int j = i; j < 3; j++) {
				if (info.chars[i] == info.chars[j+1]) {
					sameIndex = j+1;
					break;
				}
			}
			if (sameIndex > 0) {
				var indexs = new int[4];
				for (int k = 0; k < 4; k++) {
					indexs[k] = boxIndexs[k];
				}
				var index = indexs[i];
				indexs[i] = indexs[sameIndex];
				indexs[sameIndex] = index;
				answerSameIndexList.Add(indexs);
			}
		}
		for (int i = 4; i < 15; i++) {
			do {
				otherChar = wordLists[typeIndex][rand.Next(wordLists[typeIndex].Count)].chars[rand.Next(4)];
			} while (boxChars.Contains(otherChar));
			boxTexts[boxIndexs[i]].text = otherChar;
			boxChars += otherChar;
		}

		descText.text = info.desc;
		question = info.word;
		type = typeCodes[typeIndex];
	}

	private void Answer(int index) {
		if (index != -1) {
			topTexts[currentTopIndex].text = boxTexts[index].text;
			boxTexts[index].text = "";
			userAnswerIndexs[currentTopIndex] = index;

			for (int i = 0; i < 4; i++) {
				if (userAnswerIndexs[i] == -1) {
					currentTopIndex = i;
					return;
				}
			}
		}
		currentTopIndex = -1;

		var success = Enumerable.SequenceEqual(answerIndexs, userAnswerIndexs);
		if (!success && answerSameIndexList.Count > 0) {
			foreach (var indexs in answerSameIndexList) {
				success = Enumerable.SequenceEqual(indexs, userAnswerIndexs);
				if (success) {
					break;
				}
			}
		}

		if (success) {
			Game.self.Right();
			SaveQuestion();
			question = "";
			reaction = "";

			tipButton.interactable = false;
			foreach (var btn in topButtons) {
				btn.interactable = false;
				btn.GetComponent<Image>().color = bgColors[1];
			}
			foreach (var btn in boxButtons) {
				btn.interactable = false;
			}

			Utils.Instance.StartDelayProcess(1.5f, delegate() {
				CreateQuestion();
			});
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
		json.AddField("param_1",   		tips.Length == 0 ? "0" : tips); // 提示 
		return json;
	}

	public override void GameOver() {
		if (reaction == "" && question != "") {
			Game.self.Next(true, false);
		}
	}
}
