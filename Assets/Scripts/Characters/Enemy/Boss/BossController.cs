using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
public class BossController : NetworkBehaviour
{
    [Header("BossBehaviour")]
    [SerializeField]
    private BossCombat _bossCombat;
    [SerializeField]
    private BossHealth _health;
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private NavMeshAgent _agent;
    //FSM
    [SerializeField]
    private BosscConfig _config;
    private BossContext _bossContext;
    private BossStateMachine _bossFSM;

    private void Awake()
    {
        //_nextSlamTime = Time.time + 10f;
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer) { enabled = false; return; }
        _health.OnBossDied += _OnBossDied;

        _bossContext = new BossContext();
        _bossFSM = BossFSMBuilder.BuildFSM(_bossContext);
        _bossFSM.InitStateMachine(_bossContext, _bossCombat, _health, _config,_animator,_agent);
        _bossContext.IsBossActivate = true;
    }
    public override void OnNetworkDespawn()
    {
        if (!IsServer) { return; }
        _health.OnBossDied -= _OnBossDied;
        _bossContext = null;
        _bossFSM = null;
    }
    private void Update()
    {

        _bossFSM?.Update(_bossContext);
    }
    private void FixedUpdate()
    {
        _bossFSM?.FixedUpdate(_bossContext);
    }
    private void _OnBossDied()
    {
        _bossCombat.enabled = false;
        _health.enabled = false;
        BattleManager.Instance.OnBossDied(true);
    }
}
