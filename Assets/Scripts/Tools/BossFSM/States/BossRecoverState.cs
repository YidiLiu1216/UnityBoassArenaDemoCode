using UnityEngine;

public class BossRecoverState:IState<BossContext>
{
    public string Name => "Recover";
    private float _nextStateTime;
    public void OnEnter(BossContext context)
    {
        Debug.Log("BossInRecoverState");
        context.bossCombat.HideSlamWarning();
        _nextStateTime = Time.time + context.config.RecoverTime;
    }
    public void OnUpdate(BossContext context)
    {
        if (Time.time >= _nextStateTime)
        {
            context.IsRecovering = false;
            return;
        }
    }
    public void OnFixedUpdate(BossContext context) { }
    public void OnExit(BossContext context)
    {

    }

}
