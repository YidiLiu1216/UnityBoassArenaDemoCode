using UnityEngine;

public class BossIdleState : IState<BossContext>
{
    public string Name => "Idle";
    public void OnEnter(BossContext context)
    {
        Debug.Log("BossInIdleState");
        context.TargetPlayer=null;
        context.IsReselectTarget=false;
        context.IsGoingToPunch=false;
        context.ISGoingToSlam=false;
        context.IsRecovering=false;
       
}
    public void OnUpdate(BossContext context)
    {
    }
    public void OnFixedUpdate(BossContext context) { }
    public void OnExit(BossContext context) {
        context.bossCombat.enabled = true;
        context.health.enabled = true;
        context.health.InitBossHealth();
        //init agent
        context.agent.ResetPath();
        context.agent.speed = context.config.ChasingSpeed;
        context.agent.angularSpeed = context.config.RotationSpeed;
        float nextslamtime = Random.Range(context.config.MinSlamCDTime, context.config.MaxSlamCDTime)+Time.time;
        context.NextSlamTime = nextslamtime;
    }

}
