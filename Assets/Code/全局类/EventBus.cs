using System;
using System.Collections.Generic;
using UnityEngine;


public static class EventBus_Unit
{
    public static event System.Action<BaseUnit> OnUnitActivated;
    public static event System.Action<BaseUnit> OnUnitDeactivated;

    public static void InvokeActivated(BaseUnit unit)
        => OnUnitActivated?.Invoke(unit);

    public static void InvokeDeactivated(BaseUnit unit)
        => OnUnitDeactivated?.Invoke(unit);
}