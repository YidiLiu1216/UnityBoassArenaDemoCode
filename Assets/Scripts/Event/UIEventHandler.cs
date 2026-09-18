using System;
using UnityEngine;

public static class UIEventHandler
{
    public static event Action OnSessionCreateButtonClikced;
    public static void TriggerSessionCreateButton()
    {
        OnSessionCreateButtonClikced?.Invoke();
    }
}
