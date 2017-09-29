using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Game_12 : GameBase {

	private GameObject[] buttons = new GameObject[12];
	private Vector3[] positions = new Vector3[9];
	private GameObject[] lines = new GameObject[4];
	private Color[] colors = new Color[2];
	protected Transform arrowPanel;
	protected GameObject[] arrows = null;
	private GameObject selectImage;
	private GameObject[] startArrows = new GameObject[4];
	private GameObject startButton;

	private int lineW = 6;

	protected int currentArrowSize = 2;
	protected int currentMatchSize = 1;
	private int currentColorIndex = 0;

	private int startIndex = 0;
	private int answerIndex = 0;

	private float rememberTime;


	public Game_12() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		for (int i = 0; i < 12; i++) {
			int index = i;
			buttons[i] = transform.FindChild("Button_" + (i + 1)).gameObject;
			buttons[i].GetComponent<Button>().onClick.AddListener(delegate() {
				audioManager.PlaySound((int)Define.Sound.Click);
				EnableButton(false);
				Answer(index);
			});
		}

		for (int i = 0; i < 9; i++) {
			positions[i] = transform.FindChild("Image_" + (i + 1)).position;
		}

		for (int i = 0; i < 4; i++) {
			lines[i] = transform.FindChild("Panel_Line/Image_Line_" + (i + 1)).gameObject;
		}

		arrowPanel = transform.FindChild("Panel_Arrow");

		colors[0] = new Color(254/255.0f, 204/255.0f, 16/255.0f, 0); // 黃
		colors[1] = new Color(249/255.0f, 83/255.0f, 96/255.0f, 0); // 紅

		selectImage = transform.FindChild("Image_Select").gameObject;
		for (int i = 0; i < 4; i++) {
			startArrows[i] = transform.FindChild("Image_Start_" + (i + 1)).gameObject;
		}

		startButton = transform.FindChild("Button_Start").gameObject;
		startButton.GetComponent<Button>().onClick.AddListener(delegate() {
			audioManager.PlaySound((int)Define.Sound.Click);
			rememberTime = (int)((Time.time - rememberTime) * 1000);
			EnableButton(true);
			startButton.SetActive(false);

			buttons[startIndex].SetActive(false);
			startArrows[startIndex/3].SetActive(true);
		});

		var gameData = SystemManager.Instance.GetGameData(UserInfo.Instance.Room.CurrentGameIndex);
		levelCondition = gameData.level;

		SetLevel(0);
		CreateQuestion();
		EnableButton(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	protected virtual void CheckLevel() {
		if (level < 3 && (Game.self.rightCount % levelCondition) == 0) {
			SetLevel(level+1);
		}
	}

	protected virtual void SetLevel(int l) {
		level = l;

		switch (level) {
			case 0:
				currentArrowSize = 2;
				currentMatchSize = 1;
				break;

			case 1:
				currentArrowSize = 2;
				currentMatchSize = 2;
				break;

			case 2:
				currentArrowSize = 3;
				currentMatchSize = 2;
				break;

			case 3:
				currentArrowSize = 3;
				currentMatchSize = 3;
				break;

			default:
				break;
		}

		if (arrows != null) {
			foreach (var a in arrows) {
				Destroy(a);
			}
		}

		var arrow = Resources.Load<GameObject>("Prefabs/Image_Arrow");
		arrows = new GameObject[currentArrowSize];

		for (int i = 0; i < currentArrowSize; i++) {
			var go = Instantiate<GameObject>(arrow);
			go.transform.localScale = Vector3.one;
			go.transform.SetParent(arrowPanel);
			arrows[i] = go;
		}

		levelValue = currentArrowSize;
		type = currentArrowSize == currentMatchSize ? "S" : "D"; // 經過箭頭數與難度相同- S,  差一種- D
	}

	protected virtual void ShowLine(UnityAction callback) {
		for (int i = 0; i <= currentMatchSize; i++) {
			UnityAction cb = i == currentMatchSize ? callback : null;

			lines[i].SetActive(true);
			lines[i].GetComponent<Image>().color = colors[currentColorIndex];
			Utils.Instance.DelayPlayAnimation(i * 0.1f, lines[i].GetComponent<Animation>(), cb, 1.0f, "fadein");
		}
	}

	protected virtual void EnableButton(bool enable) {
		foreach (var button in buttons) {
			button.GetComponent<Button>().interactable = enable;
		}
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();

		reaction = "";

		buttons[startIndex].SetActive(true);
		startArrows[startIndex/3].SetActive(false);

		foreach (var line in lines) {
			line.SetActive(false);
		}

		startIndex = rand.Next(12);
		var arrowIndex = startIndex / 3;

		startArrows[arrowIndex].SetActive(false);
		startArrows[arrowIndex].transform.position = buttons[startIndex].transform.position;

		int[] posIndexs = new int[] {
			0, 1, 2,
			3, 4, 5,
			6, 7, 8,
		};

		List<int> posList = new List<int>();
		int lastIndex = 0;
		int dir = 0;
		int size = 0;
		Vector3 pos = buttons[startIndex].transform.position;
		Vector3 endPos = pos;
		Vector2 sizeDelta;

		if (startIndex < 3) {
			lastIndex = startIndex - 3;
			dir = 1; // 下
		} else if (startIndex < 6) {
			lastIndex = 3 * (startIndex - 2);
			dir = 2; // 左
		} else if (startIndex < 9) {
			lastIndex = 11 - startIndex + 6;
			dir = 0; // 上
		} else {
			lastIndex = 5 - (startIndex - 9) * 3;
			dir = 3; // 右
		}

		for (int i = 0; i <= currentMatchSize; i++) {
			sizeDelta = lines[i].GetComponent<RectTransform>().sizeDelta;

			switch (dir) {
				case 0: // 上
					if (lastIndex >= 3 || i == 0) {
						size = i == 0 ? 3 : (lastIndex / 3);
						size = i == currentMatchSize ? size : (rand.Next(size) + 1);
						for (int j = 0; j < size; j++) {
							lastIndex -= 3;
							if (!posList.Contains(posIndexs[lastIndex])) {
								posList.Add(posIndexs[lastIndex]);
							}
						}
					}

					endPos = i == currentMatchSize ? endPos : positions[lastIndex];
					sizeDelta.y = Mathf.Abs(pos.y - endPos.y);
					pos.y += sizeDelta.y / 2;
					sizeDelta.x = lineW;
					sizeDelta.y /= transform.lossyScale.y;
					break;
				case 1: // 下
					if (lastIndex <= 5 || i == 0) {
						size = i == 0 ? 3 : (2 - lastIndex / 3);
						size = i == currentMatchSize ? size : (rand.Next(size) + 1);
						for (int j = 0; j < size; j++) {
							lastIndex += 3;
							if (!posList.Contains(posIndexs[lastIndex])) {
								posList.Add(posIndexs[lastIndex]);
							}
						}
					}

					endPos = i == currentMatchSize ? endPos : positions[lastIndex];
					sizeDelta.y = Mathf.Abs(pos.y - endPos.y);
					pos.y -= sizeDelta.y / 2;
					sizeDelta.x = lineW;
					sizeDelta.y /= transform.lossyScale.y;
					break;
				case 2: // 左
					if (lastIndex % 3 != 0 || i == 0) {
						size = i == 0 ? 3 : (lastIndex % 3);
						size = i == currentMatchSize ? size : (rand.Next(size) + 1);
						for (int j = 0; j < size; j++) {
							lastIndex -= 1;
							if (!posList.Contains(posIndexs[lastIndex])) {
								posList.Add(posIndexs[lastIndex]);
							}
						}
					}

					endPos = i == currentMatchSize ? endPos : positions[lastIndex];
					sizeDelta.x = Mathf.Abs(pos.x - endPos.x);
					pos.x -= sizeDelta.x / 2;
					sizeDelta.x /= transform.lossyScale.x;
					sizeDelta.y = lineW;
					break;
				case 3: // 右
					if (lastIndex % 3 != 2 || i == 0) {
						size = i == 0 ? 3 : (2 - (lastIndex % 3));
						size = i == currentMatchSize ? size : (rand.Next(size) + 1);
						for (int j = 0; j < size; j++) {
							lastIndex += 1;
							if (!posList.Contains(posIndexs[lastIndex])) {
								posList.Add(posIndexs[lastIndex]);
							}
						}
					}

					endPos = i == currentMatchSize ? endPos : positions[lastIndex];
					sizeDelta.x = Mathf.Abs(pos.x - endPos.x);
					pos.x += sizeDelta.x / 2;
					sizeDelta.x /= transform.lossyScale.x;
					sizeDelta.y = lineW;
					break;
			}

			lines[i].GetComponent<RectTransform>().sizeDelta = sizeDelta;
			lines[i].transform.position = pos;
			pos = positions[lastIndex];

			if (i == currentMatchSize) {
				break;
			}

			// 轉向
			dir = (dir < 2 ? 2 : 0) + rand.Next(2);

			// 檢查是否碰壁
			if (i != (currentMatchSize - 1)) {
				switch (dir) {
					case 0: // 上
						if (lastIndex < 3) {
							dir = 1;
						}
						break;
					case 1: // 下
						if (lastIndex > 5) {
							dir = 0;
						}
						break;
					case 2: // 左
						if (lastIndex % 3 == 0) {
							dir = 3;
						}
						break;
					case 3: // 右
						if (lastIndex % 3 == 2) {
							dir = 2;
						}
						break;
				}
			} else {
				switch (dir) {
					case 0: // 上
						answerIndex = lastIndex % 3;
						break;
					case 1: // 下
						answerIndex = 8 - (lastIndex % 3);
						break;
					case 2: // 左
						answerIndex = 11 - (lastIndex / 3);
						break;
					case 3: // 右
						answerIndex = 3 + (lastIndex / 3);
						break;
				}

				if (answerIndex == startIndex) {
					CreateQuestion();
					return;
				}

				endPos = buttons[answerIndex].transform.position;
			}

			arrows[i].SetActive(true);
			arrows[i].transform.position = pos;
			arrows[i].transform.localEulerAngles = new Vector3(0, 0, dir < 2 ? (90 + 180 * dir) : (90 * (dir % 3)));
			Utils.Instance.PlayAnimation(arrows[i].GetComponent<Animation>(), null, 0.0f, "card_fadein");
		}

		int index = 0;
		int[] otherPosIndexs = new int[9 - posList.Count()];
		for (int i = 0; i < 9; i++) {
			if (!posList.Contains(i)) {
				otherPosIndexs[index] = i;
				index++;
			}
		}
		otherPosIndexs = otherPosIndexs.OrderBy(n => System.Guid.NewGuid()).ToArray();
		index = 0;

		for (int i = currentMatchSize; i < currentArrowSize; i++) {
			dir = rand.Next(4);
			arrows[i].SetActive(true);
			arrows[i].transform.position = positions[otherPosIndexs[index]];
			arrows[i].transform.localEulerAngles = new Vector3(0, 0, dir < 2 ? (90 + 180 * dir) : (90 * (dir % 3)));
			Utils.Instance.PlayAnimation(arrows[i].GetComponent<Animation>(), null, 0.0f, "card_fadein");
			index++;
		}
	}

	private void Answer(int index) {
		reaction = ((index + 10) % 12 + 1).ToString();

		if (answerIndex == index) {
			Game.self.Right();
			currentColorIndex = 0;
		} else {
			Game.self.Wrong();
			currentColorIndex = 1;
		}

		SaveQuestion();

		selectImage.transform.position = buttons[index].transform.position;
		selectImage.SetActive(true);

		ShowLine(delegate() {
			if (answerIndex == index) {
				CheckLevel();
			}
			CreateQuestion();
			selectImage.SetActive(false);
			startButton.SetActive(true);
			rememberTime = Time.time;
		});
	}

	public override JSONObject CreateHistory() {
		question = ((startIndex + 10) % 12 + 1).ToString();
		string end = ((answerIndex + 10) % 12 + 1).ToString();

		var json = new JSONObject();
		json.AddField("level",   		level); // 難度
		json.AddField("level_value",   	levelValue); // 難度
		json.AddField("type",   		type); // 類型
		json.AddField("question",   	question); // 起點
		json.AddField("right",   		end); // 終點
		json.AddField("reaction",   	reaction); // 反應
		json.AddField("remember_ms",   	(int)rememberTime); // 記憶時間
		return json;
	}

	public override void GameOver() {
		if (startButton.activeSelf == false) {
			rememberTime = 0;
		}
		if (reaction == "") {
			Game.self.Next(true, false);
		}
	}
}
