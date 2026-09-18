using UnityEngine;
using Unity.Netcode;
public class BossMovement : NetworkBehaviour
{
    [Header("Movement Setting")]
    [SerializeField]
    private float _movmentSpeed=5.0f;

    private bool _isMoving;
   
    

}
