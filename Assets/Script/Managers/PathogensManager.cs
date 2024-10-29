using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathogensManager : AgentsManager
{
    private List<Genetics.GenVel> GenVelLib; // lib to hold the gene move data
    private List<Genetics.GenKurmto> GenKurLib; // lib to hold gene kurmto data

    [SerializeField]
    private float emitionTimer = 1.0f;
    private float timeGate = 0;

    void Start()
    {
        agents = new Pathogen[parameters.amongAgentsAtStart];

        GPUStruct = new GPUCompute.GPUData[parameters.amongAgentsAtStart];
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();
        GPUOutput = new GPUCompute.GPUOutput[parameters.amongAgentsAtStart];

        for (int i=0; i< parameters.amongAgentsAtStart; i++)
            AddPathogen(i);
    }

    public void AddPathogen(int i)
    {
        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;

        Pathogen thisPathogen = Instantiate(prefabsAgents[UnityEngine.Random.Range(0,prefabsAgents.Length)], pos, Quaternion.identity, this.transform).GetComponent<Pathogen>();

        thisPathogen.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them

        thisPathogen.geneticAntigenKey.Reset();

        agents[realAmountAgents] = thisPathogen;

        GPUStruct[realAmountAgents].SetFromKuramoto(thisPathogen.kuramoto);
        GPUStruct[realAmountAgents].pos = agents[i].transform.position;
        GPUOutput[realAmountAgents].Setup();

        realAmountAgents++;
    }

    private void DuplicatePathogen(Pathogen pathogen, int duplications=2)
    {
        if (realAmountAgents<parameters.amongAgentsAtStart -2)
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

    private void Update()
    {
        if(Time.time > emitionTimer + timeGate && realAmountAgents < parameters.amongAgentsAtStart)
        {
            timeGate = Time.time;
            AddPathogen(realAmountAgents);
        }

        List<int> toRemove = new List<int>();

        for (int i = 0; i < realAmountAgents; i++)
        {
            if (agents[i].kuramoto.dead) 
            {
                toRemove.Add(i);
            }
            else  if (agents[i].kuramoto.age > parameters.MaxAge )
            {
                agents[i].kuramoto.age = 0;
                DuplicatePathogen(agents[i] as Pathogen,1);
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

        int nextIndex = 0;
        for (int i=0;i<parameters.amongAgentsAtStart;i++)
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
    }

    public void ResetSentinel(int i, bool genOn = false)
    {
        Agent thisPathogen = agents[i];

        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;

        thisPathogen.transform.position = pos;

        if (!genOn)
        {
            thisPathogen.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);
            thisPathogen.geneticMovement.Reset();
        }
        else if (GenKurLib.Count < 500)
        {
            Genetics.GenKurmto genKurm = new Genetics.GenKurmto(thisPathogen.kuramoto.speedBPM, thisPathogen.kuramoto.noiseScl, thisPathogen.kuramoto.coupling, thisPathogen.kuramoto.couplingRange, thisPathogen.kuramoto.attractionSclr, thisPathogen.kuramoto.fitness);
            GenKurLib.Add(genKurm);
            Genetics.GenVel vels = new Genetics.GenVel(agents[i].geneticMovement.geneticMovement, thisPathogen.kuramoto.fitness);
            GenVelLib.Add(vels);

            thisPathogen.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them

            thisPathogen.geneticMovement.Reset();
        }
        else
        {
            int rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData1 = GenKurLib[rand];
            rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData2 = GenKurLib[rand];

            float[] Settings = kurData1.BlendAttributes(kurData2.Settings);

            thisPathogen.kuramoto.SetupData(Settings);

            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel1 = GenVelLib[rand];
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel2 = GenVelLib[rand];

            Vector3[] Vels = genVel2.BlendAttributes(genVel1.Vels);

            thisPathogen.geneticMovement.Reset();
            thisPathogen.geneticMovement.geneticMovement = genVel1.BlendAttributes(Vels);
        } 
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 1f);
    }
    #endif
}
