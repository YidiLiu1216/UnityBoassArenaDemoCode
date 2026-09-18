using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateRoomUI : MonoBehaviour 
{
    [Header("Inputs")]
    [SerializeField]
    private TMP_InputField _playerNameInput;

    [SerializeField]
    private Toggle _privateToggle;

    [SerializeField]
    private TMP_InputField _passwordInput;

    [Header("Buttons")]
    [SerializeField]
    private Button _createButton;

    [SerializeField]
    private Button _updateButton;

    [SerializeField]
    private Button _exitButton;

    [SerializeField] 
    private Button _copyCodeButton;

    [SerializeField]
    private Button _updatePlayerNameButton;

    [SerializeField]
    private Button _prepareButton;

    [SerializeField]
    private Button _gameStartButton;

    [Header("Output")]
    [SerializeField]
    private TMP_Text _errorText;
    [SerializeField]
    private PlayerRoomInfoPanel[] _playerInfoSlots = new PlayerRoomInfoPanel[4];

    [Header("Info")]
    [SerializeField]
    RoomInfoPanel _roomInfoPanel;

    [SerializeField]
    private TMP_Text _playerNameText;

    [SerializeField]
    private TMP_Text _passwordDefaultText;

    private Dictionary<string, PlayerRoomInfoPanel> _playerUIs = new();

    private void Awake()
    {
        _createButton.onClick.AddListener(_OnCreateClicked);
        _updateButton.onClick.AddListener(_OnUpdateClicked);
        _exitButton.onClick.AddListener(_OnExitClicked);
        _updatePlayerNameButton.onClick.AddListener(_OnUpdatePlayerNameClicked);
        _prepareButton.onClick.AddListener(_OnPrepareClicked);
        _gameStartButton.onClick.AddListener(_OnGameStartClicked);
        _copyCodeButton.onClick.AddListener(_OnCopyCodeClicked);
        _createButton.interactable = true;
        _updateButton.interactable = false;
        _exitButton.interactable = true;
    }
    private void OnEnable()
    {
        GameBootstrap.Instance.Session.OnPlayersChanged += _RefreshPlayerList;
        GameBootstrap.Instance.Session.OnRoomSettingsChanged += _RefreshRoomSetting;
        GameBootstrap.Instance.Session.OnRemovedFromSession += _HandleRemovedFromSession;
        GameBootstrap.Instance.Session.OnHostChanged += _ChangeHostRefershRoom;
        _RefreshRoomUI();

    }
    private void OnDisable()
    {
        GameBootstrap.Instance.Session.OnPlayersChanged -= _RefreshPlayerList;
        GameBootstrap.Instance.Session.OnRoomSettingsChanged -= _RefreshRoomSetting;
        GameBootstrap.Instance.Session.OnRemovedFromSession -= _HandleRemovedFromSession;
        GameBootstrap.Instance.Session.OnHostChanged -= _ChangeHostRefershRoom;
    }

    private async void _OnCreateClicked()
    {
        string roomName = "Room #" + Random.Range(1, 9999).ToString();
        string playerName=_playerNameInput.text;
        string passWord = _passwordInput.text;
        bool isPrivate = _privateToggle.isOn;

        if (!SessionUtility.IsValidatePassword(passWord))
        {
            string text = "Password must be 8 characters, or leave it empty.";
            _UpdateErrorInfo(text);
            return;
        }

        try
        {
            _createButton.interactable = false;
            passWord = string.IsNullOrWhiteSpace(passWord)?null:passWord;
            JoinSessionResult sessionresult = await GameBootstrap.Instance.Session.CreateSessionAsync(roomName, isPrivate, passWord);
            if(!sessionresult.IsSuccess)
            {
                //TODO: 暂时没有需要直接UI显示的错误信息，后续根据需求添加
                return;
            }
            _roomInfoPanel.UpdateRoomInfo(roomName, sessionresult.Session.Code);
            _updateButton.interactable = true;
        }
        catch (System.Exception e) { 
            _UpdateErrorInfo($"Create room failed: {e.Message}");
        }
        finally
        {
            _RefreshRoomUI();

        }

    }
    private async void _OnUpdatePlayerNameClicked()
    {
        string playerName = _playerNameInput.text;
        //Debug.Log($"Updating player name to {playerName}");
        if (string.IsNullOrWhiteSpace(playerName))
        {
            _UpdateErrorInfo("Player name cannot be empty.");
            return;
        }
        try
        {
            bool result = await GameBootstrap.Instance.Authentication.UpdatePlayerNameAsync(playerName);
            if(!result)
            {
                _UpdateErrorInfo("Failed to update player name.");
                return;
            }
            //_playerNameText.text = AuthenticationService.Instance.PlayerName;
        }
        catch (System.Exception e) {
            Debug.Log(e);
        }
    }
    private async void _OnUpdateClicked()
    {
        string passWord = _passwordInput.text;
        bool isPrivate = _privateToggle.isOn;
        if (!SessionUtility.IsValidatePassword(passWord))
        {
            string text = "Password must be 8 characters, or leave it empty.";
            _UpdateErrorInfo(text);
            return;
        }
        try
        {
            passWord = string.IsNullOrWhiteSpace(passWord) ? string.Empty : passWord;
            Debug.Log($"Updating room settings: isPrivate={isPrivate}, password={(passWord == null ? "null" : passWord)}");
            await GameBootstrap.Instance.Session.UpdateSessionAsync(isPrivate, passWord);
            _passwordDefaultText.text = passWord;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    private async void _OnPrepareClicked() {
        try
        {
            bool isReady = GameBootstrap.Instance.Session.IsPlayerReady(AuthenticationService.Instance.PlayerId);
            if (isReady)
            {
                await GameBootstrap.Instance.Session.SetPlayerReadyStatusAsync(false);
                _prepareButton.GetComponentInChildren<TMP_Text>().text = "Ready";

            }
            else
            {
                await GameBootstrap.Instance.Session.SetPlayerReadyStatusAsync(true);
                _prepareButton.GetComponentInChildren<TMP_Text>().text = "Unready";
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    private async void _OnExitClicked()
    {
        if (GameBootstrap.Instance.Session.CurrentSession != null) {
            if (NetworkManager.Singleton.IsHost) 
            {
                await GameBootstrap.Instance.Session.LeaveSessionAsHostAsync();
            }
            else
            {
                await GameBootstrap.Instance.Session.LeaveSessionAsync();
            }
        }
        UIManager.Instance.ShowMain();
    }

    private void _OnGameStartClicked()
    {
        if (GameBootstrap.Instance.Session.CurrentSession?.IsHost==true&& GameBootstrap.Instance.Session.IsAllPlayersReady())
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Battle1",LoadSceneMode.Single);
        }
       
    }
    private void _OnCopyCodeClicked()
    {
        if(GameBootstrap.Instance.Session.CurrentSession != null)
        {
            string code = GameBootstrap.Instance.Session.CurrentSession.Code;
            GUIUtility.systemCopyBuffer = code;
            Debug.Log($"Copied session code: {code}");
        }
    }
    private void _UpdateErrorInfo(string message)
    {
        _errorText.text = message;
        _errorText.gameObject.SetActive(true);
    }

    private void _RefreshPlayerList()
    {
        Debug.Log("Refresh player list");
        foreach (var slot in _playerInfoSlots)
        {
            slot?.Clear();
            slot?.gameObject.SetActive(false);
        }
        ISession session = GameBootstrap.Instance.Session.CurrentSession;
        if(session == null) { return; }

        HashSet<string> currentplayerIds = new(); 
        foreach (var player in session.Players) { currentplayerIds.Add(player.Id); }
        //delete old player
        List<string> removedPlayers = new();
        foreach (var pair in _playerUIs)
        {
            if (!currentplayerIds.Contains(pair.Key))
            {
                removedPlayers.Add(pair.Key);
            }
        }
        foreach (string player in removedPlayers)
        {
            _playerUIs.Remove(player);
        }

        //Add/refresh new player
        foreach (var player in session.Players) {
            string playerId = player.Id;  
            if(_playerUIs.TryGetValue(playerId,out PlayerRoomInfoPanel playerInfo)){
                playerInfo.gameObject.SetActive(true);
                playerInfo.UpdatePlayerInfo(player, session.Host);
            }
            else
            {
                PlayerRoomInfoPanel playerRoomInfo = _GetFreePlayerSlot();
                _playerUIs.Add(playerId, playerRoomInfo);
                playerRoomInfo.gameObject.SetActive(true);
                playerRoomInfo.UpdatePlayerInfo(player,session.Host);
            }
        }
        string displayName = SessionUtility.ToDisplayName(AuthenticationService.Instance.PlayerName);
        _playerNameText.text = displayName;

        if(GameBootstrap.Instance.Session.CurrentSession?.IsHost == true)
        {
            _gameStartButton.interactable = GameBootstrap.Instance.Session.IsAllPlayersReady();
        }
    }
    
    private void _RefreshRoomSetting()
    {
        Debug.Log("Refresh room setting");
        ISession session = GameBootstrap.Instance.Session.CurrentSession;
        if (session == null)
            return;
        _privateToggle.isOn = session.IsPrivate;
        if (!session.HasPassword)
        {
            _passwordDefaultText.text = "";
        }
        else if(GameBootstrap.Instance.Session.CurrentSession?.IsHost==false)
        {
            _passwordDefaultText.text = "*********";
        }
     
    }
    private PlayerRoomInfoPanel _GetFreePlayerSlot() {

        foreach (PlayerRoomInfoPanel p in _playerInfoSlots) {
            if (!_playerUIs.ContainsValue(p)){
                return p;
            }
        }
        return null;
    }
    private void _HandleRemovedFromSession()
    {
        UIManager.Instance.ShowRemoved();
    }
    private void _ChangeHostRefershRoom()
    {
        
        if (GameBootstrap.Instance.Session.CurrentSession?.IsHost == true)
        {
            Debug.Log("Host change to this player");
            _RefreshRoomUI();
        }
        else
        {
            Debug.Log("Host change, this player is now Client.");
        }
    }
    private void _RefreshRoomUI() { 
         if(GameBootstrap.Instance.Session.CurrentSession == null)
         {
             _createButton.interactable = true;
             _createButton.gameObject.SetActive(true);
            _updateButton.interactable = false;
            _updateButton.gameObject.SetActive(false);
            _updatePlayerNameButton.gameObject.SetActive(false);
            _copyCodeButton.gameObject.SetActive(false);
            _prepareButton.interactable = false;
            _privateToggle.interactable = true;
            _passwordInput.interactable = true;
            _gameStartButton.interactable = false;
            _gameStartButton.gameObject.SetActive(false);
        }
        else if(GameBootstrap.Instance.Session.CurrentSession?.IsHost == true)
        {
            _createButton.interactable = false;
            _createButton.gameObject.SetActive(false);
            _updateButton.interactable = true;
            _updateButton.gameObject.SetActive(true);
            _copyCodeButton.gameObject.SetActive(true);
            _prepareButton.interactable = true;
            _updatePlayerNameButton.gameObject.SetActive(true);
            _privateToggle.interactable = true;
            _passwordInput.interactable = true;
            _gameStartButton.gameObject.SetActive(true);
        }
        else //player is client
        {
            _createButton.interactable = false;
            _createButton.gameObject.SetActive(false);
            _updateButton.interactable = false;
            _updateButton.gameObject.SetActive(false);
            _updatePlayerNameButton.gameObject.SetActive(true);
            _copyCodeButton.gameObject.SetActive(true);
            _prepareButton.interactable = true;
            _privateToggle.interactable = false;
            _passwordInput.interactable = false;
            _gameStartButton.gameObject.SetActive(false);
        }
        _prepareButton.GetComponentInChildren<TMP_Text>().text = GameBootstrap.Instance.Session.IsPlayerReady(AuthenticationService.Instance.PlayerId) ? "Unready" : "Ready";
        string roomname= GameBootstrap.Instance.Session.CurrentSession!=null? GameBootstrap.Instance.Session.CurrentSession.Name:string.Empty;
        _roomInfoPanel.UpdateRoomInfo(roomname, GameBootstrap.Instance.Session.CurrentSession?.Code);
        _RefreshPlayerList();

    }
}
