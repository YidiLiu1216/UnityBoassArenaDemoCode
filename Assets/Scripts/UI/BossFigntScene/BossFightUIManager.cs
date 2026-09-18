using UnityEngine;
using Unity.Netcode;
using NUnit.Framework;
using System.Collections.Generic;
public class BossFightUIManager :MonoBehaviour
{
    [Header("Host")]
    [SerializeField]
    private PlayerHealthBar _hostHealthBar;
    
    [Header("Client")]
    [SerializeField]
    private PlayerHealthBar[] _clientHealthBars = new PlayerHealthBar[3];

    private readonly Dictionary<ulong, PlayerHealthBar> _bindings = new();

    public static BossFightUIManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        _hostHealthBar.gameObject.SetActive(false);
        foreach(var ui in _clientHealthBars)
        {
            ui.gameObject.SetActive(false);
        }
    }
    private void Start()
    {
        _RegisterExistingPlayers();
    }
    public void RegisterPlayer(PlayerHealth playerHealth)
    {
        Debug.Log($"Register{playerHealth.OwnerClientId} to Player UI");
        ulong clientId = playerHealth.OwnerClientId;
        if (_bindings.ContainsKey(clientId))return;

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Host Register to UI");
            _hostHealthBar.Bind(playerHealth);
            _bindings.Add(clientId, _hostHealthBar);
            return;
        }
        foreach(var ui in _clientHealthBars)
        {
            if (_bindings.ContainsValue(ui)) { continue; }
            Debug.Log("Client Register to UI");
            ui.Bind(playerHealth);
            _bindings.Add(clientId, ui);
            
            return;
        }
        
        Debug.Log("Fail to bind UI");
    }
    public void UnregisterPlayer(PlayerHealth playerHealth) {
        ulong cliendId = playerHealth.OwnerClientId;
        if(! _bindings.TryGetValue(cliendId,out PlayerHealthBar ui))
        {
            return;
        }
        ui.UnBind();
        ui.gameObject.SetActive(false);
        _bindings.Remove(cliendId);
    }
    public void UpdatePlayerNameDisplay(PlayerBattleStats[] stats)
    {
        foreach (PlayerBattleStats stat in stats) {
            if (stat.ClientId == NetworkManager.Singleton.LocalClientId) {
                continue;//do not name this computer player          
             }
            _bindings.TryGetValue(stat.ClientId, out PlayerHealthBar ui);
            if (ui != null) {
                ui.UpdatePlayerName(stat.Name);
            }
        }
    }
    private void _RegisterExistingPlayers()
    {
        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        foreach (PlayerHealth player in players)
        {
            RegisterPlayer(player);
        }
    }
}
