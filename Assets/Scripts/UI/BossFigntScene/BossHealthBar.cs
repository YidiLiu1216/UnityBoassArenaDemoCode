using UnityEngine;

public class BossHealthBar :MonoBehaviour
{
    [SerializeField]
    private BossHealth _bossHealth;

    [SerializeField] 
    private RectTransform _healthBarPanel;

    private float _maxHealthBarWidth;

    private void Awake()
    {
        _maxHealthBarWidth = _healthBarPanel.rect.width;
    }
    
    private void OnEnable()//Todo: change to on netwrorkspawn after it comes to spanw on network
    {
        _bossHealth.BossCurrentHealth.OnValueChanged +=  _UpdateHealthUI;
        _UpdateHealthUI(_bossHealth.BossCurrentHealth.Value, _bossHealth.BossCurrentHealth.Value);
    }
    private void OnDisable()
    {
        _bossHealth.BossCurrentHealth.OnValueChanged -= _UpdateHealthUI;
    }

    private void _UpdateHealthUI(int oldValue, int newValue) {

       
        float targetWidth = ((float)newValue /_bossHealth.BossMaxHealth) * _maxHealthBarWidth;
        Debug.Log($"The health is now{newValue},Update the UI Health Value to{targetWidth}");
        _healthBarPanel.sizeDelta = new Vector2(targetWidth, _healthBarPanel.sizeDelta.y);
    }
}
