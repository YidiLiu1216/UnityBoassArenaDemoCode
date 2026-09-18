
using UnityEngine;
using Unity.Cinemachine;
public class CameraManager : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField]
    private CinemachineCamera _followCamera;
    private bool _isBinding = false;
    public static CameraManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }
    
    private void Update()
    {
        
    }
    private void OnDisable()
    {
       
    }
    private void _TryFindLocalPlayer()
    {
        PlayerMovement[] players =FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

        foreach (PlayerMovement player in players)
        {
            if (!player.IsSpawned|| !player.IsOwner)
                continue;

            BindingCamera(player.transform);
            return;
        }
    }
    public void BindingCamera(Transform player) {
        _followCamera.Follow = player;
        _isBinding = true;
    }
}
