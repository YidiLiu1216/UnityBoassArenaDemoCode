using UnityEngine;
using Unity.Netcode;
public class NetworkPlayerCharacter : NetworkBehaviour
{
    private PlayerMovement _movement;
    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _movement.SetInputEnabled(IsOwner);
    }
}
