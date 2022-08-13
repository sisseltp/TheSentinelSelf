using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlasticManager2 : MonoBehaviour
{
    [Tooltip("The gameobject for each agent in this manager")]
    [SerializeField]
    private GameObject sentinel; // holds the sentinel prefab
    [Tooltip("The object to emit from")]
    [SerializeField]
    private Transform emitionOrigin;
    [Tooltip("Number of the agents to be produced by this manager")]
    [Range(1, 3000)]
    [SerializeField]
    public int startingNumAgents = 10; // number of them to be made
    public int MaxSentinels = 15;
    public float emitionSpeed = 10;
    public int RealNumSentinels = 0;
    private float timeGate = 0;

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
    public GPUData[] GPUStruct; // list of struct ot hold data, maybe for gpu acceleration
    
    private List<Genetics.GenVel> GenVelLib; // lib to hold the gene move data

    private List<Genetics.GenKurmto> GenKurLib; // lib to hold gene kurmto data


    [Tooltip("Max age the agents will reach")]
    [SerializeField]
    private float MaxAge = 1000; // age limit to kill sentinels

    [SerializeField]
    private float speedScl = 3f;

    public struct GPUData
    {
        public float age;
        public int connections;
        public int played;
        public float speed;
        public float phase;
        public float cohPhi;
        public float coherenceRadius;
        public float couplingRange;
        public float noiseScl;
        public float coupling;
        public float attractionScl;
        public float fittness;
        public Vector3 vel;
        public Vector3 pos;




        public void SetFromKuramoto(KuramotoPlasticAgent kuramoto)
        {

            speed = kuramoto.speed;
            phase = kuramoto.phase;
            coherenceRadius = kuramoto.coherenceRadius;
            couplingRange = kuramoto.couplingRange;
            noiseScl = kuramoto.noiseScl;
            coupling = kuramoto.coupling;
            attractionScl = kuramoto.attractionSclr;
            age = 0;
            fittness = 0;
        }





    }


    // Start is called before the first frame update
    void Start()
    {
        //MaxSentinels = Mathf.FloorToInt(startingNumAgents * 1.5f);


        // create list to hold object
        sentinels = new GameObject[MaxSentinels];
        // create list to hold data structs
        GPUStruct = new GPUData[MaxSentinels];
        // create the two lib lists
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();
       



        // loop over the startingNumAgents
        for (int i=0; i<startingNumAgents; i++)
        {

            RealNumSentinels++;


            Vector3 pos = emitionOrigin.position + UnityEngine.Random.insideUnitSphere*spawnArea;

            // instantiate a new sentinel as child and at pos
            GameObject thisSentinel = Instantiate(sentinel, pos, Quaternion.identity, this.transform);

            // get its kurmto component
            KuramotoPlasticAgent kuramoto = thisSentinel.GetComponent<KuramotoPlasticAgent>();
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them

            // add the object to the list
            sentinels[i] = thisSentinel;

            // set data in the struct
            GPUStruct[i].SetFromKuramoto(kuramoto);
            GPUStruct[i].pos = sentinels[i].transform.position;
            

        }
        //Debug.Log(GPUStruct.Length);
    }

    private void Update()
    {
        //Debug.Log(RealNumSentinels);
        List<int> toRemove = new List<int>();
        // loop over the n sentinels

        if (emitionSpeed + timeGate < Time.time)
        {
            timeGate = Time.time;
            AddCell();
        }


        for (int i = 0; i < RealNumSentinels; i++)
        {

            if(sentinels[i] == null) { continue; }
            // get the kurmto
            KuramotoPlasticAgent kuramoto = sentinels[i].GetComponent<KuramotoPlasticAgent>();
            
            if( GPUStruct[i].age > MaxAge || kuramoto.dead)
            {
                toRemove.Add(i);

            

            }
            else
            {

                kuramoto.fitness = GPUStruct[i].fittness;
                kuramoto.age = GPUStruct[i].age;
                kuramoto.phase = GPUStruct[i].phase;
               
                sentinels[i].GetComponent<Rigidbody>().AddForceAtPosition( GPUStruct[i].vel* Time.deltaTime * speedScl, sentinels[i].transform.position + sentinels[i].transform.up);
                
                GPUStruct[i].pos = sentinels[i].transform.position;

            }

          
        }

        // if the lib is greater than ...
        if (GenVelLib.Count > 1000)
        {
            // negative selection
            GenVelLib = Genetics.NegativeSelection(GenVelLib);
            GenKurLib = Genetics.NegativeSelection(GenKurLib);

        }



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
            GPUStruct[nxtIndx] = new GPUData();
            sentinels[nxtIndx] = null;

        }

    }
   
    public void ResetSentinel(int i)
    {

        // get i sentinel
        GameObject thisSentinel = sentinels[i];



        Vector3 pos = emitionOrigin.position + UnityEngine.Random.insideUnitSphere * spawnArea;

        thisSentinel.transform.position = pos;

        // if lib less than 500
        if (GenKurLib.Count < 500)
        {
            // add random new sentinel
            KuramotoPlasticAgent kuramoto = thisSentinel.GetComponent<KuramotoPlasticAgent>();
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them
            
            GeneticMovementPlastic genVel = thisSentinel.GetComponent<GeneticMovementPlastic>();
            genVel.Reset();

            
        }
        else
        {
            // add random sentinel from lib
            int rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData1 = GenKurLib[rand];
            rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData2 = GenKurLib[rand];

            float[] Settings = kurData1.BlendAttributes(kurData2.Settings);

            KuramotoPlasticAgent kuramoto = thisSentinel.GetComponent<KuramotoPlasticAgent>();
            //kuramoto.SetupData(Settings);
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them

            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel1 = GenVelLib[rand];
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel2 = GenVelLib[rand];

            Vector3[] Vels = genVel2.BlendAttributes(genVel1.Vels);

            GeneticMovementPlastic genMov = thisSentinel.GetComponent<GeneticMovementPlastic>();
            genMov.Reset();
            genMov.geneticMovement = genVel1.BlendAttributes(Vels);

            



        }
        
    }

    public void AddCell()
    {
        if (RealNumSentinels < MaxSentinels-1)
        {
            RealNumSentinels++;

            GameObject thisAgent =  Instantiate(sentinel, transform);
            // add the object to the list
            sentinels[RealNumSentinels-1] = thisAgent;
            KuramotoPlasticAgent kuramoto = thisAgent.GetComponent<KuramotoPlasticAgent>();

            ResetSentinel(RealNumSentinels - 1);

            // set data in the struct
            GPUData gpuStruct = new GPUData();
            gpuStruct.SetFromKuramoto(kuramoto);
            gpuStruct.pos = thisAgent.transform.position;
            GPUStruct[RealNumSentinels-1] = gpuStruct;
        }

    }


}