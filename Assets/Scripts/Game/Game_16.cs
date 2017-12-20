using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class Game_16 : GameBase {

	[SerializeField]
	private SpriteAtlas atlas;

	[SerializeField]
	private GameObject smoke;

	private GameObject trayPanel;
	private GameObject platePanel;
	private Image cover;
	private Transform smokePanel;

	private Sprite[] sprites = new Sprite[4];

	private Image[] questionCakeImages = new Image[10];
	private Image[] answerCakeImages = new Image[10];
	private GameObject[] answerWrongObjs = new GameObject[10];
	private Button[] answerBtns = new Button[10];

	private List<int> answerList = new List<int>();
	private int[] cakeIndexs;
	

	private int currentCakeMaxSize = 2;
	private int currentCakeMinSize = 1;
	private int currentHoleSize = 4;
	private int reactionCount = 0;



	public Game_16() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		var camera = GameObject.Find("Main Camera").GetComponent<Camera>();
		var canvas = GameObject.Find("Canvas_Game").GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
		canvas.worldCamera = camera;
		GameObject.Find("Canvas").GetComponent<Canvas>().worldCamera = camera;
		Camera.SetupCurrent(camera);

		sprites[0] = atlas.GetSprite("game2_2_bread_1");
		sprites[1] = atlas.GetSprite("game2_2_bread_2");
		sprites[2] = atlas.GetSprite("game2_2_bread_3");
		sprites[3] = atlas.GetSprite("game2_2_bread_4");

		trayPanel = transform.Find("Panel_1").gameObject;
		platePanel = transform.Find("Panel_2").gameObject;
		cover = trayPanel.transform.Find("Image_Cover").GetComponent<Image>();
		smokePanel = platePanel.transform.Find("Panel_Smoke");

		for (int i = 0; i < 10; i++) {
			int index = i;
			questionCakeImages[i] = transform.Find("Panel_1/Panel/Panel_" + (i / 2 + 1) + "/Image_" + (i + 1)).GetComponent<Image>();
			answerCakeImages[i] = transform.Find("Panel_2/Panel/Panel_" + (i / 2 + 1) + "/Image_" + (i + 1)).GetComponent<Image>();
			answerWrongObjs[i] = transform.Find("Panel_2/Panel/Panel_" + (i / 2 + 1) + "/Image_Wrong_" + (i + 1)).gameObject;
			answerBtns[i] = transform.Find("Panel_2/Panel/Panel_" + (i / 2 + 1) + "/Button_" + (i + 1)).gameObject.GetComponent<Button>();
			answerBtns[i].onClick.AddListener(delegate() {
				audioManager.PlaySound((int)Define.Sound.Click);
				answerBtns[index].interactable = false;
				Answer(index);
			});
		}

		var gameData = SystemManager.Instance.GetGameData(UserInfo.Instance.Room.CurrentGameIndex);
		levelCondition = gameData.level;

		cakeIndexs = Enumerable.Range(0, currentHoleSize).ToArray();
		EnableCake(false);
		SetLevel(0);
		CreateQuestion();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void CheckLevel() {
		if (Game.self.rightCount%levelCondition == 0) {
			SetLevel(level+1);
		}
	}

	private void SetLevel(int l) {
		level = l;

		switch (level) {
			case 0:
				currentCakeMaxSize = 2;
				currentCakeMinSize = 1;
				currentHoleSize = 4;
				break;

			case 1:
				currentCakeMaxSize = 3;
				currentCakeMinSize = 2;
				currentHoleSize = 6;
				break;

			case 2:
				currentCakeMaxSize = 4;
				currentCakeMinSize = 2;
				currentHoleSize = 8;
				break;

			case 3:
				currentCakeMaxSize = 4;
				currentCakeMinSize = 2;
				currentHoleSize = 10;
				break;

			default:
				break;
		}

		trayPanel.transform.Find("Panel/Panel_" + (currentHoleSize/2)).gameObject.SetActive(true);
		platePanel.transform.Find("Panel/Panel_" + (currentHoleSize/2)).gameObject.SetActive(true);

		trayPanel.GetComponent<Image>().sprite = atlas.GetSprite("game2_2_bake_x"+currentHoleSize+"_left");
		platePanel.GetComponent<Image>().sprite = atlas.GetSprite("game2_2_bake_x"+currentHoleSize+"_right");
		trayPanel.GetComponent<Image>().SetNativeSize();
		platePanel.GetComponent<Image>().SetNativeSize();

		cover.sprite = atlas.GetSprite("game2_2_bake_x"+currentHoleSize+"_back");
		cover.SetNativeSize();

		LayoutRebuilder.MarkLayoutForRebuild(trayPanel.transform.Find("Panel") as RectTransform);
		LayoutRebuilder.MarkLayoutForRebuild(platePanel.transform.Find("Panel") as RectTransform);

		cakeIndexs = Enumerable.Range(0, currentHoleSize).ToArray();
		levelValue = 20 + (currentHoleSize/2);
	}

	private void EnableCake(bool enable) {
		for (int i = 0; i < currentHoleSize; i++) {
			answerBtns[i].interactable = enable;
		}
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();
		
		reaction = "";
		answerList.Clear();
		reactionCount = 0;

		// 點擊數量為漸進式 不是隨機式
		// int size = currentCakeMinSize + rand.Next(currentCakeMaxSize - currentCakeMinSize + 1);
		currentCakeMinSize = currentCakeMinSize >= currentCakeMaxSize ? currentCakeMaxSize : currentCakeMinSize;
		int size = currentCakeMinSize;

		// 亂數
		while (true) {
			cakeIndexs = cakeIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
			for (int i = 0; i < size; i++) {
				answerList.Add(cakeIndexs[i]);
			}

			// 檢查是否連續出現相同題目
			answerList.Sort();
			var codes = string.Join(",", answerList.Select(x => (x+1).ToString()).ToArray());
			if (question != codes) {
				question = codes;
				break;
			}

			answerList.Clear();
		}
		for (int i = 0; i < size; i++) {
			questionCakeImages[cakeIndexs[i]].sprite = sprites[rand.Next(2)];
			questionCakeImages[cakeIndexs[i]].SetNativeSize();
			questionCakeImages[cakeIndexs[i]].gameObject.SetActive(true);
		}

		EnableCake(true);
	}

	private void Answer(int index) {
		if (reaction != "") reaction += ",";
		reaction += index + 1;

		var utils = Utils.Instance;

		if (answerList.Contains(index)) {
			reactionCount++;
			answerCakeImages[index].sprite = sprites[rand.Next(2)];
			answerCakeImages[index].SetNativeSize();
			answerCakeImages[index].gameObject.SetActive(true);

			if (reactionCount == answerList.Count()) {
				EnableCake(false);

				Game.self.Right(false);
				SaveQuestion();
				reaction = "";

				currentCakeMinSize++;

				utils.PlayAnimation(trayPanel.GetComponent<Animation>(), "tray_rotation_1", delegate() {
					cover.gameObject.SetActive(true);
					utils.PlayAnimation(trayPanel.GetComponent<Animation>(), "tray_rotation_2", delegate() {
						var go = Instantiate<GameObject>(smoke);
						go.transform.SetParent(smokePanel);
						go.transform.localScale = Vector3.one;
						go.transform.localPosition = Vector3.zero;

						utils.StartDelayProcess(0.5f, delegate() {
							AudioManager.Instance.PlaySound((int)Define.Sound.Right);
							
							foreach (var i in answerList) {
								answerCakeImages[i].transform.GetChild(0).gameObject.SetActive(false);
								answerCakeImages[i].sprite = sprites[2+rand.Next(2)];
								answerCakeImages[i].SetNativeSize();
								questionCakeImages[i].gameObject.SetActive(false);
							}
							utils.PlayAnimation(trayPanel.GetComponent<Animation>(), "tray_rotation_3", delegate() {
								cover.gameObject.SetActive(false);
								utils.PlayAnimation(trayPanel.GetComponent<Animation>(), "tray_rotation_4", delegate() {
									foreach (var i in answerList) {
										answerCakeImages[i].gameObject.SetActive(false);
										answerCakeImages[i].transform.GetChild(0).gameObject.SetActive(true);
									}
									CheckLevel();
									CreateQuestion();
								}, 0.5f);
							});
						});
					});
				});
			}
		} else {
			EnableCake(false);
			Game.self.Wrong();
			SaveQuestion();
			reaction = "";

			foreach (var i in answerList) {
				if (!answerCakeImages[i].gameObject.activeSelf) {
					answerWrongObjs[i].SetActive(true);
				}
			}

			utils.StartDelayProcess(2.0f, delegate() {
				foreach (var i in answerList) {
					questionCakeImages[i].gameObject.SetActive(false);
					answerCakeImages[i].gameObject.SetActive(false);
					answerWrongObjs[i].SetActive(false);
				}
				CreateQuestion();
			});
		}
	}

	public override JSONObject CreateHistory() {
		var json = new JSONObject();
		json.AddField("level",   		level); // 難度
		json.AddField("level_value",   	levelValue); // 難度
		json.AddField("type",   		answerList.Count().ToString()); // 需點擊位置數
		json.AddField("question",   	question); // 題目
		json.AddField("reaction",   	reaction); // 反應
		return json;
	}

	public override void GameOver() {
		if (reaction != "") {
			Game.self.Next(true);
		}
	}
}
