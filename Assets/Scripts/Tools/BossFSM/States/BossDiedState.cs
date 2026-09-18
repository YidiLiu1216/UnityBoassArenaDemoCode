using UnityEngine;

public class BossDiedState: IState<BossContext>
{
    public string Name => "Died";
    public void OnEnter(BossContext context)
    {
        context.bossCombat.enabled = false;
        context.health.enabled = false;
        context.IsBossActivate = false;
    }
    public void OnUpdate(BossContext context)
    {
    }
    public void OnFixedUpdate(BossContext context) { }
    public void OnExit(BossContext context)
    {

    }
}
