using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BiomeManager : MonoBehaviour
{

    [SerializeField]
    private GameObject sentinel; // holds the sentinel prefab
    [Range(1, 3000)]
    [SerializeField]
    public int nSentinels = 10; // number of them to be made

    [Range(0.1f, 1000f)]
    [SerializeField]
    private float spawnArea = 1.0f; // area to spawn in 
    [SerializeField]
    private Vector2 speedRange = new Vector2(0, 1); // variation of speed for them to have
    [SerializeField]
    private Vector2 couplingRange = new Vector2(1, 10); // coupling range to have
    [SerializeField]
    private Vector2 noiseSclRange = new Vector2(0.01f, 0.5f); // noise Scl to have
    [SerializeField]
    private Vector2 couplingSclRange = new Vector2(0.2f, 10f); // coupling scl
    [SerializeField]
    private Vector2 attractionSclRange = new Vector2(0.2f, 1f); // coupling scl

    [HideInInspector]
    public GameObject[] sentinels; //list to hold the sentinels

    public GPUData[] GPUStruct; // list of struct ot hold data, maybe for gpu acceleration
    
    public List<Genetics.GenVel> GenVelLib; // lib to hold the gene move data

    public List<Genetics.GenKurmto> GenKurLib; // lib to hold gene kurmto data

    [SerializeField]
    private float MaxAge = 1000; // age limit to kill sentinels

    [SerializeField]
    private float speedScl = 3f;


    // struct to hold data maybe for gpu acceleration
    public struct GPUData
    {
        public int age;
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

        
        public void SetFromKuramoto(KuramotoBiomeAgent kuramoto)
        {
            speed = kuramoto.speed;
            phase = kuramoto.phase;
            cohPhi = kuramoto.cohPhi;
            coherenceRadius = kuramoto.coherenceRadius;
            couplingRange = kuramoto.couplingRange;
            noiseScl = kuramoto.noiseScl;
            coupling = kuramoto.coupling;
            connections = kuramoto.Connections;
            attractionScl = kuramoto.attractionSclr;
            age = 0;
            fittness = 0;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // create list to hold object
        sentinels = new GameObject[nSentinels];
        // create list to hold data structs
        GPUStruct = new GPUData[nSentinels];
        // create the two lib lists
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();

        // loop over the nsentinels
        for(int i=0; i<nSentinels; i++)
        {
            // set rand pos
            float x = UnityEngine.Random.Range(-spawnArea, spawnArea);
            float y = 0;
            float z = UnityEngine.Random.Range(-spawnArea, spawnArea);

            Vector3 pos = new Vector3(x, y, z);

            // instantiate a new sentinel as child and at pos
            GameObject thisSentinel = Instantiate(sentinel, pos, Quaternion.identity, this.transform);

            // get its kurmto component
            KuramotoBiomeAgent kuramoto = thisSentinel.GetComponent<KuramotoBiomeAgent>();
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them

            // add the object to the list
            sentinels[i] = thisSentinel;

            // set data in the struct
            GPUStruct[i].SetFromKuramoto(kuramoto);
            GPUStruct[i].pos = sentinels[i].transform.position;
            

        }


    }

    private void Update()
    {
        // loop over the n sentinels
        for (int i = 0; i < nSentinels; i++)
        {
            // get the kurmto
            KuramotoBiomeAgent kuramoto = sentinels[i].GetComponent<KuramotoBiomeAgent>();
            
            // if older than age 
            if (kuramoto.dead || GPUStruct[i].age > MaxAge) {
                // add data to lib
                Genetics.GenKurmto genKurm = new Genetics.GenKurmto(kuramoto.speedBPM, kuramoto.noiseScl, kuramoto.coupling, kuramoto.couplingRange, kuramoto.attractionSclr, kuramoto.fitness);
                GenKurLib.Add(genKurm);
                Genetics.GenVel vels = new Genetics.GenVel(sentinels[i].GetComponent<GeneticMovementBiome>().geneticMovement, kuramoto.fitness);
                GenVelLib.Add(vels);
                // reset its values
                ResetSentinel(i);
                
                GPUStruct[i].SetFromKuramoto(kuramoto);
                GPUStruct[i].pos = sentinels[i].transform.position;
            }
            else
            {
                kuramoto.fitness = GPUStruct[i].fittness;
                kuramoto.age = GPUStruct[i].age;
                kuramoto.phase = GPUStruct[i].phase;
                kuramoto.Connections = GPUStruct[i].connections;
                kuramoto.played = GPUStruct[i].played;
                sentinels[i].GetComponent<Rigidbody>().velocity += GPUStruct[i].vel* Time.deltaTime * speedScl;
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

    }
    // resets the i sentinel
    public void ResetSentinel(int i)
    {

        // get i sentinel
        GameObject thisSentinel = sentinels[i];

        //Random pos
        float x = UnityEngine.Random.Range(-spawnArea, spawnArea);
        float y = 0;
        float z = UnityEngine.Random.Range(-spawnArea, spawnArea);

        Vector3 pos = new Vector3(x, y, z);

        thisSentinel.transform.position = pos;

        // if lib less than 500
        if (GenKurLib.Count < 500)
        {
            // add random new sentinel
            KuramotoBiomeAgent kuramoto = thisSentinel.GetComponent<KuramotoBiomeAgent>();
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them

            GeneticMovementBiome genVel = thisSentinel.GetComponent<GeneticMovementBiome>();
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

            KuramotoBiomeAgent kuramoto = thisSentinel.GetComponent<KuramotoBiomeAgent>();
            kuramoto.SetupData(Settings);

            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel1 = GenVelLib[rand];
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel2 = GenVelLib[rand];

            Vector3[] Vels = genVel2.BlendAttributes(genVel1.Vels);

            GeneticMovementBiome genMov = thisSentinel.GetComponent<GeneticMovementBiome>();
            genMov.Reset();
            genMov.geneticMovement = genVel1.BlendAttributes(Vels);


        }
        
    }


    // functions bellow are for ui to change settings in debug
    public void setRange(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoBiomeAgent kuramoto = sentinels[i].GetComponent<KuramotoBiomeAgent>();
            kuramoto.couplingRange = range;

        }
    }

    public void setCoupling(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoBiomeAgent kuramoto = sentinels[i].GetComponent<KuramotoBiomeAgent>();
            kuramoto.coupling = range;

        }
    }

    public void setNoise(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoBiomeAgent kuramoto = sentinels[i].GetComponent<KuramotoBiomeAgent>();
            kuramoto.noiseScl = range;

        }
    }

}
