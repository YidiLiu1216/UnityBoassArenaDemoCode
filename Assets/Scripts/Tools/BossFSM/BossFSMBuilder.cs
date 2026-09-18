using UnityEngine;

public class BossFSMBuilder
{

    public static BossStateMachine BuildFSM(BossContext context)
    {
        BossStateMachine fsm = new BossStateMachine();
        //Set all state
        var bossIdleState = new BossIdleState();
        var bossSelectState = new BossSelectTargetState();
        var bossChaseState = new BossChaseState();
        var bossPunchState = new BossPunchState();
        var bossSlamState = new BossSlamState();
        var bossRecoverState = new BossRecoverState();
        var bossDiedState = new BossDiedState();
        //Set all Transitions
        fsm.AddTransition(bossIdleState, bossSelectState, 
            new BossTransition(
                context=>context.IsBossActivate,
                "StartAct"
                ));
        fsm.AddTransition(bossSelectState, bossIdleState,
            new BossTransition(
                context=>!context.IsBossActivate,
                "EndAct"));
        fsm.AddTransition(bossSelectState, bossChaseState,
            new BossTransition(
                context=>context.TargetPlayer!=null,
                "TargetSelect"));
        fsm.AddTransition(bossChaseState, bossSelectState,
            new BossTransition(
                context => context.IsReselectTarget,
                "ChangingTarget"));
        fsm.AddTransition(bossChaseState, bossPunchState,
            new BossTransition(
                context=>context.IsGoingToPunch,
                "Punch"));
        fsm.AddTransition(bossChaseState, bossSlamState,
            new BossTransition(
                context=>context.ISGoingToSlam,
                "Slam"));
        fsm.AddTransition(bossPunchState, bossRecoverState,
            new BossTransition(
                context=>context.IsRecovering,
                "AttackRecover"));
        fsm.AddTransition(bossSlamState, bossRecoverState,
            new BossTransition(
                context=>context.IsRecovering,
                "AttackRecover"));
        fsm.AddTransition(bossRecoverState, bossSelectState,
            new BossTransition(
                context => !context.IsRecovering,
                "RecoverEnd"));
        fsm.AddAnyTransition(
            new BossTransition(
                context => context.IsDied,
                "BossDied"),
            bossDiedState);
        //Start Init state
        fsm.SetInitState(bossIdleState, context);
        return fsm;
    }

}
