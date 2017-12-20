using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_2 : GameBase {

	class ColorInfo {
		public string text = "";
		public string hex = "";
		public Color color = new Color();

		public string textCode = "";
		public string colorCode = "";

		public ColorInfo() {}

		public ColorInfo(string t, string h, string tc, string cc) {
			text = t; hex = h;
     		ColorUtility.TryParseHtmlString(h, out color);
			textCode = tc; colorCode = cc;
		}
	}
	
	private GameObject[] cards = new GameObject[4];
	private Text[] cardTexts = new Text[4];
	private Image[] cardImages = new Image[4];
	
	private List<ColorInfo> colorInfos = new List<ColorInfo>();
	

	private int currentCardSize = 2;
	private int answerIndex = 0;
	private ColorInfo answerColorInfo = new ColorInfo();
	private List<ColorInfo> questionColorList = new List<ColorInfo>();


	public Game_2() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		for (int i = 0; i < 4; i++) {
			int index = i;
			cards[i] = transform.Find("Panel/Panel_" + (i / 2 + 1) + "/Button_" + (i + 1)).gameObject;
			cards[i].GetComponent<Button>().onClick.AddListener(delegate() {
				audioManager.PlaySound((int)Define.Sound.Click);
				Answer(index);
			});
			cardTexts[i] = cards[i].transform.Find("Text").GetComponent<Text>();
			cardImages[i] = cards[i].transform.Find("Image").GetComponent<Image>();
		}

		var gameData = SystemManager.Instance.GetGameData(UserInfo.Instance.Room.CurrentGameIndex);

		var colors = gameData.colors.Length >= 5 ? gameData.colors : new string[] {
			"#F46464", "#2F98D6", "#E8DF59", "#31B478", "#2F2F2F"
		};
		var texts = gameData.texts.Length >= 10 ? gameData.texts : new string[] {
			"紅", "藍", "黃", "綠", "黑",
			"RED", "BLUE", "YELLOW", "GREEN", "BLACK",
		};

		var lang = PlayerPrefs.GetString(Define.PP_Language, "Chinese") == "Chinese" ? 0 : 5;
		
		colorInfos.Add(new ColorInfo(texts[lang], colors[0], "R", "R"));
		colorInfos.Add(new ColorInfo(texts[lang+1], colors[1], "B", "B"));
		colorInfos.Add(new ColorInfo(texts[lang+2], colors[2], "Y", "Y"));
		colorInfos.Add(new ColorInfo(texts[lang+3], colors[3], "G", "G"));
		colorInfos.Add(new ColorInfo(texts[lang+4], colors[4], "K", "K"));

		if (lang == 5) {
			foreach (var text in cardTexts) {
				text.fontSize = 60;
			}
		}

		levelCondition = gameData.level;
		
		SetLevel(0);
		CreateQuestion();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void CheckLevel() {
		if (level < 2 && (Game.self.rightCount % levelCondition) == 0) {
			SetLevel(level+1);
		}
	}

	private void SetLevel(int l) {
		level = l;

		switch (level) {
			case 0:
				currentCardSize = 2;
				break;

			case 1:
				transform.Find("Panel/Panel_2").gameObject.SetActive(true);
				currentCardSize = 3;
				break;

			case 2:
				currentCardSize = 4;
				break;

			default:
				break;
		}

		for (int i = 0; i < currentCardSize; i++) {
			cards[i].SetActive(true);
		}

		levelValue = currentCardSize;
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();
		
		reaction = "";

		answerIndex = rand.Next(currentCardSize);

		// 每題每格方塊顏色皆須不同，只有一個正確答案
		int index = rand.Next(colorInfos.Count);
		int index2 = rand.Next(colorInfos.Count);
		while (index == index2) {
			index2 = rand.Next(colorInfos.Count);
		}
		answerColorInfo.text = colorInfos[index].text;
		answerColorInfo.color = colorInfos[index2].color;
		answerColorInfo.textCode = colorInfos[index].textCode;

		questionColorList.Clear();
		for (int i = 0; i < colorInfos.Count; i++) {
			if (i != index && i != index2) {
				questionColorList.Add(colorInfos[i]);
			}
		}

		ColorInfo info;
		for (int i = 0; i < currentCardSize; i++) {
			if (i == answerIndex) {
				info = answerColorInfo;
			} else {
				index = rand.Next(questionColorList.Count);
				info = questionColorList[index];
				questionColorList.RemoveAt(index);
			}
			cardImages[i].color = info.color;
			cardTexts[i].text = info.text;
			cards[i].name = info.textCode;
		}

		type = colorInfos[index2].colorCode;
		question = answerColorInfo.textCode;
	}

	private void Answer(int index) {
		reaction += cards[index].name;

		if (answerIndex == index) {
			var success = Game.self.Right();
			SaveQuestion();
			if (success) CheckLevel();
			CreateQuestion();
		} else {
			Game.self.Wrong();
		}
	}

	public override void GameOver() {
		Game.self.Next(true, Game.self.GetReactionCount() > 0);
	}
}
