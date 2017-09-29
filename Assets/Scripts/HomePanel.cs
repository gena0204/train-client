using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // 使用擴充功能(ex.陣列比較), 盡量少用

public class HomePanel : MonoBehaviour {

    class RankInfo : IComparable<RankInfo> {
        public string Caption { get ; set ; }
        public int Rank { get ; set ; }
        public RankInfo(string c, int r) {
            Caption = c; Rank = r;
        }
        public int CompareTo(RankInfo other)
        {
            // 小技巧: 對兩個int或short比較, 用減法比調用比較子更快, 尤其在x64編譯環境下
            return this.Rank - other.Rank;
            //return this.Value.CompareTo(other.Value);
        }
    }

    public static int panelIndex = 0;
    public static UnityAction enterCB;
    private static bool isInit = false;

    private Fading fading;


    private Button currentBtn = null;
    private GameObject currentPanel = null;
    private Transform barImage;
    private Vector3 barTargetPos;


    // Use this for initialization
    void Start() {
        //PlayerPrefs.DeleteAll();

        Loom.Instance.enabled = true; // 無意義, 消除unity警告用
        UserInfo userInfo = UserInfo.Instance;
        Utils utils = Utils.Instance;
        Lang lang = Lang.Instance;
        SystemManager systemManager = SystemManager.Instance;
        AudioManager audioManager = AudioManager.Instance;

        AudioManager.Instance.PlayMusic((int)Define.Music.Main);

        if (!lang.isLoad()) {
            lang.setLanguage(Resources.Load<TextAsset>("lang").text, "Chinese");
        }

        fading = transform.FindChild("Panel").GetComponent<Fading>();
        

        //=============================================================
        // Panel
        //=============================================================
        GameObject homePanel = transform.FindChild("Panel/Panel_Home").gameObject;
        GameObject trainPanel = transform.FindChild("Panel/Panel_Train").gameObject;
        GameObject rankPanel = transform.FindChild("Panel/Panel_Rank").gameObject;
        GameObject menuPanel = transform.FindChild("Panel/Panel_Menu").gameObject;
        GameObject newsPanel = transform.FindChild("Panel/Panel_News").gameObject;

        var rankScrollRect = rankPanel.transform.FindChild("Scroll View").GetComponent<ScrollRect>();
        var menuScrollRect = menuPanel.transform.FindChild("Scroll View").GetComponent<ScrollRect>();
        barImage = transform.FindChild("Panel/Panel_Bottom/Image_bar");

        var rankCaptionTexts = new Text[14];
        var rankTexts = new Text[14];

        var homeBtn = transform.FindChild("Panel/Panel_Bottom/Button_Home").GetComponent<Button>();
        homeBtn.onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            SetPanel(homePanel, homeBtn);
        });

        var trainBtn = transform.FindChild("Panel/Panel_Bottom/Button_Train").GetComponent<Button>();
        trainBtn.onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);

            if (PlayerPrefs.GetInt(Define.PP_ChallengeFinish) == 0) {
                MessagePanel.ShowMessage(lang.getString("train_limit"));
                return;
            }

            SetPanel(trainPanel, trainBtn);
        });

        var isRankLoad = false;
        var rankBtn = transform.FindChild("Panel/Panel_Bottom/Button_Rank").GetComponent<Button>();
        rankBtn.onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            SetPanel(rankPanel, rankBtn);
            rankScrollRect.verticalNormalizedPosition = 1.0f;

            if (isRankLoad) {
                return;
            }

            JSONObject rankJson = new JSONObject(JSONObject.Type.OBJECT);
            rankJson.AddField("token", userInfo.Token);
            LoadingPanel.Show();
            Restful.Instance.Request(Define.API_Rank, rankJson, (json) => {
                LoadingPanel.Close();

                if (json.HasField("errcode") && (int)json["errcode"].n > 0) {
                    return;
                }

                var ranks = new RankInfo[14];

                int i = 0;
                foreach (var rank in json["ranks"].list) {
                    int value = (int)rank.n;
                    ranks[i] = new RankInfo(Define.gameInfo[i][0], value == 0 ? int.MaxValue : value);
                    i++;
                }

                // 使用Linq的穩定排序(stable)
                var result = ranks.OrderBy(item => item);

                i = 0;
                foreach (var rank in result) {
                    rankCaptionTexts[i].text = rank.Caption;
                    rankTexts[i].text = rank.Rank == int.MaxValue ? "" : rank.Rank.ToString();
                    i++;
                }

                isRankLoad = true;
            });
        });

        var menuBtn = transform.FindChild("Panel/Panel_Bottom/Button_Menu").GetComponent<Button>();
        menuBtn.onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            SetPanel(menuPanel, menuBtn);
            menuScrollRect.verticalNormalizedPosition = 1.0f;
        });

        currentPanel = homePanel;
        currentBtn = homeBtn;

        if (panelIndex == 1 && PlayerPrefs.GetInt(Define.PP_ChallengeFinish) > 0) {
            panelIndex = 0;
            SetPanel(trainPanel, trainBtn, false);
        } else {
            MoveBar(homeBtn.transform.position, false);
        }


        //=============================================================
        //  Exit
        //=============================================================
        GameObject exitImg = transform.FindChild("Panel/Image_Exit").gameObject;
        GameObject maskPanel = transform.FindChild("Panel/Panel_Mask").gameObject;

        MessagePanel.MessageHandler logoutHandler = delegate() {
            SetPanel(homePanel, homeBtn, false);

            maskPanel.SetActive(false);
            exitImg.SetActive(false);
            
            JSONObject data = new JSONObject(JSONObject.Type.OBJECT);
            data.AddField("token", userInfo.Token);
            Restful.Instance.Request(Define.API_Logout, data, (json) => {
                userInfo.Clear();
                LocalNotification.CancelNotification(1);
                SceneManager.LoadScene(Define.SCENE_LAUNCH, LoadSceneMode.Additive);
                gameObject.SetActive(false);
            });
        };

        UnityAction backExitAction = utils.CreateBackAction(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            maskPanel.SetActive(false);
            exitImg.SetActive(false);
        });
        exitImg.transform.FindChild("Button_Back").GetComponent<Button>().onClick.AddListener(backExitAction);
        exitImg.transform.FindChild("Button_Cancel").GetComponent<Button>().onClick.AddListener(backExitAction);
        exitImg.transform.FindChild("Button_Ok").GetComponent<Button>().onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            Application.Quit();
        });
        

        UnityAction exitAction = delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            maskPanel.SetActive(true);
            exitImg.SetActive(true);
            // exitImg.GetComponent<Animator>().Play("", -1, 0f);
            utils.PushBackAction(backExitAction);
        };
        transform.FindChild("Panel/Button_Exit").GetComponent<Button>().onClick.AddListener(exitAction);
        utils.SetBeginBackAction(exitAction);

        //=============================================================
        //  首頁
        //=============================================================        
        homePanel.transform.FindChild("Button_Play").GetComponent<Button>().onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);

            int[] indexs;
            var today = DateTime.Now.ToString("dd/MM/yyyy");

            var indexsStr = PlayerPrefs.GetString(Define.PP_ChallengeIndexs);
            var date = PlayerPrefs.GetString(Define.PP_ChallengeDate);
            if (date == today && indexsStr != "" && indexsStr.Count(f => f == '-') != 2) {
                indexs = indexsStr.Split('-').Select(int.Parse).ToArray();
            } else {
                int[] numbers = Enumerable.Range(0, Define.gameInfo.Count()).ToArray();
                indexs = numbers.OrderBy(n => Guid.NewGuid()).ToArray().Take(3).ToArray();
            }

            indexsStr = "";
            
            foreach (var index in indexs) {
                if (indexsStr != "") indexsStr += "-";
                indexsStr += index;
                userInfo.Room.GameIndexs.Add(index);
            }
            
            PlayerPrefs.SetString(Define.PP_ChallengeIndexs, indexsStr);
            PlayerPrefs.SetString(Define.PP_ChallengeDate, today);
            userInfo.Room.IsChallenge = true;

            utils.FadeScene(Define.SCENE_GAME_INTRO, fading);
        });

        
        for (int i = 0; i < Define.gameInfo.Count(); i++) {
            //=============================================================
            //  訓練
            //=============================================================
            int index = i;
            string name = "Scroll View/Viewport/Content/Button_" + (i + 1);
            trainPanel.transform.FindChild(name + "/Text").GetComponent<Text>().text = Define.gameInfo[i][0];
            trainPanel.transform.FindChild(name).GetComponent<Button>().onClick.AddListener(delegate() {
                audioManager.PlaySound((int)Define.Sound.Click);
                userInfo.Room.GameIndexs.Add(index);
                utils.FadeScene(Define.SCENE_GAME_INTRO, fading);
            });

            //=============================================================
            //  成績
            //=============================================================
            name = "Scroll View/Viewport/Content/Panel_" + (i + 1);
            rankCaptionTexts[i] = rankPanel.transform.FindChild(name + "/Text_Name").GetComponent<Text>();
            rankTexts[i] = rankPanel.transform.FindChild(name + "/Text_Rank").GetComponent<Text>();
        }

        var rankArrow = rankPanel.transform.FindChild("Image").gameObject;
        rankScrollRect.GetComponent<ScrollRect>().onValueChanged.AddListener(delegate(Vector2 value) {
            if (rankArrow.activeSelf && value.y <= 0) {
                rankArrow.SetActive(false);
            } else if (!rankArrow.activeSelf && value.y > 0) {
                rankArrow.SetActive(true);
            }
        });

        //=============================================================
        //  選單
        //=============================================================
        string menuContent = "Scroll View/Viewport/Content";

        var newContentPanel = newsPanel.transform.FindChild("Scroll View/Viewport/Content");
        var newRow = Resources.Load<GameObject>("Prefabs/Panel_News_Row");
        var isNewsLoad = false;

        UnityAction backMenuAction = utils.CreateBackAction(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);

            Utils.Instance.PlayAnimation(newsPanel.GetComponent<Animation>(), delegate() {
                newsPanel.SetActive(false);
            }, 0, "page_out");

            menuPanel.SetActive(true);
            Utils.Instance.PlayAnimation(menuPanel.GetComponent<Animation>(), null, 0, "page_fadein");
        });
        newsPanel.transform.FindChild("Button_Back").GetComponent<Button>().onClick.AddListener(backMenuAction);

        menuPanel.transform.FindChild(menuContent + "/Button_News").GetComponent<Button>().onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);

            newsPanel.SetActive(true);
            Utils.Instance.PlayAnimation(newsPanel.GetComponent<Animation>(), null, 0, "page_in");

            Utils.Instance.PlayAnimation(menuPanel.GetComponent<Animation>(), delegate() {
                menuPanel.SetActive(false);
            }, 0, "page_fadeout");

            utils.PushBackAction(backMenuAction);

            if (isNewsLoad) return;
            isNewsLoad = true;

            JSONObject newsJson = new JSONObject(JSONObject.Type.OBJECT);
            newsJson.AddField("token", userInfo.Token);
            LoadingPanel.Show();
            Restful.Instance.Request(Define.API_News, newsJson, (json) => {
                LoadingPanel.Close();

                if (json.HasField("errcode") && (int)json["errcode"].n > 0) {
                    return;
                }

                foreach (var news in json["news"].list) {
                    var go = Instantiate<GameObject>(newRow);
                    go.transform.SetParent(newContentPanel);
                    go.transform.localScale = Vector3.one;
                    go.transform.FindChild("Text_Date").GetComponent<Text>().text = news["create_time"].str;
                    go.transform.FindChild("Text_Content").GetComponent<Text>().text = news["content"].str;
                }
            });
        });

        menuPanel.transform.FindChild(menuContent + "/Panel_Version/Text").GetComponent<Text>().text
            += Application.version;
        
        var urls = new string[] {
            "https://sites.google.com/site/humanlearningmemory1/home", // 人類記憶實驗室
            "http://ball.ling.sinica.edu.tw/brain/index.html", // 大腦與語言實驗室
            "http://emrlab.nccu.edu.tw/", // 眼動與閱讀實驗室
            "http://www.viscol.org/" // 視覺認知實驗室
        };
        for (int i = 0; i < urls.Length; i++){
            var url = urls[i];
            menuPanel.transform.FindChild(menuContent + "/Button_" + (i + 1)).GetComponent<Button>().onClick.AddListener(delegate() {
                audioManager.PlaySound((int)Define.Sound.Click);
                Application.OpenURL(url);
            });
        }

        //------------------------
        // music
        //------------------------
        Toggle musicToggle = menuPanel.transform.FindChild(menuContent + "/Panel_Music/Toggle").GetComponent<Toggle>();
        Image musicOffBg = menuPanel.transform.FindChild(menuContent + "/Panel_Music/Toggle/Background").GetComponent<Image>();
        musicToggle.onValueChanged.AddListener(delegate(bool isOn) {
            audioManager.PlaySound((int)Define.Sound.Click);
            musicOffBg.enabled = !isOn;
            PlayerPrefs.SetInt(Define.PP_Music, isOn ? 1 : 0);
            audioManager.SetMusicEnable(isOn);
        });
        musicToggle.isOn = PlayerPrefs.GetInt(Define.PP_Music, 1) > 0;

        //------------------------
        // sound
        //------------------------
        Toggle soundToggle = menuPanel.transform.FindChild(menuContent + "/Panel_Sound/Toggle").GetComponent<Toggle>();
        Image soundOffBg = menuPanel.transform.FindChild(menuContent + "/Panel_Sound/Toggle/Background").GetComponent<Image>();
        soundToggle.onValueChanged.AddListener(delegate(bool isOn) {
            audioManager.PlaySound((int)Define.Sound.Click);
            soundOffBg.enabled = !isOn;
            PlayerPrefs.SetInt(Define.PP_Sound, isOn ? 1 : 0);
            audioManager.SetSoundEnable(isOn);
        });
        soundToggle.isOn = PlayerPrefs.GetInt(Define.PP_Sound, 1) > 0;

        menuPanel.transform.FindChild(menuContent + "/Button_Logout").GetComponent<Button>().onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            MessagePanel.ShowOkCancel(lang.getString("is_logout"), delegate() {
                logoutHandler();
            });
        });


        //=============================================================
        // Network
        //=============================================================
        var accountText = homePanel.transform.FindChild("Text_Account").GetComponent<Text>();

        UnityAction initCB = delegate() {
            accountText.text = "ID: " + userInfo.Account;
        };

        enterCB = delegate() {
            gameObject.SetActive(true);

            JSONObject jsonData = new JSONObject(JSONObject.Type.OBJECT);
            jsonData.AddField("token", userInfo.Token);
#if UNITY_ANDROID
            jsonData.AddField("os", 1);
#elif UNITY_IOS
            jsonData.AddField("os", 2);
#else 
            jsonData.AddField("os", 3);
#endif
            jsonData.AddField("os_version", SystemInfo.operatingSystem);
            jsonData.AddField("version", UnityEngine.Application.version);

            Restful.Instance.Request(Define.API_Enter, jsonData, (json) => {
                LoadingPanel.Close();

                if (json.HasField("errcode") && (int)json["errcode"].n > 0) {
                    userInfo.Clear();
                    MessagePanel.ShowMessage(json["msg"].str, delegate() {
                        SceneManager.LoadScene(Define.SCENE_LAUNCH, LoadSceneMode.Additive);
                        gameObject.SetActive(false);
                    });
                    return;
                }
                
                if (json.HasField("need_upgrade")) {
        
                }

                //------------------------------------------
                // Challenge Finish Count
                //------------------------------------------
                var today = DateTime.Now.ToString("dd/MM/yyyy");
                var date = PlayerPrefs.GetString(Define.PP_ChallengeDate);
                if (date != today) {
                    PlayerPrefs.SetInt(Define.PP_ChallengeFinish, 0);
                }

                Utils.SendRepeatingNotification();
                MessagePanel.ShowMessage(userInfo.Account + " " + lang.getString("enter_hi"));

                initCB();
                isInit = true;
            });
        };

        if (isInit) {
            initCB();
            return;
        }

        UnityAction loginCB = delegate() {
            if (userInfo.Token != "") {
                enterCB();
            } else {
                SceneManager.LoadScene(Define.SCENE_LAUNCH, LoadSceneMode.Additive);
                LoadingPanel.Close();
                gameObject.SetActive(false);
            }
        };

        LoadingPanel.Show();

        //------------------------------------------
        // Check Network
        //------------------------------------------
        switch (Network.TestConnection()) {
            case ConnectionTesterStatus.Error:
                LoadingPanel.Close();
                MessagePanel.ShowMessage(lang.getString("error_network"), delegate() {
                    Application.Quit();
                });
                return;

            default:
                break;
        }

        //------------------------------------------
        // Update
        //------------------------------------------
        JSONObject dataVersions = new JSONObject(JSONObject.Type.ARRAY);
        foreach (var version in PlayerPrefs.GetString(Define.PP_DataVersion, "").Split(',')){
            dataVersions.Add(version);
        }

        JSONObject updateJson = new JSONObject(JSONObject.Type.OBJECT);
        updateJson.AddField("version", "");
        updateJson.AddField("data_versions", dataVersions);

        Restful.Instance.Request(Define.API_Update, updateJson, (json) => {
            if (json.HasField("errcode") && (int)json["errcode"].n > 0) {
                LoadingPanel.Close();
                MessagePanel.ShowMessage(json["msg"].str, delegate() {
                    Application.Quit();
                    // userInfo.Clear();
                    // SceneManager.LoadScene(Define.SCENE_LAUNCH, LoadSceneMode.Additive);
                    // LocalNotification.CancelNotification(1);
                    // gameObject.SetActive(false);
                });
                return;
            }

            Queue actionQ = new Queue();
            UnityAction action;

            //------------------------------------------
            // version
            //------------------------------------------
            string versions = "";
            foreach (var version in json["data_versions"].list) {
                if (versions != "") versions += ",";
                versions += version.str;
            }
            PlayerPrefs.SetString(Define.PP_DataVersion, versions);

            //------------------------------------------
            // init game data
            //------------------------------------------
            bool isNeedUpdate = false;
            JSONObject gameDatas;
            if (json.HasField("games")) { // new game data
                isNeedUpdate = true;
                gameDatas = json["games"];
                PlayerPrefs.SetString(Define.PP_GameData, gameDatas.Print());

                // Loom.QueueOnMainThread(() => { // 主線程執行
                //     if (System.IO.Directory.Exists(Define.DOWNLOAD_PATH)) {
                //         // System.IO.Directory.Delete(Define.DOWNLOAD_PATH, true);
                //         // System.IO.Directory.CreateDirectory(Define.DOWNLOAD_PATH);
                //         System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Define.DOWNLOAD_PATH);
                //         foreach (System.IO.FileInfo file in dir.GetFiles()) {
                //             file.Delete(); 
                //         }
                //     }
                // });
            } else {
                gameDatas = new JSONObject(PlayerPrefs.GetString(Define.PP_GameData));
            }
            if (gameDatas.IsNull) {
                LoadingPanel.Close();
                PlayerPrefs.SetString(Define.PP_DataVersion, "");
                MessagePanel.ShowMessage(lang.getString("crash"), delegate() {
                    Application.Quit();
                });
                return;
            }

            foreach (var game in gameDatas.list) {
                var data = new GameData();
                data.index      = (int)game["index"].n;
                data.caption    = game["caption"].str;

                if (game.HasField("level")) {
                    data.level = (int)game["level"].n;
                }

                var strList = new List<string>();
                if (game.HasField("color")) {
                    foreach (var item in game["color"].list) {
                        strList.Add(item.str);
                    }
                    data.colors = strList.ToArray();
                }

                if (game.HasField("text")) {
                    strList.Clear();
                    foreach (var item in game["text"].list) {
                        strList.Add(item.str);
                    }
                    data.texts = strList.ToArray();
                }

                if (game.HasField("image")) {
                    strList.Clear();
                    foreach (var item in game["image"].list) {
                        var image = item.str;
                        if (isNeedUpdate) {
                            var url = Define.FILE_URL + image;
                            action = delegate() {
                                utils.DownloadAndSaveImage(url, delegate() {
                                    LoadingPanel.Next();
                                    PlayerPrefs.SetString(image, url);
                                    ((UnityAction)actionQ.Dequeue())();
                                });
                            };
                            actionQ.Enqueue(action);
                        }
                        strList.Add(image);
                    }
                    data.images = strList.ToArray();
                }

                systemManager.SetGameData(data);
            }

            //------------------------------------------
            // slogan
            //------------------------------------------
            JSONObject slogans;
            if (json.HasField("slogans")) { // new slogan data
                slogans = json["slogans"];
                PlayerPrefs.SetString(Define.PP_SloganData, slogans.Print());
            } else {
                slogans = new JSONObject(PlayerPrefs.GetString(Define.PP_SloganData));
            }
            if (!slogans.IsNull) {
                foreach (var slogan in slogans.list) {
                    systemManager.AddSlogan(slogan.str);
                }
            }
            
            //------------------------------------------
            // system
            //------------------------------------------
            if (json.HasField("systems")) {
                foreach (JSONObject system in json["systems"].list) {
                    systemManager.SetValue(system["key"].str, system["value"].str);
                }
            }

            action = delegate() {
                loginCB();
            };
            actionQ.Enqueue(action);
            ((UnityAction)actionQ.Dequeue())();
        });
    }

    // Update is called once per frame
    void Update () {
		if (barImage.position == barTargetPos) {
			return;
		}

		barImage.position = Vector3.Lerp(barImage.position, barTargetPos, 10.0f * Time.deltaTime);
	}

    private void SetPanel(GameObject panel, Button btn, bool animation = true) {
        currentPanel.SetActive(false);
        panel.SetActive(true);
        currentBtn.interactable = true;
        btn.interactable = false;
        currentPanel = panel;
        currentBtn = btn;
        MoveBar(btn.transform.position, animation);
    }

    private void MoveBar(Vector3 pos, bool animation = true) {
        pos.y = barImage.position.y;
        barTargetPos = pos;
        if (!animation) {
            barImage.position = barTargetPos;
        }
    }
}
