using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathogensManager : AgentsManager
{
    private void DuplicatePathogen(Pathogen pathogen, int duplications=2)
    {
        if (realAmountAgents<parameters.maxAmountAgents - duplications)
        {
            for (int l = 0; l < duplications; l++)
            {
                Pathogen thisPathogen = Instantiate(pathogen, pathogen.transform.position, pathogen.transform.rotation, this.transform).GetComponent<Pathogen>();

                thisPathogen.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange);// setup its setting to randomize them

                agents[realAmountAgents] = thisPathogen;

                GPUStruct[realAmountAgents].SetFromKuramoto(thisPathogen.kuramoto);

                realAmountAgents++;

                thisPathogen.geneticAntigenKey.antigen = new Genetics.Antigen();
                thisPathogen.geneticAntigenKey.antigen.Key = pathogen.GetComponentInChildren<GeneticAntigenKey>().antigen.Key;
            }
        }
    }

    public override void OnAgentDead(int i)
    {
        toRemove.Add(i);
    }

    public override void OnAgentAged(int i)
    {
        agents[i].kuramoto.age = 0f;
        DuplicatePathogen(agents[i] as Pathogen, 1);
    }
}
