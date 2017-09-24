using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingPanel : MonoBehaviour {

    private static LoadingPanel loadingPanel = null;

    // private Text content = null;
    // private Transform bar = null;

    // private static string contentString = "";
    // private static string countString = "";
    // private static float finishCount = 0;
    // private static float loadCount = 0;
    private static bool isShow = false;
    private static bool isClose = true;
    // private static bool isUpdate = false;
    // private static Vector2 oriSize;
    // private static Vector2 targetSize;

    // private const float moveSpeed = 120.0f;


	// Use this for initialization
	void Start () {
        loadingPanel = this;

        // content = transform.FindChild("Panel_Loading/Image_Icon/Text_Content").GetComponent<Text>();
        // bar = transform.FindChild("Panel_Loading/Image_Icon/Image_Bar");

        // targetSize = oriSize = bar.GetComponent<RectTransform>().sizeDelta;
        // targetSize.x = 0;
        // bar.GetComponent<RectTransform>().sizeDelta = targetSize;

        // content.text = contentString;
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

		if (!isShow) {
			Close();
		}
	}

    public static void Show(int count = 0) {
        if (!loadingPanel) {
            isShow = true;
            isClose = false;
            // loadCount = count;
            // RefreshFinishCountText();
            SceneManager.LoadScene(Define.SCENE_LOADING, LoadSceneMode.Additive);
        }
    }

    public static void Close() {
        if (isClose) {
            return;
        }

        isShow = false;
        // isUpdate = false;
        // loadCount = 0;
        // finishCount = 0;
        // contentString = "";

        if (loadingPanel) {
            isClose = true;
            loadingPanel = null;
            SceneManager.UnloadSceneAsync(Define.SCENE_LOADING);
            // Loom.QueueOnMainThread(() => { // 主線程執行
            //     Destroy(loadingPanel.gameObject);
            //     loadingPanel = null;
            // });
        }
    }

    // public static void SetWaitCount(int count) {
    //     loadCount = count;
    //     RefreshFinishCountText();
    // }

    // public static void SetText(string text) {
    //     if (loadingPanel) {
    //         loadingPanel.content.text = text;
    //     } else {
    //         contentString = text;
    //     }
    // }

    public static bool IsShow() {
        return isShow;
    }

    // public static void Progress(float person) {
    //     targetSize = oriSize;
    //     targetSize.x *= person / 100;
    //     isUpdate = true;
    // }

    public static void Next() {
    //     if (finishCount >= loadCount) {
    //         return;
    //     }
    //     finishCount++;
    //     Progress(finishCount / loadCount * 100);
    //     RefreshFinishCountText();
    //     isUpdate = true;
    }

    // private static void RefreshFinishCountText() {
    //     if (loadCount > 0) {
    //         countString = "(" + finishCount + "/" + loadCount + ")";
    //     }
    //     else {
    //         countString = "";
    //     }
    // }
}
