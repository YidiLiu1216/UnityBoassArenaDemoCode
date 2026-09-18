using UnityEngine;

public interface ITransition<T>
{
    bool CanTransit(T context);
    string Reason { get; }
}
