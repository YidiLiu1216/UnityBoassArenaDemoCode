using UnityEngine;

// 有限状态机状态接口。T 为状态机的拥有者（如玩家、敌人等），方便状态访问外部数据。
public interface IState<T>
{
    string Name { get; }
    void OnEnter(T context);

    void OnUpdate(T context);

    void OnFixedUpdate(T context);
    void OnExit(T context);
}
