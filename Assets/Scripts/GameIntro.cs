using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameIntro : MonoBehaviour {

	[SerializeField]
    private Fading fading;

	// Use this for initialization
	void Start () {
		Utils utils = Utils.Instance;
		Lang lang = Lang.Instance;
		AudioManager audioManager = AudioManager.Instance;

		var gameIndex = UserInfo.Instance.Room.CurrentGameIndex;
		var gameTag = gameIndex + 1;
		var helpSize = Define.gameInfo[gameIndex];
		var langTag = "";

		if (PlayerPrefs.GetString(Define.PP_Language, "Chinese") == "English") {
			if (gameIndex == 1 || gameIndex == 16 || gameIndex == 17) {
				langTag = "_en";
			}
		}

		UnityAction backAction = delegate() {
			audioManager.PlaySound((int)Define.Sound.Click);
			if (!UserInfo.Instance.Room.IsChallenge) {
				HomePanel.panelIndex = 1; // 回訓練頁面
			}
			UserInfo.Instance.Room.Clear();
            utils.FadeScene(Define.SCENE_MAIN, fading);
		};
		transform.Find("Button_Back").GetComponent<Button>().onClick.AddListener(backAction);
		utils.SetBeginBackAction(backAction);

        transform.Find("Button_Start").GetComponent<Button>().onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            utils.FadeScene(Define.SCENE_GAME, fading);
        });

		transform.Find("Text_Name").GetComponent<Text>().text = lang.getString("game_name_" + gameTag);

		var arrowLeftBtn = transform.Find("Scroll View/Button_Arrow_Left").gameObject;
		var arrowRightBtn = transform.Find("Scroll View/Button_Arrow_Right").gameObject;

		var pagePrefab = Resources.Load<GameObject>("Prefabs/Panel_IntroPage");
		var dotPrefab = Resources.Load<GameObject>("Prefabs/Toggle_PageDot");

		var content = transform.Find("Scroll View/Viewport/Content");
		var pagination = transform.Find("Scroll View/Panel_Pagination");
		var pageText = transform.Find("Scroll View/Text_Number").GetComponent<Text>();

		GameObject go;
		Image img;

		for (int i = 0; i < helpSize; i++) {
			int page = i + 1;
			go = Instantiate<GameObject>(pagePrefab);
			go.transform.SetParent(content);
			go.transform.localScale = Vector3.one;
        	go.transform.localPosition = Vector3.zero;
			go.transform.Find("Text").GetComponent<Text>().text = lang.getString("game_help_" + gameTag + "_" + page).Replace("\\n", "\n");
			img = go.transform.Find("Image").GetComponent<Image>();
			img.sprite = Resources.Load<Sprite>("Sprites/help/help_" + gameTag + "_" + page + langTag);
			if (!img.sprite) {
				img.gameObject.SetActive(false);
			}

			if (helpSize > 1) {
				go = Instantiate<GameObject>(dotPrefab);
				go.transform.SetParent(pagination);
				go.transform.localScale = Vector3.one;
				go.transform.localPosition = Vector3.zero;

				go.GetComponent<Toggle>().onValueChanged.AddListener(delegate(bool isOn) {
					if (isOn) {
						pageText.text = page + "/" + helpSize;
						arrowLeftBtn.SetActive(page != 1);
						arrowRightBtn.SetActive(page != helpSize);
					}
				});
			}
		}

		if (helpSize > 1) {
			pageText.text = "1/" + helpSize;
			pageText.gameObject.SetActive(true);
			arrowRightBtn.SetActive(true);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
