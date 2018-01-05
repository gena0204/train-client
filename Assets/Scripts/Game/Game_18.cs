using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Game_18 : GameBase {

	private Vector3[] positions = new Vector3[5];
	private Color[] colors = new Color[4];
	// private Vector3[] scales = new Vector3[2];
	private GameObject[][] objects = new GameObject[2][];

	private Text questionText;
	private int[] shapeIndexs;
	private int[] colorIndexs;
	private int[] scaleIndexs;

	private string[] lastQuestions = new string[2];
	private string nextLine = "";

	private int dir = 0;
	private int errorTypeIndex = 0;
	private string positionText = "";
	private int[] positionIndex = new int[2];
	private int answerIndex = 0;

	private string[] questionTemplates;
	private string[] sizeDescs;
	private string[] dirDescs;
	
	private string[] shapeTexts; // 形狀  (圓形C, 方形S, 菱形D, 梯形T) 
	private string[] colorTexts = new string[] {"", "", "", ""}; // 顏色  (紅r, 黃y, 藍b, 綠g)

	private string[] typeCodes = new string[] {"S", "P"}; // (比大小Ｓ/ 比位置Ｐ)
	private string[] errorCodes = new string[] {"R", "C", "*"}; // 大小 or 位置R / 顏色C/ 無錯誤*
	private string[] shapeCodes = new string[] {"C", "S", "D", "T"}; // 形狀  (圓形C, 方形S, 菱形D, 梯形T) 
	private string[] colorCodes = new string[] {"r", "y", "b", "g"}; // 顏色  (紅r, 黃y, 藍b, 綠g)
	private string[] answerCodes = new string[] {"T", "F"}; // 正解 - =T, ✖=F 


	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		questionText = transform.Find("Text").gameObject.GetComponent<Text>();
		
		for (int i = 0; i < 2; i++) {
			int index = i;
			transform.Find("Button_" + (i+1)).gameObject.GetComponent<Button>().onClick.AddListener(delegate() {
				audioManager.PlaySound((int)Define.Sound.Click);
				Answer(index);
			});
		}

		for (int i = 0; i < 5; i++) {
			positions[i] = transform.Find("Image_" + (i+1)).position;
		}

		// scales[0] = Vector3.one;
		// scales[1] = new Vector3(0.6f, 0.6f, 1.0f);

		if (PlayerPrefs.GetString(Define.PP_Language, "Chinese") == "Chinese") {
			questionText.fontSize = 90;
			questionTemplates = new string[] {"{0}{1}比{2}{3}{4}{5}", "{0}{1}在{2}{3}{4}的{5}方"};
			sizeDescs = new string[] {"大", "小"};
			dirDescs = new string[] {"上", "下", "左", "右"};
		} else {
			questionText.fontSize = 65;
			questionTemplates = new string[] {"{0}{1} is {2}{5} than {3}{4}", "{0}{1} is {2}{5} {3}{4}"};
			sizeDescs = new string[] {"larger", "smaller"};
			dirDescs = new string[] {"above", "below", "to the left of", "to the right of"};
		}

		var gameData = SystemManager.Instance.GetGameData(UserInfo.Instance.Room.CurrentGameIndex);
		levelCondition = gameData.level;

		// color
		var hexs = gameData.colors.Length >= 4 ? gameData.colors : new string[] {
			"#f46464", "#f7ad00", "#2f98d6", "#31b478",
		}; // 桃紅R/鵝黃Y/天藍B/蘋果綠G
		for (int i = 0; i < 4; i++) {
			colors[i] = new Color();
			ColorUtility.TryParseHtmlString(hexs[i], out colors[i]);
			colors[i].a = 0.0f;
		}

		SetLevel(0);
		shapeIndexs = Enumerable.Range(0, 4).ToArray();
		colorIndexs = Enumerable.Range(0, 4).ToArray();
		scaleIndexs = Enumerable.Range(0, 2).ToArray();

		var prefab = Resources.Load<GameObject>("Prefabs/Image_Shape");
		// if (gameData.images.Length == objects.Length) {
			// string[] names = gameData.images;
			// for (int i = 0; i < names.Length; i++) {
			// 	string code = (i+1).ToString();
			// 	Utils.Instance.LoadImage(names[i], delegate(Sprite sprite) {
			// 		spriteList.Add(new SpriteInfo(sprite, code));

			// 		if (code == "5") {
			// 			CreateQuestion();
			// 		}
			// 	});
			// }
		// } else {
			string[][] names = new string[][] {
				new string[] {"game2_4_shape_1_s", "game2_4_shape_2_s", "game2_4_shape_3_s", "game2_4_shape_4_s"},
				new string[] {"game2_4_shape_1", "game2_4_shape_2", "game2_4_shape_3", "game2_4_shape_4"},
			};
			var scale =  new Vector3(1.5f, 1.5f, 1.0f);
			for (int i = 0; i < 2; i++) {
				objects[i] = new GameObject[4];
				for (int j = 0; j < 4; j++) {
					var go = Instantiate<GameObject>(prefab);
					go.transform.SetParent(transform);
					// go.transform.localScale = Vector3.one;
					go.transform.localScale = scale;
					go.SetActive(false);
					objects[i][j] = go;

					var img = go.GetComponent<Image>();
					img.sprite = Resources.Load<Sprite>("Sprites/" + names[i][j]);
					img.SetNativeSize();
				}
			}

			CreateQuestion();
		// }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void CheckLevel() {
		if (level < 1 && Game.self.rightCount%levelCondition == 0) {
			SetLevel(level+1);
		}
	}

	private void SetLevel(int l) {
		level = l;

		switch (level) {
			case 0:
				// 形狀  (圓形C, 方形S, 菱形D, 梯形T) 
				shapeTexts = new string[] {
					Lang.Instance.getString("game_18_shape_1"),
					Lang.Instance.getString("game_18_shape_2"),
					Lang.Instance.getString("game_18_shape_3"),
					Lang.Instance.getString("game_18_shape_4"),
				}; 
				break;

			case 1:
				nextLine = "\n";
				// 顏色  (紅r, 黃y, 藍b, 綠g)
				colorTexts = new string[] {
					Lang.Instance.getString("game_18_color_1"),
					Lang.Instance.getString("game_18_color_2"),
					Lang.Instance.getString("game_18_color_3"),
					Lang.Instance.getString("game_18_color_4"),
				}; 
				break;
		}
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();

		question = "";
		reaction = "";
		positionText = "";

		for (int i = 0; i < 2; i++) {
			objects[scaleIndexs[0]][shapeIndexs[i]].SetActive(false);
			objects[scaleIndexs[1]][shapeIndexs[i]].SetActive(false);
		}

		// 關係- 比大小S/ 比位置P  （機率各半）
		var typeIndex = rand.Next(2);
		type = typeCodes[typeIndex];

		if (typeIndex == 0) {
			positionIndex[0] = 4;
			positionIndex[1] = 4;
		} else {
			// 兩個圖案可能的位置組合：上下-(1,3),(1,5),(5,3)  左右-(2,4),(2,5),(5,4) 
			// 12/15: 位置組合: 上下-(1,3) 左右-(2,4)
			dir = rand.Next(2);
			int[] numbers = dir == 0 ? new int[2] {0, 2} : new int[2] {1, 3};
			numbers = numbers.OrderBy(n => System.Guid.NewGuid()).ToArray();
			positionIndex[0] = numbers[0];
			positionIndex[1] = numbers[1];
		}

		// 錯誤類型- 大小 or 位置R / 顏色C/ 無錯誤*（機率各1/3）
		errorTypeIndex = rand.Next(level+1);

		// 圖案題目
		// 第一碼：形狀  (圓形C, 方形S, 菱形D, 梯形T)
		// 第二碼：顏色  (紅r, 黃y, 藍b, 綠g)
		// 第三碼：大小 （大=1 , 小=0） (比大小時圖案須一大一小，比位置時兩個圖案大小隨機） 12/15: 比位置改成都是大
		colorIndexs = colorIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
		int[] scaleIndexs2 = typeIndex == 0 ?
			scaleIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray() : new int[] {1, 1};

		// 題目不可重複
		string[] questions = new string[2];
		do {
			shapeIndexs = shapeIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
			questions[0] = shapeIndexs[0]+""+scaleIndexs2[0];
			questions[1] = shapeIndexs[1]+""+scaleIndexs2[1];
		} while (lastQuestions.Contains(questions[0]) && lastQuestions.Contains(questions[1]));
		lastQuestions = questions;		

		for (int i = 0; i < 2; i++) {
			int index = shapeIndexs[i];
			int scaleIndex = scaleIndexs2[i];
			objects[scaleIndex][index].GetComponent<Image>().color = colors[colorIndexs[i]];
			// objects[scaleIndex][index].transform.localScale = scales[scaleIndex];
			objects[scaleIndex][index].transform.position = positions[positionIndex[i]];
			objects[scaleIndex][index].SetActive(true);
			objects[scaleIndex][index].GetComponent<Canvas>().sortingOrder = scaleIndex == 0 ? 1 : 0;
		}

		question += shapeCodes[shapeIndexs[0]] + colorCodes[colorIndexs[0]] + scaleIndexs2[0] + ",";
		question += shapeCodes[shapeIndexs[1]] + colorCodes[colorIndexs[1]] + scaleIndexs2[1];
		positionText = (positionIndex[0] + 1) + "," + (positionIndex[1] + 1);

		// 正解 (文字題目模板：X比Y大or小 /  X在Y的上or下or左or右方)
		answerIndex = rand.Next(2);

		var obj1 = objects[scaleIndexs2[0]][shapeIndexs[0]];
		var obj2 = objects[scaleIndexs2[1]][shapeIndexs[1]];
		
		if (answerIndex == 1) {
			// 顏色對調
			int colorIndex = colorIndexs[0];
			colorIndexs[0] = colorIndexs[1];
			colorIndexs[1] = colorIndex;

			if (errorTypeIndex == 0) { // 關係錯誤
				int shapeIndex = shapeIndexs[0];
				shapeIndexs[0] = shapeIndexs[1];
				shapeIndexs[1] = shapeIndex;
			}
		} else {
			errorTypeIndex = 2;
		}

		var text = questionTemplates[typeIndex];
		text = string.Format(text,
			colorTexts[colorIndexs[0]],
			shapeTexts[shapeIndexs[0]],
			nextLine,
			colorTexts[colorIndexs[1]],
			shapeTexts[shapeIndexs[1]],
			typeIndex == 0 ? 
				(scaleIndexs2[0] > scaleIndexs2[1] ? sizeDescs[0] : sizeDescs[1]) :
				(dir == 0 ?
					(obj1.transform.position.y > obj2.transform.position.y ? dirDescs[0] : dirDescs[1]) :
					(obj1.transform.position.x < obj2.transform.position.x ? dirDescs[2] : dirDescs[3]))
		);

		questionText.text = text;
	}

	private void Answer(int index) {
		reaction = answerCodes[index];

		if (answerIndex == index) {
			var success = Game.self.Right();
			SaveQuestion();
			if (success) CheckLevel();
		} else {
			Game.self.Wrong();
			SaveQuestion();
		}

		CreateQuestion();
	}

	public override JSONObject CreateHistory() {
		var json = new JSONObject();
		json.AddField("level",   		level); // 難度
		json.AddField("level_value",   	levelValue); // 難度
		json.AddField("type",   		type); // 關係
		json.AddField("question",   	question); // 圖案
		json.AddField("param_1",   		positionText); // 位置
		json.AddField("wrong",   		errorCodes[errorTypeIndex]); // 錯誤類型
		json.AddField("right",   		answerCodes[answerIndex]); // 正解
		json.AddField("reaction",   	reaction); // 反應
		return json;
	}

	public override void GameOver() {
		if (reaction == "") {
			Game.self.Next(true, false);
		}
	}
}
