using UnityEngine;
using UnityEngine.AI;

public class BossContext
{
    //Game Setting
    public bool IsBossActivate=false;
    //FSM logic
    public PlayerHealth TargetPlayer;
    public bool IsReselectTarget;
    public bool IsGoingToPunch;
    public bool ISGoingToSlam;
    public bool IsRecovering;
    public bool IsDied=>health.IsDead;
    //Status
    public float NextSlamTime;
    public float NextReselectTime;
    public float NextPunchTime;
    //Reference
    public BosscConfig config;
    public BossCombat bossCombat;
    public BossHealth health;
    public Animator animator;
    public NavMeshAgent agent;
}
