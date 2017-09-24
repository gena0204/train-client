using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Game_5 : GameBase {

	private GameObject[] cards = new GameObject[9];
	private Color[] colors = new Color[2];

	private int[] indexs;

	private int currentCardSize = 3;
	private Queue<int> answerIndexQ = new Queue<int>();

	private bool isStart = false;

	private string lastQuestion = "";

	public Game_5() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		for (int i = 0; i < 9; i++) {
			int index = i;
			cards[i] = transform.FindChild("Panel/Button_" + (i + 1)).gameObject;
			cards[i].GetComponent<Button>().onClick.AddListener(delegate() {
				if (!isStart) return;
				audioManager.PlaySound((int)Define.Sound.Click);
				cards[index].GetComponent<Button>().interactable = false;
				// cards[index].GetComponent<Image>().color = colors[1];
				Answer(index);
			});
		}

		var gameData = SystemManager.Instance.GetGameData(UserInfo.Instance.Room.CurrentGameIndex);
		levelCondition = gameData.level;

		colors[0] = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		// colors[1] = new Color(169/255.0f, 207/255.0f, 234/255.0f, 1.0f);

		indexs = Enumerable.Range(0, 9).ToArray();
		SetLevel(0);
		CreateQuestion();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void CheckLevel() {
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
				currentCardSize = 4;
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
		isStart = false;

		foreach (var card in cards) {
			card.GetComponent<Button>().interactable = false;
			card.GetComponent<Image>().color = Color.clear;
		}

		// 亂數, 不要連續出現兩題一樣的題目
		while (true) {
			indexs = indexs.OrderBy(n => System.Guid.NewGuid()).ToArray();

			var str = "";
			for (int i = 0; i < currentCardSize; i++) {
				str += indexs[i].ToString();
			}

			if (str != lastQuestion) {
				lastQuestion = str;
				break;
			}
		}

		answerIndexQ.Clear();

		for (int i = 0; i < currentCardSize; i++) {
			int cardIndex = indexs[i];
			answerIndexQ.Enqueue(cardIndex);

			var card = cards[cardIndex];

			//card.GetComponent<Button>().interactable = true;
			card.GetComponent<Image>().color = colors[0];

			UnityEngine.Events.UnityAction callback = null;
			if ((i + 1) == currentCardSize) {
				callback = delegate() {
					foreach (var index in answerIndexQ) {
						cards[index].GetComponent<Button>().interactable = true;
					}
					isStart = true;
				};
			}
				
			Utils.Instance.DelayPlayAnimation(0.2f * (i+1), card.GetComponent<Animation>(), callback, 0.0f, "card_fadein");

			question += cardIndex + 1;
		}
	}

	private void Answer(int index) {
		reaction += index + 1;

		if (answerIndexQ.Dequeue() == index) {
			if (answerIndexQ.Count() == 0) {
				Game.self.Right();
				SaveQuestion();
				CheckLevel();
				CreateQuestion();
			}
		} else {
			Game.self.Wrong();
			SaveQuestion();
			CreateQuestion();
		}
	}

	public override void GameOver() {
		if (reaction != "") {
			Game.self.Next(true);
		}
	}
}
