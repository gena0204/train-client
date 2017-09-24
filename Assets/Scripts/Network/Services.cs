using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using WebSocketSharp;
using System.Diagnostics;

public class Services : Singleton<Services> {
    private WebSocket _webSocket = null;
    private Channel _channel = null;

    private UnityAction errorCallback = null;

    void Awake() {
        if (_channel == null) {
            _channel = new Channel();
            if (_channel != null) {
                _channel.RegisterChannel(Define.Channel_S2C_Message, (json, e) => {
                    MessagePanel.ShowMessage(json["message"].str);
                });

                _channel.RegisterChannel(Define.Channel_S2C_Error, (json, e) => {
                    LoadingPanel.Close();
                    MessagePanel.ShowMessage(json["message"].str, delegate() {
                        if (errorCallback != null) {
                            errorCallback();
                            errorCallback = null;
                        }
                        else {
                            Utils.Instance.FadeScene(
                                Define.SCENE_MAIN,
                                GameObject.Find("Canvas").GetComponent<Fading>());
                        }
                    });
                });

                // _channel.RegisterChannel(Define.Channel_S2C_Money, (json, e) => {
                //     UserInfo.Instance.Money = (int)json["money"].n;
                // });

                _channel.RegisterChannel(Define.Channel_S2C_Kick, (json, e) => {
                    LoadingPanel.Close();
                    MessagePanel.ShowMessage(json["message"].str, delegate() {
                        Application.Quit();
                    });
                });
            }
        }
    }

	// Use this for initialization
	void Start() {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void onDestroy(){
		Release ();
	}

    public void Init() {
        if (_webSocket == null) {
            Connect(Define.WEBSOCKET_URL);
        }
    }

    public void Release() {
        if (_webSocket != null && _webSocket.ReadyState == WebSocketState.Open) {
            _webSocket.CloseAsync();
			_webSocket = null;
        }
    }

    private void Connect(string url) {
        _webSocket = new WebSocket(url);
        if (_webSocket != null) {
            _webSocket.OnOpen += (sender, e) => {
                // JSONObject data = new JSONObject(JSONObject.Type.OBJECT);
                // data.AddField("channel", Define.Channel_C2S_Enter);
                // data.AddField("version", Define.VERSION_CLIENT);
                // this.Send(data.Print());
            };
            _webSocket.OnMessage += (sender, e) => {
                switch (e.Type) {
                    case Opcode.Text: // e.Data
                        JSONObject json = new JSONObject(e.Data);
                        _channel.OnMessage(json["channel"].str, json);
                        break;

                    case Opcode.Binary: // e.RawData
                        break;
                }
            };
            _webSocket.OnClose += (sender, e) => {
                Loom.QueueOnMainThread(() => { // 主線程執行
                    LoadingPanel.Close();
					if(_webSocket != null){
	                    MessagePanel.ShowMessage(Lang.Instance.getString("disconnect"), delegate() {
	                        Application.Quit();
	                    });
					}
                });
            };
            _webSocket.OnError += (sender, e) => {
                if (url != Define.WEBSOCKET_URL_IPV6) {
                    Connect(Define.WEBSOCKET_URL_IPV6);
                }
            };

            _webSocket.ConnectAsync();
        }
    }

    public void RegisterChannel(string channel, ChannelHandler handler) {
        if (_channel != null) {
            _channel.RegisterChannel(channel, handler);
        }
    }

    public void UnRegisterChannel(string channel) {
        if (_channel != null) {
            _channel.UnRegisterChannel(channel);
        }
    }

    public void Send(string data) {
        if (_webSocket != null) {
            _webSocket.Send(data);
        }
    }

    public void SetErrorCallback(UnityAction callback) {
        errorCallback = callback;
    }
}
