using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_10 : GameBase {

	private GameObject questionObj;
	private Text questionText;
	private int answerIndex = 0;
	private string lastQuestion = "";


	public Game_10() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		for (int i = 0; i < 4; i++) {
			int index = i;
			transform.FindChild("Button_" + (i+1)).GetComponent<Button>().onClick.AddListener(delegate() {
				audioManager.PlaySound((int)Define.Sound.Click);
				Answer(index);
			});
		}

		questionObj = transform.FindChild("Text").gameObject;
		questionText = questionObj.GetComponent<Text>();

		CreateQuestion();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();

		reaction = "";
		
		answerIndex = rand.Next(4);

		int num1 = rand.Next(9) + 1;
		int num2 = rand.Next(9) + 1;
		int num3 = 0;

		switch (answerIndex) {
			case 0: // 加
				num3 = num1 + num2;
				break;
			case 1: // 減
				num3 = num1 - num2;
				break;
			case 2: // 乘
				num3 = num1 * num2;
				break;
			case 3: // 除
				num3 = rand.Next(9) + 1;
				num2 = rand.Next(8) + 2;
				num1 = num2 * num3;
				break;
		}

		// 檢查答案是否有兩種
		for (int i = 0; i < 4; i++) {
			if (i == answerIndex) {
				continue;
			}
			switch (i) {
				case 0: // 加
					if ((num1 + num2) == num3) {
						CreateQuestion();
						return;
					}
					break;
				case 1: // 減
					if ((num1 - num2) == num3) {
						CreateQuestion();
						return;
					}
					break;
				case 2: // 乘
					if ((num1 * num2) == num3) {
						CreateQuestion();
						return;
					}
					break;
				case 3: // 除
					if ((num1 / num2) == num3) {
						CreateQuestion();
						return;
					}
					break;
			}
		}

		string str = string.Format("{0}{1}{2}{3}", num1, num2, num3, answerIndex);
		if (lastQuestion == str) {
			CreateQuestion();
			return;
		}
		lastQuestion = str;

		questionText.text = string.Format("{0}  ▢  {1}  =  {2}", num1, num2, num3);
		Utils.Instance.PlayAnimation(questionObj.GetComponent<Animation>(), null, 0.0f, "fadein_up");

		question = (answerIndex + 1).ToString(); // 題目- 加-1, 減-2, 乘-3, 除-4 
	}

	private void Answer(int index) {
		reaction += (index + 1).ToString(); // 題目- 加-1, 減-2, 乘-3, 除-4 

		if (answerIndex == index) {
			Game.self.Right();
			SaveQuestion();
			CreateQuestion();
		} else {
			Game.self.Wrong();
		}
	}

	public override void GameOver() {
		Game.self.Next(true, false);
	}
}
