using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TCellManager : MonoBehaviour
{
    [Tooltip("The gameobject for each agent in this manager")]
    [SerializeField]
    private GameObject[] tCell; // holds the sentinel prefab
    [Tooltip("Number of the agents to be produced by this manager")]
    [Range(1, 3000)]
    [SerializeField]
    public int nSentinels = 10; // number of them to be made
    private int MaxSentinels = 15;
    [HideInInspector]
    public int RealNumSentinels = 0;

    [Tooltip("radius to be spawned in from this obects transform")]
    [Range(0.1f, 1000f)]
    [SerializeField]
    private float spawnArea = 1.0f; // area to spawn in 
    [Tooltip("Kuramoto speed, measured in bpm, x=min y=max")]
    [SerializeField]
    private Vector2 speedRange = new Vector2(0, 1); // variation of speed for them to have  
    [Tooltip("Kuramoto, range for the max distance for the effect, x=min y=max")]
    [SerializeField]
    private Vector2 couplingRange = new Vector2(1, 10); // coupling range to have
    [Tooltip("Kuramoto, range for noise effect, x=min y=max")]
    [SerializeField]
    private Vector2 noiseSclRange = new Vector2(0.01f, 0.5f); // noise Scl to have
    [Tooltip("Kuramoto, range for the strength of the coupling effect, x=min y=max")]
    [SerializeField]
    private Vector2 couplingSclRange = new Vector2(0.2f, 10f); // coupling scl
    [Tooltip("Kuramoto, range for the scaling the clustering/attraction effect, x=min y=max")]
    [SerializeField]
    private Vector2 attractionSclRange = new Vector2(0.2f, 1f); // coupling scl

    [HideInInspector]
    public GameObject[] sentinels; //list to hold the sentinels
    [HideInInspector]
    public GPUCompute.GPUData[] GPUStruct; // list of struct ot hold data, maybe for gpu acceleration
    public GPUCompute.GPUOutput[] GPUOutput;

    private List<Genetics.GenVel> GenVelLib; // lib to hold the gene move data

    private List<Genetics.GenKurmto> GenKurLib; // lib to hold gene kurmto data

    private List<Genetics.Antigen> antigenLib;
    [Tooltip("Max age the agents will reach")]
    [SerializeField]
    private float MaxAge = 1000; // age limit to kill sentinels

    [SerializeField]
    private float speedScl = 3f;

    [SerializeField]
    private float emitionRate = 4;

    // Start is called before the first frame update
    void Start()
    {
        MaxSentinels = Mathf.FloorToInt(nSentinels * 1.5f);


        // create list to hold object
        sentinels = new GameObject[MaxSentinels];
        // create list to hold data structs
        GPUStruct = new GPUCompute.GPUData[MaxSentinels];
        // create the two lib lists
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();
        antigenLib = new List<Genetics.Antigen>();

        GPUOutput = new GPUCompute.GPUOutput[MaxSentinels];

        // loop over the nsentinels
        for (int i=0; i<nSentinels; i++)
        {

            


            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere*spawnArea;

            int randindx = UnityEngine.Random.Range(0, tCell.Length);

            // instantiate a new sentinel as child and at pos
            GameObject thisSentinel = Instantiate(tCell[randindx], pos, Quaternion.identity, this.transform);

            // get its kurmto component
            KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them

            thisSentinel.GetComponent<GeneticAntigenKey>().Reset();

            // add the object to the list
            sentinels[i] = thisSentinel;

            // set data in the struct
            GPUStruct[i].SetFromKuramoto(kuramoto);
            GPUStruct[i].pos = sentinels[i].transform.position;
            GPUOutput[i].Setup();


        }
        RealNumSentinels = nSentinels;

        StartCoroutine(emition(emitionRate));
    }
    IEnumerator emition( float rate)
    {
        while (true)
        {
            if (CanAddCell())
            {
                Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnArea;

                int randindx = UnityEngine.Random.Range(0, tCell.Length);

                // instantiate a new sentinel as child and at pos
                GameObject thisSentinel = Instantiate(tCell[randindx], pos, Quaternion.identity, this.transform);

                // get its kurmto component
                KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
                kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them

                AddTCell(thisSentinel);
            }

            yield return new WaitForSeconds(rate);

        }
    }
    private void Update()
    {
        
        List<int> toRemove = new List<int>();
        // loop over the n sentinels
       
        for (int i = 0; i < RealNumSentinels; i++)
        {

            if(sentinels[i] == null) { continue; }
            // get the kurmto
            KuramotoAffectedAgent kuramoto = sentinels[i].GetComponent<KuramotoAffectedAgent>();
            
            // if older than age 
            if (kuramoto.dead || kuramoto.age > MaxAge) {
                if (i > nSentinels)
                {

                    toRemove.Add(i);
                    
                    continue;
                }


                // reset its values
                ResetSentinel(i);
                

            }
            else
            {
                if (kuramoto.played == 3)
                {
                    kuramoto.age += Time.deltaTime*10;

                }
                else
                {
                    kuramoto.age += Time.deltaTime;
                }
                kuramoto.phase += GPUOutput[i].phaseAdition * Time.deltaTime;
                if (kuramoto.phase > 1) { kuramoto.phase = kuramoto.phase-1; }
                GPUStruct[i].played = kuramoto.played;
                GPUStruct[i].phase = kuramoto.phase;

                sentinels[i].GetComponent<Rigidbody>().AddForceAtPosition(GPUOutput[i].vel * speedScl * Time.deltaTime * kuramoto.phase, sentinels[i].transform.position + sentinels[i].transform.up);

                GPUStruct[i].pos = sentinels[i].transform.position;

            }

          
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
            Destroy(sentinels[indx]);

            if (i != toRemove.Count-1)
            {
                 nxtIndx = toRemove[i+1];
            }
            else
            {
                nxtIndx = RealNumSentinels;
            }

            for (int p = indx+1; p <= nxtIndx ; p++)
            {
                GPUStruct[p - (i+1)] = GPUStruct[p];
                sentinels[p - (i+1)] = sentinels[p];

            }            

        }
        
        RealNumSentinels -= toRemove.Count;
        
        if (nxtIndx != -1) {
            GPUStruct[nxtIndx] = new GPUCompute.GPUData();
            sentinels[nxtIndx] = null;

        }

    }
   
    public void ResetSentinel(int i ,bool genOn = false)
    {

        // get i sentinel
        GameObject thisSentinel = sentinels[i];

        thisSentinel.GetComponent<Renderer>().material.SetFloat("KeyTrigger", 0);

        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnArea;

        thisSentinel.transform.position = pos;

        if (!genOn)
        {
            // reset bothe genetic values to random
            KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);

            GeneticMovementTcell genVel = thisSentinel.GetComponent<GeneticMovementTcell>();
            genVel.Reset();

            thisSentinel.GetComponent<GeneticAntigenKey>().Reset();

        }
        else if (GenKurLib.Count < 500)
        {
            KuramotoAffecterAgent kuramoto = thisSentinel.GetComponent<KuramotoAffecterAgent>();


            // add it settings to the librarys
            Genetics.GenKurmto genKurm = new Genetics.GenKurmto(kuramoto.speedBPM, kuramoto.noiseScl, kuramoto.coupling, kuramoto.couplingRange, kuramoto.attractionSclr, kuramoto.fitness);
            GenKurLib.Add(genKurm);
            Genetics.GenVel vels = new Genetics.GenVel(sentinels[i].GetComponent<GeneticMovementSentinel>().geneticMovement, kuramoto.fitness);
            GenVelLib.Add(vels);
            // add random new sentinel
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them
            
            GeneticMovementTcell genVel = thisSentinel.GetComponent<GeneticMovementTcell>();
            genVel.Reset();

            thisSentinel.GetComponent<GeneticAntigenKey>().Reset();
        }
        else
        {
            // add random sentinel from lib
            int rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData1 = GenKurLib[rand];
            rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData2 = GenKurLib[rand];

            float[] Settings = kurData1.BlendAttributes(kurData2.Settings);

            KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
            kuramoto.SetupData(Settings);

            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel1 = GenVelLib[rand];
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel2 = GenVelLib[rand];

            Vector3[] Vels = genVel2.BlendAttributes(genVel1.Vels);

            GeneticMovementTcell genMov = thisSentinel.GetComponent<GeneticMovementTcell>();
            genMov.Reset();
            genMov.geneticMovement = genVel1.BlendAttributes(Vels);

            GeneticAntigenKey antigenKey = thisSentinel.GetComponent<GeneticAntigenKey>();
            antigenKey.Reset();



        }
        
    }

    public bool CanAddCell()
    {
        if (RealNumSentinels < MaxSentinels - 1)
        {
            return true;
        }
        return false;
    }

    public void AddTCell(GameObject TCell)
    {
        
            RealNumSentinels++;


            // add the object to the list
            sentinels[RealNumSentinels-1] = TCell;
            KuramotoAffectedAgent kuramoto = TCell.GetComponent<KuramotoAffectedAgent>();

            // set data in the struct
            GPUCompute.GPUData gpuStruct = new GPUCompute.GPUData();
            gpuStruct.SetFromKuramoto(kuramoto);
            gpuStruct.pos = TCell.transform.position;
            GPUStruct[RealNumSentinels] = gpuStruct;
        

    }



}
