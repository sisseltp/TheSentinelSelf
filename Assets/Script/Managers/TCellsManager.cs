using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TCellsManager : AgentsManager
{
    private void Update()
    {
        for (int i = 0; i < parameters.maxAmountAgents; i++)
        {
            if (agents[i] == null)
                continue;
            
            if (agents[i].kuramoto.dead || agents[i].kuramoto.age > parameters.MaxAge) 
            {
                ResetAgentAtIndex(i);
            }
            else
            {
                agents[i].kuramoto.age += Time.deltaTime*(agents[i].kuramoto.played == 3?10f:1f);

                agents[i].kuramoto.phase += GPUOutput[i].phaseAdition * Time.deltaTime;
                if (agents[i].kuramoto.phase > 1)
                    agents[i].kuramoto.phase--;

                GPUStruct[i].played = agents[i].kuramoto.played;
                GPUStruct[i].phase = agents[i].kuramoto.phase;
                agents[i].rigidBody.AddForceAtPosition(GPUOutput[i].vel * parameters.speedScl * Time.deltaTime * agents[i].kuramoto.phase, agents[i].transform.position + agents[i].transform.up);
                GPUStruct[i].pos = agents[i].transform.position;
            }

            if (agents[i].renderer.isVisible)
                agents[i].renderer.material.SetFloat("Phase", agents[i].kuramoto.phase);
        }
    }
}
