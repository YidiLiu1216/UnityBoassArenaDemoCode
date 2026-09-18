using UnityEngine;
using UnityEngine.UI;

public class MainUIPanel : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _joinRoomButton;


    private void Awake()
    {
        _createRoomButton.onClick.AddListener(_OnCreateRoomButtonClicked);
        _joinRoomButton.onClick.AddListener(_OnJoinRoomButtonClicked);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void _OnCreateRoomButtonClicked()
    {
        UIManager.Instance.ShowCreateRoom();
    }
    private void _OnJoinRoomButtonClicked()
    {
        UIManager.Instance.ShowBrowseRoom();
    }
}
