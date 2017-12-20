using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.Events;

public class Game_15 : GameBase {

	[SerializeField]
	private SpriteAtlas atlas;

	class CandyInfo {
		public int spriteIndex = 0;
		public int colorIndex = 0;
		public Button btn;
		public CandyInfo(Button b) {
			btn = b;
		}
	}

	private GameObject startButton;

	private List<CandyInfo> candyList = new List<CandyInfo>();
	private Vector3[] positions = new Vector3[9];
	private Sprite[][] sprites = new Sprite[4][];
	private List<string> answerCodeList = new List<string>();
	private Color clearColor;

	private int[] spriteIndexs;
	private int[] colorIndexs;
	private int[] posIndexs;

	private int currentCandySize = 2;
	private int currentOtherSize = 2;

	private bool isStart = false;

	private float rememberTime;
	private string otherCandy;
	private int reactionCount = 0;

	private string[] colorCodes =
		new string[] {"G", "g", "B", "b", "R", "r", "O", "o"}; // 綠色G,淺綠g,藍色B,淺藍b,紅色R,淺紅r,橘O,淺橘o


	public Game_15() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;
		Utils utils = Utils.Instance;


		for (int i = 0; i < 9; i++) {
			positions[i] = transform.Find("Panel/Image_" + (i + 1)).position;
		}

		startButton = transform.Find("Button_Start").gameObject;
		startButton.GetComponent<Button>().onClick.AddListener(delegate() {
			audioManager.PlaySound((int)Define.Sound.Click);
			rememberTime = (int)((Time.time - rememberTime) * 1000);
			startButton.SetActive(false);
			StartGame();
		});


		var gameData = SystemManager.Instance.GetGameData(UserInfo.Instance.Room.CurrentGameIndex);
		levelCondition = gameData.level;

		for (int i = 0; i < 4; i++) {
			sprites[i] = new Sprite[8];
			for (int j = 0; j < 8; j++) {
				sprites[i][j] = atlas.GetSprite("game2_1_shape_"+i+""+j);
			}
		}

		clearColor = Color.white;
		clearColor.a = 0;

		spriteIndexs = Enumerable.Range(0, sprites.Count()).ToArray();
		colorIndexs = Enumerable.Range(0, 8/2).ToArray();
		posIndexs = Enumerable.Range(0, positions.Count()).ToArray();
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
				currentCandySize = 2;
				currentOtherSize = 2;
				break;

			case 1:
				currentCandySize = 2;
				currentOtherSize = 3;
				break;

			case 2:
				currentCandySize = 3;
				currentOtherSize = 4;
				break;

