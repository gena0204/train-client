using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // 使用擴充功能(ex.陣列比較), 盡量少用
using System.Net;
using System.Net.Sockets;
using Language;

public class HomePanel : MonoBehaviour {

    [SerializeField]
    private Fading fading;

    [SerializeField]
    private Font[] fonts;
    [SerializeField]
    private int[] fontSizes;

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

    private Button currentBtn = null;
    private GameObject currentPanel = null;
    private Transform barImage;
    private Vector3 barTargetPos;

    void Awake () {
        if (!Lang.Instance.isLoad()) {
            string language = PlayerPrefs.GetString(Define.PP_Language, "Chinese");
            Lang.Instance.setLanguage(Resources.Load<TextAsset>("lang").text, language);
            LanguageService.Instance.Language = new LanguageInfo(language);
            Restful.Instance.AcceptLanguage = language == "Chinese" ? "zh-TW" : "en";
        }
	}

    // Use this for initialization
    void Start() {
        // PlayerPrefs.DeleteAll();

        UserInfo userInfo = UserInfo.Instance;
        Utils utils = Utils.Instance;
        Lang lang = Lang.Instance;
        SystemManager systemManager = SystemManager.Instance;
        AudioManager audioManager = AudioManager.Instance;

        AudioManager.Instance.PlayMusic((int)Define.Music.Main);
        // Loom.Instance.enabled = true; // 無意義, 消除unity警告用
        Restful.Instance.Timeout = 20;
        // Restful.Instance.RetryLimit = 3;
        

        //=============================================================
        // Panel
        //=============================================================
        GameObject homePanel = transform.Find("Panel/Panel_Home").gameObject;
        GameObject trainPanel = transform.Find("Panel/Panel_Train").gameObject;
        GameObject rankPanel = transform.Find("Panel/Panel_Rank").gameObject;
        GameObject menuPanel = transform.Find("Panel/Panel_Menu").gameObject;
        GameObject newsPanel = transform.Find("Panel/Panel_News").gameObject;

        var rankScrollRect = rankPanel.transform.Find("Scroll View").GetComponent<ScrollRect>();
        var menuScrollRect = menuPanel.transform.Find("Scroll View").GetComponent<ScrollRect>();
        barImage = transform.Find("Panel/Panel_Bottom/Image_bar");

        var trainCaptionTexts = new Text[Define.gameInfo.Count()];
        var rankCaptionTexts = new Text[Define.gameInfo.Count()];
        var rankTexts = new Text[Define.gameInfo.Count()];

        var challengeDate = PlayerPrefs.GetString(Define.PP_ChallengeDate);

        var homeBtn = transform.Find("Panel/Panel_Bottom/Button_Home").GetComponent<Button>();
        homeBtn.onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            SetPanel(homePanel, homeBtn);
        });

        var trainBtn = transform.Find("Panel/Panel_Bottom/Button_Train").GetComponent<Button>();
        trainBtn.onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);

// #if !UNITY_EDITOR
            if (systemManager.GetInt("challeng_limit_enable") == 1 && challengeDate != DateTime.Now.ToString("dd/MM/yyyy")) {
                MessagePanel.SetOkCancelText(LanguageService.Instance.GetStringByKey("UIHome.BtnPlay", "GO!"));
                MessagePanel.ShowMessage(lang.getString("train_limit"), delegate() {
                    homePanel.transform.Find("Button_Play").GetComponent<Button>().onClick.Invoke();
                });
                return;
            }
