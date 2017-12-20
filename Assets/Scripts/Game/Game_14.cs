using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Game_14 : GameBase {

	private Color[] colors = new Color[2];
	private GameObject[] cards = new GameObject[9];
	private Image[] cardImages = new Image[9];
	private Text[] cardTexts = new Text[9];
	private string[] texts = new string[9];
	private int[] textIndexs;
	private int currentCardSize = 6;
	private int currentCardIndex = -1;
	private string[] questionTexts = new string[6];



	public Game_14() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		for (int i = 0; i < 9; i++) {
			int index = i;
			cards[i] = transform.Find("Panel/Button_" + (i + 1)).gameObject;
			cards[i].GetComponent<Button>().onClick.AddListener(delegate() {
				audioManager.PlaySound((int)Define.Sound.Click);
				cards[index].GetComponent<Button>().interactable = false;
				Answer(index);
			});

			cardImages[i] = cards[i].transform.GetComponent<Image>();
			cardTexts[i] = cards[i].transform.Find("Text").GetComponent<Text>();
		}

		colors[0] = new Color(67/255.0f, 99/255.0f, 244/255.0f);
		colors[1] = new Color(231/255.0f, 86/255.0f, 39/255.0f);

		texts = new string[] {"1", "2", "3", "4", "5", "6", "7", "8"};
		textIndexs = Enumerable.Range(0, texts.Count()).ToArray();

		var gameData = SystemManager.Instance.GetGameData(UserInfo.Instance.Room.CurrentGameIndex);
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
				currentCardSize = 6;
				break;

			case 1:
				currentCardSize = 9;
				questionTexts = new string[currentCardSize];
				for (int i = 6; i < currentCardSize; i++) {
					cards[i].SetActive(true);
				}
				break;

			default:
				break;
		}

		levelValue = currentCardSize == 6 ? 23 : 33;
	}

	private void EnableCard(bool enable) {
		for (int i = 0; i < 6; i++) {
			cards[i].GetComponent<Button>().interactable = enable;
		}
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();

		question = "";
		reaction = "*";

		// 亂數
		int i = 0;
		textIndexs = textIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
		for (i = 0; i < currentCardSize-1; i++) {
			questionTexts[i] = texts[textIndexs[i]];
		}
		questionTexts[i] = questionTexts[i-1];

		questionTexts = questionTexts.OrderBy(n => System.Guid.NewGuid()).ToArray();
		for (i = 0; i < currentCardSize; i++) {
			cardImages[i].color = colors[0];
			cardTexts[i].text = questionTexts[i];
			cards[i].GetComponent<Button>().interactable = true;
		}

		for (i = 0; i < currentCardSize; i++) {
			for (int j = 0; j < currentCardSize; j++) {
				if (i == j) continue;
				if (questionTexts[i] == questionTexts[j]) {
					question = (i+1) + "" + (j+1);
					return;
				}
			}
		}
	}

	private void Answer(int index) {
		if (currentCardIndex == -1) {
			currentCardIndex = index;
			cardImages[index].color = colors[1];
			return;
		}

		if (reaction == "*") {
			reaction = "";
		} else if (reaction != "") {
			reaction += ",";
		}
		reaction += (currentCardIndex + 1) + "" + (index + 1);

		cardImages[index].color = colors[0];
		cardImages[currentCardIndex].color = colors[0];
		cards[index].GetComponent<Button>().interactable = true;
		cards[currentCardIndex].GetComponent<Button>().interactable = true;

		if (questionTexts[currentCardIndex] == questionTexts[index]) {
			var success = Game.self.Right();
			SaveQuestion();
			if (success) CheckLevel();
			CreateQuestion();
		} else {
			Game.self.Wrong();
		}

		currentCardIndex = -1;
	}

	public override void GameOver() {
		if (Game.self.GetReactionCount() == 0) {
			Game.self.Next(true, false);
		}
	}

	private IEnumerator Wait(float delayTime, UnityAction callback) {
        yield return new WaitForSeconds(delayTime);
        callback();
    }
}
