using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameIntro : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Utils utils = Utils.Instance;
		AudioManager audioManager = AudioManager.Instance;

		Fading fading = transform.GetComponent<Fading>();

		var gameInfo = Define.gameInfo[UserInfo.Instance.Room.CurrentGameIndex];

		UnityAction backAction = delegate() {
			audioManager.PlaySound((int)Define.Sound.Click);
			if (!UserInfo.Instance.Room.IsChallenge) {
				HomePanel.panelIndex = 1; // 回訓練頁面
			}
			UserInfo.Instance.Room.Clear();
            utils.FadeScene(Define.SCENE_MAIN, fading);
		};
		transform.FindChild("Button_Back").GetComponent<Button>().onClick.AddListener(backAction);
		utils.SetBeginBackAction(backAction);

        transform.FindChild("Button_Start").GetComponent<Button>().onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            utils.FadeScene(Define.SCENE_GAME, fading);
        });

		transform.FindChild("Text_Name").GetComponent<Text>().text = gameInfo[0];

		var pagePrefab = Resources.Load<GameObject>("Prefabs/Panel_IntroPage");
		var dotPrefab = Resources.Load<GameObject>("Prefabs/Toggle_PageDot");

		var content = transform.FindChild("Scroll View/Viewport/Content");
		var pagination = transform.FindChild("Scroll View/Panel_Pagination");

		GameObject go;
		Image img;
		var gameTag = UserInfo.Instance.Room.CurrentGameIndex + 1;

		for (int i = 1; i < gameInfo.Length; i++) {
			go = Instantiate<GameObject>(pagePrefab);
			go.transform.SetParent(content);
			go.transform.localScale = Vector3.one;
        	go.transform.localPosition = Vector3.zero;
			go.transform.FindChild("Text").GetComponent<Text>().text = gameInfo[i];
			img = go.transform.FindChild("Image").GetComponent<Image>();
			img.sprite = Resources.Load<Sprite>("Sprites/help_" + gameTag + "_" + i);
			if (!img.sprite) {
				img.gameObject.SetActive(false);
			}

			if (gameInfo.Length > 2) {
				go = Instantiate<GameObject>(dotPrefab);
				go.transform.SetParent(pagination);
				go.transform.localScale = Vector3.one;
				go.transform.localPosition = Vector3.zero;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
