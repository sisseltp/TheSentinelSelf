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
    public int RealAmountPlasstics = 0;
    private float timeGate = 0;

    
   

    [HideInInspector]
    public GameObject[] plastics; //list to hold the sentinels
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
        plastics = new GameObject[MaxPlastics];
        GPUStruct = new GPUCompute.GPUData[MaxPlastics];
        GPUOutput = new GPUCompute.GPUOutput[MaxPlastics];

        for (int i=0; i< parameters.amongAgentsAtStart; i++)
        {
            Vector3 pos = emitionOrigin.position + UnityEngine.Random.insideUnitSphere* parameters.spawnArea;

            GameObject thisSentinel = Instantiate(prefabPlastic, pos, Quaternion.identity, this.transform);

            KuramotoPlasticAgent kuramoto = thisSentinel.GetComponent<KuramotoPlasticAgent>();
            kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them

            GPUStruct[i].SetFromKuramoto(kuramoto);
            GPUStruct[i].pos = thisSentinel.transform.position;
            GPUOutput[i].Setup();

            plastics[i] = thisSentinel;
        }

        RealAmountPlasstics = parameters.amongAgentsAtStart;
    }

    private void Update()
    {
        List<int> toRemove = new List<int>();

        if (emitionSpeed + timeGate < Time.time)
        {
            timeGate = Time.time;
            AddCell();
        }

        for (int i = 0; i < RealAmountPlasstics; i++)
        {
            if(plastics[i] == null) { continue; }
            // get the kurmto
            KuramotoPlasticAgent kuramoto = plastics[i].GetComponent<KuramotoPlasticAgent>();
            
            if(kuramoto.age > parameters.MaxAge || kuramoto.dead)
            {
                toRemove.Add(i);
            }
            else
            {
                kuramoto.age += Time.deltaTime;
                kuramoto.phase += GPUOutput[i].phaseAdition * Time.deltaTime ;
                if (kuramoto.phase > 1) { kuramoto.phase = kuramoto.phase - 1; }
                GPUStruct[i].phase = kuramoto.phase;
                plastics[i].GetComponent<Rigidbody>().AddForceAtPosition(GPUOutput[i].vel * parameters.speedScl * Time.deltaTime * kuramoto.phase, plastics[i].transform.position + plastics[i].transform.up);
                GPUStruct[i].pos = plastics[i].GetComponent<Rigidbody>().position;
            }

            Renderer rendr = plastics[i].GetComponent<Renderer>();

            if (rendr.isVisible)
                rendr.material.SetFloat("Phase", kuramoto.phase);
        }


        /*
        // if the lib is greater than ...
        if (GenVelLib.Count > 1000)
        {
            // negative selection
            GenVelLib = Genetics.NegativeSelection(GenVelLib);
            GenKurLib = Genetics.NegativeSelection(GenKurLib);

        }
        */


        int nxtIndx = -1;
        
        for ( int i=0; i<toRemove.Count; i++)
        {
            int indx = toRemove[i];
            Destroy(plastics[indx]);

            if (i != toRemove.Count-1)
            {
                 nxtIndx = toRemove[i+1];
            }
            else
            {
                nxtIndx = RealAmountPlasstics;
            }

            for (int p = indx+1; p <= nxtIndx ; p++)
            {
                GPUStruct[p - (i+1)] = GPUStruct[p];
                plastics[p - (i+1)] = plastics[p];

            }            

        }
        RealAmountPlasstics -= toRemove.Count;
        
        if (nxtIndx != -1) {
            GPUStruct[nxtIndx] = new GPUCompute.GPUData();
            plastics[nxtIndx] = null;

        }

    }
   
    public void ResetPlastic(int i)
    {
        GameObject thisPlastic = plastics[i];

        Vector3 pos = emitionOrigin.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;

        thisPlastic.transform.position = pos;

       
            // add random new sentinel
            KuramotoPlasticAgent kuramoto = thisPlastic.GetComponent<KuramotoPlasticAgent>();
            kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them
            
            GeneticMovementPlastic genVel = thisPlastic.GetComponent<GeneticMovementPlastic>();
            genVel.Reset();

    }

    public void AddCell()
    {
        if (RealAmountPlasstics < MaxPlastics-1)
        {
            RealAmountPlasstics++;


            GameObject thisAgent =  Instantiate(prefabPlastic, transform);
            // add the object to the list
            plastics[RealAmountPlasstics-1] = thisAgent;
            KuramotoPlasticAgent kuramoto = thisAgent.GetComponent<KuramotoPlasticAgent>();

            ResetPlastic(RealAmountPlasstics - 1);

            // set data in the struct
            GPUCompute.GPUData gpuStruct = new GPUCompute.GPUData();
            gpuStruct.SetFromKuramoto(kuramoto);
            gpuStruct.pos = thisAgent.transform.position;
            GPUStruct[RealAmountPlasstics-1] = gpuStruct;
        }

    }


}
