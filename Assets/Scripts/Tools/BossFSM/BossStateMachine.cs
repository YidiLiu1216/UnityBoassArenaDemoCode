using UnityEngine;
using UnityEngine.AI;

public class BossStateMachine:StateMachine<BossContext>
{
    public void InitStateMachine(BossContext context,BossCombat combat,BossHealth health,BosscConfig config,Animator animator,NavMeshAgent agent)
    {
        context.bossCombat = combat;
        context.health = health;
        context.config = config;
        context.animator = animator;
        context.agent = agent;
    }
}
