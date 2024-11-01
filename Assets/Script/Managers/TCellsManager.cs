using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TCellsManager : AgentsManager
{
    public override void OnAgentDead(int i)
    {
        ResetAgentAtIndex(i);
    }

    public override void OnAgentAged(int i)
    {
        OnAgentDead(i);
    }
}
