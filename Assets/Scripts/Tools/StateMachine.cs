using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T>
{
    //public T Context { get; }
    public IState<T> CurrentState { get; private set; }
    public IState<T> PreviousState { get; private set; }

    //普通状态转移表
    private readonly Dictionary<IState<T>, List<(ITransition<T> cond, IState<T> to)>> _graph = new();
    //全局状态转移表 Any 表示无论当前状态是什么，只要满足条件就可以转移
    private readonly List<(ITransition<T> cond, IState<T> to)> _any = new();

    public event Action<IState<T>> OnStateChanged;



    public void ChangeState(IState<T> newState, T context)
    {
        if (newState == null) return;

        CurrentState?.OnExit(context);

        PreviousState = CurrentState;
        CurrentState = newState;

        CurrentState?.OnEnter(context);
        OnStateChanged?.Invoke(CurrentState);
    }
    public void SetInitState(IState<T> initState, T context)
    {
        CurrentState = initState;
        CurrentState?.OnEnter(context);
    }
    public void AddTransition(IState<T> from, IState<T> to, ITransition<T> condition)
    {
        if (!_graph.ContainsKey(from))
        {
            _graph[from] = new List<(ITransition<T>, IState<T>)>();
        }
        _graph[from].Add((condition, to));
    }
    public void AddAnyTransition(ITransition<T> condition, IState<T> to)
    {
        _any.Add((condition, to));
    }

    public void Update(T context)
    {
        if (_TryGetTransition(context, out var nextState, out var reason))
        {
            ChangeState(nextState, context);
            Debug.Log($"State Changed Because {reason}");
        }
        CurrentState?.OnUpdate(context);
    }

    public void FixedUpdate(T context)
    {
        CurrentState?.OnFixedUpdate(context);
    }
    private bool _TryGetTransition(T context, out IState<T> nextState, out string reason)
    {
        // Check for transitions from the current state
        if (CurrentState != null && _graph.TryGetValue(CurrentState, out var transitions))
        {
            foreach (var (condition, to) in transitions)
            {
                if (condition.CanTransit(context))
                {
                    nextState = to;
                    reason = condition.Reason;
                    return true;
                }
            }
        }
        // Check for any transitions
        foreach (var (condition, to) in _any)
        {
            if (condition.CanTransit(context))
            {
                nextState = to;
                reason = condition.Reason;
                return true;
            }
        }
        nextState = null;
        reason = null;
        return false;
    }
}
