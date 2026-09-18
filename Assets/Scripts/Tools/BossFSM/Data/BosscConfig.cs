using UnityEngine;

[CreateAssetMenu(fileName = "BosscConfig", menuName = "Scriptable Objects/BosscConfig")]
public class BosscConfig : ScriptableObject
{
    [Header("Slam")]
    public float MaxSlamCDTime;
    public float MinSlamCDTime;
    public float SlamPrepareTime;
    public float SlamActionPrepareTime;
    public float SlamAnimationTime;

    [Header("Punch")]
    public float PunchRange;
    public float PunchAngle;
    public float PunchActionPrepareTime;
    public float PunchAnimationTime;
    public float PunchCDTime;

    [Header("Reselct")]
    public float ReselectTime;

    [Header("Chasing")]
    public float ChasingSpeed;
    public float RotationSpeed;

    [Header("Recover")]
    public float RecoverTime;
}
