using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentsManager : MonoBehaviour
{
    public AgentsManagerParameters parameters;
    public GameObject[] prefabsAgents;
    [HideInInspector]
    public Agent[] agents;
    [HideInInspector]
    public int realAmountAgents = 0;

    [HideInInspector]
    public GPUCompute.GPUData[] GPUStruct; // list of agent struct, that will hold the data for gpu compute
    [HideInInspector]
    public GPUCompute.GPUOutput[] GPUOutput;
}
