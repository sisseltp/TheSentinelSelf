using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KuramotoSentinelMaker : MonoBehaviour
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
    private float speedVariation = 0.1f;  // variation of speed
    [SerializeField]
    private float couplingRange = 1; // coupling distance
    [SerializeField]
    private float noiseScl = 1; // scls the noise
    [SerializeField]
    private float coupling = 0.5f; // coupling value

    [HideInInspector]
    public GameObject[] sentinels; //list to hold the sentinels

    public Sentinel[] sentinelsStruct; // list of struct ot hold data, maybe for gpu acceleration

    public List<GenVel> GenVelLib; // lib to hold the gene move data

    public List<GenKurmto> GenKurLib; // lib to hold gene kurmto data

    [SerializeField]
    private float age = 1000; // age limit to kill sentinels

    // struct to hold data maybe for gpu acceleration
    public struct Sentinel
    {
        public float speed;
        public float phase;
        public float cohPhi;
        public float coherenceRadius;
        public float couplingRange;
        public float noiseScl;
        public float coupling;
        public int counter;
        public Vector3 vel;
        public Vector3[] GenVel;
    }
    // struct to hold the genetic move data
    public struct GenVel
    {

        public GenVel(Vector3[] vels, float fit)
        {
            Vels = vels;
            fitness = fit;
        }

        public Vector3[] Vels;
        public float fitness;

    }
    // struct to holg gene kurmto data
     public struct GenKurmto
    {
        public float[] Settings;
        public float fitness;
        // constructor 
        public GenKurmto(float speed, float noiseScl, float coupling, float couplingRange, float fit)
        {
            Settings = new float[4];
            Settings[0] = speed;
            Settings[1] = noiseScl;
            Settings[2] = coupling;
            Settings[3] = couplingRange;
            fitness = fit;
        }
    }
     

    // Start is called before the first frame update
    void Start()
    {
        // create list to hold object
        sentinels = new GameObject[nSentinels];
        // create list to hold data structs
        sentinelsStruct = new Sentinel[nSentinels];
        // create the two lib lists
        GenKurLib = new List<GenKurmto>();
        GenVelLib = new List<GenVel>();

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
            KuramotoSentinel kuramoto = thisSentinel.GetComponent<KuramotoSentinel>();
            kuramoto.Reset();// randomize its settings

            // add the object to the list
            sentinels[i] = thisSentinel;

            // set data in the struct
            sentinelsStruct[i].speed = kuramoto.speed;
            sentinelsStruct[i].phase = kuramoto.phase;
            sentinelsStruct[i].cohPhi = kuramoto.cohPhi;
            sentinelsStruct[i].coherenceRadius = kuramoto.coherenceRadius;
            sentinelsStruct[i].couplingRange = kuramoto.couplingRange;
            sentinelsStruct[i].noiseScl = kuramoto.noiseScl;
            sentinelsStruct[i].coupling = kuramoto.coupling;
            sentinelsStruct[i].counter = kuramoto.counter;
            sentinelsStruct[i].vel = thisSentinel.GetComponent<Rigidbody>().velocity;
            sentinelsStruct[i].GenVel = thisSentinel.GetComponent<GeneticMovementSentinel>().geneticMovement;
           

        }


    }

    private void Update()
    {
        // loop over the n sentinels
        for (int i = 0; i < nSentinels; i++)
        {
            // get the kurmto
            KuramotoSentinel kuramoto = sentinels[i].GetComponent<KuramotoSentinel>();
            // set its variable
            sentinelsStruct[i].speed = kuramoto.speed;
            sentinelsStruct[i].phase = kuramoto.phase;
            sentinelsStruct[i].cohPhi = kuramoto.cohPhi;
            sentinelsStruct[i].coherenceRadius = kuramoto.coherenceRadius;
            sentinelsStruct[i].couplingRange = kuramoto.couplingRange;
            sentinelsStruct[i].noiseScl = kuramoto.noiseScl;
            sentinelsStruct[i].coupling = kuramoto.coupling;
            sentinelsStruct[i].counter = kuramoto.counter;
            sentinelsStruct[i].vel = sentinels[i].GetComponent<Rigidbody>().velocity;
            sentinelsStruct[i].GenVel = sentinels[i].GetComponent<GeneticMovementSentinel>().geneticMovement;

            // if older than age 
            if (kuramoto.age > age) {
                // add data to lib
                GenKurmto genKurm = new GenKurmto(kuramoto.speed, kuramoto.noiseScl, kuramoto.coupling, kuramoto.couplingRange, kuramoto.fitness);
                GenKurLib.Add(genKurm);
                GenVel vels = new GenVel(sentinelsStruct[i].GenVel, kuramoto.fitness);
                GenVelLib.Add(vels);
                // reset its values
                ResetSentinel(i);
            }

            // if the lib is greater than ...
            if (GenVelLib.Count > 1000)
            {
                // reorder the lib by fitness
                Debug.Log("Resize");
                Debug.Log(GenVelLib[0].fitness);
                GenVelLib.Sort(SortByScore);
                GenKurLib.Sort(SortByScore);
                Debug.Log(GenVelLib[0].fitness);
                // remove the first 250
                GenVelLib.RemoveRange(0, 250);
                GenKurLib.RemoveRange(0, 250);


            }
        }

    }
    // functions for the list sort function, to order values
    private int SortByScore(GenKurmto x, GenKurmto y)
    {
        return x.fitness.CompareTo(y.fitness);
    }

    private int SortByScore(GenVel x, GenVel y)
    {
        return x.fitness.CompareTo(y.fitness);
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
            KuramotoSentinel kuramoto = thisSentinel.GetComponent<KuramotoSentinel>();
            kuramoto.Reset();

            GeneticMovementSentinel genVel = thisSentinel.GetComponent<GeneticMovementSentinel>();
            genVel.Reset();
        }
        else
        {
            // add random sentinel from lib
            int rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            
            GenKurmto kurData = GenKurLib[rand];

            KuramotoSentinel kuramoto = thisSentinel.GetComponent<KuramotoSentinel>();
            kuramoto.Reset();
            kuramoto.speed = kurData.Settings[0];
            kuramoto.noiseScl = kurData.Settings[1];
            kuramoto.couplingRange = kurData.Settings[3];
            kuramoto.coupling = kurData.Settings[2];

            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            GenVel genVel = GenVelLib[rand];

            GeneticMovementSentinel genMov = thisSentinel.GetComponent<GeneticMovementSentinel>();
            genMov.geneticMovement = genVel.Vels;


        }
        
    }


    // functions bellow are for ui to change settings in debug
    public void setRange(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoSentinel kuramoto = sentinels[i].GetComponent<KuramotoSentinel>();
            kuramoto.couplingRange = range;

        }
    }

    public void setCoupling(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoSentinel kuramoto = sentinels[i].GetComponent<KuramotoSentinel>();
            kuramoto.coupling = range;

        }
    }

    public void setNoise(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoSentinel kuramoto = sentinels[i].GetComponent<KuramotoSentinel>();
            kuramoto.noiseScl = range;

        }
    }
}
