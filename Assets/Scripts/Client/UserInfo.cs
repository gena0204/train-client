using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class UserInfo : Singleton<UserInfo> {
    private Player player = new Player();
    private Room room = new Room();

    private string token = "";

    private bool isBattle = false;
    private int roomPlayerIndex = 0;

    

    void Awake() {
        // get player prefs
        Token = LocalToken;
        Account = LocalAccount;
    }

    // Use this for initialization
    void Start() { }

    // Update is called once per frame
    void Update() { }

    //--------------------------------------------------------------------
    public string LocalToken {
        get { return PlayerPrefs.GetString("token", ""); }
        set { PlayerPrefs.SetString("token", value); }
    }

    public string LocalAccount {
        get { return PlayerPrefs.GetString("account", ""); }
        set { PlayerPrefs.SetString("account", value); }
    }

    //--------------------------------------------------------------------
    public bool IsBattle {
        get { return isBattle; }
        set { isBattle = value; }
    }

    public string Token {
        get { return token; }
        set { token = value; }
    }

    // public string Name {
    //     get { return LocalToken == "" ? "guest" : LocalAccount; }
    // }

    public string Account {
        get { return player.account; }
        set { player.account = value; }
    }

    public int RoomPlayerIndex {
        get { return roomPlayerIndex; }
        set { roomPlayerIndex = value; }
    }

    public Room Room {
        get { return room; }
    }

	public float MusicVol {
		get { return PlayerPrefs.GetFloat("musicVol", 1f); }
		set { PlayerPrefs.SetFloat("musicVol", value); }
	}

	public float SoundVol {
		get { return PlayerPrefs.GetFloat("soundVol", 1f); }
		set { PlayerPrefs.SetFloat("soundVol", value); }
	}

    public void Clear() {
        LocalToken = "";
        LocalAccount = "";
        Token = "";
        Account = "";
    }
}

public class Player {
    public string id = "";
	public int teamId = 0;
    public string account = "";
    public int title = 0;
    public bool isReady = false;
}

// 目前無多人對戰
public class Room {
    private int roomId = 0;
    private int level = 0;
    private string character = "";
    private bool isChallenge = false;
    private int stageIndex = 0;
    private List<int> gameIndexs = new List<int>();
    
    public int Id {
        get { return roomId; }
        set { roomId = value; }
    }

    public int Level {
        get { return level; }
        set { level = value; }
    }

    public string Character {
        get { return character; }
        set { character = value; }
    }

    public bool IsChallenge {
        get { return isChallenge; }
        set { isChallenge = value; }
    }

    public int StageIndex {
        get { return stageIndex; }
        set { stageIndex = value; }
    }

    public List<int> GameIndexs {
        get { return gameIndexs; }
    }

    public int CurrentGameIndex {
        get { return gameIndexs[stageIndex]; }
    }

    public void Clear() {
        roomId = 0;
        level = 0;
        character = "";
        isChallenge = false;
        stageIndex = 0;
        gameIndexs.Clear();
    }
}
