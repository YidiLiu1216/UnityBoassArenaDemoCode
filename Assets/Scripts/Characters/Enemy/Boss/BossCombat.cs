using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BossCombat : NetworkBehaviour
{
    [Header("Slam Setting")]
    [SerializeField]
    private float _slamRadius = 7f;
    [SerializeField]
    private Transform _warningCircle;
    [SerializeField]
    private float _circlePrepareTime = 0.5f;
    [SerializeField]
    private float _slamPrepareTime = 1.5f;
    [SerializeField]
    private float _slamActionPrepareTime = 0.3f;
    [SerializeField]
    private float _slamRecoverTime = 0.5f;
    [SerializeField]
    private int _slamDamage = 20;
    [SerializeField]
    private LayerMask _playerLayer;
    [Header("Punch Setting")]
    private int _punchDamage = 10;
    [Header("Animation")]
    [SerializeField]
    private Animator _animator;

    private bool _isUsingSkill;

    private void Awake()
    {
   
    }
    private void Update()
    {
     
    
    }
    public void StartGroundSlam(float preparetime,float animepretime)
    {
        StartCoroutine(_GroundSlamRoutine(preparetime,animepretime));
    }
    public void StartPunch(float hittime, PlayerHealth target,float range,float angle)
    {
        StartCoroutine(_PunchRoutine(hittime,target,range,angle));
    }
    public PlayerHealth SelectTarget()
    {
        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        List<PlayerHealth> alivePlayers = new();

        foreach (PlayerHealth player in players)
        {
            if (!player.IsDeath)
            {
                alivePlayers.Add(player);
            }
        }
        if (alivePlayers.Count == 0)
        {
            return null;
        }
        int randomIndex = Random.Range(0, alivePlayers.Count);
        return alivePlayers[randomIndex];
    }
    public Vector3 GetVectorToTarget(Transform target)
    {
        Vector3 toTarget = target.position-transform.position;
        toTarget.y = 0;
        return toTarget;
    }
    public void HideSlamWarning()
    {
        _HideSlamWarningServerRPC();
    }
    private IEnumerator _GroundSlamRoutine(float preparetime,float animepreparetime)
    {
        _isUsingSkill = true;
        _ShowSlamWarningServerRPC();
        yield return new WaitForSeconds(preparetime);
        _animator.SetTrigger("Smash");
        yield return new WaitForSeconds(animepreparetime);
        _DoGroundSlamDamage();
       // yield return new WaitForSeconds(_slamRecoverTime );
       // _HideSlamWarningServerRPC();
        _isUsingSkill = false;
    }
    private IEnumerator _AnimateWarningCircle() {
        float timer = 0f;
        Vector3 startScale = Vector3.one * 0.1f;
        Vector3 endScale = Vector3.one * 2* _slamRadius;
        _warningCircle.localScale = startScale;
        while (timer<_circlePrepareTime) {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer /_circlePrepareTime);
            _warningCircle.localScale = Vector3.Lerp(startScale,endScale,t);
            yield return null;
        }

        _warningCircle.localScale = endScale;
    }
    private IEnumerator _PunchRoutine(float punchhitTime,PlayerHealth target,float range,float angle)
    {
        _animator.SetTrigger("Punch");
        yield return new WaitForSeconds(punchhitTime);
        _CheckPunchHit(target,range,angle);
    }
    private void _DoGroundSlamDamage() {
        if (!IsServer) { return; }
        Collider[] hits = Physics.OverlapSphere(transform.position,_slamRadius,_playerLayer);
        HashSet<PlayerHealth> damagedPlayers =new();
        Debug.Log($"smash at{transform.position},{hits.Length} Players in Range!");
        foreach (Collider hit in hits) { 
           PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
           playerHealth?.TakeDamage(_slamDamage);
        }
    }
    private void _CheckPunchHit(PlayerHealth target,float range,float angle)
    {
        if(!IsServer||target.IsDeath) { return; }
        Vector3 toTarget = GetVectorToTarget(target.transform);
        if (toTarget.magnitude > range) { return; }
        float curangle= Vector3.Angle( transform.forward,toTarget.normalized);
        if(curangle > angle/2) { return; }
        target.TakeDamage(_punchDamage);
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void _ShowSlamWarningServerRPC()
    {
        _warningCircle.gameObject.SetActive(true);
        StartCoroutine(_AnimateWarningCircle());
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void _HideSlamWarningServerRPC()
    {
        _warningCircle?.gameObject.SetActive(false);
    }
}
