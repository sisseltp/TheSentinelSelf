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
        GPUStruct[i].pos = agents[i].rigidBody.position;
    }

    public override void OnAgentAged(int i)
    {
        OnAgentDead(i);
    }
}
