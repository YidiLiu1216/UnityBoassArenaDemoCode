
using System;
using Unity.Netcode;
using UnityEngine;

public class BossHealth : NetworkBehaviour
{
    [SerializeField]
    private int _bossMaxHealth = 500;

    [SerializeField]
    private Animator _animator;
    public NetworkVariable<int> BossCurrentHealth=new();

    private bool _isDead;
    public bool IsDead => _isDead;
    public event Action OnBossDied;
    public int BossMaxHealth => _bossMaxHealth;
  
    public void InitBossHealth()
    {
        BossCurrentHealth.Value = _bossMaxHealth;
        _isDead = false;
    }
    public void TakeDamage(int damage)
    {
        if (!IsServer){ return; }
        BossCurrentHealth.Value -= damage;
        Debug.Log($"Boss Health is Update to {BossCurrentHealth}");
        if (BossCurrentHealth.Value <= 0)
        {
            BossCurrentHealth.Value = 0;
            _animator.SetTrigger("Death");
            _isDead=true;
            OnBossDied?.Invoke();
            return;
        }
        _animator.SetTrigger("Damaged");
    }

}
