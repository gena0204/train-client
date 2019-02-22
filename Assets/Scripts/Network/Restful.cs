using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class Restful : Singleton<Restful> {

    private string _authorization = "";
    private string _language = "zh-TW";
    private float _timeoutSecond = 30;
    private int _retryLimit = 0;

    public string Authorization {
        get { return _authorization; }
        set { _authorization = value; PlayerPrefs.SetString("authorization", value); }
    }

    public string AcceptLanguage {
        get { return _language; }
        set { _language = value; }
    }

    public float Timeout {
        get { return _timeoutSecond; }
        set { _timeoutSecond = value; }
    }

    public int RetryLimit {
        get { return _retryLimit; }
        set { _retryLimit = value; }
    }

	// Use this for initialization
	void Start () {
        ServicePointManager.ServerCertificateValidationCallback = TrustCertificate; // Ignore untrusted certificate
        _authorization = PlayerPrefs.GetString("authorization", "");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private static bool TrustCertificate(object sender, X509Certificate x509Certificate, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors) {
        return true; // Accept all Certificates
    }

#if true
    public void Request(string url, JSONObject json, RestfulHandler handler = null) {
#if UNITY_EDITOR
        UnityEngine.Debug.Log("<< [" + url + "] " + (json != null ? json.ToString() : ""));
#endif

        // StartCoroutine(IERequest(url, json, handler));
        HttpWebRequest request = null;

        try {
            // WebRequest支援ServicePointManager.ServerCertificateValidationCallback
            request = (HttpWebRequest) WebRequest.Create(Define.RESTFUL_URL + url);
            request.Method = WebRequestMethods.Http.Post;
            request.Headers.Add("Authorization", _authorization);
            request.Headers.Add("Accept-Language", _language);
            request.Timeout = (int)(_timeoutSecond * 1000);
            if (json != null) {
                var postData = System.Text.Encoding.UTF8.GetBytes(json.Print());
                request.ContentType = "application/json";
                request.ContentLength = postData.Length;
                using (Stream st = request.GetRequestStream()) {
                    st.Write(postData, 0, postData.Length);
                }
            }

            if (handler == null) {
                return;
            }

            using (var response = request.GetResponse() as HttpWebResponse) {
                using (var reader = new StreamReader(response.GetResponseStream())) {
                    json = new JSONObject(reader.ReadToEnd());
                }
                response.Close();
            }
        } catch (WebException ex) {
            var response = ex.Response as HttpWebResponse;
            if (response != null) {
                using (var reader = new StreamReader(response.GetResponseStream())) {
                    json = new JSONObject(reader.ReadToEnd());
                    if (response.StatusCode != HttpStatusCode.OK) {
                        if (!json.HasField("errcode")) {
                            json.AddField("errcode", (int)response.StatusCode);
                        }
                        if (response.StatusCode == HttpStatusCode.RequestTimeout) {
                            json.SetField("msg", Lang.Instance.getString("timeout"));
                        } else if (!json.HasField("msg")) {
                            json.AddField("msg", Lang.Instance.getString("server_timeout"));
                        }
                        UnityEngine.Debug.Log("response.StatusCode:" + response.StatusCode);
                    }
                }
                response.Close();
            } else {
                json.AddField("errcode", (int)HttpStatusCode.NotFound);
                if (Application.internetReachability == NetworkReachability.NotReachable) {
                    json.AddField("msg", Lang.Instance.getString("no_network"));
                } else {
                    json.AddField("msg", Lang.Instance.getString("server_timeout"));
                }
            }
        } catch (System.Exception ex) {
            json.AddField("errcode", (int)HttpStatusCode.ExpectationFailed);
            json.AddField("msg", ex.Message.ToString());
        }

#if UNITY_EDITOR
        UnityEngine.Debug.Log(">> [" + url + "] " + (json != null ? json.ToString() : ""));
#endif

        if (handler != null) {
            handler(json);
        }
    }

#else
    // 新版建議採用UnityWebRequest, WWW之後會淘汰
    // WWW不支援ServicePointManager.ServerCertificateValidationCallback
    public void Request(string url, JSONObject json, RestfulHandler handler) {
        StartCoroutine(IERequest(url, json, handler));
    }

    IEnumerator IERequest(string url, JSONObject json, RestfulHandler handler, int retryCount = 0) {
        float timer = 0; 
        bool timeout = false;

        var headers = new Dictionary<string, string>();
        headers.Add("Content-Type", "application/json");
        headers.Add("Authorization", _authorization);
        headers.Add("Accept-Language", _language);
        var postData = json != null ? System.Text.Encoding.UTF8.GetBytes(json.Print()) : null;
        var www = new WWW(Define.RESTFUL_URL + url, postData, headers);

        while (!www.isDone) {
            if (_timeoutSecond > 0) {
                if (timer > _timeoutSecond) { timeout = true; break; }
                timer += Time.deltaTime;
            }
            yield return null;
        }

        if (timeout) {
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
                StartCoroutine(IERequest(url, json, handler, retryCount+1));
                yield break;
            }

            // JSONObject errorJson = new JSONObject(JSONObject.Type.OBJECT);
            response.AddField("errcode", 1);
            // response.AddField("msg",  "錯誤: " + www.error);  
            response.AddField("msg",  Lang.Instance.getString("error_server"));
        }

        handler(response);
        www.Dispose();
    }
#endif
}

public delegate void RestfulHandler(JSONObject json);
