using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
//Save Player In Battle Data
public class PlayerBattleStats : INetworkSerializable
{
    public ulong ClientId;
    public string Name;
    public int DamageDealt;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref DamageDealt);
    }
}
public class BattleManager : NetworkBehaviour
{
    public static BattleManager Instance { get; private set; }
    private Dictionary<ulong, PlayerBattleStats> _stats = new();
    private void Awake()
    {
        if(Instance == null) { Instance=this; }
        else { Destroy(this); }
    }
    public override void OnNetworkSpawn()
    {
       
    }
    public override void OnNetworkDespawn()
    {
        _stats.Clear();
    }
    public void OnBossDied(bool isSuccess)
    {
        _EndBattle(isSuccess);
    }
    public void CheckAllPlayersDead()
    {
        PlayerHealth[] players =FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        foreach (var player in players) {
            if (!player.IsDeath){ return; }
        }
        _EndBattle(false);
    
    }
 
    public void RecordDamageDealt(ulong clientId, int damage)
    {
        if (!_stats.TryGetValue(clientId,out var stat))
        {
            stat = new PlayerBattleStats
            {
                ClientId = clientId
            };

            _stats.Add(clientId, stat);
        }
        stat.DamageDealt += damage;
    }
    public void RecordPlayerName(ulong clientId,string name)
    {
        if (!_stats.TryGetValue(clientId, out var stat))
        {
            stat = new PlayerBattleStats
            {
                ClientId = clientId
            };

            _stats.Add(clientId, stat);
        }
        stat.Name = name;
        //If all players name set, update all players name ui
        //foreach(ulong client in NetworkManager.Singleton.ConnectedClientsIds)
        //{
            //if (!_stats.ContainsKey(clientId) || _stats[clientId].Name == null) { return; }
        //}
       // _DisplayPlayerNameClientRpc(_stats.Values.ToArray());

    }
    public PlayerBattleStats GetPlayerBattleStatbyId(ulong cliendId)
    {
          _stats.TryGetValue(cliendId,out var stat);
         return stat;
    }
    private void _ClearAllSetting()
    {
        _stats.Clear();
    }
    private void _EndBattle(bool isSuccess)
    {
        //EndBossMovement
        _DisablePlayerClientRpc();
        PlayerBattleStats[] finalStats = _stats.Values.ToArray();
        _ShowResultClientRpc(isSuccess,finalStats);
    }
   // [Rpc(SendTo.ClientsAndHost)]
   // private void _DisplayPlayerNameClientRpc(PlayerBattleStats[] stats)
   // {
    //    BossFightUIManager.Instance.UpdatePlayerNameDisplay(stats);
    //}

    [Rpc(SendTo.ClientsAndHost)]
    private void _DisablePlayerClientRpc()
    {
        foreach (var player in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
        {
            if (player.IsOwner)
            { player.SetInputEnabled(false); }
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void _ShowResultClientRpc(bool isSuccess, PlayerBattleStats[] stats)
    {
        BossUIManager.Instance.ShowEndBattle(isSuccess,stats);
    }
}
