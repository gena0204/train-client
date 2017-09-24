﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour {

	public static Game self;

	public GameBase currentGame;
	private int currentGameIndex = 0;

	private AudioManager audioManager;

	private Fading fading;

	private Text timerText;
	private GameObject borderFailImage;

	private GameObject gameoverPanel;
	private GameObject resultPanel;

	private Text resultTitle;
	private Text[] resultTexts = new Text[4];
	private List<Button> resultButtonList = new List<Button>();

	private RectTransform percentBarRT;
	private Vector2 percentBarTargetSize;
	private float percentMaxW = 0;

	private RectTransform timeBarRT;
	private Vector2 timeBarTargetSize;
	private float timeBarUnit = 0; // 單位秒

	private float startTime;
	private float pauseTime;
	private int gameTime = 120;
	private float gameoverTime = 1;

	private float questionStartTime;
	private int reactionCount = 0;

	private JSONObject answerJSONs = new JSONObject(JSONObject.Type.ARRAY);
	public int rightCount = 0;
	private int wrongCount = 0;
	private bool isSuccess;


	void OnApplicationPause(bool pauseStatus) {
        if (pauseStatus) {
			pauseTime = Time.realtimeSinceStartup;
		} else {
			gameTime -= (int)(Time.realtimeSinceStartup - pauseTime);
			if (gameTime < 0) gameTime = 0;
			if (timerText) {
				RefreshTime();
				timeBarRT.sizeDelta = new Vector2(timeBarUnit * gameTime, timeBarRT.sizeDelta.y);
			}
		}
	}

	void Awake() {
		self = this;
	}

	// Use this for initialization
	void Start () {
		UserInfo userInfo = UserInfo.Instance;
		Utils utils = Utils.Instance;
		audioManager = AudioManager.Instance;

		fading = transform.GetComponent<Fading>();

		currentGameIndex = userInfo.Room.CurrentGameIndex;

		gameTime = SystemManager.Instance.GetInt("game_time", gameTime);
		gameoverTime = SystemManager.Instance.GetFloat("gameover_time", gameoverTime);

		UnityAction exitAction = delegate() {
			audioManager.PlaySound((int)Define.Sound.Click);
			HomePanel.panelIndex = 1; // 回訓練頁面
			StopCountDown();
            Exit();
		};
		transform.FindChild("Button_Exit").GetComponent<Button>().onClick.AddListener(exitAction);
		utils.SetBeginBackAction(exitAction);

		transform.FindChild("Text_Title").GetComponent<Text>().text = Define.gameInfo[userInfo.Room.CurrentGameIndex][0];

		timerText = transform.FindChild("Text_Time").GetComponent<Text>(); 
		borderFailImage = transform.FindChild("Image_Border_Fail").gameObject;

		timeBarRT = transform.FindChild("Image_TimeBar").GetComponent<RectTransform>();
		timeBarTargetSize = new Vector2(0, timeBarRT.sizeDelta.y);
		timeBarUnit = timeBarRT.sizeDelta.x / gameTime;


		//=============================================================
        // Game Over
        //=============================================================
		gameoverPanel = transform.FindChild("Panel_Gameover").gameObject;
		resultPanel = transform.FindChild("Panel_Result").gameObject;

		resultTitle = resultPanel.transform.FindChild("Text_Title").GetComponent<Text>();
		resultTexts[0] = resultPanel.transform.FindChild("Image_Right/Text_Right").GetComponent<Text>();
		resultTexts[1] = resultPanel.transform.FindChild("Image_Wrong/Text_Wrong").GetComponent<Text>();
		resultTexts[2] = resultPanel.transform.FindChild("Image_Percent/Text_Percent").GetComponent<Text>();
		resultTexts[3] = resultPanel.transform.Find("Text_Slogan").GetComponent<Text>();

		percentBarRT = resultPanel.transform.FindChild("Image_Percent/Image_Bar").GetComponent<RectTransform>();
		percentBarTargetSize = percentBarRT.sizeDelta;
		percentMaxW = percentBarTargetSize.x;

		resultTexts[3].text = SystemManager.Instance.GetSlogan();

		if (userInfo.Room.IsChallenge) {
			if (userInfo.Room.StageIndex >= (userInfo.Room.GameIndexs.Count-1)) {
				Button btn = resultPanel.transform.FindChild("Button_Train_2").GetComponent<Button>();
				btn.onClick.AddListener(delegate() {
					audioManager.PlaySound((int)Define.Sound.Click);
					HomePanel.panelIndex = 1; // 回訓練頁面
					Game.self.Exit();
				});
				resultButtonList.Add(btn);
			} else {
				Button btn = resultPanel.transform.FindChild("Button_Next").GetComponent<Button>();
				btn.onClick.AddListener(delegate() {
					audioManager.PlaySound((int)Define.Sound.Click);
					userInfo.Room.StageIndex = userInfo.Room.StageIndex + 1;
					utils.FadeScene(Define.SCENE_GAME_INTRO, fading);
				});
				resultButtonList.Add(btn);
			}
		} else {
			Button btn = resultPanel.transform.FindChild("Button_Again").GetComponent<Button>();
			btn.onClick.AddListener(delegate() {
				audioManager.PlaySound((int)Define.Sound.Click);
				// utils.FadeScene(Define.SCENE_GAME, fading);
				utils.FadeScene(Define.SCENE_GAME_INTRO, fading);
			});
			resultButtonList.Add(btn);

			btn = resultPanel.transform.FindChild("Button_Train").GetComponent<Button>();
			btn.onClick.AddListener(delegate() {
				audioManager.PlaySound((int)Define.Sound.Click);
				HomePanel.panelIndex = 1; // 回訓練頁面
				Game.self.Exit();
			});
			resultButtonList.Add(btn);
		}

		SceneManager.LoadScene("Game_" + (userInfo.Room.CurrentGameIndex + 1), LoadSceneMode.Additive);
		StartCountDown();
	}
	
	// Update is called once per frame
	void Update () {
		if (timeBarRT.sizeDelta.x > 0) {
			timeBarRT.sizeDelta = Vector2.MoveTowards(timeBarRT.sizeDelta, timeBarTargetSize, timeBarUnit * Time.deltaTime);
		}

		if (percentBarRT.sizeDelta != percentBarTargetSize) {
			percentBarRT.sizeDelta = Vector2.Lerp(percentBarRT.sizeDelta, percentBarTargetSize, 1.8f * Time.deltaTime);

			if (Vector2.Distance(percentBarRT.sizeDelta, percentBarTargetSize) < 1.0f) {
				percentBarRT.sizeDelta = percentBarTargetSize;
				ShowButton();
			}

			resultTexts[2].text = System.Math.Round(percentBarRT.sizeDelta.x / percentMaxW * 100, 1) + "%";
		}
	}
	
	private void StartCountDown() {
        startTime = Time.time;
        StopCountDown();
        InvokeRepeating("CountDown", 0, 1);
    }

    public void StopCountDown() {
        CancelInvoke("CountDown");
    }

    private void CountDown() {
        RefreshTime();
        gameTime--;
		if (gameTime < 0) {
			currentGame.GameOver();
			GameOver();
		} else if (gameTime == 4) {
			timeBarRT.transform.GetComponent<Image>().color = new Color(252/255.0f, 105/255.0f, 106/255.0f);
		}
    }

    private void RefreshTime() {
        TimeSpan t = TimeSpan.FromSeconds(gameTime);
        timerText.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }

	public static void SetGame(GameBase game) {
		if (self != null) {
			self.currentGame = game;
		}
	}

	public int GetReactionCount() {
		return reactionCount;
	}

	public void StartQuestion() {
		questionStartTime = Time.time;
	}

	public bool Right() {
		audioManager.PlaySound((int)Define.Sound.Right);

		if (reactionCount == 0) {
			rightCount++;
		}
		isSuccess = true;
		reactionCount++;

		return reactionCount == 1;
	}

	public void Wrong() {
		audioManager.PlaySound((int)Define.Sound.Wrong);
		#if UNITY_ANDROID || UNITY_IOS 
			Handheld.Vibrate();
		#endif

		if (reactionCount == 0) {
			wrongCount++;
		}
		isSuccess = false;
		reactionCount++;

		borderFailImage.SetActive(true);
		Utils.Instance.PlayAnimation(borderFailImage.GetComponent<Animation>(), delegate() {
			borderFailImage.SetActive(false);
		}, 0.5f);
	}

	public void Next(bool ignore = false, bool time = true) {
		if (reactionCount == 0 && !ignore) {
			return;
		}

		if (ignore) {
			reactionCount = 1;
		} 
		
		var reactionMS = time ? (int)((Time.time - questionStartTime) * 1000) : 0;
		var averageMS = 0;

		switch (currentGameIndex) {
			case 4:
			case 5:
			case 6:
				if (currentGame.reaction.Length > 0) {
					averageMS = (int)(reactionMS / currentGame.reaction.Length);
				}
				break;
			case 7:
				if (currentGame.reaction.Length > 0) {
					averageMS = (int)(reactionMS / currentGame.reaction.Split(',').Length);
				}
				break;
			default:
				if (reactionCount > 0) {
					averageMS = (int)(reactionMS / reactionCount);
				}
				break;
		}

		var json = currentGame.CreateHistory();
		json.AddField("index",   		answerJSONs.Count); // 第幾題
		json.AddField("reaction_ms",	reactionMS); // 反應時間
		json.AddField("average_ms",		averageMS); // 平均時間
		json.AddField("count",   		ignore ? -1 : (isSuccess ? reactionCount : 0)); // 答題狀況
		json.AddField("success",		isSuccess);
        answerJSONs.Add(json);

		isSuccess = false;
		reactionCount = 0;
	}

	public void GameOver() {
		var userInfo = UserInfo.Instance;

		audioManager.PlaySound((int)Define.Sound.End, 0);

		if (userInfo.Room.IsChallenge) {
			var indexsStr = PlayerPrefs.GetString(Define.PP_ChallengeIndexs);
            var indexs = indexsStr.Split('-');
			indexsStr = "";
			for (int i = 1; i < indexs.Length; i++) {
				if (indexsStr != "") indexsStr += "-";
				indexsStr += indexs[i];
			}
			PlayerPrefs.SetString(Define.PP_ChallengeIndexs, indexsStr);
			if (indexsStr == "") {
				var challengeFinish = PlayerPrefs.GetInt(Define.PP_ChallengeFinish);
				PlayerPrefs.SetInt(Define.PP_ChallengeFinish, challengeFinish + 1);
			}
		}

		currentGame.enabled = false;
		StopCountDown();
		gameoverPanel.SetActive(true);
		Utils.Instance.PlayAnimation(gameoverPanel.GetComponent<Animation>(), delegate() {
			resultPanel.SetActive(true);

			// percent
			float total = (float)(rightCount + wrongCount);
			if (rightCount > 0) {
				percentBarTargetSize.x *= (rightCount / total);
				percentBarRT.sizeDelta = new Vector2(0, percentBarTargetSize.y);
				percentBarRT.gameObject.SetActive(true);
			} else {
				ShowButton();
			}
		}, gameoverTime);

		Utils.Instance.DelayPlayAnimation(0.5f, gameoverPanel.transform.FindChild("Text").GetComponent<Animation>(),
			null, 0, "pop");

		resultTitle.text = Define.gameInfo[userInfo.Room.CurrentGameIndex][0];

		resultTexts[0].text = "" + rightCount;
		resultTexts[1].text = "" + wrongCount;

		// 最後一題尚未達完, 需紀錄
		if (reactionCount > 0) {
			Next();
		}

		// send statistic
		JSONObject data = new JSONObject(JSONObject.Type.OBJECT);
        data.AddField("token",      userInfo.Token);
        data.AddField("mode",       userInfo.Room.IsChallenge ? 1 : 0);
		data.AddField("game_index",	userInfo.Room.CurrentGameIndex);
        data.AddField("second",     Time.time - startTime);
		data.AddField("history",    answerJSONs);

        // LoadingPanel.Show();
        Restful.Instance.Request(Define.API_Statistic_Create, data, (json) => {
        //     LoadingPanel.Close();
		});
	}

	private void ShowButton() {
		foreach (var btn in resultButtonList) {
			btn.gameObject.SetActive(true);
		}
		Utils.Instance.DelayPlayAnimation(0.4f, resultTexts[3].GetComponent<Animation>(),
			null, 0, "pop");
	}

	public void Exit() {
		UserInfo.Instance.Room.Clear();
		Utils.Instance.FadeScene(Define.SCENE_MAIN, fading);
		Resources.UnloadUnusedAssets();
	}
}
