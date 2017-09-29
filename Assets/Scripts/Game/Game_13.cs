using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Game_13 : Game_12 {

	public Game_13() : base() {
    }

	// Use this for initialization
	// void Start () {
	// }
	
	// Update is called once per frame
	void Update () {
		
	}

	protected override void CheckLevel() {
		if (level < 2 && (Game.self.rightCount % levelCondition) == 0) {
			SetLevel(level+1);
		}
	}

	protected override void SetLevel(int l) {
		level = l;

		switch (level) {
			case 0:
				currentArrowSize = 2;
				break;

			case 1:
				currentArrowSize = 3;
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
	}

	protected override void ShowLine(UnityAction callback) {
		base.ShowLine(callback);

		for (int i = 0; i < currentMatchSize; i++) {
			arrows[i].SetActive(true);
		}
	}

	protected override void EnableButton(bool enable) {
		base.EnableButton(enable);

		if (enable) {
			foreach (var arrow in arrows) {
				arrow.SetActive(false);
			}
		}
	}

	protected override void CreateQuestion() {
		// 題目箭頭數量為3時，抵達終點可能用到的箭頭只能是兩個或三個
		if (currentArrowSize == 3) {
			currentMatchSize = rand.Next(currentArrowSize-1) + 2;
		} else {
			currentMatchSize = rand.Next(currentArrowSize) + 1;
		}
		base.CreateQuestion();

		type = currentArrowSize == currentMatchSize ? "S" : "D"; // 經過箭頭數與難度相同- S,  差一種- D
	}
}
