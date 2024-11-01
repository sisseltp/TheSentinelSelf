using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasticsManager : AgentsManager
{
    public override void OnAgentDead(int i)
    {
        toRemove.Add(i);
    }

    public override void OnAgentAged(int i)
    {
        OnAgentDead(i);
    }
}