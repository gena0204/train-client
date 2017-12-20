using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.Events;

public class Game_20 : GameBase {

	[SerializeField]
	private SpriteAtlas atlas;

	private Transform boxTransform;
	private GameObject[] colorButtons = new GameObject[4];

	private Vector3[] positions = new Vector3[5];
	private string[] colorNames = new string[] {"r", "g", "y", "b"};
	private Image[] objectImages = new Image[5];
	private Button[] objectBtns = new Button[5];
	private Animation[] objectAnims = new Animation[5];

	private int[] colorIndexs = new int[5];
	private int[] shapeIndexs;
	private int[] positionIndexs;
	private int currentCookieSize = 2;
	private int currentCookieIndex = 0 ;
	private bool isWrong = false;

	private int answerIndex = 0;
	private List<string> colorReactionList = new List<string>();

	public Game_20() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		boxTransform = transform.Find("Image_Box");

		for (int i = 0; i < 5; i++) {
			positions[i] = transform.Find("Image_" + (i+1)).position;
		}

		for (int i = 0; i < 4; i++) {
			int index = i;
			colorButtons[i] = transform.Find("Button_" + (i+1)).gameObject;
			colorButtons[i].GetComponent<Button>().onClick.AddListener(delegate() {
				audioManager.PlaySound((int)Define.Sound.Click);
				AnswerColor(index);
			});
		}

		var gameData = SystemManager.Instance.GetGameData(UserInfo.Instance.Room.CurrentGameIndex);
		levelCondition = gameData.level;

		shapeIndexs = Enumerable.Range(0, 5).ToArray();
		positionIndexs = Enumerable.Range(0, 5).ToArray();
		
		var prefab = Resources.Load<GameObject>("Prefabs/Button_Cookie");
		for (int i = 0; i < 5; i++) {
			int index = i;

			var go = Instantiate<GameObject>(prefab);
			go.transform.SetParent(transform);
			go.SetActive(false);
			objectImages[i] = go.GetComponent<Image>();
			objectBtns[i] = go.GetComponent<Button>();
			objectAnims[i] = go.GetComponent<Animation>();

			objectBtns[i].onClick.AddListener(delegate() {
				audioManager.PlaySound((int)Define.Sound.Click);
				Answer(index);
			});
		}

		SetLevel(0);
		CreateQuestion();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void CheckLevel() {
		if (level < 2 && Game.self.rightCount%levelCondition == 0) {
			SetLevel(level+1);
		}
	}

	private void SetLevel(int l) {
		level = l;

		switch (level) {
			case 0:
				currentCookieSize = 2;
				break;
			case 1:
				currentCookieSize = 3;
				break;
			case 2:
				currentCookieSize = 4;
				break;
		}

		levelValue = currentCookieSize;
	}

	private void NextCookie(int index = 0) {
		int shapeIndex = 0;

		if (index == 0) {
			for (int i = 0; i < 4; i++) {
				colorButtons[i].SetActive(true);
			}
			boxTransform.gameObject.SetActive(false);
		} else {
			shapeIndex = shapeIndexs[currentCookieIndex];
			objectImages[shapeIndex].gameObject.SetActive(false);
		}

		if (index < currentCookieSize) { // 下一個餅乾
			currentCookieIndex = index;
			shapeIndex = shapeIndexs[index];
			objectImages[shapeIndex].transform.position = positions[0];
			objectImages[shapeIndex].sprite = atlas.GetSprite("game2_6_shape_"+(shapeIndex+1)+"_"+colorNames[colorIndexs[index]]);
			objectImages[shapeIndex].gameObject.SetActive(true);
		} else { // 顯示全部餅乾
			for (int i = 0; i < 4; i++) {
				colorButtons[i].SetActive(false);
			}
			for (int i = 0; i <= currentCookieSize; i++) {
				shapeIndex = shapeIndexs[i];
				objectImages[shapeIndex].transform.position = positions[positionIndexs[i]];
				objectImages[shapeIndex].sprite = atlas.GetSprite("game2_6_shape_"+(shapeIndex+1)+"_0");
				objectImages[shapeIndex].gameObject.SetActive(true);
				objectBtns[shapeIndex].interactable = true;
			}
			boxTransform.gameObject.SetActive(true);

			Game.self.StartQuestion();
		}
	}

