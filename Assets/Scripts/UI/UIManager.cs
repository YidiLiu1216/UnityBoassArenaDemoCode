using UnityEngine;

public enum LobbyPage { 
   Main,
   CreateRoom,
   BrowseRoom,
   Password,
    Removed
}
public class UIManager:MonoBehaviour 
{
    public static UIManager Instance;

    [Header("Panels")]
    [SerializeField]
    private GameObject _mainMenuPanel;
    [SerializeField]
    private GameObject _createRoomPanel;
    [SerializeField]
    private GameObject _roomBrowsePanel;
    [SerializeField]
    private GameObject _passwordPanel;
    [SerializeField]
    private GameObject _removedPanel;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(this); }
    }
    private void Start()
    {
       

        if (GameBootstrap.Instance.Session.CurrentSession != null)
        {
            ShowCreateRoom();
        }
    }

    public void ShowPage(LobbyPage page)
    {
        _mainMenuPanel.SetActive(page == LobbyPage.Main);
        _createRoomPanel.SetActive(page==LobbyPage.CreateRoom);
        _roomBrowsePanel.SetActive(page==LobbyPage.BrowseRoom);
        _passwordPanel.SetActive(page==LobbyPage.Password);
        _removedPanel.SetActive(page==LobbyPage.Removed);
    }
    public void ShowMain() { ShowPage(LobbyPage.Main); }
    public async void ShowCreateRoom() { ShowPage(LobbyPage.CreateRoom);  }
    public void ShowBrowseRoom() { ShowPage(LobbyPage.BrowseRoom); }
    public void ShowPassword(string sessionId=null,string code=null) { 
        ShowPage(LobbyPage.Password); 
        _passwordPanel.GetComponent<PasswordPanelUI>().SetSessionIDorCode(sessionId,code);
    }
    public void ShowRemoved() { ShowPage(LobbyPage.Removed); }
}