// #endif

            SetPanel(trainPanel, trainBtn);
        });

        var isRankLoad = false;
        var langRemoveIndexs = new int[] {};
        var rankBtn = transform.Find("Panel/Panel_Bottom/Button_Rank").GetComponent<Button>();
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

                var ranks = new List<RankInfo>();

                if (!json.HasField("ranks") || !json["ranks"].IsArray) {
                    for (int i = 0; i < ranks.Count(); i++) {
                        if (langRemoveIndexs.Contains(i)) {
                            continue;
                        }
                        ranks.Add(new RankInfo(lang.getString("game_name_" + (i + 1)), int.MaxValue));
                    }
                } else {
                    int i = 0;
                    foreach (var rank in json["ranks"].list) {
                        if (langRemoveIndexs.Contains(i)) {
                            i++; continue;
                        }
                        int value = (int)rank.n;
                        ranks.Add(new RankInfo(lang.getString("game_name_" + (i + 1)), value == 0 ? int.MaxValue : value));
                        i++;
                    }
                }
                
                // 使用Linq的穩定排序(stable)
                var result = ranks.OrderBy(item => item);

                int index = 0;
                foreach (var rank in result) {
                    rankCaptionTexts[index].text = rank.Caption;
                    rankTexts[index].text = rank.Rank == int.MaxValue ? "" : rank.Rank.ToString();
                    index++;
                }

                isRankLoad = true;
            });
        });

        var menuBtn = transform.Find("Panel/Panel_Bottom/Button_Menu").GetComponent<Button>();
        menuBtn.onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            SetPanel(menuPanel, menuBtn);
            menuScrollRect.verticalNormalizedPosition = 1.0f;
        });

        currentPanel = homePanel;
        currentBtn = homeBtn;

        if (panelIndex == 1) {
            panelIndex = 0;
            SetPanel(trainPanel, trainBtn, false);
        } else {
            MoveBar(homeBtn.transform.position, false);
        }


        //=============================================================
        //  Exit
        //=============================================================
        GameObject exitImg = transform.Find("Panel/Image_Exit").gameObject;
        GameObject maskPanel = transform.Find("Panel/Panel_Mask").gameObject;

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
        exitImg.transform.Find("Button_Back").GetComponent<Button>().onClick.AddListener(backExitAction);
        exitImg.transform.Find("Button_Cancel").GetComponent<Button>().onClick.AddListener(backExitAction);
        exitImg.transform.Find("Button_Ok").GetComponent<Button>().onClick.AddListener(delegate() {
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
        transform.Find("Panel/Button_Exit").GetComponent<Button>().onClick.AddListener(exitAction);
        utils.SetBeginBackAction(exitAction);

        //=============================================================
        //  首頁
        //=============================================================        
        homePanel.transform.Find("Button_Play").GetComponent<Button>().onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);

            int[] indexs;

            var indexsStr = PlayerPrefs.GetString(Define.PP_ChallengeIndexs);
            if (indexsStr != "" && indexsStr.Count(f => f == '-') != 2) {
                indexs = indexsStr.Split('-').Select(int.Parse).ToArray();
            } else {
                int[] numbers = Enumerable.Range(0, Define.gameInfo.Count()).Where(v => !langRemoveIndexs.Contains(v)).ToArray();
                indexs = numbers.OrderBy(n => Guid.NewGuid()).ToArray().Take(3).ToArray();
            }

            indexsStr = "";
            
            foreach (var index in indexs) {
                if (indexsStr != "") indexsStr += "-";
                indexsStr += index;
                userInfo.Room.GameIndexs.Add(index);
            }
            
            PlayerPrefs.SetString(Define.PP_ChallengeIndexs, indexsStr);
            userInfo.Room.IsChallenge = true;

            utils.FadeScene(Define.SCENE_GAME_INTRO, fading);
        });

        var groups = new int[][] {
            new int[] {2, 4, 14, 15, 19},
            new int[] {5, 6, 7, 8, 20},
            new int[] {9, 10, 11},
            new int[] {1, 3, 12, 13, 16},
            new int[] {17, 18, 21, 22},
        };
        for (int i = 0; i < groups.Length; i++) {
            for (int j = 0; j < groups[i].Length; j++) {
                //=============================================================
                //  訓練
                //=============================================================
                int index = groups[i][j] - 1;
                string name = "Scroll View/Viewport/Content/ScrollView_" + (i+1) + "/Viewport/Content/Button_" + groups[i][j];
                trainCaptionTexts[index] = trainPanel.transform.Find(name + "/Text").GetComponent<Text>();
                trainCaptionTexts[index].text = lang.getString("game_name_" + (index + 1));
                trainPanel.transform.Find(name).GetComponent<Button>().onClick.AddListener(delegate() {
                    audioManager.PlaySound((int)Define.Sound.Click);
                    userInfo.Room.GameIndexs.Add(index);
                    utils.FadeScene(Define.SCENE_GAME_INTRO, fading);
                });

                //=============================================================
                //  成績
                //=============================================================
                name = "Scroll View/Viewport/Content/Panel_" + groups[i][j];
                rankCaptionTexts[index] = rankPanel.transform.Find(name + "/Text_Name").GetComponent<Text>();
                rankTexts[index] = rankPanel.transform.Find(name + "/Text_Rank").GetComponent<Text>();
            }
        }
        var groupTexts = new Text[] {
            trainPanel.transform.Find("Scroll View/Viewport/Content/Text_1").GetComponent<Text>(),
            trainPanel.transform.Find("Scroll View/Viewport/Content/Text_2").GetComponent<Text>(),
            trainPanel.transform.Find("Scroll View/Viewport/Content/Text_3").GetComponent<Text>(),
            trainPanel.transform.Find("Scroll View/Viewport/Content/Text_4").GetComponent<Text>(),
            trainPanel.transform.Find("Scroll View/Viewport/Content/ScrollView_5/Text_5").GetComponent<Text>(),
        };

        var rankArrow = rankPanel.transform.Find("Image").gameObject;
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

        var newContentPanel = newsPanel.transform.Find("Scroll View/Viewport/Content");
        var newRow = Resources.Load<GameObject>("Prefabs/Panel_News_Row");
        var isNewsLoad = false;

        UnityAction backMenuAction = utils.CreateBackAction(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);

            Utils.Instance.PlayAnimation(newsPanel.GetComponent<Animation>(), "page_out", delegate() {
                newsPanel.SetActive(false);
            });

            menuPanel.SetActive(true);
            Utils.Instance.PlayAnimation(menuPanel.GetComponent<Animation>(), "page_fadein");
        });
        newsPanel.transform.Find("Button_Back").GetComponent<Button>().onClick.AddListener(backMenuAction);

        menuPanel.transform.Find(menuContent + "/Button_News").GetComponent<Button>().onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);

            newsPanel.SetActive(true);
            Utils.Instance.PlayAnimation(newsPanel.GetComponent<Animation>(), "page_in");

            Utils.Instance.PlayAnimation(menuPanel.GetComponent<Animation>(), "page_fadeout", delegate() {
                menuPanel.SetActive(false);
            });

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
                    var text = news["content"].str;
                    text = text.Replace("\\r\\n", "\n"); 
                    text = text.Replace("\\n", "\n");

                    var go = Instantiate<GameObject>(newRow);
                    go.transform.SetParent(newContentPanel);
                    go.transform.localScale = Vector3.one;
                    go.transform.Find("Text_Date").GetComponent<Text>().text = news["create_time"].str;
                    go.transform.Find("Text_Content").GetComponent<Text>().text = text;
                }
            });
        });

        menuPanel.transform.Find(menuContent + "/Panel_Version/Text").GetComponent<Text>().text
            += Application.version;
        
        var urls = new string[] {
            "https://sites.google.com/site/humanlearningmemory1/home", // 人類記憶實驗室
            "http://ball.ling.sinica.edu.tw/brain/index.html", // 大腦與語言實驗室
            "http://emrlab.nccu.edu.tw/", // 眼動與閱讀實驗室
            "http://www.viscol.org/" // 視覺認知實驗室
        };
        for (int i = 0; i < urls.Length; i++){
            var index = i;
            menuPanel.transform.Find(menuContent + "/Button_" + (i + 1)).GetComponent<Button>().onClick.AddListener(delegate() {
                audioManager.PlaySound((int)Define.Sound.Click);
                Application.OpenURL(urls[index]);
            });
        }

        //------------------------
        // music
        //------------------------
        Toggle musicToggle = menuPanel.transform.Find(menuContent + "/Panel_Music/Toggle").GetComponent<Toggle>();
        Image musicOffBg = menuPanel.transform.Find(menuContent + "/Panel_Music/Toggle/Background").GetComponent<Image>();
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
        Toggle soundToggle = menuPanel.transform.Find(menuContent + "/Panel_Sound/Toggle").GetComponent<Toggle>();
        Image soundOffBg = menuPanel.transform.Find(menuContent + "/Panel_Sound/Toggle/Background").GetComponent<Image>();
        soundToggle.onValueChanged.AddListener(delegate(bool isOn) {
            audioManager.PlaySound((int)Define.Sound.Click);
            soundOffBg.enabled = !isOn;
            PlayerPrefs.SetInt(Define.PP_Sound, isOn ? 1 : 0);
            audioManager.SetSoundEnable(isOn);
        });
        soundToggle.isOn = PlayerPrefs.GetInt(Define.PP_Sound, 1) > 0;

        //------------------------
        // language
        //------------------------
        LanguageText[] langTexts = Resources.FindObjectsOfTypeAll(typeof(LanguageText)) as LanguageText[];
        var langDropdown = menuPanel.transform.Find(menuContent + "/Panel_Language/Dropdown").GetComponent<Dropdown>();
        // var trainPanel5 = trainPanel.transform.Find("Scroll View/Viewport/Content/ScrollView_5").gameObject;
        var trainImg2 = trainPanel.transform.Find("Scroll View/Viewport/Content/ScrollView_1/Viewport/Content/Button_2").GetComponent<Image>();
        var trainImg17 = trainPanel.transform.Find("Scroll View/Viewport/Content/ScrollView_5/Viewport/Content/Button_17").GetComponent<Image>();
        var langList = new List<string>() {lang.getString("lang_1"), lang.getString("lang_2")};
        var langs = new List<string>() {"Chinese", "English"};

        var trainItems = new GameObject[] {
            trainPanel.transform.Find("Scroll View/Viewport/Content/ScrollView_5/Viewport/Content/Button_21").gameObject,
            trainPanel.transform.Find("Scroll View/Viewport/Content/ScrollView_5/Viewport/Content/Button_22").gameObject,
        };

        var rankItems = new GameObject[] {
            rankPanel.transform.Find("Scroll View/Viewport/Content/Panel_19").gameObject,
            rankPanel.transform.Find("Scroll View/Viewport/Content/Panel_20").gameObject,
            rankPanel.transform.Find("Scroll View/Viewport/Content/Panel_21").gameObject,
            rankPanel.transform.Find("Scroll View/Viewport/Content/Panel_22").gameObject,
        };

        UnityAction<int> changeTrainLang = delegate(int value) {
            var font = fonts[value];
            var fontSize = fontSizes[value];
            foreach (var text in groupTexts) {
				text.font = font;
                text.fontSize = fontSize;
            }

            var isChinese = value == 0;
            // trainPanel5.SetActive(isChinese);
            foreach (var item in trainItems) {
				item.SetActive(isChinese);
            }
            trainImg2.sprite = Resources.Load<Sprite>("Sprites/train_no_2" + (isChinese ? "" : "_en"));
            trainImg17.sprite = Resources.Load<Sprite>("Sprites/train2_no_3" + (isChinese ? "" : "_en"));

            isRankLoad = false;
            langRemoveIndexs = isChinese ? new int[] {} : new int[] {20, 21};
            foreach (var item in rankItems) {
				item.SetActive(isChinese);
            }
        };

        langDropdown.AddOptions(langList);
        langDropdown.value = langList.IndexOf(PlayerPrefs.GetString(Define.PP_Language, "Chinese"));
        langDropdown.onValueChanged.AddListener(delegate {
            string language = langs[langDropdown.value];
            PlayerPrefs.SetString(Define.PP_Language, language);
            lang.setLanguage(Resources.Load<TextAsset>("lang").text, language);

            LanguageService.Instance.Language = new LanguageInfo(language);
			foreach (var text in langTexts) {
				text.Reload();
			}

            changeTrainLang(langDropdown.value);

            for (int i = 0; i < Define.gameInfo.Length; i++) {
                trainCaptionTexts[i].text = lang.getString("game_name_" + (i + 1));
            }

            for (int i = 0; i < 4; i++) {
                urls[i] = systemManager.GetValue("link_" + (language == "Chinese" ? "tw_" : "en_") + (i+1));
            }

            // 重整挑戰模式關卡
            var indexsStr = PlayerPrefs.GetString(Define.PP_ChallengeIndexs);
            if (indexsStr != "") {
                int[] numbers = Enumerable.Range(0, Define.gameInfo.Count()).Where(v => !langRemoveIndexs.Contains(v)).ToArray();
                int[] indexs = numbers.OrderBy(n => Guid.NewGuid()).ToArray().Take(3).ToArray();
                indexsStr = "";
            
                foreach (var index in indexs) {
                    if (indexsStr != "") indexsStr += "-";
                    indexsStr += index;
                }
                
                PlayerPrefs.SetString(Define.PP_ChallengeIndexs, indexsStr);
            }

            Restful.Instance.AcceptLanguage = language == "Chinese" ? "zh-TW" : "en";
        });
        changeTrainLang(langDropdown.value);

        menuPanel.transform.Find(menuContent + "/Button_Logout").GetComponent<Button>().onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            MessagePanel.ShowOkCancel(lang.getString("is_logout"), delegate() {
                logoutHandler();
            });
        });


        //=============================================================
        // Network
        //=============================================================
        var accountText = homePanel.transform.Find("Text_Account").GetComponent<Text>();

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
                    MessagePanel.ShowMessage(json["msg"].str, delegate() {
                        Application.Quit();
                    });
                    return;
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
        if (Application.internetReachability == NetworkReachability.NotReachable) {
            LoadingPanel.Close();
            MessagePanel.ShowMessage(lang.getString("no_network"), delegate() {
                Application.Quit();
            });
            return;
        } /*else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork) { // WiFi
        } else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork) { // 行動網路
        }*/

        // 判斷網絡環境 IPV4/IPV6 (主機不支援IPV6，須利用工作室轉跳，iOS送審用)
