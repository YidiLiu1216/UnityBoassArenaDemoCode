using UnityEngine;
using TMPro;
using Unity.Netcode;
public class PlayerInfoPanel : MonoBehaviour
{
    [Header("Output")]
    [SerializeField]
    private TextMeshProUGUI _playerName;
    [SerializeField]
    private TextMeshProUGUI _playerDamageDealt;

    public void UpdatePlayerStatInfo(PlayerBattleStats stat)
    {
        if (stat == null) {  return; }
        _playerName.text = SessionUtility.ToDisplayName(stat.Name);
        _playerDamageDealt.text = $"Damage Dealt: \n {stat.DamageDealt}";
    }
}
