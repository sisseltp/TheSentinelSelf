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
    [SerializeField]
    private Vector2 attractionSclRange = new Vector2(0.2f, 1f); // coupling scl

    [HideInInspector]
    public GameObject[] sentinels; // list of the sentinel object

    public GPUData[] GPUStruct; // list of sentinel struct, that will hold the data for gpu compute

    public List<Genetics.GenVel> GenVelLib; // list of the GenVel data to act as the library

    public List<Genetics.GenKurmto> GenKurLib;// list of the GenKurmto data to act as the library

    [SerializeField]
    private float MaxAge = 1000; // age limit to kill sentinels

    // struct to hold all the sentinels data potential gpu compute
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

        public void SetFromKuramoto(KuramotoSentinelAgent kuramoto)
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
        GPUStruct = new GPUData[nSentinels];
        // create the two libs as lists
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();

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
            kuramoto.Setup(noiseSclRange,couplingRange,speedRange, couplingSclRange, attractionSclRange,  0.2f);// setup its setting to randomize them

            sentinels[i] = thisSentinel; // set the sentinel object to the list


            GPUStruct[i].SetFromKuramoto(kuramoto);
            GPUStruct[i].pos = sentinels[i].transform.position;

        }


    }

    private void Update()
    {
        
        for (int i = 0; i < nSentinels; i++)
        {
            // get the components kuramoto component
            KuramotoSentinelAgent kuramoto = sentinels[i].GetComponent<KuramotoSentinelAgent>();
            

            // if the agent is dead
            if (kuramoto.dead || GPUStruct[i].age> MaxAge) {
                // add it settings to the librarys
                Genetics.GenKurmto genKurm = new Genetics.GenKurmto(kuramoto.speedBPM, kuramoto.noiseScl, kuramoto.coupling, kuramoto.couplingRange, kuramoto.attractionSclr, kuramoto.fitness);
                GenKurLib.Add(genKurm);
                Genetics.GenVel vels = new Genetics.GenVel(sentinels[i].GetComponent<GeneticMovementSentinel>().geneticMovement, kuramoto.fitness);
                GenVelLib.Add(vels);
                // call the reset function
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
                //sentinels[i].GetComponent<Rigidbody>().velocity += sentinelsStruct[i].vel;
                GPUStruct[i].pos = sentinels[i].transform.position;
            }

        }


        // if lib is greater than ...
        if (GenVelLib.Count > 1000)
        {
            // reorder on fitness
            Debug.Log("Resize");
            Debug.Log(GenVelLib[0].fitness);
            // negative selection
            GenVelLib = Genetics.NegativeSelection(GenVelLib);
            GenKurLib = Genetics.NegativeSelection(GenKurLib);
            Debug.Log(GenVelLib[0].fitness);

        }
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
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);

            GeneticMovementSentinel genVel = thisSentinel.GetComponent<GeneticMovementSentinel>();
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

            KuramotoSentinelAgent kuramoto = thisSentinel.GetComponent<KuramotoSentinelAgent>();
            kuramoto.SetupData(Settings);
           

            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel1 = GenVelLib[rand];
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel2 = GenVelLib[rand];

            Vector3[] Vels = genVel2.BlendAttributes(genVel1.Vels);

            GeneticMovementSentinel genMov = thisSentinel.GetComponent<GeneticMovementSentinel>();
            genMov.Reset();
            genMov.geneticMovement = genVel1.BlendAttributes(Vels);


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
