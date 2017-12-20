using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_19 : GameBase {

	private Image[] images = new Image[3];
	private Sprite[] sprites = new Sprite[3];

	private GameObject[] rightImages = new GameObject[2];
	private GameObject[] wrongImages = new GameObject[2];

	private List<int> typeList = new List<int>();
	private int answerIndex = 0;

	private string[] answerCodes = new string[] {"L", "R"};
	private string[] typeCodes = new string[] {"N", "C", "I"};


	public Game_19() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		for (int i = 0; i < 2; i++) {
			rightImages[i] = transform.Find("Image_Right_" + (i+1)).gameObject;
			wrongImages[i] = transform.Find("Image_Wrong_" + (i+1)).gameObject;
		}

		transform.Find("Button_Left").GetComponent<Button>().onClick.AddListener(delegate() {
			audioManager.PlaySound((int)Define.Sound.Click);
			Answer(0);
		});
		transform.Find("Button_Right").GetComponent<Button>().onClick.AddListener(delegate() {
			audioManager.PlaySound((int)Define.Sound.Click);
			Answer(1);
		});

		string[] names = new string[] {
			"game2_5_item_1", "game2_5_item_2", "game2_5_item_3"
		};
		for (int i = 0; i < 3; i++) {
			images[i] = transform.Find("Image_" + (i+1)).GetComponent<Image>();
			sprites[i] = Resources.Load<Sprite>("Sprites/" + names[i]);
		}

		typeList.Add(-1);
		typeList.Add(-1);
		CreateQuestion();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();

		reaction = "";

		// 三種題目類型隨機出現，但避免連續三題都是同樣類型 
		int typeIndex = 0;
		do {
			typeIndex = rand.Next(3);
		} while (typeList[0] == typeList[1] && typeList[0] == typeIndex);

		answerIndex = rand.Next(2);

		switch (typeIndex) {
			case 0: // 圓右圓 圓左圓
				images[0].sprite = sprites[2];
				images[1].sprite = sprites[answerIndex];
				images[2].sprite = sprites[2];
				break;
			case 1: // 右右右 左左左
				images[0].sprite = sprites[answerIndex];
				images[1].sprite = sprites[answerIndex];
				images[2].sprite = sprites[answerIndex];
				break;
			case 2: // 左右左 右左右
				var otherIndex = (answerIndex+1) % 2;
				images[0].sprite = sprites[otherIndex];
				images[1].sprite = sprites[answerIndex];
				images[2].sprite = sprites[otherIndex];
				break;
		}
		
		question = answerCodes[answerIndex];
		type = typeCodes[typeIndex];
		typeList.Add(typeIndex);
		typeList.RemoveAt(0);

		for (int i = 0; i < 3; i++) {
			Utils.Instance.PlayAnimation(images[i].gameObject.GetComponent<Animation>());
		}
	}

	private void Answer(int index) {
		if (reaction != "") { return; }
		reaction = answerCodes[index];

		if (answerIndex == index) {
			Game.self.Right();
			rightImages[answerIndex].SetActive(true);
		} else {
			Game.self.Wrong();
			wrongImages[answerIndex].SetActive(true);
		}

		SaveQuestion();

		Utils.Instance.StartDelayProcess(0.5f, delegate() {
			rightImages[answerIndex].SetActive(false);
			wrongImages[answerIndex].SetActive(false);

			// 12/5: 每題之間請加入短暫的間隔（作答後回饋結束，插入短暫的空白畫面，再出現下一題）
			for (int i = 0; i < 3; i++) {
				images[i].gameObject.SetActive(false);
			}
			Utils.Instance.StartDelayProcess(0.3f, delegate() {
				for (int i = 0; i < 3; i++) {
					images[i].gameObject.SetActive(true);
				}
				CreateQuestion();
			});
		});
	}

	public override JSONObject CreateHistory() {
		var json = new JSONObject();
		json.AddField("level",   		level); // 難度
		json.AddField("level_value",   	levelValue); // 難度
		json.AddField("type",   		type); // 類型
		json.AddField("question",   	question); // 題目
		json.AddField("reaction",   	reaction); // 反應
		return json;
	}

	public override void GameOver() {
		if (reaction == "") {
			Game.self.Next(true, false);
		}
	}
}
