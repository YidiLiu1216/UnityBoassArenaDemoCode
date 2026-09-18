using UnityEngine;

public class BossPunchState: IState<BossContext>
{
    public string Name => "Punch";
    private float _nextStateTime;
    public void OnEnter(BossContext context)
    {
        Debug.Log("BossInPunchState");
        context.animator.SetBool("IsWalking", false);
        context.agent.ResetPath();
        context.NextPunchTime = Time.time + context.config.PunchCDTime;
        _nextStateTime = Time.time + context.config.PunchAnimationTime;
        context.bossCombat.StartPunch(context.config.PunchActionPrepareTime,context.TargetPlayer,context.config.PunchRange,context.config.PunchAngle);
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
        
    }

}
