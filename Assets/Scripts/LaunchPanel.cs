using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LaunchPanel : MonoBehaviour {

    [SerializeField]
    private Fading fading;

    // Use this for initialization
    void Start() {
        UserInfo userInfo = UserInfo.Instance;
        AudioManager audioManager = AudioManager.Instance;
        Utils utils = Utils.Instance;
        Lang lang = Lang.Instance;

        // GameObject startImg = transform.FindChild("Image_Start").gameObject;
        GameObject loginImg = transform.Find("Image_Login").gameObject;
        // GameObject contactPanel = transform.FindChild("Panel_Contact").gameObject;
        GameObject privacyPanel = transform.Find("Panel_Privacy").gameObject;
        GameObject registerImg = transform.Find("Image_Register").gameObject;
        GameObject register2Img = transform.Find("Image_Register_2").gameObject;
       
    
        // -------------------------------------------------
        // Register 2
        // -------------------------------------------------
        InputField accountRInput = registerImg.transform.Find("InputField_Account").GetComponent<InputField>();
        InputField passwordRInput = registerImg.transform.Find("InputField_Password").GetComponent<InputField>();

        UnityAction backReg2Action = utils.CreateBackAction(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            Utils.Instance.PlayAnimation(register2Img.GetComponent<Animation>(), "page_out");
            Utils.Instance.PlayAnimation(registerImg.GetComponent<Animation>(), "page_fadein");
        });
        register2Img.transform.Find("Button_Back").GetComponent<Button>().onClick.AddListener(backReg2Action);

        int currentSexIndex = 0;
        var sexToggles = new Toggle[3];
        var sexTexts = new Text[3];
        var sexBgs = new Image[3];
        var color = new Color(255/255.0f, 159/255.0f, 56/255.0f);
        for (int i = 0; i < 3; i++) {
            int index = i;
            sexToggles[i] = register2Img.transform.Find("Toggle_" + (i + 1)).GetComponent<Toggle>();
            sexTexts[i] = register2Img.transform.Find("Toggle_" + (i + 1) + "/Label").GetComponent<Text>();
            sexBgs[i]= register2Img.transform.Find("Toggle_" + (i + 1) + "/Background").GetComponent<Image>();
            sexToggles[i].onValueChanged.AddListener(delegate(bool isOn) {
                audioManager.PlaySound((int)Define.Sound.Click);
                sexBgs[index].enabled = !isOn;
                sexTexts[index].color = color;
                sexBgs[index].enabled = true;
                sexTexts[currentSexIndex].color = Color.white;
                sexToggles[currentSexIndex].isOn = false;
                sexToggles[currentSexIndex].enabled = true;
                sexToggles[index].enabled = false;
                currentSexIndex = index;
            });
        }

        var yearDropdown = register2Img.transform.Find("Dropdown_Year").GetComponent<Dropdown>();
        var year = System.DateTime.Now.Year;
        var yearList = new List<string>();
        for (int i = year; i >= (year - 100); i--) {
            yearList.Add(i.ToString());
        }
        yearDropdown.AddOptions(yearList);

        var educationDropdown = register2Img.transform.Find("Dropdown_Education").GetComponent<Dropdown>();
        var educationList = new List<string>();
        for (int i = 0; i < 6; i++) {
            educationList.Add(lang.getString("education_" + (i + 1)));
        }
        educationDropdown.AddOptions(educationList);

        register2Img.transform.Find("Button_Register").GetComponent<Button>().onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            if (LoadingPanel.IsShow()) {
                return;
            }

            LoadingPanel.Show();
            // password = passwordRInput.text;

            JSONObject data = new JSONObject(JSONObject.Type.OBJECT);
            data.AddField("account", accountRInput.text);
            data.AddField("password", passwordRInput.text);
            data.AddField("sex", currentSexIndex);
            data.AddField("birth", yearList[yearDropdown.value]);
            data.AddField("education", educationDropdown.value);

            Restful.Instance.Request(Define.API_Register, data, (json) => {
                if (json.HasField("errcode") && (int)json["errcode"].n > 0) {
                    MessagePanel.ShowMessage(json["msg"].str);
                } else {
                    MessagePanel.ShowMessage(lang.getString("register_success"));

                    userInfo.Token = userInfo.LocalToken = json["token"].str;
                    userInfo.Account = userInfo.LocalAccount = accountRInput.text;

                    Restful.Instance.Authorization = "Bearer " + userInfo.Token;

                    // Destroy(GameObject.Find("Canvas_Launch"));
                    SceneManager.UnloadSceneAsync(Define.SCENE_LAUNCH);
                    utils.PopBackAction(); // pop exit dialog

                    HomePanel.enterCB();
                }

                LoadingPanel.Close();
            });
        });

        // -------------------------------------------------
        // Register
        // -------------------------------------------------
        UnityAction backRegAction = utils.CreateBackAction(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            Utils.Instance.PlayAnimation(registerImg.GetComponent<Animation>(), "page_out");
            Utils.Instance.PlayAnimation(loginImg.GetComponent<Animation>(), "page_fadein");
        });
        registerImg.transform.Find("Button_Back").GetComponent<Button>().onClick.AddListener(backRegAction);

        bool isPrivacyAgree = false;
        registerImg.transform.Find("Toggle_Privacy").GetComponent<Toggle>().onValueChanged.AddListener(delegate(bool isOn) {
            audioManager.PlaySound((int)Define.Sound.Click);
            isPrivacyAgree = isOn;
        });

        registerImg.transform.Find("Button_Next").GetComponent<Button>().onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            if (LoadingPanel.IsShow()) {
                return;
            }

            if (accountRInput.text == "") {
                MessagePanel.ShowMessage(lang.getString("input_account"));
            } else if (passwordRInput.text == "") {
                MessagePanel.ShowMessage(lang.getString("input_password"));
            } else if (passwordRInput.text.Length < 4) {
                MessagePanel.ShowMessage(lang.getString("input_password_len"));
            } else if (!isPrivacyAgree) {
                MessagePanel.ShowMessage(lang.getString("need_privacy"));
            } else {
                LoadingPanel.Show();

                JSONObject data = new JSONObject(JSONObject.Type.OBJECT);
                data.AddField("account", accountRInput.text);

                Restful.Instance.Request(Define.API_AccountCheck, data, (json) => {
                    LoadingPanel.Close();

                    if (json.HasField("errcode") && (int)json["errcode"].n > 0) {
                        MessagePanel.ShowMessage(json["msg"].str);
                        return;
                    }

                    register2Img.SetActive(true);
                    Utils.Instance.PlayAnimation(registerImg.GetComponent<Animation>(), "page_fadeout");
                    Utils.Instance.PlayAnimation(register2Img.GetComponent<Animation>(), "page_in");
                    utils.PushBackAction(backReg2Action);
                });
            }
        });

        UnityAction backPrivacyAction = utils.CreateBackAction(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            Utils.Instance.PlayAnimation(privacyPanel.GetComponent<Animation>(), "page_out", delegate() {
                privacyPanel.SetActive(false);
            });
            Utils.Instance.PlayAnimation(registerImg.GetComponent<Animation>(), "page_fadein");
        });
        privacyPanel.transform.Find("Button_Back").GetComponent<Button>().onClick.AddListener(backPrivacyAction);

        privacyPanel.transform.Find("Text_Title").GetComponent<Text>().text = 
            lang.getString("privacy_title");
        privacyPanel.transform.Find("Scroll View/Viewport/Content/Text").GetComponent<Text>().text = 
            lang.getString("privacy_content");

        registerImg.transform.Find("Button_Privacy").GetComponent<Button>().onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            privacyPanel.SetActive(true);
            Utils.Instance.PlayAnimation(privacyPanel.GetComponent<Animation>(), "page_in");
            Utils.Instance.PlayAnimation(registerImg.GetComponent<Animation>(), "page_fadeout");
            utils.PushBackAction(backPrivacyAction);
        });

        // -------------------------------------------------
        // Login
        // -------------------------------------------------
        InputField accountInput = loginImg.transform.Find("InputField_Account").GetComponent<InputField>();
        InputField passwordInput = loginImg.transform.Find("InputField_Password").GetComponent<InputField>();

        UnityAction existAction = delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            MessagePanel.ShowOkCancel(lang.getString("is_exit"), delegate() {
                Application.Quit();
            });
        };

        // UnityAction backLoginAction = utils.CreateBackAction(delegate() {
        //     audioManager.PlaySound((int)Define.Sound.Click);
        //     loginImg.SetActive(false);
        //     startImg.SetActive(true);
        // });
        // UnityAction backLoginAction = utils.CreateBackAction(existAction);
        UnityAction backLoginAction = existAction;
        
        loginImg.transform.Find("Button_Back").GetComponent<Button>().onClick.AddListener(backLoginAction);

        loginImg.transform.Find("Button_Login").GetComponent<Button>().onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            if (accountInput.text == "") {
                MessagePanel.ShowMessage(lang.getString("input_account"));
            } else if (passwordInput.text == "") {
                MessagePanel.ShowMessage(lang.getString("input_password"));
            } /*else if (passwordInput.text.Length < 4) {
                MessagePanel.ShowMessage(lang.getString("input_password_len"));
            }*/ else {
                // password = passwordInput.text;

                JSONObject data = new JSONObject(JSONObject.Type.OBJECT);
                data.AddField("account", accountInput.text);
                data.AddField("password", passwordInput.text);

                Restful.Instance.Request(Define.API_Login, data, (json) => {
                    if (json.HasField("errcode") && (int)json["errcode"].n > 0) {
                        MessagePanel.ShowMessage(json["msg"].str);
                    }
                    else {
                        userInfo.Token = userInfo.LocalToken = json["token"].str;
                        userInfo.Account = userInfo.LocalAccount = accountInput.text;

                        Restful.Instance.Authorization = "Bearer " + userInfo.Token;

                        // Destroy(GameObject.Find("Canvas_Launch"));
                        SceneManager.UnloadSceneAsync(Define.SCENE_LAUNCH);
                        utils.PopBackAction(); // pop exit dialog

                        HomePanel.enterCB();
                    }

                    LoadingPanel.Close();
                });
            }
        });

        loginImg.transform.Find("Button_Register").GetComponent<Button>().onClick.AddListener(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            registerImg.SetActive(true);
            registerImg.GetComponent<CanvasGroup>().alpha = 1;
            Utils.Instance.PlayAnimation(loginImg.GetComponent<Animation>(), "page_fadeout");
            Utils.Instance.PlayAnimation(registerImg.GetComponent<Animation>(), "page_in");
            utils.PushBackAction(backRegAction);
        });

        // UnityAction backContactAction = utils.CreateBackAction(delegate() {
        //     audioManager.PlaySound((int)Define.Sound.Click);
        //     Utils.Instance.PlayAnimation(contactPanel.GetComponent<Animation>(), delegate() {
        //         contactPanel.SetActive(false);
        //     }, 0, "page_out");
        //     Utils.Instance.PlayAnimation(loginImg.GetComponent<Animation>(), null, 0, "page_fadein");
        // });
        // contactPanel.transform.FindChild("Button_Back").GetComponent<Button>().onClick.AddListener(backContactAction);

        // loginImg.transform.FindChild("Button_Contact").GetComponent<Button>().onClick.AddListener(delegate() {
        //     audioManager.PlaySound((int)Define.Sound.Click);
        //     contactPanel.SetActive(true);
        //     Utils.Instance.PlayAnimation(contactPanel.GetComponent<Animation>(), null, 0, "page_in");
        //     Utils.Instance.PlayAnimation(loginImg.GetComponent<Animation>(), null, 0, "page_fadeout");
        //     utils.PushBackAction(backContactAction);
        // });

        // var urls = new string[] {
        //     "https://sites.google.com/site/humanlearningmemory1/home", // 人類記憶實驗室
        //     "http://ball.ling.sinica.edu.tw/brain/index.html", // 大腦與語言實驗室
        //     "http://emrlab.nccu.edu.tw/", // 眼動與閱讀實驗室
        //     "http://www.viscol.org/" // 視覺認知實驗室
        // };
        // string contactContent = "Scroll View/Viewport/Content";
        // for (int i = 0; i < urls.Length; i++){
        //     var url = urls[i];
        //     contactImg.transform.FindChild(contactContent + "/Button_" + (i + 1)).GetComponent<Button>().onClick.AddListener(delegate() {
        //         audioManager.PlaySound((int)Define.Sound.Click);
        //         Application.OpenURL(url);
        //     });
        // }
        


        // -------------------------------------------------
        // Start
        // -------------------------------------------------
        // btn = startImg.transform.FindChild("Button_Login").GetComponent<Button>();
        // btn.onClick.AddListener(delegate() {
        //     audioManager.PlaySound((int)Define.Sound.Click);
        //     startImg.SetActive(false);
        //     loginImg.SetActive(true);
        //     utils.PushBackAction(backLoginAction);
        // });

        // btn = startImg.transform.FindChild("Button_Start").GetComponent<Button>();
        // btn.onClick.AddListener(delegate() {
        //     audioManager.PlaySound((int)Define.Sound.Click);
        //     JSONObject data = new JSONObject(JSONObject.Type.OBJECT);
        //     string account = userInfo.LocalAccount;
        //     if (account != "") { // 自動登入
        //         data.AddField("channel", Define.Channel_C2S_Login);
        //         data.AddField("account", account);
        //     }
        //     else {
        //         data.AddField("channel", Define.Channel_C2S_Register);
        //         data.AddField("account", "");
        //     }
        //     data.AddField("password", "");
        //     services.Send(data.Print());
        // });

        // -------------------------------------------------
        // init
        // -------------------------------------------------
        utils.PushBackAction(existAction);

// #if UNITY_IOS
//         if (PlayerPrefs.GetInt(Define.PP_Privacy, 0) == 0) {
//             Application.LoadLevelAdditive(Define.SCENE_PRIVACY);
//         }
// #endif
    }

    // Update is called once per frame
    void Update() {

    }
}
