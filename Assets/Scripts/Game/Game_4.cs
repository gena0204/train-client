using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_4 : GameBase {

	class Question {
		public string code = "";
		public int answerIndex = 0;
		public GameObject[] objects = new GameObject[2];
	}
	
	private Transform basket;
	private Transform[] bgs = new Transform[2];
	private GameObject[] objectPrefabs = new GameObject[2];
	private Vector3[] basketPos = new Vector3[2];
	private Vector3[] objectPos = new Vector3[2];

	private Queue<GameObject>[] objectQ = new Queue<GameObject>[2] {
		new Queue<GameObject>(), new Queue<GameObject>()
	};
	private Queue<Question> questionQ = new Queue<Question>();
	private Queue<GameObject> finishObjQ = new Queue<GameObject>();
	private GameObject lastQuestionObj = null;

	private int currentBgIndex = 0;
	private float bgH = 0;
	private Vector3 startBgPos;
	private Vector3[] endBgPos = new Vector3[2];
	private float endBgY;
	private float offset = 0;
	private int speed = 3500;


	public Game_4() : base() {
    }

	// Use this for initialization
	void Start () {
		AudioManager audioManager = AudioManager.Instance;

		basket = transform.FindChild("Image_Basket");
		bgs[0] = transform.FindChild("Panel_1");
		bgs[1] = transform.FindChild("Panel_2");

		objectPrefabs[0] = Resources.Load<GameObject>("Prefabs/Image_Eggs");
		objectPrefabs[1] = Resources.Load<GameObject>("Prefabs/Image_Stone");

		basketPos[0] = objectPos[0] = transform.FindChild("Image_Left").position;
		basketPos[1] = objectPos[1] = transform.FindChild("Image_Right").position;
		basketPos[0].y = basket.position.y; basketPos[1].y = basket.position.y;
		objectPos[0].y = basket.position.y; objectPos[1].y = basket.position.y;

		startBgPos = bgs[1].position;
		endBgPos[0] = bgs[0].position;
		endBgPos[1] = bgs[1].position;

		Rect rect = ((RectTransform)bgs[0]).rect;
        rect.height *= transform.lossyScale.y;
		endBgY = bgs[0].position.y - rect.height;
		bgH = rect.height;

		offset = Screen.height / 5;

		transform.FindChild("Button_Left").GetComponent<Button>().onClick.AddListener(delegate() {
			Answer(0);
		});
		transform.FindChild("Button_Right").GetComponent<Button>().onClick.AddListener(delegate() {
			Answer(1);
		});

		for (int i = 0; i < 4; i++) {
			objectPos[0].y += offset; objectPos[1].y += offset;
			CreateQuestion();
		}
		objectPos[0].y += offset; objectPos[1].y += offset;
	}
	
	// Update is called once per frame
	void Update () {
		if (bgs[0].position == endBgPos[0] && bgs[1].position == endBgPos[1]) {
			return;
		}

        var delta = speed * transform.lossyScale.y * Time.deltaTime;
		for (int i = 0; i < 2; i++) {
			bgs[i].position = Vector3.MoveTowards(bgs[i].position, endBgPos[i], delta);
			CheckMoveEnd(i);
		}
	}

	private void Move(int basketIndex) {
		basket.position = basketPos[basketIndex];

		if (bgs[currentBgIndex].position.y + bgH < objectPos[0].y) {
			currentBgIndex = (currentBgIndex + 1) % 2;
		}

		for (int i = 0; i < 2; i++) {
			bgs[i].position = endBgPos[i];
			endBgPos[i].y -= offset;
			CheckMoveEnd(i);
		}

		if (finishObjQ.Count > 0) {
			var obj = finishObjQ.Peek();
			while (obj) {
				obj.SetActive(false);
				finishObjQ.Dequeue();
				obj = finishObjQ.Count > 0 ? finishObjQ.Peek() : null;
			}
		}
	}

	private void CheckMoveEnd(int index) {
		if (bgs[index].position.y <= endBgY) {
			var pos = startBgPos;
			pos.y -= (endBgY - bgs[index].position.y);
			bgs[index].position = pos;
			endBgPos[index].y = startBgPos.y - (endBgY - endBgPos[index].y);
		}
	}

	private GameObject GenerateObject(int index) {
		GameObject go;
		if (objectQ[index].Count < 2) {
			go = Instantiate<GameObject>(objectPrefabs[index]);
			go.name = "" + index;
			go.transform.localScale = Vector3.one;
        	go.transform.localPosition = Vector3.zero;
		} else {
			go = objectQ[index].Dequeue();
		}
		go.GetComponent<Image>().color = Color.white;
		go.SetActive(true);
		return go;
	}

	private void RecycleObject(GameObject go) {
		if (go) {
			go.SetActive(false);
			objectQ[go.name == "0" ? 0 : 1].Enqueue(go);
		}
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();

		reaction = "";
		
		int answerIndex = rand.Next(2);
		int wrongIndex = (answerIndex + 1) % 2;

		Question question = new Question();
		question.answerIndex = answerIndex;

		if (lastQuestionObj != null) {
			for (int i = 0; i < 2; i++) {
				objectPos[i].y = lastQuestionObj.transform.position.y + offset;
			}
		}

		// 金蛋 石頭 空地
		bool hasStone = false;
		if (rand.Next(2) == 0) { // 金蛋
			var go = GenerateObject(0);
			go.transform.SetParent(bgs[currentBgIndex]);
			go.transform.localScale = Vector3.one;
			go.transform.position = objectPos[answerIndex];
			question.objects[answerIndex] = go;
			lastQuestionObj = go;

			// 石頭 空地 (目前設定有金蛋就不能同時有石頭)
			// if (rand.Next(2) == 0) {
			// 	hasStone = true;
			// }
		} else { // 空地
			hasStone = true;
		}

		question.code = hasStone ? "D" : "S"; // 相反- D,  相同- S

		if (hasStone) { // 石頭
			var go = GenerateObject(1);
			go.transform.SetParent(bgs[currentBgIndex]);
			go.transform.localScale = Vector3.one;
			go.transform.position = objectPos[wrongIndex];
			question.objects[wrongIndex] = go;
			lastQuestionObj = go;
		}

		questionQ.Enqueue(question);
	}

	private void Answer(int index) {
		reaction += index == 0 ? "L" : "R"; // 左- L,  右- R

		var q = questionQ.Peek();

		type = q.code; // 相反- D,  相同- S
		if (type == "S") {
			question = q.answerIndex == 0 ? "L" : "R";
		} else {
			question = q.answerIndex == 0 ? "R" : "L";
		}

		if (q.answerIndex == index) {
			questionQ.Dequeue();
			Move(index);
			for (int i = 0; i < 2; i++) {
				if (q.objects[i]) {
					var obj = q.objects[i];
					finishObjQ.Enqueue(obj);
					Utils.Instance.PlayAnimation(obj.GetComponent<Animation>(), delegate() {
						RecycleObject(obj);
					}, 0, "fadeout");
				}
			}

			Game.self.Right();
			SaveQuestion();
			CreateQuestion();
		} else {
			Game.self.Wrong();
		}
	}

	public override void GameOver() {
		reaction = "*";

		var q = questionQ.Peek();
		type = q.code; // 相反- D,  相同- S
		if (type == "S") {
			question = q.answerIndex == 0 ? "L" : "R";
		} else {
			question = q.answerIndex == 0 ? "R" : "L";
		}

		Game.self.Next(true, false);
	}
}
