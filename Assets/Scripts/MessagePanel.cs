// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.Events;
// using UnityEngine.SceneManagement;
// using System;
// using System.Collections;


// public class MessagePanel : MonoBehaviour {

//     public delegate void MessageHandler();

//     private static string message = "";
//     private static bool isOkCancel = false;
//     private static MessageHandler okHandler = null;
// 	private static bool isClose = false;

//     private static string okText = "";
//     private static string cancelText = "";
    

//     public static void ShowMessage(string text, MessageHandler handler = null) {
//         message = text;
//         okHandler = handler;
//         isOkCancel = false;
//         SceneManager.LoadSceneAsync(Define.SCENE_MESSAGE, LoadSceneMode.Additive);
//     }

//     public static void ShowOkCancel(string text, MessageHandler handler = null) {
//         message = text;
//         okHandler = handler;
//         isOkCancel = true;
//         SceneManager.LoadSceneAsync(Define.SCENE_MESSAGE, LoadSceneMode.Additive);
//     }

//     public static void SetOkCancelText(string ok, string cancel = "") {
//         okText = ok; cancelText = cancel;
//     }

//     public static void Close() {
// 		isClose = true;
//     }

// 	// Use this for initialization
// 	void Start () {
//         Utils utils = Utils.Instance;
//         AudioManager audioManager = AudioManager.Instance;

//         Transform msgImg = transform.Find("Panel_Message/Image_Message");
//         msgImg.Find("Text_Content").GetComponent<Text>().text = message;

//         MessageHandler handler = okHandler;
//         okHandler = null;

//         UnityAction okAction = utils.CreateBackAction(delegate() {
//             audioManager.PlaySound((int)Define.Sound.Click);
//             if (handler != null) {
//                 handler();
//             }
//             Exit();
//         });

//         UnityAction cancelAction = null;

//         if (isOkCancel) {
//             cancelAction = utils.CreateBackAction(delegate() {
//                 audioManager.PlaySound((int)Define.Sound.Click);
//                 Exit();
//             });

//             var okBtn = msgImg.Find("Button_Ok").gameObject;
//             okBtn.GetComponent<Button>().onClick.AddListener(okAction);
//             okBtn.SetActive(true);

//             var cancelBtn = msgImg.Find("Button_Cancel").gameObject;
//             cancelBtn.GetComponent<Button>().onClick.AddListener(cancelAction);
//             cancelBtn.SetActive(true);

//             if (okText != "") {
//                 okBtn.transform.Find("Text").GetComponent<Text>().text = okText;
//                 okText = "";
//             }
//             if (cancelText != "") {
//                 cancelBtn.transform.Find("Text").GetComponent<Text>().text = cancelText;
//                 cancelText = "";
//             }
//         } else {
//             cancelAction = utils.CreateBackAction(delegate() {
//                 Utils.Instance.PushBackAction(cancelAction);
//             });

//             var enterBtn = msgImg.Find("Button_Enter").gameObject;
//             enterBtn.GetComponent<Button>().onClick.AddListener(okAction);
//             enterBtn.SetActive(true);

//             if (okText != "") {
//                 enterBtn.transform.Find("Text").GetComponent<Language.LanguageText>().enabled = false;
//                 enterBtn.transform.Find("Text").GetComponent<Text>().text = okText;
//                 okText = "";
//             }
//         }

//         Utils.Instance.PushBackAction(cancelAction);
// 	}
	
// 	// Update is called once per frame
// 	void Update () {
// 		if (isClose) {
// 			isClose = false;
// 			Exit();
// 		}
// 	}

//     private void Exit() {
//         // Destroy(transform.gameObject);
//         SceneManager.UnloadSceneAsync(Define.SCENE_MESSAGE);
//     }
// }
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;


public class MessagePanel : MonoBehaviour {
     private struct Data {
        public bool isOkCancel;
        public string message;
        public UnityAction okHandler;
        public string okText;
        public string cancelText;
    }

    private static List<Data> dataList = new List<Data>();
    private static bool isShow = false;


    public static void ShowMessage(string text, UnityAction handler = null, string ok = "") {
#if UNITY_EDITOR
        UnityEngine.Debug.Log("[Debug] " + text);
#endif

        dataList.Add(new Data {isOkCancel = false, message = text, okHandler = handler, okText = ok});
        Show();
    }

