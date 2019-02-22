// #define _DEBUG

using UnityEngine;
using System.Collections;

public class Define {

#if _DEBUG
    public const bool DEBUG                 = true;
    private const string HOST               = "jack0204.ddns.net";
    private const string HOST_IPV6          = "jack0204.ddns.net";
#else
    public const bool DEBUG                 = false;
    private const string HOST               = "140.109.150.188";
    // private const string HOST_IPV6          = "gleam-max.ddns.net"; // 工作室轉跳 (大陸會擋NOIP，IOS審核會失敗)
    private const string HOST_IPV6          = "[2001:b011:4800:7a8:94d0:8300:5c8b:3882]"; // 工作室轉跳
#endif

    public const string RESTFUL_URL_IPV4    = "http://" + HOST + ":8090/";  
    public const string RESTFUL_URL_IPV6    = "http://" + HOST_IPV6 + ":8090/";

	public const string WEBSOCKET_URL_IPV4  = "ws://" + HOST + ":8009/train";
    public const string WEBSOCKET_URL_IPV6  = "ws://" + HOST_IPV6 + ":8009/train";

    public const string FILE_URL_IPV4       = "http://" + HOST + ":8090/files";
    public const string FILE_URL_IPV6       = "http://" + HOST_IPV6 + ":8090/files";

    public static string RESTFUL_URL        = RESTFUL_URL_IPV4;
    public static string WEBSOCKET_URL      = WEBSOCKET_URL_IPV4;
    public static string FILE_URL           = FILE_URL_IPV4;

    //--------------------------------------------------------------------------------------------
    public static readonly string VERSION_CLIENT    = Application.version; // (強制更新版本).(新增/調整功能).(Bug修正)

    public static readonly string DOWNLOAD_PATH     = Application.persistentDataPath + "/Downloads/";
    public const int    DOWNLOAD_TIMEOUT            = 30;

    //--------------------------------------------------------------------------------------------
    public const string API_Crash                   = "crash";

    public const string API_Update                  = "update";

    public const string API_Login                   = "login";
    public const string API_Logout                  = "logout";
    public const string API_AccountCheck            = "account/check";
    public const string API_Register                = "register";

    public const string API_Enter                   = "enter";

    public const string API_News                    = "news";
    public const string API_Slogan                  = "slogan";
    public const string API_Rank                    = "rank";

    public const string API_Statistic_Create        = "statistic/create";

    //--------------------------------------------------------------------------------------------
    public const string Channel_S2C_Message         = "S2C_Message";
    public const string Channel_S2C_Error           = "S2C_Error";
    public const string Channel_S2C_Kick            = "S2C_Kick";

    // public const string Channel_S2C_Money           = "S2C_Money";

    // public const string Channel_C2S_CreateRoom      = "C2S_CreateRoom";
    // public const string Channel_S2C_CreateRoom      = "S2C_CreateRoom";

	// public const string Channel_C2S_GetRooms	    = "C2S_GetRooms";
	// public const string Channel_S2C_GetRooms	    = "S2C_GetRooms";

    // public const string Channel_C2S_JoinRoom        = "C2S_JoinRoom";
    // public const string Channel_S2C_JoinRoom        = "S2C_JoinRoom";

    // public const string Channel_C2S_ExitRoom        = "C2S_ExitRoom";
    // public const string Channel_S2C_ExitRoom        = "S2C_ExitRoom";

	// public const string Channel_C2S_Talk    	    = "C2S_Talk";
	// public const string Channel_S2C_Talk    	    = "S2C_Talk";

    // public const string Channel_C2S_StartRoom       = "C2S_StartRoom";
    // public const string Channel_S2C_StartRoom       = "S2C_StartRoom";

	// public const string Channel_C2S_ChangeTeam      = "C2S_ChangeTeam";
	// public const string Channel_S2C_ChangeTeam      = "S2C_ChangeTeam";

	// public const string Channel_C2S_KickUser        = "C2S_KickUser";
	// public const string Channel_S2C_KickUser        = "S2C_KickUser";

    // public const string Channel_C2S_StartGame       = "C2S_StartGame";
    // public const string Channel_S2C_StartGame       = "S2C_StartGame";

    // public const string Channel_C2S_StartBattle     = "C2S_StartBattle";
    // public const string Channel_S2C_StartBattle     = "S2C_StartBattle";

    // public const string Channel_C2S_FinishGame      = "C2S_FinishGame";
    // public const string Channel_S2C_FinishGame      = "S2C_FinishGame";

	// public const string Channel_C2S_Answer          = "C2S_Answer";
	// public const string Channel_S2C_Score           = "S2C_Score";   

    //-------------------------------------------------------------------------------------------
    public const string SCENE_LAUNCH        = "Launch";
    public const string SCENE_MAIN          = "Main";
    public const string SCENE_RANK          = "Rank";
    public const string SCENE_GAME_INTRO    = "GameIntro";
    public const string SCENE_GAME		    = "Game";
    public const string SCENE_MESSAGE       = "Message";
    public const string SCENE_LOADING       = "Loading";

    //---------------------------------------------------------------------------------------
    // Game
    //---------------------------------------------------------------------------------------
    public static int[] gameInfo = new int[] {
        2, // 方向感,
        1, // 文字陷阱,
        2, // 圖卡分類",
        2, // 接金蛋",
        1, // 順向點擊",
        1, // 逆向點擊",
        2, // 彩色方塊",
        2, // 記憶拼圖",
        2, // 比大小",
        1, // 用盡心計",
        1, // 絕對量感",
        1, // 路徑終點I",
        2, // 路徑終點II",
        1, // 極速配對",
        3, // 糖果追緝令",
        1, // 紅豆餅",
        1, // 語意達人",
        1, // 比對王",
        1, // 黃金開口笑",
        2, // 餅乾禮盒",
        2, // 戳泡泡",
        2, // 成語偵探",
    };

    //---------------------------------------------------------------------------------------
    // Audio
    //---------------------------------------------------------------------------------------
    public enum Sound {
        Click = 0,
        Right,
        Wrong,
        End,
    }

    public enum Music {
        Main = 0,
    }

    public static string[] soundFiles = new string[] {
        "Sounds/click",
        "Sounds/correct",
        "Sounds/incorrect",
		"Sounds/end_effect",
    };

    public static string[] musicFiles = new string[] {
        "Sounds/Snack_Time",
    };

    //---------------------------------------------------------------------------------------
    // PlayerPrefs
    //---------------------------------------------------------------------------------------
    public const string PP_Language             = "pp_language";

    public const string PP_Privacy              = "pp_privacy";  

    public const string PP_Sound                = "pp_sound";
    public const string PP_Music                = "pp_music";

    public const string PP_DataVersion          = "pp_data_version";
    public const string PP_GameData             = "pp_game_data";
    public const string PP_GameExtra            = "pp_game_extra";
    public const string PP_SloganData           = "pp_slogan_data";
    
    public const string PP_ChallengeIndexs      = "pp_challenge_indexs";
    public const string PP_ChallengeDate        = "pp_challenge_date";
    public const string PP_ChallengeLastDate    = "pp_challenge_last_date";
}