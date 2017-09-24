using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_3 : GameBase {

	class ColorInfo {
		public Color color;
		public string code;
		public ColorInfo(Color cr, string c) {
			color = cr; code = c;
		}
	}

	class SpriteInfo {
		public Sprite sprite;
		public string code;
		public SpriteInfo(Sprite s, string c) {
			sprite = s; code = c;
		}
	}
	
	private GameObject[] cardTops = new GameObject[2];
	private GameObject[] cards = new GameObject[5];
	private Image[] cardImages = new Image[5];

	private List<ColorInfo> colorList = new List<ColorInfo>();
	private List<SpriteInfo> spriteList = new List<SpriteInfo>();

	private List<ColorInfo> topColorList = new List<ColorInfo>();
	private List<SpriteInfo> topSpriteList = new List<SpriteInfo>();

	private List<string> questionList = new List<string>();

	private int currentCardSize = 2;
	private int answerIndex = 0;
	private int questionType = 0;
	private string rightCode = "";
	private string wrongCode = "";


	public Game_3() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		for (int i = 0; i < 2; i++) {
			cardTops[i] = transform.FindChild("Panel/Panel_Top/Button_" + (i + 1)).gameObject;
		}
		for (int i = 0; i < 5; i++) {
			int index = i;
			cards[i] = transform.FindChild("Panel/Panel_" + (i / 3 + 1) + "/Button_" + (i + 1)).gameObject;
			cards[i].GetComponent<Button>().onClick.AddListener(delegate() {
				audioManager.PlaySound((int)Define.Sound.Click);
				Answer(index);
			});
			cardImages[i] = cards[i].transform.FindChild("Image").GetComponent<Image>();
		}

		var gameData = SystemManager.Instance.GetGameData(UserInfo.Instance.Room.CurrentGameIndex);

		var hexs = gameData.colors.Length >= 5 ? gameData.colors : new string[] {
			"#F46464", "#2F98D6", "#31B478", "#2F2F2F", "#E8DF59"
		}; // 桃紅 R／天藍B／ 鵝黃Y／蘋果綠G / 紫色P
		string[] colorCodes = new string[] {
			"R", "B", "Y", "G", "P"
		}; // 桃紅 R／天藍B／ 鵝黃Y／蘋果綠G / 紫色P
		for (int i = 0; i < hexs.Length; i++) {
			Color color = new Color();
			ColorUtility.TryParseHtmlString(hexs[i], out color);
			colorList.Add(new ColorInfo(color, colorCodes[i]));
		}

		levelCondition = gameData.level;
		type = questionType == 0 ? "S" : "D"; // 相同-S, 不同-D
		SetLevel(0);

		if (gameData.images.Length >= 5) {
			string[] names = gameData.images;
			for (int i = 0; i < names.Length; i++) {
				string code = (i+1).ToString();
				Utils.Instance.LoadImage(names[i], delegate(Sprite sprite) {
					spriteList.Add(new SpriteInfo(sprite, code));

					if (code == "5") {
						CreateQuestion();
					}
				});
			}
		} else {
			string[] names = new string[] {
				"game_3_shape_1", "game_3_shape_2", "game_3_shape_3", "game_3_shape_4", "game_3_shape_5"
			};
			for (int i = 0; i < names.Length; i++) {
				spriteList.Add(new SpriteInfo(Resources.Load<Sprite>("Sprites/" + names[i]), (i+1).ToString()));
			}

			CreateQuestion();
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void CheckLevel() {
		if (Game.self.rightCount > 0 && Game.self.rightCount % 3 == 0) {
			questionType = (questionType + 1) % 2;
			type = questionType == 0 ? "S" : "D"; // 相同-S, 不同-D
		}
		if (Game.self.rightCount == levelCondition) {
			SetLevel(1);
		}
	}

	private void SetLevel(int l) {
		level = l;

		switch (level) {
			case 0:
				currentCardSize = 3;
				break;

			case 1:
				cardTops[1].SetActive(true);
				transform.FindChild("Panel/Panel_2").gameObject.SetActive(true);
				currentCardSize = 5;
				break;

			default:
				break;
		}

		levelValue = currentCardSize;
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();

		question = "";
		reaction = "";
		rightCode = "";
		wrongCode = "";

		string key;
		Image image;
		ColorInfo colorInfo;
		SpriteInfo spriteInfo;
		
		answerIndex = rand.Next(currentCardSize);

		foreach (var color in topColorList) {
			colorList.Add(color);
		}
		foreach (var sprite in topSpriteList) {
			spriteList.Add(sprite);
		}
		topColorList.Clear();
		topSpriteList.Clear();
		
		for (int i = 0; i <= level; i++) {
			int index = rand.Next(colorList.Count);
			topColorList.Add(colorList[index]);
			colorList.RemoveAt(index);

			index = rand.Next(spriteList.Count);
			topSpriteList.Add(spriteList[index]);
			spriteList.RemoveAt(index);

			image = cardTops[i].transform.FindChild("Image").GetComponent<Image>();
			colorInfo = topColorList[i];
			spriteInfo = topSpriteList[i];
			image.color = colorInfo.color;
			image.sprite = spriteInfo.sprite;

			question += colorInfo.code + spriteInfo.code;
		}

		questionList.Clear();

		for (int i = 0; i < currentCardSize; i++) {
			if (i == answerIndex) {
				if (questionType == 0) { // 完全一樣
					int index = rand.Next(topColorList.Count);
					colorInfo = topColorList[index];
					spriteInfo = topSpriteList[index];
				} else { // 完全不一樣
					colorInfo = colorList[rand.Next(colorList.Count)];
					spriteInfo = spriteList[rand.Next(spriteList.Count)];
				}

				key = colorInfo.code + spriteInfo.code;
				rightCode = key;
			} else {
				if (questionType == 1 || rand.Next(2) == 0) { // 一個上一個下
					if (rand.Next(2) == 0) {
						colorInfo = topColorList[rand.Next(topColorList.Count)];
						spriteInfo = spriteList[rand.Next(spriteList.Count)];
					} else {
						colorInfo = colorList[rand.Next(colorList.Count)];
						spriteInfo = topSpriteList[rand.Next(topSpriteList.Count)];
					}
				} else { // 兩個下
					colorInfo = colorList[rand.Next(colorList.Count)];
					spriteInfo = spriteList[rand.Next(spriteList.Count)];
				}

				key = colorInfo.code + spriteInfo.code;
				if (questionList.Contains(key)) {
					i--; continue;
				}
				questionList.Add(key);
			}

			cardImages[i].color = colorInfo.color;
			cardImages[i].sprite = spriteInfo.sprite;
			// cardImages[i].SetNativeSize();

			cards[i].name = key;
		}
	}

	private void Answer(int index) {
		reaction += cards[index].name;
		int i = reaction.Length-2;
		int j = reaction.Length-1;

		if (answerIndex == index) {
			var success = Game.self.Right();
			SaveQuestion();
			if (success) CheckLevel();
			CreateQuestion();
		} else {
			Game.self.Wrong();

			// 顏色錯誤-C,  形狀錯誤-S,  兩者皆錯-B
			if (reaction[i] != rightCode[0] && reaction[j] != rightCode[1]) {
				wrongCode += "B"; 
			} else if (reaction[i] != rightCode[0]) {
				wrongCode += "C"; 
			} else {
				wrongCode += "S"; 
			}
		}
	}

	public override JSONObject CreateHistory() {
		var json = new JSONObject();
		json.AddField("level",   		level); // 難度
		json.AddField("level_value",   	levelValue); // 難度
		json.AddField("type",   		type); // 類型
		json.AddField("question",   	question); // 題目
		json.AddField("right",   		rightCode); // 正解 
		json.AddField("reaction",   	reaction); // 反應
		json.AddField("wrong",   		wrongCode == "" ? "*" : wrongCode); // 錯誤類型 
		return json;
	}

	public override void GameOver() {
		Game.self.Next(true, Game.self.GetReactionCount() > 0);
	}
}
