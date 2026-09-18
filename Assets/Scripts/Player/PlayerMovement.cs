using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Setting")]
    [SerializeField]
    private float _moveSpeed = 5f;
    private bool _inputEnabled;

    [SerializeField]
    private float _rotateSpeed = 10f;
    [SerializeField]
    private Rigidbody _rigidbody;
    [Header("Input")]
    [SerializeField]
    private InputActionReference _moveAction;
    [SerializeField]
    private InputActionReference _attackAction;

    [Header("AttackSetting")]
    [SerializeField]
    private float _attackRange = 5.0f;
    [SerializeField]
    private float _attackAngle = 90f;
    [SerializeField]
    private int _attackDamage = 20;
    [SerializeField]
    private float _attackPrepareTime = 0.1f;

    [Header("Animation")]
    [SerializeField]
    private Animator _animator;

    [Header("Timers")]
    [SerializeField]
    private float _attackTimeInterval=1.0f;
    private float _nextAttackTime;
    [Header("Compnents")]
    [SerializeField]
    private PlayerHealth _health;

    private bool _isWalking=false;
    private Vector3 _moveDirection=Vector3.zero;
 
    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
    }
    public override void OnNetworkSpawn()
    {
        _health.OnPlayerDied += _OnPlayerDied;
        if (IsOwner)
        {
            _moveAction.action.Enable();
            _attackAction.action.Enable();
            _inputEnabled = true;
            _isWalking = false;
            CameraManager.Instance.BindingCamera(transform);
        }
    }
    public override void OnNetworkDespawn()
    {
        _health.OnPlayerDied -= _OnPlayerDied;
        if (IsOwner)
        {
            _moveAction.action.Disable();
            _attackAction.action.Disable();
            _inputEnabled = false;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner||!_inputEnabled) { return; } 
        Vector2 input = _moveAction.action.ReadValue<Vector2>();
        Vector3 direction=new Vector3(input.x, 0, input.y);
        _isWalking = direction.sqrMagnitude > 0.01f;

        if (_isWalking)
        {
            //Debug.Log($"Player {OwnerClientId} is walking");
            _moveDirection = direction.normalized;
            //transform.position += direction * _moveSpeed * Time.deltaTime;
            //Quaternion targetRotation = Quaternion.LookRotation(direction);
            //transform.rotation =Quaternion.Slerp(transform.rotation,targetRotation, _rotateSpeed * Time.deltaTime);
        }
         //_animator?.SetBool("IsWalking",_isWalking);
        _SetWalkStatusServerRpc(_isWalking);

        if (_attackAction.action.WasPressedThisFrame())
        {
            _RequestAttackServerRPC();
        }
    }
    private void FixedUpdate()
    {
        if (!IsOwner || !_inputEnabled||!_isWalking) { return; }
        Vector3 targetPosition = _rigidbody.position +_moveDirection * _moveSpeed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(targetPosition);
        Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
        Quaternion newRotation = Quaternion.Slerp(_rigidbody.rotation,targetRotation,_rotateSpeed * Time.fixedDeltaTime);
        _rigidbody.MoveRotation(newRotation);
    }
    [Rpc(SendTo.Server)]
    private void _RequestAttackServerRPC()
    {
        Debug.Log($"Player {OwnerClientId} send an attack rpc");
        if (Time.time < _nextAttackTime)
            return;
        _nextAttackTime = Time.time + _attackTimeInterval;
       
        _PlayAttackClientRpc();
        StartCoroutine(_AttackRoutine());
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void _PlayAttackClientRpc()
    {
        Debug.Log($"Player {OwnerClientId} attack.");
        _animator?.SetTrigger("Attack");
    }
    [Rpc(SendTo.Server)]
    private void _SetWalkStatusServerRpc(bool iswalking)
    {
        _animator?.SetBool("IsWalking", iswalking);
    }
    private void _CheckAttackHit() {
        BossHealth boss =FindFirstObjectByType<BossHealth>();
        Vector3 toBoss = boss.transform.position - transform.position;
        toBoss.y = 0f;
        Debug.Log($"BOSS IS in{toBoss.magnitude} distance");
        if (toBoss.magnitude > _attackRange) { return; }

        float angle =Vector3.Angle(transform.forward,toBoss.normalized);
        Debug.Log($"BOSS IS in{angle} angle");
        if (angle > _attackAngle / 2f)
            return;
        Debug.Log($"Make damage to boss {boss.NetworkObject.NetworkObjectId}");
       
        boss.TakeDamage(_attackDamage);
        BattleManager.Instance.RecordDamageDealt(OwnerClientId, _attackDamage);
    }
    private IEnumerator _AttackRoutine()
    {
        yield return new WaitForSeconds(_attackPrepareTime);
        _CheckAttackHit();
    }
    private void _OnPlayerDied()
    {
        _inputEnabled= false;
        if (IsOwner)
        {
            _moveAction.action.Disable();
            _attackAction.action.Disable();
        }
    }

}
