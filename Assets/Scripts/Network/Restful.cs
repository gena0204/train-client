using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restful : Singleton<Restful> {

    private string _authorization = "";
    private string _language = "zh-TW";
    private float _timeout = 0;
    private int _retryLimit = 0;

    public string Authorization {
        get { return _authorization; }
        set { _authorization = value; }
    }

    public string AcceptLanguage {
        get { return _language; }
        set { _language = value; }
    }

    public float Timeout {
        get { return _timeout; }
        set { _timeout = value; }
    }

    public int RetryLimit {
        get { return _retryLimit; }
        set { _retryLimit = value; }
    }

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Request(string url, JSONObject json, RestfulHandler handler) {
        StartCoroutine(IERequest(url, json, handler));
    }

    IEnumerator IERequest(string url, JSONObject json, RestfulHandler handler, int retryCount = 0) {
        float timer = 0; 
        bool failed = false;

        var headers = new Dictionary<string, string>();
        headers.Add("Content-Type", "application/json");
        headers.Add("Authorization", _authorization);
        headers.Add("Accept-Language", _language);
        var postData = json != null ? System.Text.Encoding.UTF8.GetBytes(json.Print()) : null;
        var www = new WWW(Define.RESTFUL_URL + url, postData, headers);

        while (!www.isDone) {
            if (_timeout > 0) {
                if (timer > _timeout) { failed = true; break; }
                timer += Time.deltaTime;
            }
            yield return null;
        }

        if (failed) {
            www.Dispose();
            JSONObject errorJson = new JSONObject(JSONObject.Type.OBJECT);
            errorJson.AddField("errcode", 10);
            errorJson.AddField("msg",  Lang.Instance.getString("server_timeout"));
            handler(errorJson);
            yield break;
        }

        var response = new JSONObject(www.text);
        if (!string.IsNullOrEmpty(www.error)) {
            if (retryCount < _retryLimit) {
                www.Dispose();
                StartCoroutine(IERequest(url, json, handler, retryCount++));
                yield break;
            }

            // JSONObject errorJson = new JSONObject(JSONObject.Type.OBJECT);
            response.AddField("errcode", 1);
            // json.AddField("msg",  "錯誤: " + www.error);    
            response.AddField("msg",  Lang.Instance.getString("server_timeout"));
        }

        handler(response);
        www.Dispose();
    }
}

public delegate void RestfulHandler(JSONObject json);
