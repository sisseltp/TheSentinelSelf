using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentinelsManager : AgentsManager
{
    public override void OnAgentDead(int i)
    {
        ResetAgentAtIndex(i);
        GPUStruct[i].SetFromKuramoto(agents[i].kuramoto);
    }

    public override void OnAgentAged(int i)
    {
        OnAgentDead(i);
    }
}
