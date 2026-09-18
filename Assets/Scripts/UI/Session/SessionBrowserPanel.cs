using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
public class SessionBrowserPanel : MonoBehaviour
{
    [Header("Rooms")]
    [SerializeField]
    private GameObject[] _rooms = new GameObject[20];

    [Header("Buttons")]
    [SerializeField]
    private Button _refreshButton;
    [SerializeField]
    private Button _quickJoinButton;
    [SerializeField]
    private Button _joinWithCodeButton;
    [SerializeField]
    private Button _exitButton;

    [Header("Input")]
    [SerializeField]
    private TMP_InputField _joinCode;
    private bool _isBusy;

    private void Awake()
    {
        _exitButton.onClick.AddListener(_OnExitButtonClicked);
        _refreshButton.onClick.AddListener(_OnFreshClicked);
        _quickJoinButton.onClick.AddListener(_OnQuickJoinButtonClicked);
        _joinWithCodeButton.onClick.AddListener(_OnJoinWithCodeButtonClicked);
        _isBusy = false;
    }
    private async void OnEnable()
    {
        await _RefreshSessionsAsync();
    }

    private async void _OnFreshClicked()
    {
        await _RefreshSessionsAsync();
    }
    private async Task _RefreshSessionsAsync()
    {
        if (_isBusy) return;
        try
        {
            _SetBusy(true);
            _ClearList();
            QuerySessionsResults results = await GameBootstrap.Instance.Session.BrowseSessionsAsync();
            int roomcounter = 0;
            if (results != null)
            {
                foreach (var session in results.Sessions)
                {
                    _rooms[roomcounter].SetActive(true);
                    _rooms[roomcounter].GetComponent<RoomListItemUI>()?.SetUp(session);
                    roomcounter++;
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        finally
        {
            _SetBusy(false);
        }
    }
    private void _ClearList()
    {
        foreach (GameObject room in _rooms)
        {
            room.SetActive(false);
        }
    }
    private void _SetBusy(bool busy)
    {
        _isBusy = busy;

        _refreshButton.interactable = !busy;
        _quickJoinButton.interactable = !busy;
        //TODO: maybe add loading indicator
    }
    private void _OnExitButtonClicked()
    {
        UIManager.Instance.ShowMain();
    }
    private async void _OnJoinWithCodeButtonClicked()
    {
        string code = _joinCode.text;
        if (string.IsNullOrEmpty(code))
        {
            Debug.Log("Join code is empty.");
            return;
        }
        JoinSessionResult joinResult = await GameBootstrap.Instance.Session.JoinSessionByCodeAsync(code);
        if (joinResult.IsSuccess)
        {
            UIManager.Instance.ShowCreateRoom();
        }
        else if (joinResult.Type == JoinSessionResultType.InCorrectPassword)
        { 
            UIManager.Instance.ShowPassword(null,code);
        } else
        {
            Debug.Log($"Failed to join session with code: {code}. Reason: {joinResult.DebugMessage}"); //TODO: handle fail case with UI feedback
        }
    }

    private async void _OnQuickJoinButtonClicked()
    {
        JoinSessionResult joinResult = await GameBootstrap.Instance.Session.QuickJoinSessionAsync();
        if (joinResult.IsSuccess)
        {
            UIManager.Instance.ShowCreateRoom();
        }
        else
        {
            Debug.Log($"Failed to quick join session. Reason: {joinResult.DebugMessage}"); //TODO: handle fail case with UI feedback
        }
    }
}
