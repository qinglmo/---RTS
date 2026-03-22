using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class WorkUnit : BaseUnit
{
    public IResource currentTarget;

    private void CancelMove()
    {
        movement.CancelMove();
    }
}
