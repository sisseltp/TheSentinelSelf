using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentinelManager : MonoBehaviour
{
    public AgentsManagerParameters parameters;

    [SerializeField]
    private GameObject prefabSentinel;
   
    
    [HideInInspector]
    public Sentinel[] sentinels; // list of the sentinel object
    [HideInInspector]
    public GPUCompute.GPUData[] GPUStruct; // list of sentinel struct, that will hold the data for gpu compute
    public GPUCompute.GPUOutput[] GPUOutput;

    private List<Genetics.GenVel> GenVelLib; // list of the GenVel data to act as the library

    private List<Genetics.GenKurmto> GenKurLib;// list of the GenKurmto data to act as the library


    [HideInInspector]
    public Vector3[] Lymphondes;
    [HideInInspector]
    public Vector3[] PathogenEmitters;
    [HideInInspector]
    public PathogenManager[] pathogenManagers;
    [HideInInspector]
    public TCellManager[] tcellManagers;

    void Start()
    {
        sentinels = new Sentinel[parameters.amongAgentsAtStart];
        GPUStruct = new GPUCompute.GPUData[parameters.amongAgentsAtStart];
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();
        GPUOutput = new GPUCompute.GPUOutput[parameters.amongAgentsAtStart];

        for (int i=0; i< parameters.amongAgentsAtStart; i++)
        {

            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;

            Sentinel newSentinel = Instantiate(prefabSentinel, pos, Quaternion.identity, this.transform).GetComponent<Sentinel>();
            
            newSentinel.kuramotoAffectedAgent.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange,  0.2f);// setup its setting to randomize them

            GPUStruct[i].SetFromKuramoto(newSentinel.kuramotoAffectedAgent);
            GPUStruct[i].pos = newSentinel.transform.position;
            GPUOutput[i].Setup();

            sentinels[i] = newSentinel;
        }

        GameObject[] lymphs = GameObject.FindGameObjectsWithTag("Lymphonde");
        Lymphondes = new Vector3[lymphs.Length];
        tcellManagers = new TCellManager[lymphs.Length];
        for (int i = 0; i < lymphs.Length; i++)
        {
            tcellManagers[i] = lymphs[i].GetComponent<TCellManager>();
            Lymphondes[i] = lymphs[i].transform.position;
        }

        GameObject[] pathogens = GameObject.FindGameObjectsWithTag("PathogenEmitter");
        PathogenEmitters = new Vector3[pathogens.Length];
        pathogenManagers = new PathogenManager[pathogens.Length];

        for (int i = 0; i < pathogens.Length; i++)
        {
            PathogenEmitters[i] = pathogens[i].transform.position;
            pathogenManagers[i] = pathogens[i].GetComponent<PathogenManager>();
        }
    }

    private void Update()
    {  
        for (int i = 0; i < parameters.amongAgentsAtStart; i++)
        {
            if (sentinels[i].kuramotoAffectedAgent.dead || sentinels[i].kuramotoAffectedAgent.age> parameters.MaxAge) 
            {
                ResetSentinel(i);

                GPUStruct[i].SetFromKuramoto(sentinels[i].kuramotoAffectedAgent);
                GPUStruct[i].pos = sentinels[i].rigidBody.position;
            }
            else
            {
                sentinels[i].kuramotoAffectedAgent.age += Time.deltaTime;
                sentinels[i].kuramotoAffectedAgent.phase += GPUOutput[i].phaseAdition * Time.deltaTime;
                if (sentinels[i].kuramotoAffectedAgent.phase > 1) 
                    sentinels[i].kuramotoAffectedAgent.phase--; 
                GPUStruct[i].phase = sentinels[i].kuramotoAffectedAgent.phase;

                sentinels[i].rigidBody.AddForceAtPosition(GPUOutput[i].vel * parameters.speedScl * Time.deltaTime * sentinels[i].kuramotoAffectedAgent.phase, sentinels[i].transform.position + sentinels[i].transform.up);

                GPUStruct[i].speed = sentinels[i].kuramotoAffectedAgent.speed;
                GPUStruct[i].pos = sentinels[i].rigidBody.position;
            }

            if (sentinels[i].renderer.isVisible)
                sentinels[i].renderer.material.SetFloat("Phase", sentinels[i].kuramotoAffectedAgent.phase);
        }
    }

    public void ResetSentinel(int i, bool genOn= false)
    {
        Sentinel thisSentinel = sentinels[i];

        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;
        thisSentinel.transform.position = pos;

        thisSentinel.fosilising.enabled = false;
        thisSentinel.gameObject.SetActive(true);

        if (!genOn)
        {
            // reset bothe genetic values to random
            thisSentinel.kuramotoAffectedAgent.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);
            thisSentinel.geneticMovementSentinel.Reset();
        }
        else if (GenKurLib.Count < 500)
        {
            // add it settings to the librarys
            Genetics.GenKurmto genKurm = new Genetics.GenKurmto(thisSentinel.kuramotoAffectedAgent.speedBPM, thisSentinel.kuramotoAffectedAgent.noiseScl, thisSentinel.kuramotoAffectedAgent.coupling, thisSentinel.kuramotoAffectedAgent.couplingRange, thisSentinel.kuramotoAffectedAgent.attractionSclr, thisSentinel.kuramotoAffectedAgent.fitness);
            GenKurLib.Add(genKurm);
            Genetics.GenVel vels = new Genetics.GenVel(sentinels[i].geneticMovementSentinel.geneticMovement, thisSentinel.kuramotoAffectedAgent.fitness);
            GenVelLib.Add(vels);

            // reset bothe genetic values to random
            thisSentinel.kuramotoAffectedAgent.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);
            thisSentinel.geneticMovementSentinel.Reset();
        }
        else
        {
            // add random sentinel from lib
            int rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData1 = GenKurLib[rand];
            rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData2 = GenKurLib[rand];

            float[] Settings = kurData1.BlendAttributes(kurData2.Settings);

            thisSentinel.kuramotoAffectedAgent.SetupData(Settings);
           

            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel1 = GenVelLib[rand];
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel2 = GenVelLib[rand];

            Vector3[] Vels = genVel2.BlendAttributes(genVel1.Vels);

            thisSentinel.geneticMovementSentinel.Reset();
            thisSentinel.geneticMovementSentinel.geneticMovement = Vels;
        }
        
    }

    // all bellow is for ui to change all sentinels values 
    public void setRange(float range)
    {
        for (int i = 0; i < parameters.amongAgentsAtStart; i++)
            sentinels[i].kuramotoAffectedAgent.couplingRange = range;
    }

    public void setCoupling(float range)
    {
        for (int i = 0; i < parameters.amongAgentsAtStart; i++)
            sentinels[i].kuramotoAffectedAgent.coupling = range;
    }

    public void setNoise(float range)
    {
        for (int i = 0; i < parameters.amongAgentsAtStart; i++)
            sentinels[i].kuramotoAffectedAgent.noiseScl = range;

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
