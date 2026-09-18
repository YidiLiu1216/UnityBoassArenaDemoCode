using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
public class PlayerHealthBar :MonoBehaviour
{
    [SerializeField]
    private RectTransform _healthBarPanel;
    [SerializeField]
    private TextMeshProUGUI _playerName;
    private float _maxHealthBarWidth;

    private PlayerHealth _playerHealth;
    private void Awake()
    {
        _maxHealthBarWidth=_healthBarPanel.rect.width;
    }

    public void Bind(PlayerHealth playerHealth)
    {
        gameObject.SetActive(true);
        _playerHealth = playerHealth;
        _playerHealth.PlayerCurrentHealth.OnValueChanged += _UpdateHealthUI;
        _UpdateHealthUI(_playerHealth.PlayerCurrentHealth.Value,_playerHealth.PlayerCurrentHealth.Value);
        if (playerHealth.OwnerClientId != NetworkManager.Singleton.LocalClientId)
        {
            _playerHealth.PlayerName.OnValueChanged += _OnPlayerNameChanged;
            _OnPlayerNameChanged(default, _playerHealth.PlayerName.Value); 
        }
    }
    public void UnBind() {
        _playerHealth.PlayerCurrentHealth.OnValueChanged -= _UpdateHealthUI;
        _playerHealth.PlayerName.OnValueChanged -= _OnPlayerNameChanged;
        _playerHealth = null;
        gameObject.SetActive(false);
    }
    public void UpdatePlayerName(string name)
    {
        _playerName.text = SessionUtility.ToDisplayName(name);
    }

    private void _UpdateHealthUI(int oldValue, int newValue)
    {

        float targetWidth = ((float)newValue / _playerHealth.PlayerMaxHealth) * _maxHealthBarWidth;
          float healthPercent =
            Mathf.Clamp01(
                (float)newValue /
                _playerHealth.PlayerMaxHealth
            );
        Debug.Log($"The player health is now{newValue},Update the UI Health Value to{targetWidth}");
    
        _healthBarPanel.sizeDelta = new Vector2(targetWidth, _healthBarPanel.sizeDelta.y);
    }
    private void _OnPlayerNameChanged(FixedString64Bytes prev, FixedString64Bytes cur)
    {  
        UpdatePlayerName(cur.ToString());
    }
}
