using System;
using UnityEngine;

public class BossTransition : ITransition<BossContext>
{
    public string Reason { get; }
    private readonly Func<BossContext, bool> _pred;//Î¯ÍÐ£¬action ºÍ Func delegate

    public BossTransition(Func<BossContext,bool> pred, string reason)
    {
        _pred = pred;
        Reason = reason;
    }
    public bool CanTransit(BossContext context)
    {
        return _pred(context);
    }
}
