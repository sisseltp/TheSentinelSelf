using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TCellsManager : AgentsManager
{
    private int MaxTCells = 15;


    private List<Genetics.GenVel> GenVelLib; // lib to hold the gene move data
    private List<Genetics.GenKurmto> GenKurLib; // lib to hold gene kurmto data

    [SerializeField]
    private float emitionRate = 4;

    void Start()
    {
        MaxTCells = Mathf.FloorToInt(parameters.amongAgentsAtStart * 1.5f);
        agents = new TCell[MaxTCells];

        // create list to hold data structs
        GPUStruct = new GPUCompute.GPUData[MaxTCells];
        // create the two lib lists
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();


        GPUOutput = new GPUCompute.GPUOutput[MaxTCells];

        for (int i=0; i< parameters.amongAgentsAtStart; i++)
        {
            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere* parameters.spawnArea;

            TCell newTCell = Instantiate(prefabsAgents[UnityEngine.Random.Range(0, prefabsAgents.Length)], pos, Quaternion.identity, this.transform).GetComponent<TCell>();

            KuramotoAffectedAgent kuramoto = newTCell.kuramoto;
            kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them

            newTCell.geneticAntigenKey.Reset();

            GPUStruct[i].SetFromKuramoto(kuramoto);
            GPUStruct[i].pos = newTCell.transform.position;
            GPUOutput[i].Setup();

            agents[i] = newTCell;
        }

        realAmountAgents = parameters.amongAgentsAtStart;

        StartCoroutine(emition(emitionRate));
    }

    IEnumerator emition( float rate)
    {
        while (true)
        {
            if (CanAddCell())
            {
                Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;
                int randindx = UnityEngine.Random.Range(0, prefabsAgents.Length);
                TCell thisSentinel = Instantiate(prefabsAgents[randindx], pos, Quaternion.identity, this.transform).GetComponent<TCell>();

                thisSentinel.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them

                AddTCell(thisSentinel);
            }

            yield return new WaitForSeconds(rate);
        }
    }
    private void Update()
    {
        List<int> toRemove = new List<int>();

        for (int i = 0; i < realAmountAgents; i++)
        {
            if (agents[i] == null)
                continue;
            
            KuramotoAffectedAgent kuramoto = agents[i].kuramoto;
            
            if (kuramoto.dead || kuramoto.age > parameters.MaxAge) 
            {
                if (i > parameters.amongAgentsAtStart)
                {
                    toRemove.Add(i);
                    continue;
                }

                ResetTCell(i);
            }
            else
            {
                if (kuramoto.played == 3)
                    kuramoto.age += Time.deltaTime*10;
                else
                    kuramoto.age += Time.deltaTime;

                kuramoto.phase += GPUOutput[i].phaseAdition * Time.deltaTime;
                if (kuramoto.phase > 1) { kuramoto.phase = kuramoto.phase-1; }
                GPUStruct[i].played = kuramoto.played;
                GPUStruct[i].phase = kuramoto.phase;

                agents[i].rigidBody.AddForceAtPosition(GPUOutput[i].vel * parameters.speedScl * Time.deltaTime * kuramoto.phase, agents[i].transform.position + agents[i].transform.up);

                GPUStruct[i].pos = agents[i].transform.position;

            }

            if (agents[i].renderer.isVisible)
                agents[i].renderer.material.SetFloat("Phase", kuramoto.phase);
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
    }
   
    public void ResetTCell(int i ,bool genOn = false)
    {
        Agent thisTCell = agents[i];

        thisTCell.renderer.material.SetFloat("KeyTrigger", 0);

        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;

        thisTCell.transform.position = pos;

        if (!genOn)
        {
            thisTCell.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);
            thisTCell.geneticMovement.Reset();
            (thisTCell as TCell).geneticAntigenKey.Reset();
        }
        else if (GenKurLib.Count < 500)
        {
            KuramotoAffectedAgent kuramoto = thisTCell.kuramoto;

            Genetics.GenKurmto genKurm = new Genetics.GenKurmto(kuramoto.speedBPM, kuramoto.noiseScl, kuramoto.coupling, kuramoto.couplingRange, kuramoto.attractionSclr, kuramoto.fitness);
            GenKurLib.Add(genKurm);
            Genetics.GenVel vels = new Genetics.GenVel(agents[i].geneticMovement.geneticMovement, kuramoto.fitness);
            GenVelLib.Add(vels);
            // add random new sentinel
            kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them
            
            thisTCell.geneticMovement.Reset();
            (thisTCell as TCell).geneticAntigenKey.Reset();
        }
        else
        {
            // add random sentinel from lib
            int rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData1 = GenKurLib[rand];
            rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData2 = GenKurLib[rand];

            float[] Settings = kurData1.BlendAttributes(kurData2.Settings);

            KuramotoAffectedAgent kuramoto = thisTCell.kuramoto;
            kuramoto.SetupData(Settings);

            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel1 = GenVelLib[rand];
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel2 = GenVelLib[rand];

            Vector3[] Vels = genVel2.BlendAttributes(genVel1.Vels);

            thisTCell.geneticMovement.Reset();
            thisTCell.geneticMovement.geneticMovement = genVel1.BlendAttributes(Vels);
            (thisTCell as TCell).geneticAntigenKey.Reset();
        }
    }

    public bool CanAddCell() => realAmountAgents<MaxTCells - 1;

    public void AddTCell(TCell tCell)
    {
        realAmountAgents++;
        agents[realAmountAgents-1] = tCell;
        KuramotoAffectedAgent kuramoto = tCell.kuramoto;
        tCell.geneticAntigenKey.Reset();

        GPUCompute.GPUData gpuStruct = new GPUCompute.GPUData();
        gpuStruct.SetFromKuramoto(kuramoto);
        gpuStruct.pos = tCell.transform.position;
        GPUStruct[realAmountAgents] = gpuStruct;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 1f);
    }
#endif
}
