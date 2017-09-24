using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Channel {
    private Dictionary<string, ChannelHandler> _channel = new Dictionary<string, ChannelHandler>();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void RegisterChannel(string channel, ChannelHandler handler) {
        if (_channel.ContainsKey(channel)) {
            _channel.Remove(channel);
        }
        _channel.Add(channel, handler);
    }

    public void UnRegisterChannel(string channel) {
        _channel.Remove(channel);
    }

    public void OnMessage(string channel, JSONObject json) {
        if (_channel.ContainsKey(channel)) {
            Loom.QueueOnMainThread(() => { // 主線程執行
                _channel[channel](json, null);
            });
        }
    }
}

public delegate void ChannelHandler(JSONObject json, EventArgs e);