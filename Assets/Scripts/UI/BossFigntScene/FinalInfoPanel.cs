using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class FinalInfoPanel : MonoBehaviour
{
    [Header("Output")]
    [SerializeField]
    private TextMeshProUGUI _battleEndInfo;
    [SerializeField]
    private List<PlayerInfoPanel> _playerInfos = new List<PlayerInfoPanel>();
    [Header("Button")]
    [SerializeField]
    private Button _backToRoomButton;

    private void Awake()
    {
        _backToRoomButton.onClick.AddListener(_OnBackToRoomClicked);
    }
    public void UpdateBattleEndInfo(bool isSuccess, PlayerBattleStats[] stats)
    {
        if (isSuccess)
        {
            _battleEndInfo.text = "You Win !!!";
        }
        else { 
            _battleEndInfo.text = "Battle Failed .....";
        }
        _UpdatePlayerStatsInfo(stats);
        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        _backToRoomButton.gameObject.SetActive(isHost);

    }
    private void _UpdatePlayerStatsInfo(PlayerBattleStats[] stats)
    {
        int playercount = 0;
        foreach (PlayerBattleStats stat in stats) {
            _playerInfos[playercount].gameObject.SetActive(true);
            _playerInfos[playercount].UpdatePlayerStatInfo(stat);
            playercount += 1;
        }
        for(int i=playercount;i < _playerInfos.Count; i++)
        {
            _playerInfos[i].gameObject.SetActive(false);
        }
    }
    private void _OnBackToRoomClicked()
    {
       
            NetworkManager.Singleton.SceneManager.LoadScene("StartScene", UnityEngine.SceneManagement.LoadSceneMode.Single);

    }
}
