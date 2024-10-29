using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentinelsManager : AgentsManager
{

    private List<Genetics.GenVel> GenVelLib; // list of the GenVel data to act as the library
    private List<Genetics.GenKurmto> GenKurLib;// list of the GenKurmto data to act as the library

    [HideInInspector]
    public PathogensManager[] pathogensManagers;
    [HideInInspector]
    public TCellsManager[] tcellsManagers;

    void Start()
    {
        agents = new Sentinel[parameters.amongAgentsAtStart];
        GPUStruct = new GPUCompute.GPUData[parameters.amongAgentsAtStart];
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();
        GPUOutput = new GPUCompute.GPUOutput[parameters.amongAgentsAtStart];

        for (int i=0; i< parameters.amongAgentsAtStart; i++)
        {

            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;

            Sentinel newSentinel = Instantiate(prefabsAgents[UnityEngine.Random.Range(0,prefabsAgents.Length)], pos, Quaternion.identity, this.transform).GetComponent<Sentinel>();
            
            newSentinel.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange,  0.2f);// setup its setting to randomize them

            GPUStruct[i].SetFromKuramoto(newSentinel.kuramoto);
            GPUStruct[i].pos = newSentinel.transform.position;
            GPUOutput[i].Setup();

            agents[i] = newSentinel;
        }

        GameObject[] lymphs = GameObject.FindGameObjectsWithTag("Lymphonde");

        tcellsManagers = new TCellsManager[lymphs.Length];
        for (int i = 0; i < lymphs.Length; i++)
        {
            tcellsManagers[i] = lymphs[i].GetComponent<TCellsManager>();

        }

        GameObject[] pathogens = GameObject.FindGameObjectsWithTag("PathogenEmitter");
        pathogensManagers = new PathogensManager[pathogens.Length];

        for (int i = 0; i < pathogens.Length; i++)
        {
            pathogensManagers[i] = pathogens[i].GetComponent<PathogensManager>();
        }
    }

    private void Update()
    {  
        for (int i = 0; i < parameters.amongAgentsAtStart; i++)
        {
            if (agents[i].kuramoto.dead || agents[i].kuramoto.age> parameters.MaxAge) 
            {
                ResetSentinel(i);

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

    public void ResetSentinel(int i, bool genOn= false)
    {
        Agent thisSentinel = agents[i];

        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;
        thisSentinel.transform.position = pos;

        (thisSentinel as Sentinel).fosilising.enabled = false;
        thisSentinel.gameObject.SetActive(true);

        if (!genOn)
        {
            // reset bothe genetic values to random
            thisSentinel.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);
            thisSentinel.geneticMovement.Reset();
        }
        else if (GenKurLib.Count < 500)
        {
            // add it settings to the librarys
            Genetics.GenKurmto genKurm = new Genetics.GenKurmto(thisSentinel.kuramoto.speedBPM, thisSentinel.kuramoto.noiseScl, thisSentinel.kuramoto.coupling, thisSentinel.kuramoto.couplingRange, thisSentinel.kuramoto.attractionSclr, thisSentinel.kuramoto.fitness);
            GenKurLib.Add(genKurm);
            Genetics.GenVel vels = new Genetics.GenVel(agents[i].geneticMovement.geneticMovement, thisSentinel.kuramoto.fitness);
            GenVelLib.Add(vels);

            // reset bothe genetic values to random
            thisSentinel.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);
            thisSentinel.geneticMovement.Reset();
        }
        else
        {
            // add random sentinel from lib
            int rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData1 = GenKurLib[rand];
            rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData2 = GenKurLib[rand];

            float[] Settings = kurData1.BlendAttributes(kurData2.Settings);

            thisSentinel.kuramoto.SetupData(Settings);
           

            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel1 = GenVelLib[rand];
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel2 = GenVelLib[rand];

            Vector3[] Vels = genVel2.BlendAttributes(genVel1.Vels);

            thisSentinel.geneticMovement.Reset();
            thisSentinel.geneticMovement.geneticMovement = Vels;
        }
        
    }

    // all bellow is for ui to change all sentinels values 
    public void setRange(float range)
    {
        for (int i = 0; i < parameters.amongAgentsAtStart; i++)
            agents[i].kuramoto.couplingRange = range;
    }

    public void setCoupling(float range)
    {
        for (int i = 0; i < parameters.amongAgentsAtStart; i++)
            agents[i].kuramoto.coupling = range;
    }

    public void setNoise(float range)
    {
        for (int i = 0; i < parameters.amongAgentsAtStart; i++)
            agents[i].kuramoto.noiseScl = range;

        JsonUtility.ToJson(GenVelLib);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 1f);
    }
#endif
}
