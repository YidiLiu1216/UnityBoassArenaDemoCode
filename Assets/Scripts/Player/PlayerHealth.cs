using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Collections;
public class PlayerHealth : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> PlayerName = new(default, NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    [SerializeField]
    private int _playerMaxHealth = 50;
    [SerializeField]
    private Animator _animator;

    public NetworkVariable<int> PlayerCurrentHealth = new();

    private bool _isDeath = false;
    public int PlayerMaxHealth => _playerMaxHealth;
    public bool IsDeath => _isDeath;
    public event Action OnPlayerDied;

    public override void OnNetworkSpawn() {
        if (IsServer) {
            PlayerCurrentHealth.Value = _playerMaxHealth;
        }
        if (IsOwner)
        {
            PlayerName.Value = GameBootstrap.Instance.Session.GetMyPlayerName() ?? "Unknown";
        }
        BossFightUIManager.Instance?.RegisterPlayer(this);
    }
    public override void OnNetworkDespawn() { 

        BossFightUIManager.Instance?.UnregisterPlayer(this);
    }
    public void TakeDamage(int damgage) {
        if (!IsServer||_isDeath) { return; }
        PlayerCurrentHealth.Value -= damgage;
        if(PlayerCurrentHealth.Value <= 0) {  
            PlayerCurrentHealth.Value = 0;
            _animator.SetTrigger("Death");
            _PlayerDeathClientRPC();
            BattleManager.Instance.CheckAllPlayersDead();
            return;
        }
        Debug.Log($"Player Health Update to{PlayerCurrentHealth}");
        _animator.SetTrigger("Damaged");
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void _PlayerDeathClientRPC()
    {
        _isDeath = true;
        OnPlayerDied?.Invoke();
    }

}
