using System.Collections;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[Serializable]
public class PlayerTasks
{
    public string taskName;
    public int currentTaskDone;
    public int maxTaskToBeDone;
    public int taskMultiplier;
    public Button taskButton;
    public Text taskCount, taskInfo;
    public GameObject completeTaskIndicator;
}

[Serializable]
public class UpdatePlayerTask
{
    public int twitter_task;
    public int telegram_task;
}

[Serializable]
public class UpdateReferrerName
{
    public string referral_address;
    public string referrer_username;
}


[Serializable]
public class PlayerProfile
{
    public string address;
}

[Serializable]
public class PlayerLoginData
{
    public string address;
    public string signature;
    public string message;
}

[Serializable]
public class SeasonData
{
    public int season;
}

[Serializable]
public class healthcheck
{
    public int season;
}

[Serializable]
public class PlayerPoints
{
    public int score;
    public int season;
    public int player;
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    public bool reset;
    public string serverUrl;
    public string userAddress, userSignature, message;
    public string accessToken;
    public Text info;
    public Text healthText;
    public Text menuPlayerScoreText;
    public InputField referrerNameText;
    public Text referralNameText;
    public Text referrerErrorInfo;
    public Text referralNumText;
    public GameObject copiedGO;
    public int livesLeft;
    public bool IsLoggedIn;

    [SerializeField] PlayerTasks[] playerTasks;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start() => StartCoroutine(Login());

    void Update()
    {
        healthText.text = IsLoggedIn ? livesLeft.ToString() : "∞";
    } 