			default:
				break;
		}

		var candy = Resources.Load<GameObject>("Prefabs/Button_Candy");
		for (int i = candyList.Count(); i < (currentCandySize+currentOtherSize); i++) {
			var go = Instantiate<GameObject>(candy);
			go.transform.SetParent(transform);
			go.transform.localScale = Vector3.one;
			
			var btn = go.GetComponent<Button>();
			btn.onClick.AddListener(delegate() {
				if (!isStart) {
					return;
				}
				AudioManager.Instance.PlaySound((int)Define.Sound.Click);
				btn.interactable = false;
				Answer(go.name);
			});

			candyList.Add(new CandyInfo(btn));
		}

		levelValue = currentCandySize * 10 + currentOtherSize;
	}

	private void StartGame() {
		int end = currentCandySize + currentOtherSize;
		UnityAction callback = delegate() {
			// 糖果位移 (不重複)
			int[] indexs = null;
			do {
				indexs = posIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
				for (int i = 0; i < end; i++) {
					if (indexs[i] == posIndexs[i]) {
						indexs = null;
						break;
					}
				}
			} while (indexs == null);
			posIndexs = indexs;

			UnityAction callback2 = delegate() {
				isStart = true;
				Game.self.StartQuestion();

				for (int i = 0; i < end; i++) {
					candyList[i].btn.enabled = true;
				}
			};
			
			for (int i = 0; i < end; i++) {
				UnityAction cb = i == (end-1) ? callback2 : null;
				Utils.Instance.MoveOverSeconds(candyList[i].btn.gameObject, positions[posIndexs[i]], 0.8f, cb);
			}
		};
		for (int i = currentCandySize; i < end; i++) {
			UnityAction cb = i == (end-1) ? callback : null;
			Utils.Instance.PlayAnimation(candyList[i].btn.gameObject.GetComponent<Animation>(), "card_fadein", cb);
		}
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();
		
		isStart = false;
		question = "";
		otherCandy = "";
		reaction = "";
		answerCodeList.Clear();
		reactionCount = 0;

		for (int i = 0; i < (currentCandySize+currentOtherSize); i++) {
			candyList[i].btn.enabled = false;
			candyList[i].btn.interactable = true;
		}

		int spriteIndex = 0;
		int colorIndex = 0;
		string code = "";

		// 圖案條件：
		// 1.隨機選擇最初n個不重複糖果形狀（數字表示1,2）
		// 2.隨機選擇最初n個不重複糖果顏色（大寫英文表示 B,G）
		// 3.隨機調整最初n個糖果顏色深淺（小寫英文表示 b,G）   
		spriteIndexs = spriteIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
		colorIndexs = colorIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
		posIndexs = posIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
		for (int i = 0; i < currentCandySize; i++) {
			spriteIndex = spriteIndexs[i];
			colorIndex = colorIndexs[i]*2+rand.Next(2);
			code = (spriteIndex+1) + "" + colorCodes[colorIndex];
			answerCodeList.Add(code);

			var image = candyList[i].btn.gameObject.GetComponent<Image>();
			image.sprite = sprites[spriteIndex][colorIndex];

			candyList[i].btn.gameObject.name = code;
			candyList[i].btn.transform.position = positions[posIndexs[i]];
			candyList[i].spriteIndex = spriteIndex;
			candyList[i].colorIndex = colorIndex;

			Utils.Instance.PlayAnimation(candyList[i].btn.gameObject.GetComponent<Animation>(), "card_fadein");

			if (question != "") question += ",";
			question += code;
		}

		// 其他糖果
		// 其他糖果形狀和顏色之搭配和最初糖果對調
		// ex 最初糖果：形狀1顏色b/形狀2顏色G; 其他糖果：形狀2顏色b/形狀1顏色G
		// 當其他糖果數筆最初糖果多時，選擇最初第一顆糖果作為多出的“顏色深淺混淆糖果”
		// ex 最初糖果：形狀1顏色G/形狀2顏色b/形狀3顏色R; 其他糖果：形狀3顏色G/形狀1顏色b/形狀2顏色R+形狀1顏色g 
		for (int i = currentCandySize; i < (currentCandySize+currentOtherSize); i++) {
			if (i >= (currentCandySize*2)) { // 混淆糖果
				spriteIndex = candyList[0].spriteIndex;
				colorIndex = candyList[0].colorIndex;
				colorIndex += (colorIndex%2 == 0) ? 1 : -1;
			} else {
				spriteIndex = candyList[(i+(currentCandySize-1))%currentCandySize].spriteIndex;
				colorIndex = candyList[i-currentCandySize].colorIndex;
			}
			code = (spriteIndex+1) + "" + colorCodes[colorIndex];

			var image = candyList[i].btn.gameObject.GetComponent<Image>();
			image.sprite = sprites[spriteIndex][colorIndex];
			image.color = clearColor;

			candyList[i].btn.gameObject.name = code;
			candyList[i].btn.transform.position = positions[posIndexs[i]];
			candyList[i].spriteIndex = spriteIndex;
			candyList[i].colorIndex = colorIndex;

			if (otherCandy != "") otherCandy += ",";
			otherCandy += code;
		}

		startButton.SetActive(true);
		rememberTime = Time.time;
	}

	private void Answer(string code) {
		if (reaction != "") reaction += ",";
		reaction += code;

		if (answerCodeList.Contains(code)) {
			reactionCount++;
			if (reactionCount == answerCodeList.Count()) {
				var success = Game.self.Right();
				SaveQuestion();
				if (success) CheckLevel();
				CreateQuestion();
			}
		} else {
			Game.self.Wrong();
			SaveQuestion();
			CreateQuestion();
		}
	}

	public override JSONObject CreateHistory() {
		var json = new JSONObject();
		json.AddField("level",   		level); // 難度
		json.AddField("level_value",   	levelValue); // 難度
		json.AddField("question",   	question); // 正解
		json.AddField("param_1",   		otherCandy); // 其他糖果
		json.AddField("reaction",   	reaction); // 反應
		json.AddField("remember_ms",   	(int)rememberTime); // 記憶時間
		return json;
	}

	public override void GameOver() {
		if (startButton.activeSelf == true) {
			rememberTime = 0;
		}
		Game.self.Next(true, reaction != "");
	}
}
