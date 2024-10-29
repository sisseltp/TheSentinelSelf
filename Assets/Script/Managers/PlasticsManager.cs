using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlasticsManager : AgentsManager
{
    [Tooltip("The object to emit from")]
    [SerializeField]
    private Transform emitionOrigin;

    public int MaxPlastics = 15;
    public float emitionSpeed = 10;
    private float timeGate = 0;

    void Start()
    {
        agents = new Plastic[MaxPlastics];
        GPUStruct = new GPUCompute.GPUData[MaxPlastics];
        GPUOutput = new GPUCompute.GPUOutput[MaxPlastics];

        for (int i=0; i< parameters.amongAgentsAtStart; i++)
        {
            Vector3 pos = emitionOrigin.position + UnityEngine.Random.insideUnitSphere* parameters.spawnArea;

            Plastic thisSentinel = Instantiate(prefabsAgents[UnityEngine.Random.Range(0,prefabsAgents.Length)], pos, Quaternion.identity, this.transform).GetComponent<Plastic>();

            thisSentinel.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them

            GPUStruct[i].SetFromKuramoto(thisSentinel.kuramoto);
            GPUStruct[i].pos = thisSentinel.transform.position;
            GPUOutput[i].Setup();

            agents[i] = thisSentinel;
        }

        realAmountAgents = parameters.amongAgentsAtStart;
    }

    private void Update()
    {
        List<int> toRemove = new List<int>();

        if (emitionSpeed + timeGate < Time.time)
        {
            timeGate = Time.time;
            AddCell();
        }

        for (int i = 0; i < realAmountAgents; i++)
        {
            if(agents[i] == null)
                continue;
            
            if(agents[i].kuramoto.age > parameters.MaxAge || agents[i].kuramoto.dead)
            {
                toRemove.Add(i);
            }
            else
            {
                agents[i].kuramoto.age += Time.deltaTime;
                agents[i].kuramoto.phase += GPUOutput[i].phaseAdition * Time.deltaTime ;
                if (agents[i].kuramoto.phase > 1) { agents[i].kuramoto.phase = agents[i].kuramoto.phase - 1; }
                GPUStruct[i].phase = agents[i].kuramoto.phase;
                agents[i].rigidBody.AddForceAtPosition(GPUOutput[i].vel * parameters.speedScl * Time.deltaTime * agents[i].kuramoto.phase, agents[i].transform.position + agents[i].transform.up);
                GPUStruct[i].pos = agents[i].rigidBody.position;
            }

            if (agents[i].renderer.isVisible)
                agents[i].renderer.material.SetFloat("Phase", agents[i].kuramoto.phase);
        }

        int nextIndex = 0;
        for (int i = 0; i < parameters.amongAgentsAtStart; i++)
        {
            if (toRemove.Contains(i))
            {
                if (agents[i] != null)
                {
                    Destroy(agents[i].gameObject);
                    agents[i] = null;
                }
                continue;
            }

            agents[nextIndex] = agents[i];
            GPUStruct[nextIndex] = GPUStruct[i];

            if (nextIndex != i)
            {
                agents[i] = null;
                GPUStruct[i] = new GPUCompute.GPUData();
            }

            nextIndex++;
        }

        realAmountAgents -= toRemove.Count;

        /*int nxtIndx = -1;
        
        for ( int i=0; i<toRemove.Count; i++)
        {
            int indx = toRemove[i];
            Destroy(plastics[indx].gameObject);

            if (i != toRemove.Count-1)
                 nxtIndx = toRemove[i+1];
            else
                nxtIndx = RealAmountPlastics;

            for (int p = indx+1; p <= nxtIndx ; p++)
            {
                GPUStruct[p - (i+1)] = GPUStruct[p];
                plastics[p - (i+1)] = plastics[p];
            }            
        }

        RealAmountPlastics -= toRemove.Count;
        
        if (nxtIndx != -1) 
        {
            GPUStruct[nxtIndx] = new GPUCompute.GPUData();
            plastics[nxtIndx] = null;
        }*/
    }
   
    public void ResetPlastic(int i)
    {
        Agent thisPlastic = agents[i];

        Vector3 pos = emitionOrigin.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;

        thisPlastic.transform.position = pos;

        thisPlastic.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them
        thisPlastic.geneticMovement.Reset();
    }

    public void AddCell()
    {
        if (realAmountAgents < MaxPlastics-1)
        {
            realAmountAgents++;

            Plastic thisAgent =  Instantiate(prefabsAgents[UnityEngine.Random.Range(0,prefabsAgents.Length)], transform).GetComponent<Plastic>();

            agents[realAmountAgents-1] = thisAgent;

            ResetPlastic(realAmountAgents - 1);

            GPUCompute.GPUData gpuStruct = new GPUCompute.GPUData();
            gpuStruct.SetFromKuramoto(thisAgent.kuramoto);
            gpuStruct.pos = thisAgent.transform.position;
            GPUStruct[realAmountAgents-1] = gpuStruct;
        }
    }
}