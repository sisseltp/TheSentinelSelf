using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentinelsManager : AgentsManager
{
    private void Update()
    {  
        for (int i = 0; i < parameters.maxAmountAgents; i++)
        {
            if (agents[i] == null)
                continue;

            if (agents[i].kuramoto.dead || agents[i].kuramoto.age> parameters.MaxAge) 
            {
                ResetAgentAtIndex(i);

                GPUStruct[i].SetFromKuramoto(agents[i].kuramoto);
                GPUStruct[i].pos = agents[i].rigidBody.position;
            }
            else
            {
                agents[i].kuramoto.age += Time.deltaTime;
                agents[i].kuramoto.phase += GPUOutput[i].phaseAdition * Time.deltaTime;

                if (agents[i].kuramoto.phase > 1) 
                    agents[i].kuramoto.phase--; 
                GPUStruct[i].phase = agents[i].kuramoto.phase;

                agents[i].rigidBody.AddForceAtPosition(GPUOutput[i].vel * parameters.speedScl * Time.deltaTime * agents[i].kuramoto.phase, agents[i].transform.position + agents[i].transform.up);

                GPUStruct[i].speed = agents[i].kuramoto.speed;
                GPUStruct[i].pos = agents[i].rigidBody.position;
            }

            if (agents[i].renderer.isVisible)
                agents[i].renderer.material.SetFloat("Phase", agents[i].kuramoto.phase);
        }
    }

    public void SetRange(float range)
    {
        for (int i = 0; i < parameters.maxAmountAgents; i++)
            agents[i].kuramoto.couplingRange = range;
    }

    public void SetCoupling(float range)
    {
        for (int i = 0; i < parameters.maxAmountAgents; i++)
            agents[i].kuramoto.coupling = range;
    }

    public void SetNoise(float range)
    {
        for (int i = 0; i < parameters.maxAmountAgents; i++)
            agents[i].kuramoto.noiseScl = range;

        JsonUtility.ToJson(GenVelLib);
    }
}
