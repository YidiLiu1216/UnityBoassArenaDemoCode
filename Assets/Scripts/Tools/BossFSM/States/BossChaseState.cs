using UnityEngine;

public class BossChaseState:IState<BossContext>
{
    public string Name => "Chase";
    public void OnEnter(BossContext context)
    {
        Debug.Log("BossInChasingMode");
        //Init ReselectTime
        context.NextReselectTime = Time.time + context.config.ReselectTime;
        context.animator.SetBool("IsWalking", true);
    }
    public void OnUpdate(BossContext context)
    {
        //First,Check if need to Slam
        if(Time.time>= context.NextSlamTime)
        {
            context.ISGoingToSlam = true;
            return;
        }
        //Seceond.Check if need to Punch
        Vector3 toTarget = context.bossCombat.GetVectorToTarget(context.TargetPlayer.transform);
        if (toTarget.magnitude<context.config.PunchRange&&Time.time>=context.NextPunchTime) { 
            context.IsGoingToPunch = true;
            return;
        }
        //Third.Check if need to ReSelect
        if (context.TargetPlayer.IsDeath|| Time.time >= context.NextReselectTime)
        {
            context.IsReselectTarget = true;
            return;
        }
        //Finally DoUpdatenormaly
        /*
        Vector3 direction = toTarget.normalized;
        context.bossCombat.transform.position +=direction *context.config.ChasingSpeed *Time.deltaTime;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        context.bossCombat.transform.rotation =Quaternion.Slerp(context.bossCombat.transform.rotation,targetRotation,context.config.RotationSpeed * Time.deltaTime);*/
        context.agent.SetDestination(context.TargetPlayer.transform.position);

    }
    public void OnFixedUpdate(BossContext context) { }
    public void OnExit(BossContext context)
    {
        context.ISGoingToSlam= false;
        context.IsGoingToPunch = false;
        context.IsReselectTarget = false;
       
    }

}
