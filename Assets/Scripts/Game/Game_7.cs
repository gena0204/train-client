using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Game_7 : GameBase {

	private GameObject startButton;

	private GameObject[] cards = new GameObject[4];
	private GameObject[] cardTops = new GameObject[4];
	private Color[] colors = new Color[4];

	private int[] colorIndexs;

	private int count = 0;
	private int currentCardSize = 3;
	private int currentColorSize = 2;
	private int[] answerIndexs;
	private int[] questionIndexs;

	private bool isStart = false;

	private float rememberTime;
	private string rememberCodes;

	private string[] colorCodes = new string[] {"R", "B", "Y", "G"}; // 桃紅R/天藍B/鵝黃Y/蘋果綠G


	public Game_7() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		for (int i = 0; i < 4; i++) {
			int index = i;
			cardTops[i] = transform.Find("Panel_Top/Image_" + (i + 1)).gameObject;
			cards[i] = transform.Find("Panel/Button_" + (i + 1)).gameObject;
			cards[i].GetComponent<Button>().onClick.AddListener(delegate() {
				if (!isStart) return;
				audioManager.PlaySound((int)Define.Sound.Click);
				cards[index].GetComponent<Button>().interactable = false;
				cards[index].GetComponent<Image>().color = colors[questionIndexs[index]];
				Utils.Instance.PlayAnimation(cards[index].GetComponent<Animation>(), "card_fadein");
				Answer(index);
			});
		}

		startButton = transform.Find("Button_Start").gameObject;
		startButton.GetComponent<Button>().onClick.AddListener(delegate() {
			audioManager.PlaySound((int)Define.Sound.Click);
			rememberTime = (int)((Time.time - rememberTime) * 1000);
			startButton.SetActive(false);
			ShowTop();
		});

		var gameData = SystemManager.Instance.GetGameData(UserInfo.Instance.Room.CurrentGameIndex);

		var hexs = gameData.colors.Length >= 4 ? gameData.colors : new string[] {
			"#F46464", "#2F98D6", "#31B478", "#2F2F2F",
		}; // 桃紅R/天藍B/鵝黃Y/蘋果綠G

		for (int i = 0; i < 4; i++) {
			colors[i] = new Color();
			ColorUtility.TryParseHtmlString(hexs[i], out colors[i]);
			colors[i].a = 0.0f;
		}

		levelCondition = gameData.level;
		colorIndexs = Enumerable.Range(0, 4).ToArray();

		HideTop();
		SetLevel(0);
		CreateQuestion();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void CheckLevel() {
		if (level < 3 && (Game.self.rightCount % levelCondition) == 0) {
			SetLevel(level+1);
		}
	}

	private void SetLevel(int l) {
		level = l;

		switch (level) {
			case 0:
				currentCardSize = 3;
				currentColorSize = 2;
				break;

			case 1:
				currentCardSize = 3;
				currentColorSize = 3;
				break;

			case 2:
				currentCardSize = 4;
				currentColorSize = 3;
				transform.Find("Panel").GetComponent<GridLayoutGroup>().constraintCount = 2;
				break;

			case 3:
				currentCardSize = 4;
				currentColorSize = 4;
				break;

			default:
				break;
		}
		
		answerIndexs = new int[currentCardSize];
		questionIndexs = new int[currentCardSize];

		for (int i = 0; i < currentCardSize; i++) {
			cards[i].SetActive(true);
			cardTops[i].SetActive(true);
		}

		levelValue = currentCardSize;
	}

	private void ShowTop() {
		for (int i = 0; i < currentCardSize; i++) {
			var card = cardTops[i];

			card.GetComponent<Image>().color = colors[answerIndexs[i]];

			UnityEngine.Events.UnityAction callback = null;
			if ((i + 1) == currentCardSize) {
				callback = delegate() {
					isStart = true;
				};
			}
				
			Utils.Instance.DelayPlayAnimation(0.3f * i, card.GetComponent<Animation>(), callback, 0.0f, "card_fadein");
		}

		foreach (var card in cards) {
			card.GetComponent<Image>().color = Color.gray;
		}

		startButton.SetActive(false);
	}

	private void HideTop() {
		foreach (var card in cardTops) {
			card.GetComponent<Image>().color = Color.clear;
		}
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();

		type = currentCardSize == currentColorSize ? "S" : "D"; // 顏色數與方塊數相同- S,  差一種- D
		
		question = "";
		reaction = "";
		rememberCodes = "";
		count = 0;
		isStart = false;

		// 亂數
		colorIndexs = colorIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
		for (int i = 0; i < currentCardSize; i++) {
			answerIndexs[i] = colorIndexs[i < currentColorSize ? i : rand.Next(currentColorSize)];
		}
		answerIndexs = answerIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
		questionIndexs = answerIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();

		for (int i = 0; i < currentCardSize; i++) {
			var card = cards[i];

			card.GetComponent<Button>().interactable = true;
			card.GetComponent<Image>().color = colors[questionIndexs[i]];

			Utils.Instance.PlayAnimation(card.GetComponent<Animation>(), "card_fadein");

			rememberCodes += colorCodes[questionIndexs[i]];
		}

		startButton.SetActive(true);

		rememberTime = Time.time;

		string indexs = "";
		for (int i = 0; i < answerIndexs.Length; i++) {
			for (int j = 0; j < questionIndexs.Length; j++) {
				var index = j + 1;
				if (answerIndexs[i] == questionIndexs[j] && !indexs.Contains(index.ToString())) {
					indexs += index;
					break;
				}
			}
		}
		question += indexs;

		if (currentCardSize > currentColorSize) {
			indexs = "";
			for (int i = 0; i < answerIndexs.Length; i++) {
				for (int j = questionIndexs.Length-1; j >= 0; j--) {
					var index = j + 1;
					if (answerIndexs[i] == questionIndexs[j] && !indexs.Contains(index.ToString())) {
						indexs += index;
						break;
					}
				}
			}
			question += "," + indexs;
		}
	}

	private void Answer(int index) {
		reaction += index + 1;

		if (answerIndexs[count] == questionIndexs[index]) {
			Utils.Instance.PlayAnimation(cardTops[count].GetComponent<Animation>(), "fadeout");

			count++;
			if (count == currentCardSize) {
				Game.self.Right();
				SaveQuestion();
				CheckLevel();
				CreateQuestion();
			}
		} else {
			Game.self.Wrong();
			HideTop();
			SaveQuestion();
			CreateQuestion();
		}
	}

	public override JSONObject CreateHistory() {
		var json = new JSONObject();
		json.AddField("level",   		level); // 難度
		json.AddField("level_value",   	levelValue); // 難度
		json.AddField("type",   		type); // 類型
		json.AddField("remember",   	rememberCodes); // 記憶 
		json.AddField("question",   	question); // 題目
		json.AddField("reaction",   	reaction); // 反應
		json.AddField("remember_ms",   	rememberTime); // 記憶時間 
		return json;
	}

	public override void GameOver() {
		if (startButton.activeSelf) {
			rememberTime = 0;
		}
		Game.self.Next(true, false);
	}
}