    public static void ShowOkCancel(string text, UnityAction handler = null, string ok = "", string cancel = "") {
#if UNITY_EDITOR
        UnityEngine.Debug.Log("[Debug] " + text);
#endif

        dataList.Add(new Data {isOkCancel = true, message = text, okHandler = handler, okText = ok, cancelText = cancel});
        Show();
    }

    private static void Show() {
        if (dataList.Count == 1) {
            SceneManager.LoadSceneAsync(Define.SCENE_MESSAGE, LoadSceneMode.Additive);
        } else if (isShow && !SceneManager.GetSceneByName(Define.SCENE_MESSAGE).IsValid()) {
            var data = dataList[dataList.Count - 1];
            dataList.Clear();
            dataList.Add(data);
            SceneManager.LoadSceneAsync(Define.SCENE_MESSAGE, LoadSceneMode.Additive);
        }
    }

    public static void CloseAll() {
        if (dataList.Count > 0) {
            if (isShow && SceneManager.GetSceneByName(Define.SCENE_MESSAGE).IsValid()) {
                SceneManager.UnloadSceneAsync(Define.SCENE_MESSAGE);
            }
            dataList.Clear();
            isShow = false;
        }
    }


    //-------------------------------------------------------------------------------------------------
    private UnityAction okAction = null;

    void OnDestroy() {
        isShow = false;
    }

	// Use this for initialization
	void Start () {
        if (dataList.Count == 0) { // 還沒顯示就呼叫CloseAll
            Exit();
            return;
        }

        isShow = true;
        var data = dataList[0];
        Init(data.isOkCancel, data.message, data.okHandler, data.okText, data.cancelText);
	}
	
	// Update is called once per frame
	void Update () {
        #if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
                if (okAction != null) {
                    okAction();
                }
            }
        #endif
	}

    public void Init(bool isOkCancel, string message, UnityAction okHandler, string okText, string cancelText) {
        Utils utils = Utils.Instance;
        AudioManager audioManager = AudioManager.Instance;

        Transform msgPanel = transform.Find("Panel_Message");
        msgPanel.Find("Text_Content").GetComponent<Text>().text = message;

        UnityAction handler = okHandler;
        okHandler = null;

        okAction = utils.CreateBackAction(delegate() {
            audioManager.PlaySound((int)Define.Sound.Click);
            if (handler != null) {
                handler();
            }
            Exit();
        });

        UnityAction cancelAction = null;

        if (isOkCancel) {
            cancelAction = utils.CreateBackAction(delegate() {
                audioManager.PlaySound((int)Define.Sound.Click);
                Exit();
            });

            var okBtn = msgPanel.Find("Button_Ok").gameObject;
            okBtn.GetComponent<Button>().onClick.AddListener(okAction);
            okBtn.SetActive(true);

            var cancelBtn = msgPanel.Find("Button_Cancel").gameObject;
            cancelBtn.GetComponent<Button>().onClick.AddListener(cancelAction);
            cancelBtn.SetActive(true);

            if (okText != "") {
                okBtn.transform.Find("Text").GetComponent<Text>().text = okText;
                okText = "";
            }
            if (cancelText != "") {
                cancelBtn.transform.Find("Text").GetComponent<Text>().text = cancelText;
                cancelText = "";
            }
        } else {
            cancelAction = utils.CreateBackAction(delegate() {
                if (handler == null) {
                    audioManager.PlaySound((int)Define.Sound.Click);
                    Exit();
                } else {
                    Utils.Instance.PushBackAction(cancelAction); // 只能按確定
                }
            });

            var enterBtn = msgPanel.Find("Button_Enter").gameObject;
            enterBtn.GetComponent<Button>().onClick.AddListener(okAction);
            enterBtn.SetActive(true);

            if (okText != "") {
                //enterBtn.transform.Find("Text").GetComponent<Language.LanguageText>().enabled = false;
                enterBtn.transform.Find("Text").GetComponent<Text>().text = okText;
                okText = "";
            }
        }

        Utils.Instance.PushBackAction(cancelAction);
    }

    private void Exit() {
        isShow = false;
        SceneManager.UnloadSceneAsync(Define.SCENE_MESSAGE);
        if (dataList.Count > 0) {
            dataList.RemoveAt(0);
            if (dataList.Count > 0) {
                SceneManager.LoadSceneAsync(Define.SCENE_MESSAGE, LoadSceneMode.Additive);
            }
        }
    }
}