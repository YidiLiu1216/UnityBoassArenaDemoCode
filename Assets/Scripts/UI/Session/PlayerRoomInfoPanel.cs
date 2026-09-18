using TMPro;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.LowLevelPhysics2D.PhysicsLayers;
public class PlayerRoomInfoPanel : MonoBehaviour
{
    [Header("Output")]
    [SerializeField]
    private TMP_Text _playerNameText;

    [SerializeField]
    private Image _panelImage;

    [SerializeField]
    private Image _playerPrepareCheck;

    [Header("Input")]
    [SerializeField]
    private Button _kickButton;

    [Header("Player Status")]
    [SerializeField]
    private Sprite _playerPrepareStatus;

    [SerializeField]
    private Sprite _playerNotPrepareStatus;

    [SerializeField]
    private Sprite _hostPlayerPanelSprite;

    [SerializeField]
    private Sprite _clientPlayerPanelSprite;

    private string _playerId;
    private void Awake()
    {
        _kickButton.onClick.AddListener(_OnKickButtonClicked);
    }
    public void UpdatePlayerInfo(IReadOnlyPlayer player,string hostPlayerId)
    {
        _playerId = player.Id;
        string playerName = player.GetPlayerName();
        Debug.Log($"PlayerId: {player.Id}," + $"PlayerName: {playerName ?? "NULL"}");
        _playerNameText.text = SessionUtility.ToDisplayName(playerName);
     
        bool isHost = player.Id == hostPlayerId;
        _panelImage.sprite = isHost ? _hostPlayerPanelSprite : _clientPlayerPanelSprite;
        if (NetworkManager.Singleton.IsHost)
        {
            _kickButton.gameObject.SetActive(!isHost);
        }

        if(GameBootstrap.Instance.Session.IsPlayerReady(player.Id))
        {
            _playerPrepareCheck.sprite = _playerPrepareStatus;
        }
        else
        {
            _playerPrepareCheck.sprite = _playerNotPrepareStatus;
        }
    }
    public void Clear() { 
       _playerNameText.text = string.Empty;
       _playerId = string.Empty;
    }
    private async void _OnKickButtonClicked() { 
       await GameBootstrap.Instance.Session.KickPlayerAsync(_playerId);
    }
}
