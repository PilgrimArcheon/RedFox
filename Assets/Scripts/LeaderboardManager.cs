using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[System.Serializable]
public class PlayerData
{
    public string player;
    public int score;
    public int position;
}
public class LeaderboardManager : MonoBehaviour
{
    public Transform scoreBoardHolder;
    public PlayerDataUI firstPos, secondPos, thirdPos;
    public List<PlayerData> scoreBoardData = new List<PlayerData>();
    public GameObject playerDataListUIPrefab, loadingIndicator;
    public Text playerScore, playerPosition;

    void OnEnable()
    {
        GetLeaderboardInfo();
    }

    void GetLeaderboardInfo()
    {
        loadingIndicator.SetActive(true);
        scoreBoardData.Clear();
        foreach (Transform item in scoreBoardHolder) Destroy(item.gameObject);

        if (NetworkManager.Instance.IsLoggedIn)
        {
            StartCoroutine(NetworkManager.Instance.GetLeaderboardInfo(this));
            StartCoroutine(NetworkManager.Instance.GetPlayerInfo(this));
        }
    }

    public void UpdateLeaderBoard()
    {
        loadingIndicator.SetActive(false);
        foreach (PlayerData _playerData in scoreBoardData)
        {
            if (_playerData.position > 3)
            {
                PlayerDataUI _playerDataUI = Instantiate(playerDataListUIPrefab, scoreBoardHolder).GetComponent<PlayerDataUI>();
                GetPlayerData(_playerDataUI, _playerData.player, _playerData.position.ToString(), _playerData.score.ToString());
            }
            else
            {
                if (_playerData.position == 1) GetPlayerData(firstPos, _playerData.player, (_playerData.position.ToString() + "st"), _playerData.score.ToString());
                else if (_playerData.position == 2) GetPlayerData(secondPos, _playerData.player, (_playerData.position.ToString() + "nd"), _playerData.score.ToString());
                else if (_playerData.position == 3) GetPlayerData(thirdPos, _playerData.player, (_playerData.position.ToString() + "rd"), _playerData.score.ToString());
            }
        }
    }

    public void GetPlayerData(PlayerDataUI dataUI, string player, string position, string score)
    {
        dataUI.nameText.text = player;
        dataUI.positionText.text = position + ".";
        dataUI.scoreText.text = score;
    }

    public void UpdatePlayerPersonalData(int score, int position)
    {
        loadingIndicator.SetActive(false);
        playerScore.text = score.ToString();
        playerPosition.text = AddOrdinal(position) + " Position";
    }

    public string AddOrdinal(int num)
    {
        if (num <= 0) return num.ToString();

        switch (num % 100)
        {
            case 11:
            case 12:
            case 13:
                return num + "th";
        }

        switch (num % 10)
        {
            case 1:
                return num + "st";
            case 2:
                return num + "nd";
            case 3:
                return num + "rd";
            default:
                return num + "th";
        }
    }
}