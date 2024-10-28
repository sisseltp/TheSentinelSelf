using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlasticManager : MonoBehaviour
{
    public AgentsManagerParameters parameters;

    [SerializeField]
    private GameObject prefabPlastic;
    [Tooltip("The object to emit from")]
    [SerializeField]
    private Transform emitionOrigin;

    public int MaxPlastics = 15;
    public float emitionSpeed = 10;
    public int RealAmountPlastics = 0;
    private float timeGate = 0;

    [HideInInspector]
    public Plastic[] plastics; //list to hold the sentinels
    [HideInInspector]
    public GPUCompute.GPUData[] GPUStruct; // list of struct ot hold data, maybe for gpu acceleration
    public GPUCompute.GPUOutput[] GPUOutput;

    [Tooltip("colour 1 to lerp between")]
    [SerializeField]
    private Color col0;// phase col1
    [Tooltip("colour 2 to lerp between")]
    [SerializeField]
    private Color col1; // phase col2

    void Start()
    {
        plastics = new Plastic[MaxPlastics];
        GPUStruct = new GPUCompute.GPUData[MaxPlastics];
        GPUOutput = new GPUCompute.GPUOutput[MaxPlastics];

        for (int i=0; i< parameters.amongAgentsAtStart; i++)
        {
            Vector3 pos = emitionOrigin.position + UnityEngine.Random.insideUnitSphere* parameters.spawnArea;

            Plastic thisSentinel = Instantiate(prefabPlastic, pos, Quaternion.identity, this.transform).GetComponent<Plastic>();

            thisSentinel.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them

            GPUStruct[i].SetFromKuramoto(thisSentinel.kuramoto);
            GPUStruct[i].pos = thisSentinel.transform.position;
            GPUOutput[i].Setup();

            plastics[i] = thisSentinel;
        }

        RealAmountPlastics = parameters.amongAgentsAtStart;
    }

    private void Update()
    {
        List<int> toRemove = new List<int>();

        if (emitionSpeed + timeGate < Time.time)
        {
            timeGate = Time.time;
            AddCell();
        }

        for (int i = 0; i < RealAmountPlastics; i++)
        {
            if(plastics[i] == null)
                continue;
            
            if(plastics[i].kuramoto.age > parameters.MaxAge || plastics[i].kuramoto.dead)
            {
                toRemove.Add(i);
            }
            else
            {
                plastics[i].kuramoto.age += Time.deltaTime;
                plastics[i].kuramoto.phase += GPUOutput[i].phaseAdition * Time.deltaTime ;
                if (plastics[i].kuramoto.phase > 1) { plastics[i].kuramoto.phase = plastics[i].kuramoto.phase - 1; }
                GPUStruct[i].phase = plastics[i].kuramoto.phase;
                plastics[i].rigidBody.AddForceAtPosition(GPUOutput[i].vel * parameters.speedScl * Time.deltaTime * plastics[i].kuramoto.phase, plastics[i].transform.position + plastics[i].transform.up);
                GPUStruct[i].pos = plastics[i].rigidBody.position;
            }

            if (plastics[i].renderer.isVisible)
                plastics[i].renderer.material.SetFloat("Phase", plastics[i].kuramoto.phase);
        }

        int nextIndex = 0;
        for (int i = 0; i < parameters.amongAgentsAtStart; i++)
        {
            if (toRemove.Contains(i))
            {
                if (plastics[i] != null)
                {
                    Destroy(plastics[i].gameObject);
                    plastics[i] = null;
                }
                continue;
            }

            plastics[nextIndex] = plastics[i];
            GPUStruct[nextIndex] = GPUStruct[i];

            if (nextIndex != i)
            {
                plastics[i] = null;
                GPUStruct[i] = new GPUCompute.GPUData();
            }

            nextIndex++;
        }

        RealAmountPlastics -= toRemove.Count;

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
        Plastic thisPlastic = plastics[i];

        Vector3 pos = emitionOrigin.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;

        thisPlastic.transform.position = pos;

        thisPlastic.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them
        thisPlastic.geneticMovement.Reset();
    }

    public void AddCell()
    {
        if (RealAmountPlastics < MaxPlastics-1)
        {
            RealAmountPlastics++;

            Plastic thisAgent =  Instantiate(prefabPlastic, transform).GetComponent<Plastic>();

            plastics[RealAmountPlastics-1] = thisAgent;

            ResetPlastic(RealAmountPlastics - 1);

            GPUCompute.GPUData gpuStruct = new GPUCompute.GPUData();
            gpuStruct.SetFromKuramoto(thisAgent.kuramoto);
            gpuStruct.pos = thisAgent.transform.position;
            GPUStruct[RealAmountPlastics-1] = gpuStruct;
        }
    }
}