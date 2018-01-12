using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public class Utils : Singleton<Utils> {

    private object ioLock = new object();

    private Stack backHandlerS = new Stack();


    void Awake() {
        // PlayerPrefs.DeleteAll();
        cacheManager = CacheManager.Instance;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        QualitySettings.masterTextureLimit = 0;
        Application.targetFrameRate = 60;
        // Application.runInBackground = true;

        // if (PlayerPrefs.GetString("client_version") != Define.VERSION_CLIENT) {
        //     PlayerPrefs.DeleteAll();
        //     Loom.QueueOnMainThread(() => { // 主線程執行
        //         lock (ioLock) {
        //             //File.Delete(Define.CARD_DATA_FILE);
        //             if (Directory.Exists(Define.DOWNLOAD_PATH)) {
        //                 Directory.Delete(Define.DOWNLOAD_PATH, true);
        //             }
        //         }
        //     });
        //     PlayerPrefs.SetString("client_version", Define.VERSION_CLIENT);
        // }

        Loom.QueueOnMainThread(() => { // 主線程執行
            lock (ioLock) {
                if (!Directory.Exists(Define.DOWNLOAD_PATH)) {
                    Directory.CreateDirectory(Define.DOWNLOAD_PATH);
                }
            }
        });
    }

    // Use this for initialization
    void Start() {
        Application.logMessageReceived += HandleLog;
        if (Define.DEBUG) {
            FPS.Instance.Init();
        }
    }

    // Update is called once per frame
    void Update() {
        // 目前Android下, 孤獨的GameObject無法Update
        if (Input.GetKeyDown(KeyCode.Escape) && !LoadingPanel.IsShow()) {
            Back();
        }
    }

    void OnApplicationPause(bool pauseStatus) {
        if (!pauseStatus) {
            Utils.SendRepeatingNotification();
        }
    //     if (pauseStatus) {
	// 		// if (UserInfo.Instance.IsBattle && SceneManager.GetActiveScene().name == Define.SCENE_GAME) {
    //         //     Services.Instance.Release();
    //         //     UserInfo.Instance.Account = "";
    //         //     UserInfo.Instance.Room.Clear();
    //         //     SystemManager.Instance.Release();
    //         //     MessagePanel.ShowMessage(Lang.Instance.getString("restart"), delegate() {
    //         //         SceneManager.LoadScene(Define.SCENE_MAIN);
    //         //     });
    //         // }
        // }
    }

    void HandleLog(string logString, string stackTrace, LogType type) {
		if (type == LogType.Error || type == LogType.Exception) {
			logString = logString.Replace("\n", "\\n");
			stackTrace = stackTrace.Replace("\n", "\\n");

			JSONObject data = new JSONObject(JSONObject.Type.OBJECT);
            data.AddField("token",          UserInfo.Instance.Token);
			data.AddField("log",       		logString);
			data.AddField("stack_trace",	stackTrace);
			Restful.Instance.Request(Define.API_Crash, data, (json) => {
			});

            if (SceneManager.GetActiveScene().name == Define.SCENE_GAME) {
                if (Game.self) {
                    Game.self.StopCountDown();
                    MessagePanel.ShowMessage(Lang.Instance.getString("crash"), delegate() {
                        Game.self.Exit();
                    });
                }
            }
		}
	}

    public static void SendRepeatingNotification() {
        int day = SystemManager.Instance.GetInt("notification_train", 1);
        if (day <= 0) {
            return;
        }
        
        long delayMs = day * 24 * 3600 * 1000;
        var msg = string.Format(Lang.Instance.getString("notification"), day);

        //LocalNotification.CancelNotification(1);
        LocalNotification.ClearNotifications();
        LocalNotification.SendRepeatingNotification(1, delayMs, delayMs, Application.productName,
            msg, new Color32(0xff, 0x44, 0x44, 255));
    }

    public UnityAction CreateBackAction(UnityAction action) {
        return delegate() {
            action();
            PopBackAction();
        };
    }

    public void PushBackAction(UnityAction action) {
        backHandlerS.Push(action);
    }

    public UnityAction PopBackAction() {
        if (backHandlerS.Count > 1) {
            return (UnityAction)backHandlerS.Pop();
        }
		return delegate() {
		};
    }

    public void BindBackAction(UnityAction action) {
        UnityAction lastAction = (UnityAction)backHandlerS.Peek();
        UnityAction newAction = delegate() {
            action();
            lastAction();
        };
        backHandlerS.Push(newAction);
    }

    public void SetBeginBackAction(UnityAction action) {
        backHandlerS.Clear();
        backHandlerS.Push(action);
    }

    public void Back() {
        if (backHandlerS.Count == 0) {
            return;
        }
        ((UnityAction)backHandlerS.Peek())();
    }


    public void FadeScene(string scene, Fading fading) {
        StartCoroutine(EnterScene(scene, fading));
    }

    private IEnumerator EnterScene(string scene, Fading fading) {
        GameObject.Find("EventSystem").SetActive(false);
        if (fading) {
            float fadeTime = fading.BeginFade(1);
            yield return new WaitForSeconds(fadeTime);
        }

        //Application.LoadLevel(scene);
        AsyncOperation async = SceneManager.LoadSceneAsync(scene);
        yield return async;
        // complete...
    }


    //------------------------------------------------------------------------------------
    // Saving And Loading Data
    //------------------------------------------------------------------------------------
    public void Save(object data, string fileName) {
        lock (ioLock) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(fileName);

            bf.Serialize(file, data);
            file.Close();
        }
    }

    public object Load(string fileName) {
        lock (ioLock) {
            if (File.Exists(fileName)) {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(fileName, FileMode.Open);
                object data = bf.Deserialize(file);
                file.Close();
                return data;
            }
        }
        return null;
    }

    //------------------------------------------------------------------------------------
    // Download Resources
    //------------------------------------------------------------------------------------
    private Coroutine timeoutCoroutine = null; // timeout線程, 目前限制一個檔案要在時間內完成, 日後加入progress機制
    private Coroutine downloadCoroutine = null;
    private Queue preloadKeyQ = new Queue();
    private CacheManager cacheManager;

    public Sprite GetImage(string key, bool cache = true) {
        if (cacheManager.HasObject(key)) {
            return cacheManager.GetObject<Sprite>(key);
        }
        return null;
    }

    public void LoadImage(string key, UnityAction<Sprite> callback = null) {
        string url = PlayerPrefs.GetString(key, "");
        if (url == "") {
            return;
        }
        StartCoroutine(IELoadSprite(key, url, true, delegate(Sprite sprite) {
            if (callback != null) {
                callback(sprite);
            }
        }));
    }

    public void PreloadImage(string key, string url, UnityAction<Sprite> callback) {
        StartCoroutine(IELoadSprite(key, url, false, delegate(Sprite sprite) {
            preloadKeyQ.Enqueue(key);
            callback(sprite);
        }));
    }

    public void AddCache(string key, Sprite sp, bool isPreLoad = false) {
        if (cacheManager.HasObject(key)) {
            cacheManager.RemoveObject(key);
        }
        cacheManager.SetObject(key, sp);
        if (isPreLoad) {
            preloadKeyQ.Enqueue(key);
        }
    }

    public void ClearPreload() {
        foreach (string key in preloadKeyQ) {
            Sprite sp = cacheManager.GetObject<Sprite>(key);
            if (sp) {
                cacheManager.RemoveObject(key);
            }
        }
        preloadKeyQ.Clear();
        Resources.UnloadUnusedAssets();
    }

    // 更新新圖片資源
    public void DownloadAndSaveImage(string url, UnityAction callback) {
        timeoutCoroutine = StartCoroutine(IETimeout(Define.DOWNLOAD_TIMEOUT));
        downloadCoroutine = StartCoroutine(IEDownloadAndSave(url, callback));
    }

    private IEnumerator IETimeout(float time) {
        yield return new WaitForSeconds(time);
        StopCoroutine(downloadCoroutine);
        LoadingPanel.Close();
        MessagePanel.ShowMessage(Lang.Instance.getString("timeout"));
    }

    private IEnumerator IELoadSprite(string key, string url, bool isSave, UnityAction<Sprite> callback = null) {
        Sprite sprite = null;
        if (cacheManager.HasObject(key)) {
            sprite = cacheManager.GetObject<Sprite>(key);
        }
        else if (url != "") {
            // 檢查是否存在檔案
            bool isFileExist = false;
            string imageUrl = "";
            if (IsExist(url)) {
                isFileExist = true;
                imageUrl = "file:///" + Define.DOWNLOAD_PATH + System.IO.Path.GetFileName(key);
            }
            else {
                if (url.IndexOf("https://") > -1 || url.IndexOf("http://") > -1) {
                    imageUrl = url;
                }
                else {
                    imageUrl = Define.FILE_URL + url;
                }
            }

            // 下載/讀取檔案(最多重新下載3次)
            int times = 0;
            WWW www;
            while (true) {
                www = new WWW(imageUrl);
                //while (!www.isDone) {
                //Debug.Log(www.progress * 100);
                yield return www;
                //}
                times++;
                if (string.IsNullOrEmpty(www.error)) {
                    break;
                }
                else if (times >= 3) {
                    if (imageUrl.IndexOf(Define.FILE_URL) == -1) {
                        imageUrl = Define.FILE_URL + url;
                    } else {
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(www.error)) {
                if (isSave && !isFileExist) {
                    SaveTextureToFile(www.texture, key);
                }

                // create texture
                Texture2D tex = new Texture2D(4, 4, TextureFormat.ARGB4444, false);
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.LoadImage(www.texture.EncodeToPNG());

                sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100);

                cacheManager.SetObject(key, sprite);
            }
        }

        if (callback != null) {
            callback(sprite);
        }
    }

    /** 下載圖片(先檢查Cache及Local) */
    private IEnumerator IEDownloadAndSave(string url, UnityAction callback) {
        if (url != "" && !cacheManager.HasObject(url)) {
            // 檢查是否存在檔案
            if (!IsExist(url)) {
                string imageUrl = "";
                if (url.IndexOf("http://") > -1 || url.IndexOf("https://") > -1) {
                    imageUrl = url;
                }
                else {
                    imageUrl = Define.FILE_URL + url;
                }

                // 下載檔案(最多重新下載3次)
                int times = 0;
                WWW www;
                while (true) {
                    www = new WWW(imageUrl);
                    yield return www;
                    times++;
                    if (string.IsNullOrEmpty(www.error)) {
                        break;
                    }
                    else if (times >= 3) {
                        if (imageUrl.IndexOf(Define.FILE_URL) == -1) {
                            imageUrl = Define.FILE_URL + url;
                        }
                        else {
                            break;
                        }
                    }
                }

                // 儲存檔案
                if (www.error == null) {
                    SaveTextureToFile(www.texture, url);
                }
                else {
                    Debug.Log(www.error);
                }
            }
        }

        StopCoroutine(timeoutCoroutine);
        callback();
    }

    private bool IsExist(string url) {
        return File.Exists(Define.DOWNLOAD_PATH + System.IO.Path.GetFileName(url));
    }

    // IO存取須主線程
    private void SaveTextureToFile(Texture2D texture, string url, UnityAction callback = null) {
        if (texture == null) {
            return;
        }

        Loom.QueueOnMainThread(() => { // 主線程執行
            lock (ioLock) {
                File.WriteAllBytes(Define.DOWNLOAD_PATH + System.IO.Path.GetFileName(url), texture.EncodeToPNG());
            }
            if (callback != null) {
                callback();
            }
        });
    }

    //------------------------------------------------------------------------------------
    // Animation
    //------------------------------------------------------------------------------------
    private Dictionary<Animation, Coroutine> animationDict = new Dictionary<Animation, Coroutine>();

    public void PlayAnimation(Animation animation, string clipName = "", UnityAction callback = null, float callbackDelaySecond = 0) {
        StopAnimation(animation);
        
        if (clipName != "") {
            animation.Play(clipName);
        } else {
            animation.Play();
        }

        animationDict.Add(animation, StartCoroutine(WaitForAnimation(callbackDelaySecond, animation, callback)));
    }

    public void DelayPlayAnimation(float delaySecond, Animation animation, UnityAction callback = null,
        float callbackDelayTime = 0, string clipName = "") {
        if (delaySecond <= 0) {
            PlayAnimation(animation, clipName, callback, callbackDelayTime);
            return;
        }

        StopAnimation(animation);
        animationDict.Add(animation, StartCoroutine(WaitForProcess(delaySecond, delegate() {
            animationDict.Remove(animation);
            PlayAnimation(animation, clipName, callback, callbackDelayTime);
        })));
    }

    public void StopAnimation(Animation animation) {
        if (animationDict.ContainsKey(animation)) {
            StopCoroutine(animationDict[animation]);
            animationDict.Remove(animation);
            animation.Stop();
        }
    }
    
    private IEnumerator WaitForAnimation(float delaySecond, Animation animation, UnityAction callback) {
        do {
            yield return null;
        } while (animation && animation.isPlaying);

        if (delaySecond > 0) {
            yield return new WaitForSeconds(delaySecond);
        }

        if (animation) {
            animationDict.Remove(animation);
        }
        
        if (callback != null) {
            callback();
        }
    }

    //------------------------------------------------------------------------------------
    // GameObject Move Animation
    //------------------------------------------------------------------------------------
    public void MoveOverSpeed(GameObject obj, Vector3 end, float speed, UnityAction callback = null) {
        StartCoroutine(IEMoveOverSpeed(obj, end, speed, callback));
    }

    public void MoveOverSeconds(GameObject obj, Vector3 end, float seconds, UnityAction callback = null) {
        StartCoroutine(IEMoveOverSeconds(obj, end, seconds, callback));
    }

    private IEnumerator IEMoveOverSpeed(GameObject obj, Vector3 end, float speed, UnityAction callback) {
		while (obj.transform.position != end) {
			obj.transform.position = Vector3.MoveTowards(obj.transform.position, end, speed * Time.deltaTime);
			yield return new WaitForEndOfFrame();
		}
        if (callback != null) {
            callback();
        }
	}

	private IEnumerator IEMoveOverSeconds(GameObject obj, Vector3 end, float seconds, UnityAction callback) {
		float elapsedTime = 0;
		while (elapsedTime < seconds) {
			obj.transform.position = Vector3.Lerp(obj.transform.position, end, (elapsedTime / seconds));
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		obj.transform.position = end;
        if (callback != null) {
            callback();
        }
	}

    //------------------------------------------------------------------------------------
    // 
    //------------------------------------------------------------------------------------
    public Coroutine StartDelayProcess(float delaySecond, UnityAction callback) {
        return StartCoroutine(WaitForProcess(delaySecond, callback));
    }

    private IEnumerator WaitForProcess(float delaySecond, UnityAction callback) {
        if (delaySecond > 0) {
            yield return new WaitForSeconds(delaySecond);
        }
        callback();
    }
}
