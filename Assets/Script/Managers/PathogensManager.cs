using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathogensManager : AgentsManager
{
    private void Update()
    {
        List<int> toRemove = new List<int>();

        for (int i = 0; i < parameters.maxAmountAgents; i++)
        {
            if (agents[i] == null)
                continue;

            if (agents[i].kuramoto.dead)
            {
                toRemove.Add(i);
            }
            else if (agents[i].kuramoto.age > parameters.MaxAge)
            {
                agents[i].kuramoto.age = 0;
                DuplicatePathogen(agents[i] as Pathogen, 1);
            }
            else
            {
                agents[i].kuramoto.age += Time.deltaTime;

                agents[i].kuramoto.phase += GPUOutput[i].phaseAdition * Time.deltaTime;
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

    private void DuplicatePathogen(Pathogen pathogen, int duplications=2)
    {
        if (realAmountAgents<parameters.maxAmountAgents -2)
        {
            for (int l = 0; l < duplications; l++)
            {
                Pathogen thisPathogen = Instantiate(pathogen, pathogen.transform.position, pathogen.transform.rotation, this.transform).GetComponent<Pathogen>();

                thisPathogen.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them

                agents[realAmountAgents] = thisPathogen;

                GPUStruct[realAmountAgents].SetFromKuramoto(thisPathogen.kuramoto);
                GPUStruct[realAmountAgents].pos = agents[realAmountAgents].transform.position;

                realAmountAgents++;

                thisPathogen.geneticAntigenKey.antigen = new Genetics.Antigen();
                thisPathogen.geneticAntigenKey.antigen.Key = pathogen.GetComponentInChildren<GeneticAntigenKey>().antigen.Key;
            }
        }
    }

    
}
