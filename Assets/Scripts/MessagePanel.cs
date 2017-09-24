using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;
using System.Collections;


public class MessagePanel : MonoBehaviour {

    public delegate void MessageHandler();

    private static string message = "";
    private static bool isOkCancel = false;
    private static MessageHandler okHandler = null;
	private static bool isClose = false;

    public static void ShowMessage(string text, MessageHandler handler = null) {
        message = text;
        okHandler = handler;
        isOkCancel = false;
        SceneManager.LoadScene(Define.SCENE_MESSAGE, LoadSceneMode.Additive);
    }

    public static void ShowOkCancel(string text, MessageHandler handler = null) {
        message = text;
        okHandler = handler;
        isOkCancel = true;
        SceneManager.LoadScene(Define.SCENE_MESSAGE, LoadSceneMode.Additive);
    }

    public static void Close() {
		isClose = true;
    }

	// Use this for initialization
	void Start () {
        Utils utils = Utils.Instance;
        AudioManager audioManager = AudioManager.Instance;

        Transform msgImg = transform.FindChild("Panel_Message/Image_Message");
        msgImg.FindChild("Text_Content").GetComponent<Text>().text = message;

        MessageHandler handler = okHandler;
        okHandler = null;

        UnityAction okAction = utils.CreateBackAction(delegate() {
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

            var okBtn = msgImg.FindChild("Button_Ok").gameObject;
            okBtn.GetComponent<Button>().onClick.AddListener(okAction);
            okBtn.SetActive(true);

            var cancelBtn = msgImg.FindChild("Button_Cancel").gameObject;
            cancelBtn.GetComponent<Button>().onClick.AddListener(cancelAction);
            cancelBtn.SetActive(true);
        } else {
            cancelAction = utils.CreateBackAction(delegate() {
                Utils.Instance.PushBackAction(cancelAction);
            });

            var enterBtn = msgImg.FindChild("Button_Enter").gameObject;
            enterBtn.GetComponent<Button>().onClick.AddListener(okAction);
            enterBtn.SetActive(true);
        }

        Utils.Instance.PushBackAction(cancelAction);
	}
	
	// Update is called once per frame
	void Update () {
		if (isClose) {
			isClose = false;
			Exit();
		}
	}

    private void Exit() {
        // Destroy(transform.gameObject);
        SceneManager.UnloadSceneAsync(Define.SCENE_MESSAGE);
    }
}