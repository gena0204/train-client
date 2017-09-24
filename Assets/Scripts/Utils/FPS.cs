using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS : Singleton<FPS> {
	public float _UpdateInterval = 0.1f;
	private float _LastInterval;
	private int _Frames = 0;

	private float _FPS;

	public void Init() {
		_LastInterval = Time.realtimeSinceStartup;
		_Frames = 0;
	}

	// Use this for initialization
	void Start () {
	
	}

	// Update is called once per frame
	void Update() {
		_Frames++;
		if (Time.realtimeSinceStartup > _LastInterval + _UpdateInterval) {
			_FPS = _Frames / (Time.realtimeSinceStartup - _LastInterval);
			_Frames = 0;
			_LastInterval = Time.realtimeSinceStartup;
		}
	}

	public void OnGUI() {
		GUILayout.Label("FPS: " + _FPS.ToString());
	}
}