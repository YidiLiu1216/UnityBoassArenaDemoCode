using UnityEngine;

public class BossSelectTargetState:IState<BossContext>
{
    public string Name => "SelectTarget";

    public void OnEnter(BossContext context)
    {
        Debug.Log("BossInSlelectTargetState");
        context.TargetPlayer = context.bossCombat.SelectTarget();
        
    }
    public void OnUpdate(BossContext context)
    {
       
    }
    public void OnFixedUpdate(BossContext context) { }
    public void OnExit(BossContext context)
    {
       
    }

}
