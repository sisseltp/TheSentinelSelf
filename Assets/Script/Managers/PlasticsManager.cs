using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasticsManager : AgentsManager
{
    private void Update()
    {
        List<int> toRemove = new List<int>();

        for (int i = 0; i < parameters.maxAmountAgents; i++)
        {
            if(agents[i] == null)
                continue;
            
            if(agents[i].kuramoto.dead || agents[i].kuramoto.age > parameters.MaxAge)
            {
                toRemove.Add(i);
            }
            else
            {
                agents[i].kuramoto.age += Time.deltaTime;

                agents[i].kuramoto.phase += GPUOutput[i].phaseAdition * Time.deltaTime ;
                if (agents[i].kuramoto.phase > 1)
                    agents[i].kuramoto.phase--;

                GPUStruct[i].phase = agents[i].kuramoto.phase;
                agents[i].rigidBody.AddForceAtPosition(GPUOutput[i].vel * parameters.speedScl * Time.deltaTime * agents[i].kuramoto.phase, agents[i].transform.position + agents[i].transform.up);
                GPUStruct[i].pos = agents[i].rigidBody.position;
            }

            if (agents[i].renderer.isVisible)
                agents[i].renderer.material.SetFloat("Phase", agents[i].kuramoto.phase);
        }

        RemoveAgentsAtIndexes(toRemove);
    }
}