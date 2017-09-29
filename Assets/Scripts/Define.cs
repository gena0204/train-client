// #define _DEBUG

using UnityEngine;
using System.Collections;

public class Define {

#if _DEBUG
    public const string HOST                = "jack0204.ddns.net";
    public const string HOST_IPV6           = "[2001:b011:4800:984:c568:8a6b:395e:efcb]";
    
#else
    public const string HOST                = "140.109.150.188";
    public const string HOST_IPV6           = "[2001:b011:4800:984:c568:8a6b:395e:efcb]";
#endif

    public const string RESTFUL_URL         = "http://" + HOST + ":8090/";  
    public const string RESTFUL_URL_IPV6    = "http://" + HOST_IPV6 + ":8090/";

	public const string WEBSOCKET_URL       = "ws://" + HOST + ":8009/train";
    public const string WEBSOCKET_URL_IPV6  = "ws://" + HOST_IPV6 + ":8009/train";
    public const string FILE_URL            = "http://" + HOST + ":8090/files";

    public static readonly string VERSION_CLIENT    = Application.version; // (強制更新版本).(新增/調整功能).(Bug修正)

    public static readonly string DOWNLOAD_PATH     = Application.persistentDataPath + "/Downloads/";
    public const int    DOWNLOAD_TIMEOUT            = 30;

    //--------------------------------------------------------------------------------------------
    public const string API_Crash                   = RESTFUL_URL + "crash";

    public const string API_Update                  = RESTFUL_URL + "update";

    public const string API_Login                   = RESTFUL_URL + "login";
    public const string API_Logout                  = RESTFUL_URL + "logout";
    public const string API_AccountCheck            = RESTFUL_URL + "account/check";
    public const string API_Register                = RESTFUL_URL + "register";

    public const string API_Enter                   = RESTFUL_URL + "enter";

    public const string API_News                    = RESTFUL_URL + "news";
    public const string API_Slogan                   = RESTFUL_URL + "slogan";
    public const string API_Rank                    = RESTFUL_URL + "rank";

    public const string API_Statistic_Create        = RESTFUL_URL + "statistic/create";

    //--------------------------------------------------------------------------------------------
    public const string Channel_S2C_Message         = "S2C_Message";
    public const string Channel_S2C_Error           = "S2C_Error";
    public const string Channel_S2C_Money           = "S2C_Money";
    public const string Channel_S2C_Kick            = "S2C_Kick";

    public const string Channel_C2S_CreateRoom      = "C2S_CreateRoom";
    public const string Channel_S2C_CreateRoom      = "S2C_CreateRoom";

	public const string Channel_C2S_GetRooms	    = "C2S_GetRooms";
	public const string Channel_S2C_GetRooms	    = "S2C_GetRooms";

    public const string Channel_C2S_JoinRoom        = "C2S_JoinRoom";
    public const string Channel_S2C_JoinRoom        = "S2C_JoinRoom";

    public const string Channel_C2S_ExitRoom        = "C2S_ExitRoom";
    public const string Channel_S2C_ExitRoom        = "S2C_ExitRoom";

	public const string Channel_C2S_Talk    	    = "C2S_Talk";
	public const string Channel_S2C_Talk    	    = "S2C_Talk";

    public const string Channel_C2S_StartRoom       = "C2S_StartRoom";
    public const string Channel_S2C_StartRoom       = "S2C_StartRoom";

	public const string Channel_C2S_ChangeTeam      = "C2S_ChangeTeam";
	public const string Channel_S2C_ChangeTeam      = "S2C_ChangeTeam";

	public const string Channel_C2S_KickUser        = "C2S_KickUser";
	public const string Channel_S2C_KickUser        = "S2C_KickUser";

    public const string Channel_C2S_StartGame       = "C2S_StartGame";
    public const string Channel_S2C_StartGame       = "S2C_StartGame";

    public const string Channel_C2S_StartBattle     = "C2S_StartBattle";
    public const string Channel_S2C_StartBattle     = "S2C_StartBattle";

    public const string Channel_C2S_FinishGame      = "C2S_FinishGame";
    public const string Channel_S2C_FinishGame      = "S2C_FinishGame";

    public const string Channel_C2S_IAPSuccess      = "C2S_IAPSuccess";

	public const string Channel_C2S_Answer          = "C2S_Answer";
	public const string Channel_S2C_Score           = "S2C_Score";    

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
    public static string[][] gameInfo = new string[][] {
        new string[] {
            "方向感",
            "藍色：順著箭頭方向滑動",
            "紅色：反箭頭方向滑動",
        },
        new string[] {
            "文字陷阱",
            "點擊顏色與文字不符的方格"
        },
        new string[] {
            "圖卡分類",
            "點擊相同的圖卡",
            "沒有相同圖卡時，\n點擊形狀與顏色皆不同的圖卡"
        },
        new string[] {
            "接金蛋",
            "點擊位置，讓籃子接住金蛋",
            "點擊位置，讓籃子避開石頭"
        },
        new string[] {
            "順向點擊",
            "記住糖果出現的順序，依序點擊"
        },
        new string[] {
            "逆向點擊",
            "記住糖果出現的順序，\n以相反順序點擊"
        },
        new string[] {
            "彩色方塊",
            "記住各色塊的位置",
            "依顏色順序點擊位置"
        },
        new string[] {
            "記憶拼圖",
            "記住圖卡的位置",
            "翻出一樣的配對"
        },
        new string[] {
            "比大小",
            "比前一數字大，點右側",
            "比前一數字小，點左側"
        },
        new string[] {
            "用盡心計",
            "點擊正確的運算符號，使等式成立"
        },
        new string[] {
            "絕對量感",
            "點擊數量最多的顏色"
        },
        new string[] {
            "路徑終點I",
            "觀察起點與箭頭方向，\n選擇路徑終點"
        },
        new string[] {
            "路徑終點II",
            "記住箭頭的位置與方向",
            "起點出現後根據消失的箭頭方向，選擇路徑終點"
        },
        new string[] {
            "極速配對",
            "點擊相同的數字"
        },
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
    public const string PP_Privacy          = "pp_privacy";  

    public const string PP_Sound            = "pp_sound";
    public const string PP_Music            = "pp_music";

    public const string PP_DataVersion      = "pp_data_version";
    public const string PP_GameData         = "pp_game_data";
    public const string PP_SloganData       = "pp_slogan_data";
    public const string PP_ChallengeIndexs  = "pp_challenge_indexs";
    public const string PP_ChallengeDate    = "pp_challenge_date";
    public const string PP_ChallengeFinish  = "pp_challenge_Finish";
}