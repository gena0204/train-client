using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restful : Singleton<Restful> {

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Request(string url, JSONObject json, RestfulHandler handler) {
        var headers = new Dictionary<string, string>();
        headers.Add("Content-Type", "application/json");
        
        var postData = System.Text.Encoding.UTF8.GetBytes(json.Print());

        StartCoroutine(WaitForRequest(new WWW(url, postData, headers), handler));
    }

    IEnumerator WaitForRequest(WWW www, RestfulHandler handler) {
        yield return www;
 
        // check for errors
        if (www.error == null) {
            handler(new JSONObject(www.text));
            // Debug.Log(www.text);
        } else {
            JSONObject errorJson = new JSONObject(JSONObject.Type.OBJECT);
            errorJson.AddField("errcode", 1);
            // errorJson.AddField("msg",  "錯誤: " + www.error);
            errorJson.AddField("msg",  Lang.Instance.getString("server_timeout"));
            
            handler(errorJson);
        }
    }
}

public delegate void RestfulHandler(JSONObject json);
