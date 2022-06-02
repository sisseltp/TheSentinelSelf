using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentinelManager : MonoBehaviour
{

    [SerializeField]
    private GameObject sentinel; // the sentinel game object
    [SerializeField]
    private GameObject sentinelCam; // the sentinel game object
    [Range(1, 3000)]
    [SerializeField]
    public int nSentinels = 10; // number of sentinels to be made

    [Range(0.1f, 100f)]
    [SerializeField]
    private float spawnArea = 1.0f; // area to be spawned in
    [SerializeField]
    private Vector2 speedRange = new Vector2(0, 1); // variation of speed for them to have
    [SerializeField]
    private Vector2 couplingRange = new Vector2(1, 10); // coupling range to have
    [SerializeField]
    private Vector2 noiseSclRange = new Vector2( 0.01f, 0.5f); // noise Scl to have
    [SerializeField]
    private Vector2 couplingSclRange = new Vector2(0.2f,10f); // coupling scl

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
        public int Connections;
        public Vector3 vel;
        public Vector3[] GenVel;
    }
    // struct to hold the Genvels settings and fitness
    public struct GenVel
    {

        public GenVel(Vector3[] vels, float fit = 0)
        {
            Vels = vels;
            fitness = fit;
        }

        public Vector3[] BlendAttributes(Vector3[] otherVels)
        {
            Vector3[] newVels = new Vector3[Vels.Length];
            for (int i = 0; i < newVels.Length; i++)
            {

                float rand = UnityEngine.Random.value;

                if (rand < 0.33f)
                {
                    newVels[i] = Vels[i];
                }
                else if (rand < 0.66f)
                {
                    newVels[i] = otherVels[i];
                }
                else
                {
                    newVels[i] = UnityEngine.Random.insideUnitSphere;
                    newVels[i].y = Mathf.Abs(newVels[i].y);
                }
            }



            return newVels;
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
        public GenKurmto(float speedBPM, float noiseScl, float coupling, float couplingRange, float fit)
        {
            Settings = new float[4];
            Settings[0] = speedBPM;
            Settings[1] = noiseScl;
            Settings[2] = coupling;
            Settings[3] = couplingRange;
            fitness = fit;
        }

        public float[] BlendAttributes(float[] otherSettings)
        {
            float[] newSetting = new float[Settings.Length];
            for (int i = 0; i < newSetting.Length; i++)
            {

                float rand = UnityEngine.Random.value;

                if (rand < 0.5f)
                {
                    newSetting[i] = Settings[i];
                }
                else
                {
                    newSetting[i] = otherSettings[i];
                }


            }

            return newSetting;
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
            GameObject thisSentinel;

            if (i == 0) {
                thisSentinel = Instantiate(sentinelCam, pos, Quaternion.identity, this.transform);
            }
            else
            {
                thisSentinel = Instantiate(sentinel, pos, Quaternion.identity, this.transform);
            }
            // get the kuramoto sentinel
            KuramotoSentinelAgent kuramoto = thisSentinel.GetComponent<KuramotoSentinelAgent>();
            kuramoto.Setup(noiseSclRange,couplingRange,speedRange, couplingSclRange, 0.2f);// setup its setting to randomize them

            sentinels[i] = thisSentinel; // set the sentinel object to the list

            // set the values of the struct from the sentinel
            sentinelsStruct[i].speed = kuramoto.speed;
            sentinelsStruct[i].phase = kuramoto.phase;
            sentinelsStruct[i].cohPhi = kuramoto.cohPhi;
            sentinelsStruct[i].coherenceRadius = kuramoto.coherenceRadius;
            sentinelsStruct[i].couplingRange = kuramoto.couplingRange;
            sentinelsStruct[i].noiseScl = kuramoto.noiseScl;
            sentinelsStruct[i].coupling = kuramoto.coupling;
            sentinelsStruct[i].Connections = kuramoto.Connections;
            sentinelsStruct[i].vel = thisSentinel.GetComponent<Rigidbody>().velocity;
            sentinelsStruct[i].GenVel = thisSentinel.GetComponent<GeneticMovementSentinel>().geneticMovement;
           

        }


    }

    private void Update()
    {
        
        for (int i = 0; i < nSentinels; i++)
        {
            // get the components kuramoto component
            KuramotoSentinelAgent kuramoto = sentinels[i].GetComponent<KuramotoSentinelAgent>();

            // set the values of the struct from the sentinel
            sentinelsStruct[i].speed = kuramoto.speed;
            sentinelsStruct[i].phase = kuramoto.phase;
            sentinelsStruct[i].cohPhi = kuramoto.cohPhi;
            sentinelsStruct[i].coherenceRadius = kuramoto.coherenceRadius;
            sentinelsStruct[i].couplingRange = kuramoto.couplingRange;
            sentinelsStruct[i].noiseScl = kuramoto.noiseScl;
            sentinelsStruct[i].coupling = kuramoto.coupling;
            sentinelsStruct[i].Connections = kuramoto.Connections;
            sentinelsStruct[i].vel = sentinels[i].GetComponent<Rigidbody>().velocity;
            sentinelsStruct[i].GenVel = sentinels[i].GetComponent<GeneticMovementSentinel>().geneticMovement;

            // if the agent is dead
            if (kuramoto.dead || kuramoto.age>1000) {
                // add it settings to the librarys
                GenKurmto genKurm = new GenKurmto(kuramoto.speedBPM, kuramoto.noiseScl, kuramoto.coupling, kuramoto.couplingRange, kuramoto.fitness);
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
            KuramotoSentinelAgent kuramoto = thisSentinel.GetComponent<KuramotoSentinelAgent>();
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, 0.2f);

            GeneticMovementSentinel genVel = thisSentinel.GetComponent<GeneticMovementSentinel>();
            genVel.Reset();
        }
        else
        {
            // add random sentinel from lib
            int rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            GenKurmto kurData1 = GenKurLib[rand];
            rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            GenKurmto kurData2 = GenKurLib[rand];

            float[] Settings = kurData1.BlendAttributes(kurData2.Settings);

            KuramotoSentinelAgent kuramoto = thisSentinel.GetComponent<KuramotoSentinelAgent>();
            kuramoto.SetupData(Settings);
           

            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            GenVel genVel1 = GenVelLib[rand];
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            GenVel genVel2 = GenVelLib[rand];

            GeneticMovementSentinel genMov = thisSentinel.GetComponent<GeneticMovementSentinel>();
            genMov.Reset();
            genMov.geneticMovement = genVel1.BlendAttributes(genVel2.Vels);


        }
        
    }
    // all bellow is for ui to change all sentinels values 
    public void setRange(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoSentinelAgent kuramoto = sentinels[i].GetComponent<KuramotoSentinelAgent>();
            kuramoto.couplingRange = range;

        }
    }

    public void setCoupling(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoSentinelAgent kuramoto = sentinels[i].GetComponent<KuramotoSentinelAgent>();
            kuramoto.coupling = range;

        }
    }

    public void setNoise(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoSentinelAgent kuramoto = sentinels[i].GetComponent<KuramotoSentinelAgent>();
            kuramoto.noiseScl = range;

        }

        JsonUtility.ToJson(GenVelLib);
    }


}
