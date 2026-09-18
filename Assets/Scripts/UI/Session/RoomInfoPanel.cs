using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;


public class RoomInfoPanel: MonoBehaviour
{
    [SerializeField]
    private TMP_Text _roomNameText;

    [SerializeField]
    private TMP_Text _joinCodeText;


    public void UpdateRoomInfo(string roomname, string joincode)
    {
        _roomNameText.text = roomname;
        _joinCodeText.text = joincode;
        _joinCodeText.gameObject.SetActive(true);
    }
}
