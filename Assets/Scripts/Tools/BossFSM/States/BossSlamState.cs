using UnityEngine;

public class BossSlamState:IState<BossContext>
{
    public string Name => "Slam";
    private float _nextStateTime;
    public void OnEnter(BossContext context)
    {
        Debug.Log("BossInSlamState");
        context.animator.SetBool("IsWalking", false);
        context.agent.ResetPath();
        _nextStateTime = Time.time + context.config.SlamAnimationTime;
        context.bossCombat.StartGroundSlam(context.config.SlamPrepareTime, context.config.SlamActionPrepareTime);
    }
    public void OnUpdate(BossContext context)
    {

        if (Time.time >= _nextStateTime)
        {
            context.IsRecovering = true;
            return;
        }
    }
    public void OnFixedUpdate(BossContext context) { }
    public void OnExit(BossContext context)
    {
        float nextslamtime = Random.Range(context.config.MinSlamCDTime, context.config.MaxSlamCDTime)+Time.time;
        context.NextSlamTime = nextslamtime;
    }

}
