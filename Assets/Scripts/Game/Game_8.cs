using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Game_8 : GameBase {

	private GameObject startButton;

	private Sprite[] sprites;
	private GameObject[] cards = new GameObject[6];
	private Image[] cardImages = new Image[6];
	private Button[] cardButtons = new Button[6];
	private Text[] cardTexts = new Text[6];
	private string[] texts = new string[6];
	private int[] textIndexs;
	private int count = 0;
	private int reactionCount = 0;
	private int currentCardIndex = -1;
	private string[] questionTexts = new string[6];

	private float rememberTime;
	private int[] counts = new int[] {-1, -1, -1};


	public Game_8() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;
		Utils utils = Utils.Instance;

		sprites = new Sprite[] {
			Resources.Load<Sprite>("Sprites/game_8_card_back"),
			Resources.Load<Sprite>("Sprites/game_8_card_front")
		};

		for (int i = 0; i < 6; i++) {
			int index = i;
			cards[i] = transform.FindChild("Panel/Button_" + (i + 1)).gameObject;
			cards[i].GetComponent<Button>().onClick.AddListener(delegate() {
				audioManager.PlaySound((int)Define.Sound.Click);
				cards[index].GetComponent<Button>().interactable = false;
				utils.PlayAnimation(cards[index].GetComponent<Animation>(), delegate() {
					cardImages[index].sprite = sprites[1];
					cardTexts[index].text = questionTexts[index];
					utils.PlayAnimation(cards[index].GetComponent<Animation>(), delegate() {
						Answer(index);
					}, 0.0f, "card_rotation_2");
				}, 0.0f, "card_rotation_1");
			});

			cardImages[i] = cards[i].transform.GetComponent<Image>();
			cardButtons[i] = cards[i].transform.GetComponent<Button>();
			cardTexts[i] = cards[i].transform.FindChild("Text").GetComponent<Text>();
		}

		startButton = transform.FindChild("Button_Start").gameObject;
		startButton.GetComponent<Button>().onClick.AddListener(delegate() {
			audioManager.PlaySound((int)Define.Sound.Click);
			rememberTime = (int)((Time.time - rememberTime) * 1000);
			startButton.SetActive(false);
			StartGame();
		});

		var gameData = SystemManager.Instance.GetGameData(UserInfo.Instance.Room.CurrentGameIndex);
		texts = gameData.texts.Length >= 6 ? gameData.texts : new string[] {
			"✚", "◆", "○", "★", "❖", "Δ"
		};

		textIndexs = Enumerable.Range(0, texts.Count()).ToArray();
		CreateQuestion();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void StartGame() {
		for (int i = 0; i < 6; i++) {
			CloseCard(i);
		}

		startButton.SetActive(false);
	}

	private void CloseCard(int index) {
		Utils.Instance.PlayAnimation(cards[index].GetComponent<Animation>(), delegate() {
			cardImages[index].sprite = sprites[0];
			cardTexts[index].text = "";
			Utils.Instance.PlayAnimation(cards[index].GetComponent<Animation>(), delegate() {
				cards[index].GetComponent<Button>().interactable = true;
			}, 0.0f, "card_rotation_2");
		}, 0.0f, "card_rotation_1");
	}

	private void HideCard(int index) {
		Utils.Instance.PlayAnimation(cards[index].GetComponent<Animation>(), null, 0.0f, "card_fadeout");
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();
		
		question = "";
		reaction = "";
		counts[0] = -1; counts[1] = -1; counts[2] = -1;
		reactionCount = 0;
		count = 0;

		// 亂數
		textIndexs = textIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
		for (int i = 0; i < 3; i++) {
			questionTexts[i*2] = texts[textIndexs[i]];
			questionTexts[i*2+1] = texts[textIndexs[i]];
		}

		questionTexts = questionTexts.OrderBy(n => System.Guid.NewGuid()).ToArray();
		for (int i = 0; i < 6; i++) {
			Utils.Instance.StopAnimation(cards[i].GetComponent<Animation>());
			cards[i].GetComponent<CanvasGroup>().alpha = 1;
			cardTexts[i].text = questionTexts[i];
			cardButtons[i].interactable = false;
		}

		startButton.SetActive(true);

		rememberTime = Time.time;

		for (int i = 0; i < questionTexts.Length; i++) {
			for (int j = 0; j < questionTexts.Length; j++) {
				if (question.Contains((j + 1).ToString())) {
					continue;
				} else if (i != j && questionTexts[i] == questionTexts[j]) {
					if (question != "") question += ",";
					question += (i + 1) + "" + (j + 1);
				}
			}
		}
	}

	private void Answer(int index) {
		if (currentCardIndex == -1) {
			currentCardIndex = index;
			return;
		}

		if (reaction != "") reaction += ",";
		reaction += (currentCardIndex + 1) + "" + (index + 1);

		reactionCount++;

		if (questionTexts[currentCardIndex] == questionTexts[index]) {
			counts[count] = reactionCount;
			count++;

			if (count == 3) {
				Game.self.Right();
				SaveQuestion();
				CreateQuestion();
			} else {
				HideCard(index);
				HideCard(currentCardIndex);
			}
		} else {
			CloseCard(index);
			CloseCard(currentCardIndex);
			Game.self.Wrong();
		}

		currentCardIndex = -1;
	}

	public override JSONObject CreateHistory() {
		var json = new JSONObject();
		json.AddField("level",   		level); // 難度
		json.AddField("level_value",   	levelValue); // 難度
		json.AddField("type",   		type); // 類型
		json.AddField("question",   	question); // 位置
		json.AddField("reaction",   	reaction); // 反應
		json.AddField("remember_ms",   	rememberTime); // 記憶時間
		json.AddField("count_1",   		counts[0]); // 答題狀況一
		json.AddField("count_2",   		counts[1]); // 答題狀況二
		json.AddField("count_3",   		counts[2]); // 答題狀況三
		return json;
	}
}
