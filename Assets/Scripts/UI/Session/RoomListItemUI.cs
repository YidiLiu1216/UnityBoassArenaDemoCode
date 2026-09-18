using System;
using TMPro;
using Unity.Services.Multiplayer;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class RoomListItemUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField]
    private TMP_Text _roomNameText;
    [SerializeField]
    private TMP_Text _playerCountText;

    [Header("Button")]
    [SerializeField]
    private Button _joinButton;

    [Header("Icon")]
    [SerializeField]
    private Image _hasPasswordImage;

    private ISessionInfo _session;
    private Action<ISessionInfo> _onSelected;

    private void Awake()
    {
        _joinButton.onClick.AddListener(_OnJoinClicked);
    }
    public void SetUp(ISessionInfo session)
    {
        _session = session;

        _roomNameText.text = session.Name;
        _hasPasswordImage.gameObject.SetActive(session.HasPassword);
        if (session.Properties.TryGetValue("PlayerCount",out var playerCountProperty))
        {
            int playerCount = int.Parse(playerCountProperty.Value);
            _playerCountText.text =$"{playerCount}/{session.MaxPlayers}";
        }
       
    }
    private async void _OnJoinClicked()
    {
        if (_session != null)
        {
            JoinSessionResult result = await GameBootstrap.Instance.Session.JoinSessonById(_session.Id);
            if(result.IsSuccess)
            {
                UIManager.Instance.ShowCreateRoom();
            }else if(result.Type == JoinSessionResultType.InCorrectPassword)
            {
                UIManager.Instance.ShowPassword(_session.Id,null);
            }
            else
            {
                Debug.LogError($"Failed to join session: {result.DebugMessage}");
            }
         
        }
    }
}
