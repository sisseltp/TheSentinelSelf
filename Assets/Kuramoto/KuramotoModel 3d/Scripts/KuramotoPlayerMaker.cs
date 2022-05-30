using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KuramotoPlayerMaker : MonoBehaviour
{

    [SerializeField]
    private GameObject sentinel; // the sentinel game object
    [Range(1, 3000)]
    [SerializeField]
    public int nSentinels = 10; // number of sentinels to be made

    [Range(0.1f, 100f)]
    [SerializeField]
    private float spawnArea = 1.0f; // area to be spawned in
    [SerializeField]
    private float speedVariation = 0.1f; // variation of speed for them to have
    [SerializeField]
    private float couplingRange = 1; // coupling range to have
    [SerializeField]
    private float noiseScl = 1; // noise Scl to have
    [SerializeField]
    private float coupling = 0.5f; // coupling scl

    [HideInInspector]
    public GameObject[] sentinels; // list of the sentinel object

    public Sentinel[] sentinelsStruct; // list of sentinel struct, that will hold the data for gpu compute

    public List<GenVel> GenVelLib; // list of the GenVel data to act as the library

    public List<GenKurmto> GenKurLib;// list of the GenKurmto data to act as the library


    // struct to hold all the sentinels data potential gpu compute
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
    // struct to hold the Genvels settings and fitness
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
    // struct to hold the GenKurmto settings and fitness
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
        // create the sentiels list
        sentinels = new GameObject[nSentinels];
        // create the data struct list
        sentinelsStruct = new Sentinel[nSentinels];
        // create the two libs as lists
        GenKurLib = new List<GenKurmto>();
        GenVelLib = new List<GenVel>();

        // for n sentinels
        for(int i=0; i<nSentinels; i++)
        {
            // set a random pos
            float x = UnityEngine.Random.Range(-spawnArea, spawnArea);
            float y = 0;
            float z = UnityEngine.Random.Range(-spawnArea, spawnArea);
            Vector3 pos = new Vector3(x, y, z);

            // create a new sentinel asa child and at pos
            GameObject thisSentinel = Instantiate(sentinel, pos, Quaternion.identity, this.transform);
            // get the kuramoto sentinel
            KuramotoPlayer kuramoto = thisSentinel.GetComponent<KuramotoPlayer>();
            kuramoto.Reset();// reset its setting to randomize them

            sentinels[i] = thisSentinel; // set the sentinel object to the list

            // set the values of the struct from the sentinel
            sentinelsStruct[i].speed = kuramoto.speed;
            sentinelsStruct[i].phase = kuramoto.phase;
            sentinelsStruct[i].cohPhi = kuramoto.cohPhi;
            sentinelsStruct[i].coherenceRadius = kuramoto.coherenceRadius;
            sentinelsStruct[i].couplingRange = kuramoto.couplingRange;
            sentinelsStruct[i].noiseScl = kuramoto.noiseScl;
            sentinelsStruct[i].coupling = kuramoto.coupling;
            sentinelsStruct[i].counter = kuramoto.counter;
            sentinelsStruct[i].vel = thisSentinel.GetComponent<Rigidbody>().velocity;
            sentinelsStruct[i].GenVel = thisSentinel.GetComponent<GeneticMovementPlayer>().geneticMovement;
           

        }


    }

    private void Update()
    {
        
        for (int i = 0; i < nSentinels; i++)
        {
            // get the components kuramoto component
            KuramotoPlayer kuramoto = sentinels[i].GetComponent<KuramotoPlayer>();

            // set the values of the struct from the sentinel
            sentinelsStruct[i].speed = kuramoto.speed;
            sentinelsStruct[i].phase = kuramoto.phase;
            sentinelsStruct[i].cohPhi = kuramoto.cohPhi;
            sentinelsStruct[i].coherenceRadius = kuramoto.coherenceRadius;
            sentinelsStruct[i].couplingRange = kuramoto.couplingRange;
            sentinelsStruct[i].noiseScl = kuramoto.noiseScl;
            sentinelsStruct[i].coupling = kuramoto.coupling;
            sentinelsStruct[i].counter = kuramoto.counter;
            sentinelsStruct[i].vel = sentinels[i].GetComponent<Rigidbody>().velocity;
            sentinelsStruct[i].GenVel = sentinels[i].GetComponent<GeneticMovementPlayer>().geneticMovement;

            // if the agent is dead
            if (kuramoto.dead) {
                // add it settings to the librarys
                GenKurmto genKurm = new GenKurmto(kuramoto.speed, kuramoto.noiseScl, kuramoto.coupling, kuramoto.couplingRange, kuramoto.fitness);
                GenKurLib.Add(genKurm);
                GenVel vels = new GenVel(sentinelsStruct[i].GenVel, kuramoto.fitness);
                GenVelLib.Add(vels);
                // call the reset function
                ResetSentinel(i);
                // reset the dead gate
                kuramoto.dead = false;
            }

            // if lib is greater than ...
            if (GenVelLib.Count > 1000)
            {
                // reorder on fitness
                Debug.Log("Resize");
                Debug.Log(GenVelLib[0].fitness);
                GenVelLib.Sort(SortByScore);
                GenKurLib.Sort(SortByScore);
                Debug.Log(GenVelLib[0].fitness);
                // remover 250 of the lowest scores
                GenVelLib.RemoveRange(0, 250);
                GenKurLib.RemoveRange(0, 250);


            }
        }

    }

    private int SortByScore(GenKurmto x, GenKurmto y)
    {
        return x.fitness.CompareTo(y.fitness);
    }

    private int SortByScore(GenVel x, GenVel y)
    {
        return x.fitness.CompareTo(y.fitness);
    }
    // resets the sentinel
    public void ResetSentinel(int i)
    {
        // get the sentinel
        GameObject thisSentinel = sentinels[i];

        // randomize pos
        float x = UnityEngine.Random.Range(-spawnArea, spawnArea);
        float y = 0;
        float z = UnityEngine.Random.Range(-spawnArea, spawnArea);

        Vector3 pos = new Vector3(x, y, z);

        thisSentinel.transform.position = pos;


        // lib count is bellow 500
        if (GenKurLib.Count < 500)
        {
            // reset bothe genetic values to random
            KuramotoPlayer kuramoto = thisSentinel.GetComponent<KuramotoPlayer>();
            kuramoto.Reset();

            GeneticMovementPlayer genVel = thisSentinel.GetComponent<GeneticMovementPlayer>();
            genVel.Reset();
        }
        else
        {
            // select a random indx
            int rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            // add sentinel from lib
            // get that data
            GenKurmto kurData = GenKurLib[rand];
            // set it from that data
            KuramotoPlayer kuramoto = thisSentinel.GetComponent<KuramotoPlayer>();
            kuramoto.Reset();
            kuramoto.speed = kurData.Settings[0];
            kuramoto.noiseScl = kurData.Settings[1];
            kuramoto.couplingRange = kurData.Settings[3];
            kuramoto.coupling = kurData.Settings[2];

            // select random indx
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            GenVel genVel = GenVelLib[rand];
            // set from data
            GeneticMovementPlayer genMov = thisSentinel.GetComponent<GeneticMovementPlayer>();
            genMov.geneticMovement = genVel.Vels;


        }
        
    }
    // all bellow is for ui to change all sentinels values 
    public void setRange(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoPlayer kuramoto = sentinels[i].GetComponent<KuramotoPlayer>();
            kuramoto.couplingRange = range;

        }
    }

    public void setCoupling(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoPlayer kuramoto = sentinels[i].GetComponent<KuramotoPlayer>();
            kuramoto.coupling = range;

        }
    }

    public void setNoise(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoPlayer kuramoto = sentinels[i].GetComponent<KuramotoPlayer>();
            kuramoto.noiseScl = range;

        }
    }
}
