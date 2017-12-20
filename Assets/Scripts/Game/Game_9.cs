using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_9 : GameBase {

	private Text numberText;
	private int[] numbers = new int[3] {-1, -1, -1};
	private int answerIndex = 0;

	public Game_9() : base() {
    }

	void OnDestroy() {
		if (transform) {
			var num = transform.Find("Button_Number");
			if (num) {
				Utils.Instance.StopAnimation(num.GetComponent<Animation>());
			}
		}
	}

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		var leftButton = transform.Find("Button_Left").GetComponent<Button>();
		leftButton.interactable = false;
		leftButton.onClick.AddListener(delegate() {
			audioManager.PlaySound((int)Define.Sound.Click);
			Answer(0);
		});
		var rightButton = transform.Find("Button_Right").GetComponent<Button>();
		rightButton.interactable = false;
		rightButton.onClick.AddListener(delegate() {
			audioManager.PlaySound((int)Define.Sound.Click);
			Answer(1);
		});

		var numberObj = transform.Find("Button_Number");
		numberText = numberObj.Find("Text").GetComponent<Text>();

		var numberBtn = numberObj.GetComponent<Button>();
		numberBtn.interactable = false;

		CreateQuestion();
		type = "*";
		reaction = "*";
		Game.self.Next(true, false);

		Utils.Instance.PlayAnimation(numberObj.GetComponent<Animation>(), "card_fadein", delegate() {
			CreateQuestion();
			leftButton.interactable = true;
			rightButton.interactable = true;
			Utils.Instance.PlayAnimation(numberObj.GetComponent<Animation>(), "card_fadein");
		}, 0.7f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();

		reaction = "";
		
		numbers[2] = numbers[1];
		numbers[1] = numbers[0];

		do {
			numbers[0] = rand.Next(100);
		} while(numbers[0] == numbers[1] || numbers[0] == numbers[2]);

		answerIndex = numbers[0] < numbers[1] ? 0 : 1;
		numberText.text = numbers[0].ToString();
		
		type = answerIndex == 0 ? "L" : "R"; // 比前一數字大者為R，小者為L
		question = numbers[0].ToString();
	}

	private void Answer(int index) {
		reaction += index == 0 ? "L" : "R"; // 比前一數字大者為R，小者為L

		if (answerIndex == index) {
			Game.self.Right();
			SaveQuestion();
			CreateQuestion();
		} else {
			Game.self.Wrong();
		}
	}

	public override void GameOver() {
		if (reaction != "") {
			Game.self.Next(true);
		}
	}
}
