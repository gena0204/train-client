using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Game_11 : GameBase {

	private GameObject[] cards = new GameObject[16];
	private Color[] colors = new Color[3];
	private int[] cardIndexs;
	private int[] colorIndexs;
	private int[] colorSizes;
	private int currentCardSize = 9;
	private int currentColorSize = 2;
	private int currentMaxSize = 0;
	private int currentMinSize = 0;
	private int answerColorIndex = 0;
	private int[] questionColorIndexs = new int[16];
	
	private string lastQuestion = "";

	private string[] colorCodes = new string[] {"R", "Y", "B"}; // 桃紅R/鵝黃Y/天藍Ｂ

	public Game_11() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		for (int i = 0; i < 16; i++) {
			int index = i;
			cards[i] = transform.FindChild("Panel/Button_" + (i + 1)).gameObject;
			cards[i].GetComponent<Button>().onClick.AddListener(delegate() {
				audioManager.PlaySound((int)Define.Sound.Click);
				Answer(index);
			});
		}

		var gameData = SystemManager.Instance.GetGameData(UserInfo.Instance.Room.CurrentGameIndex);

		var hexs = gameData.colors.Length >= 3 ? gameData.colors : new string[] {
			"#F46464", "#2F98D6", "#31B478",
		}; // 桃紅R/天藍B/鵝黃Y

		for (int i = 0; i < 3; i++) {
			colors[i] = new Color();
			ColorUtility.TryParseHtmlString(hexs[i], out colors[i]);
			colors[i].a = 0.0f;
		}

		levelCondition = gameData.level;
		colorIndexs = Enumerable.Range(0, hexs.Length).ToArray();

		SetLevel(0);
		CreateQuestion();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void CheckLevel() {
		if (level < 4 && (Game.self.rightCount % levelCondition) == 0) {
			SetLevel(level+1);
		}
	}

	// 最簡單的題目中，最多的顏色格數：
	// Level 1: 方塊數量為3x3塊，顏色數量為2種。（6格）
	// Level 2: 方塊數量為3x3塊，顏色數量為3種。（5格）
	// Level 3: 方塊數量為4x4塊，顏色數量為2種。（10格）
	// Level 4: 方塊數量為4x4塊，顏色數量為3種。（7格）
	private void SetLevel(int l) {
		level = l;

		for (int i = 0; i < 16; i++) {
			cards[i].SetActive(false);
		}

		switch (level) {
			case 0:
				currentCardSize = 9;
				currentColorSize = 2;
				currentMaxSize = 6;
				transform.FindChild("Panel").GetComponent<GridLayoutGroup>().constraintCount = 3;
				break;

			case 1:
				currentCardSize = 9;
				currentColorSize = 3;
				currentMaxSize = 5;
				break;

			case 2:
				currentCardSize = 16;
				currentColorSize = 2;
				currentMaxSize = 10;
				transform.FindChild("Panel").GetComponent<GridLayoutGroup>().constraintCount = 4;
				break;

			case 3:
				currentCardSize = 16;
				currentColorSize = 3;
				currentMaxSize = 7;
				break;

			default:
				break;
		}

		for (int i = 0; i < currentCardSize; i++) {
			cards[i].SetActive(true);
		}

		colorSizes = new int[currentColorSize];
		cardIndexs = Enumerable.Range(0, currentCardSize).ToArray();
		levelValue = (int)System.Math.Sqrt(currentCardSize) * 10 + currentColorSize;

		if (currentColorSize == 2) {
			currentMinSize = currentCardSize - currentMaxSize;
		} else {
			currentMinSize = currentCardSize - currentMaxSize;
			currentMinSize = currentMinSize > currentMaxSize ? (currentMinSize-(currentMaxSize-1)) : 1;
		}
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();

		type = "";
		question = "";
		reaction = "";

		cardIndexs = cardIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
		colorIndexs = colorIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();

		if (currentColorSize == 2) {
			while (true) {
				colorSizes[0] = rand.Next(currentMaxSize - currentMinSize) + 1 + currentMinSize;
				if (colorSizes[0] == (currentCardSize/2)) continue;
				colorSizes[1] = currentCardSize - colorSizes[0];
				break;
			}
		} else {
			while (true) {
				colorSizes[0] = rand.Next(currentMaxSize - currentMinSize) + 1 + currentMinSize;
				colorSizes[1] = rand.Next(currentCardSize - colorSizes[0] - currentMinSize) + 1;
				if (colorSizes[1] > currentMaxSize || colorSizes[0] == colorSizes[1]) continue;
				colorSizes[2] = currentCardSize - colorSizes[0] - colorSizes[1];
				if (colorSizes[2] > currentMaxSize || colorSizes[2] == colorSizes[0] || colorSizes[2] == colorSizes[1]) continue;
				break;
			}
		}

		System.Array.Sort(colorSizes);
		System.Array.Reverse(colorSizes);
		answerColorIndex = colorIndexs[0];		

		int index = 0;
		for (int i = 0; i < currentColorSize; i++) {
			for (int j = 0; j < colorSizes[i]; j++) {
				int cardIndex = cardIndexs[index];
				questionColorIndexs[cardIndex] = colorIndexs[i];
				cards[cardIndex].GetComponent<Image>().color = colors[colorIndexs[i]];
				Utils.Instance.PlayAnimation(cards[cardIndex].GetComponent<Animation>(), null, 0.0f, "card_fadein");
				index++;
			}
		}

		// 不要連續出現兩題一樣的題目
		string str = "";
		for (int i = 0; i < currentCardSize; i++) {
			str += questionColorIndexs[i].ToString();
		}
		if (str == lastQuestion) {
			CreateQuestion();
			return;
		} else {
			lastQuestion = str;
		}

		for (int i = currentColorSize-1; i >= 0; i--) {
			type += colorSizes[i];
			question += colorCodes[colorIndexs[i]];
		}
	}

	private void Answer(int index) {
		reaction += colorCodes[questionColorIndexs[index]];

		if (answerColorIndex == questionColorIndexs[index]) {
			var success = Game.self.Right();
			SaveQuestion();
			if (success) CheckLevel();
			CreateQuestion();
		} else {
			Game.self.Wrong();
		}
	}

	public override void GameOver() {
		Game.self.Next(true, false);
	}
}