	private void MoveOverAcceleration(GameObject obj, Vector3 end, float v, float a, UnityAction callback = null) {
        StartCoroutine(IEMoveOverAcceleration(obj, end, v, a, callback));
    }

	private IEnumerator IEMoveOverAcceleration(GameObject obj, Vector3 end, float v, float a, UnityAction callback) {
		while (Vector3.Distance(obj.transform.position, end) > 0.1f) {
			v += a * Time.deltaTime;
			obj.transform.position = Vector3.MoveTowards(obj.transform.position, end, v * Time.deltaTime);
			yield return new WaitForEndOfFrame();
		}
		obj.transform.position = end;
        if (callback != null) {
            callback();
        }
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();

		question = "";
		reaction = "";
		colorReactionList.Clear();
		currentCookieIndex = 0;
		isWrong = false;

		shapeIndexs = shapeIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
		positionIndexs = positionIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
		for (int i = 0; i < currentCookieSize; i++) {
			colorIndexs[i] = rand.Next(4);

			objectBtns[shapeIndexs[i]].interactable = false;
			
			if (question != "") question += ",";
			question += (shapeIndexs[i]+1).ToString();
		}

		// 錯誤餅乾 (正解)
		answerIndex = shapeIndexs[currentCookieSize];
		
		NextCookie();
	}

	private void Answer(int index) {
		reaction += (index + 1).ToString();

		if (answerIndex == index) {
			var success = Game.self.Right();
			SaveQuestion();

			objectImages[index].gameObject.SetActive(false);

			for (int i = 0; i < currentCookieSize; i++) {
				objectBtns[shapeIndexs[i]].interactable = false;
			}

			var count = 0;
			UnityAction callback2 = delegate() {
				count++;
				if (count < currentCookieSize) {
					return;
				}

				for (int i = 0; i < currentCookieSize; i++) {
					objectImages[shapeIndexs[i]].gameObject.SetActive(false);
				}

				if (success) CheckLevel();
				CreateQuestion();
			};

			UnityAction callback1 = delegate() {
				var g = 9.81f * 400.0f * transform.lossyScale.y;
				for (int i = 0; i < currentCookieSize; i++) {
					var pos = objectAnims[shapeIndexs[i]].transform.position;
					pos.y = boxTransform.position.y;
					MoveOverAcceleration(objectAnims[shapeIndexs[i]].gameObject, pos, 0.0f, g, callback2);
				}
			};

			for (int i = 0; i < currentCookieSize; i++) {
				UnityAction cb = i == (currentCookieSize-1) ? callback1 : null;
				objectImages[shapeIndexs[i]].sprite =
					atlas.GetSprite("game2_6_shape_"+(shapeIndexs[i]+1)+"_"+colorNames[colorIndexs[i]]);
				Utils.Instance.PlayAnimation(objectAnims[shapeIndexs[i]], "card_fadein", cb);
			}
		} else {
			Game.self.Wrong();
			isWrong = true;
		}
	}

	private void AnswerColor(int index) {
		if (currentCookieIndex == colorReactionList.Count) {
			colorReactionList.Add(colorIndexs[currentCookieIndex] == index ? "1" : "0");
		}

		if (colorIndexs[currentCookieIndex] == index) {
			NextCookie(currentCookieIndex+1);
		} else {
			Game.self.Wrong();
			isWrong = true;
		}
	}

	protected override void SaveQuestion() {
		if (isWrong) {
			Game.self.SetSuccess(false);
		}
		Game.self.Next();
	}

	public override JSONObject CreateHistory() {
		var json = new JSONObject();
		json.AddField("level",   		level); // 難度
		json.AddField("level_value",   	levelValue); // 難度
		// json.AddField("type",   		type); // 類型
		json.AddField("question",   	question); // 圖形
		json.AddField("right",   		(answerIndex+1).ToString()); // 正解
		json.AddField("param_1",   		string.Join(",", colorReactionList.ToArray())); // 顏色判斷
		json.AddField("reaction",   	reaction); // 形狀反應
		return json;
	}

	public override void GameOver() {
		if (reaction == "") {
			Game.self.Next(true, false);
		}
	}
}
