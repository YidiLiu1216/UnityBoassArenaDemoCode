using NUnit.Framework;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkPlayerSpawner : NetworkBehaviour
{
    [Header("Spawn Setting")]
    [SerializeField]
    private GameObject _playerPrefab;
    [SerializeField]
    private Transform[] _spawnPoints= new Transform[4];
    public override void OnNetworkSpawn()
    {
        string name = GameBootstrap.Instance.Session.GetMyPlayerName();
        _AddPlayerNameServerRpc(name);
        if (!IsServer) { return; }
        NetworkManager.SceneManager.OnLoadComplete += _OnClientLoadComplete;
        //foreach (var client in NetworkManager.Singleton.ConnectedClientsList) {
        //    _SpawnPlayer(client.ClientId);
        //}
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer) { NetworkManager.SceneManager.OnLoadComplete -= _OnClientLoadComplete; }
    }
    private void _OnClientLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName != "Battle1") { return; } 
        _SpawnPlayer(clientId);
    }
    private void _SpawnPlayer(ulong clientId)
    {
        int index = (int)(clientId % (ulong)_spawnPoints.Length);
        Transform spawnpoint=_spawnPoints[index];
        GameObject player =Instantiate(_playerPrefab,spawnpoint.position,spawnpoint.rotation);
        NetworkObject networkObject = player.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(clientId,true);
    }
    [Rpc(SendTo.Server)]
    private void _AddPlayerNameServerRpc(string name, RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"Add Name {name} and clientId {clientId} to Server Record");
        BattleManager.Instance.RecordPlayerName(clientId, name);
    }

}
