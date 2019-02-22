using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class LoadingPanel : MonoBehaviour {

    private enum State {
        Close = 0,
        Opening,
        Open,
        Closing,
    }

    private static object _lock = new object();

    private static LoadingPanel loadingPanel = null;

    private Text detailText = null;
    // private Text content = null;
    // private Transform bar = null;

    private static string detailString = "";
    // private static string contentString = "";
    // private static string countString = "";
    // private static float finishCount = 0;
    // private static float loadCount = 0;

    private static State state = State.Close;
    // private static bool isUpdate = false;
    // private static Vector2 oriSize;
    // private static Vector2 targetSize;

    // private const float moveSpeed = 120.0f;


    private static List<EventSystem> eventSystemList = new List<EventSystem>();



    void OnDestroy() {
        loadingPanel = null;
        // state = State.Close; // 統一由Close()設定
    }

	// Use this for initialization
	void Start () {
        lock (_lock) {
            loadingPanel = this;

            // detailText = transform.Find("Text_Detail").GetComponent<Text>();
            // content = transform.Find("Panel_Loading/Image_Icon/Text_Content").GetComponent<Text>();
            // bar = transform.Find("Panel_Loading/Image_Icon/Image_Bar");

            // targetSize = oriSize = bar.GetComponent<RectTransform>().sizeDelta;
            // targetSize.x = 0;
            // bar.GetComponent<RectTransform>().sizeDelta = targetSize;

            // detailText.text = detailString;
            // content.text = contentString;

            if (state != State.Closing) {
                state = State.Open;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        // if (isUpdate) {
        //     Vector2 size = bar.GetComponent<RectTransform>().sizeDelta;
        //     bar.GetComponent<RectTransform>().sizeDelta =
        //         Vector2.MoveTowards(size, targetSize, Time.deltaTime * moveSpeed);

        //     content.text = Mathf.Round(size.x / oriSize.x * 100) + "%" + countString;
        //     if (size.x == targetSize.x) {
        //         isUpdate = false;
        //     }
        // }

        lock (_lock) {
            if (state == State.Closing) {
                Close();
            }
        }
	}

    // public static void Show(/* int count = 0 */) {
    //     lock (_lock) {
    //         EnableEventSystem(false);

    //         if (!loadingPanel && state == State.Close) {
    //             state = State.Opening;
    //             // loadCount = count;
    //             // RefreshFinishCountText();
    //             SceneManager.LoadSceneAsync(Define.SCENE_LOADING, LoadSceneMode.Additive);
    //         } else if (!loadingPanel && state == State.Closing) { // 尚未開啟時關閉但又馬上開啟
    //             state = State.Opening;
    //         }
    //     }
    // }

    public static void Show(string text = "") {
        lock (_lock) {
            EnableEventSystem(false);

            if (!loadingPanel && state == State.Close) {
                state = State.Opening;
                detailString = text;
                SceneManager.LoadSceneAsync(Define.SCENE_LOADING, LoadSceneMode.Additive);
            } else if (!loadingPanel && state == State.Closing) { // 尚未開啟時關閉但又馬上開啟
                state = State.Opening;
            } else if (text != "") {
                Loom.QueueOnMainThread(() => { // 主線程執行
                    if (loadingPanel) {
                        loadingPanel.detailText.text = text;
                    }
                });
            }
        }
    }

    public static void Close() {
        lock (_lock) {
            EnableEventSystem(true);

            if (state == State.Opening) {
                state = State.Closing;
                return;
            }

            // isUpdate = false;
            // loadCount = 0;
            // finishCount = 0;
            // contentString = "";
            detailString = "";
        
            if (loadingPanel) {
                state = State.Close;
                loadingPanel = null;
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(Define.SCENE_LOADING));
            }
        }
    }

    // public static void SetWaitCount(int count) {
    //     loadCount = count;
    //     RefreshFinishCountText();
    // }

    public static void SetDetail(string text) {
        lock (_lock) {
            if (loadingPanel) {
                Loom.QueueOnMainThread(() => { // 主線程執行
                    if (loadingPanel) {
                        loadingPanel.detailText.text = text;
                    }
                });
            } else {
                detailString = text;
            }
        }
    }

    // public static void SetText(string text) {
    //     if (loadingPanel) {
    //         loadingPanel.content.text = text;
    //     } else {
    //         contentString = text;
    //     }
    // }

    public static bool IsShow() {
        return state == State.Open || state == State.Opening;
    }

    // public static void Progress(float person) {
    //     targetSize = oriSize;
    //     targetSize.x *= person / 100;
    //     isUpdate = true;
    // }

    // public static void Next() {
    //     if (finishCount >= loadCount) {
    //         return;
    //     }
    //     finishCount++;
    //     Progress(finishCount / loadCount * 100);
    //     RefreshFinishCountText();
    //     isUpdate = true;
    // }

    // private static void RefreshFinishCountText() {
    //     if (loadCount > 0) {
    //         countString = "(" + finishCount + "/" + loadCount + ")";
    //     }
    //     else {
    //         countString = "";
    //     }
    // }

    private static void EnableEventSystem(bool enable) {
        if (enable) {
            for (int i = 0; i < eventSystemList.Count; i++) {
                if (eventSystemList[i]) {
                    eventSystemList[i].enabled = true;
                }
            }
            eventSystemList.Clear();
        } else {
            foreach (var es in Resources.FindObjectsOfTypeAll(typeof(EventSystem)) as EventSystem[]) {
                if (es.enabled) {
                    eventSystemList.Add(es);
                    es.enabled = false;
                }
            }
        }
    }
}