    public IEnumerator Login()
    {
        if (string.IsNullOrEmpty(userAddress))
        {
            livesLeft = 1000;
            info.text = "Connecting to Network....";
            yield return new WaitForSeconds(1.5f);
            info.text = "Started Logging In....";
            yield return new WaitForSeconds(1f);
            MenuManager.Instance.OpenMenu("intro");
        }
        else
        {
            info.text = "Started Logging In....";
            Debug.Log(userAddress + " " + message + " " + userSignature);

            PlayerLoginData playerLoginData = new PlayerLoginData { address = userAddress, signature = userSignature, message = this.message };
            string playerDataJson = JsonUtility.ToJson(playerLoginData);
            Debug.Log(playerDataJson);

            UnityWebRequest www = new UnityWebRequest(serverUrl + "users/login", "POST");

            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerDataJson);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            www.SetRequestHeader("Accept", "application/json");
            www.SetRequestHeader("Content-Type", "application/json");

            www.timeout = 15;
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error!!! " + www.downloadHandler.text);
            }
            else
            {
                Debug.Log("Logged In...");
                info.text = "Logged In.";
                MenuManager.Instance.OpenMenu("intro");

                var loginDetails = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
                accessToken = loginDetails["access_token"].ToString();
                Debug.Log("Logged In: " + www.downloadHandler.text);

                if (!string.IsNullOrEmpty(accessToken))
                {
                    StartCoroutine(CheckHealth());
                    StartCoroutine(UpdateTaskScore(0));
                    StartCoroutine(PlayerProfile(true));
                }

                IsLoggedIn = true;
            }
        }
    }

    string referrer_username;
    string referral_username;
    public IEnumerator PlayerProfile(bool inMenu)
    {
        PlayerProfile playerProfile = new PlayerProfile { address = userAddress };
        string playerProfileJson = JsonUtility.ToJson(playerProfile);
        Debug.Log(playerProfileJson);

        UnityWebRequest www = new UnityWebRequest(serverUrl + "users/view-profile", "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(playerProfileJson);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            var _myData = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
            int referral_count = int.Parse(_myData["referral_count"].ToString());
            int twitter_task = int.Parse(_myData["twitter_task"].ToString());
            int telegram_task = int.Parse(_myData["telegram_task"].ToString());

            Debug.Log(_myData["referrer_username"]);
            if (_myData["referrer_username"] != null)
                referrer_username = _myData["referrer_username"].ToString();
            referral_username = _myData["referral_username"].ToString();

            referralNameText.text = $"Username: {referral_username}";

            for (int i = 0; i < playerTasks.Length; i++)
            {
                playerTasks[i].currentTaskDone = Int32.Parse(_myData[playerTasks[i].taskName].ToString());

                if (playerTasks[i].taskCount)
                    playerTasks[i].taskCount.text = $"{playerTasks[i].currentTaskDone} /{playerTasks[i].maxTaskToBeDone}";

                if (playerTasks[i].taskInfo)
                {
                    int currentTaskPoints = playerTasks[i].currentTaskDone * playerTasks[i].taskMultiplier;
                    playerTasks[i].taskInfo.text = $"You Have Already Got {currentTaskPoints} Points";
                }

                if (playerTasks[i].currentTaskDone == playerTasks[i].maxTaskToBeDone)
                {
                    playerTasks[i].taskButton.interactable = false;

                    if (!inMenu) yield return new WaitForSeconds(2f);
                    playerTasks[i].completeTaskIndicator.SetActive(true);
                }
            }

            StartCoroutine(CheckHealth());
            StartCoroutine(UpdateTaskScore(0));

            if (inMenu)
            {
                if (string.IsNullOrEmpty(referrer_username))
                {
                    MenuManager.Instance.OpenMenu("referrerMenu");
                }
            }
        }
    }

    public void OnUpdateReferrerName()
    {
        string refName = referrerNameText.text;
        if (refName == referral_username)
        {
            referrerErrorInfo.text = "Player Username. Please use Referrer Username.";
        }
        else
        {
            StartCoroutine(UpdateReferrerName(refName));
        }
    }

    public IEnumerator UpdateReferrerName(string refName)
    {
        UpdateReferrerName referrerTask = new UpdateReferrerName { referral_address = userAddress, referrer_username = refName };
        string referrerTaskJson = JsonUtility.ToJson(referrerTask);
        Debug.Log("Referrer Name Update" + referrerTaskJson);

        UnityWebRequest www = new UnityWebRequest(serverUrl + "users/save-referral", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(referrerTaskJson);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error: " + www.downloadHandler.text);
            referrerErrorInfo.text = "Input a valid username.";
        }
        else
        {
            Debug.Log("Success: " + www.downloadHandler.text);

            MenuManager.Instance.OpenMenu("intro");
        }
    }

    public IEnumerator CheckHealth()
    {
        SeasonData seasonData = new SeasonData { season = 1 };
        string seasonDataJson = JsonUtility.ToJson(seasonData);
        Debug.Log(seasonDataJson);

        UnityWebRequest www = new UnityWebRequest(serverUrl + "whack-a-blob/player-lives", "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(seasonDataJson);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            var _myData = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
            string _currentAttempts = _myData["current_attempts"].ToString();
            string _livesLeft = _myData["lives_left"].ToString();
            livesLeft = int.Parse(_livesLeft);
            healthText.text = livesLeft.ToString();
        }
    }

    public IEnumerator GetLeaderboardInfo(LeaderboardManager leaderboard)
    {
        SeasonData seasonData = new SeasonData { season = 1 };
        string seasonDataJson = JsonUtility.ToJson(seasonData);
        Debug.Log(seasonDataJson);

        UnityWebRequest www = new UnityWebRequest(serverUrl + "whack-a-blob/scoreboard", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(seasonDataJson);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            Debug.Log("LEADERBOARD: " + @www.downloadHandler.text);
            // JArray boardData = JsonConvert.DeserializeObject<JArray>(@www.downloadHandler.text);
            // int _count = boardData.Count;
            // Debug.Log("SCOREBOARD INFO");
            // for (int i = 0; i < _count; i++)
            // {
            //     var _playerName = (string)boardData[i]["player"];
            //     var _score = (int)boardData[i]["score"];
            //     var _position = (int)boardData[i]["position"];
            //     Debug.Log(_playerName + ", Score: " + _score + ", Position: " + _position);
            //     AddToLeaderBoardInfo(leaderboard, _playerName, _score, _position);
            // }

            leaderboard.UpdateLeaderBoard();
        }
    }

    public void AddToLeaderBoardInfo(LeaderboardManager leaderboard, string player, int score, int position)
    {
        PlayerData _data = new PlayerData();
        _data.player = player;
        _data.score = score;
        _data.position = position;
        leaderboard.scoreBoardData.Add(_data);
    }

    public IEnumerator GetPlayerInfo(LeaderboardManager leaderboard)
    {
        SeasonData seasonData = new SeasonData { season = 1 };
        string seasonDataJson = JsonUtility.ToJson(seasonData);
        Debug.Log(seasonDataJson);

        UnityWebRequest www = new UnityWebRequest(serverUrl + "whack-a-blob/player-scoreboard", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(seasonDataJson);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            var _myData = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
            string _score = _myData["score"].ToString();
            string _position = _myData["position"].ToString();
            leaderboard.UpdatePlayerPersonalData(Int32.Parse(_score), Int32.Parse(_position));
        }
    }

    public void AddScore(int _score)
    {
        StartCoroutine(UpdatePlayerScore(_score));
    }

    public IEnumerator UpdatePlayerScore(int _newScore)
    {
        PlayerPoints playerScorePoints = new PlayerPoints { season = 1, score = _newScore };
        string scorePointJson = JsonUtility.ToJson(playerScorePoints);
        Debug.Log("Start Point Update" + scorePointJson);

        UnityWebRequest www = new UnityWebRequest(serverUrl + "whack-a-blob/add-score", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(scorePointJson);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error: " + www.downloadHandler.text);
        }
        else
        {
            Debug.Log("Success: " + www.downloadHandler.text);
            var _myData = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
            string _score = _myData["score"].ToString();

            StartCoroutine(PlayerProfile(false));
        }
    }


    public IEnumerator UpdateTaskScore(int _newScore)
    {
        PlayerPoints playerScorePoints = new PlayerPoints { season = 1, score = _newScore };
        string scorePointJson = JsonUtility.ToJson(playerScorePoints);
        Debug.Log("Task Point Update" + scorePointJson);

        UnityWebRequest www = new UnityWebRequest(serverUrl + "whack-a-blob/add-points-alone", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(scorePointJson);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error: " + www.downloadHandler.text);
        }
        else
        {
            Debug.Log("Success: " + www.downloadHandler.text);
            var _myData = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
            string _score = _myData["score"].ToString();

            menuPlayerScoreText.text = $"{_score}";
        }
    }

    public IEnumerator UpdateTaskStatus()
    {
        UpdatePlayerTask playerTask = new UpdatePlayerTask { twitter_task = playerTasks[1].currentTaskDone, telegram_task = playerTasks[2].currentTaskDone };
        string playerTaskJson = JsonUtility.ToJson(playerTask);
        Debug.Log("Task Point Update" + playerTaskJson);

        UnityWebRequest www = new UnityWebRequest(serverUrl + "users/add-task", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(playerTaskJson);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error: " + www.downloadHandler.text);
        }
        else
        {
            Debug.Log("Success: " + www.downloadHandler.text);
            // var _myData = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

            StartCoroutine(PlayerProfile(false));
        }
    }

    public void OpenTaskLink(string url)
    {
        Application.OpenURL(url);
        StartCoroutine(UpdateTaskScore(500));
    }

    public void TaskStatus(string taskName)
    {
        for (int i = 0; i < playerTasks.Length; i++)
        {
            if (playerTasks[i].taskName == taskName)
            {
                if (playerTasks[i].currentTaskDone != playerTasks[i].maxTaskToBeDone)
                {
                    playerTasks[i].currentTaskDone++;
                    break;
                }
            }
        }

        StartCoroutine(UpdateTaskStatus());
    }

    public void OnCopyToClipBaord()
    {
        var text = referral_username;
        var txtEditor = new TextEditor();
        txtEditor.text = text;
        txtEditor.SelectAll();
        txtEditor.Copy();

        StartCoroutine(ShowCopied());
    }

    IEnumerator ShowCopied()
    {
        copiedGO.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        copiedGO.SetActive(false);
        StopCoroutine(ShowCopied());
    }

    public void ResetTasks()
    {
        foreach (var _playerTasks in playerTasks)
        {
            _playerTasks.currentTaskDone = 0;
        }

        StartCoroutine(UpdateTaskStatus());
    }
}