#if UNITY_IOS
        try {
            IPAddress[] address = Dns.GetHostAddresses("www.apple.com");
            if (address[0].AddressFamily == AddressFamily.InterNetworkV6) {
                Define.RESTFUL_URL      = Define.RESTFUL_URL_IPV6;
                Define.WEBSOCKET_URL    = Define.WEBSOCKET_URL_IPV6;
                Define.FILE_URL         = Define.FILE_URL_IPV6;
            }
        } catch {
        }
#endif

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

        RestfulHandler updateHandler = null;
        updateHandler = (json) => {
            if (json.HasField("errcode") && (int)json["errcode"].n > 0) {
                if ((int)json["errcode"].n == 10 && Define.RESTFUL_URL == Define.RESTFUL_URL_IPV4) {
                    Define.RESTFUL_URL      = Define.RESTFUL_URL_IPV6;
                    Define.WEBSOCKET_URL    = Define.WEBSOCKET_URL_IPV6;
                    Define.FILE_URL         = Define.FILE_URL_IPV6;
                    Restful.Instance.Request(Define.API_Update, updateJson, updateHandler);
                    return;
                }

                LoadingPanel.Close();
                MessagePanel.ShowMessage(json["msg"].str, delegate() {
                    Application.Quit();
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

                if (game.HasField("second")) {
                    data.second = (int)game["second"].n;
                }

                systemManager.SetGameData(data);
            }

            // extra game data
            JSONObject gameExtras;
            if (json.HasField("game_extras")) { // new game extra data
                gameExtras = json["game_extras"];
                var oldGameExtras = new JSONObject(PlayerPrefs.GetString(Define.PP_GameExtra));
                if (oldGameExtras != null) {
                    for (int i = 0; i < gameExtras.list.Count; i++) {
                        if (!gameExtras.list[i].HasField("data")) {
                            gameExtras.list[i] = oldGameExtras.list[i];
                        }
                    }
                }
                PlayerPrefs.SetString(Define.PP_GameExtra, gameExtras.Print());
            } else {
                gameExtras = new JSONObject(PlayerPrefs.GetString(Define.PP_GameExtra));
            }
            foreach (var game in gameExtras.list) {
                var data = systemManager.GetGameData((int)game["index"].n);
                data.dataJSON = game["data"].str.Replace("\\\"", "\"");
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

            string language = PlayerPrefs.GetString(Define.PP_Language, "Chinese");
            for (int i = 0; i < 4; i++) {
                urls[i] = systemManager.GetValue("link_" + (language == "Chinese" ? "tw_" : "en_") + (i+1));
            }

            action = delegate() {
                loginCB();
            };
            actionQ.Enqueue(action);
            ((UnityAction)actionQ.Dequeue())();
        };
        Restful.Instance.Request(Define.API_Update, updateJson, updateHandler);
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
