using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasticManager : MonoBehaviour
{

    [SerializeField]
    private GameObject plastic; // holds the sentinel prefab
    [Range(1, 3000)]
    [SerializeField]
    public int nPlastic = 2; // number of them to be made

    [Range(0.1f, 1000f)]
    [SerializeField]
    private float spawnArea = 1.0f; // area to spawn in 
    [SerializeField]
    private Vector2 speedRange = new Vector2(80, 100); // variation of speed for them to have
    [SerializeField]
    private Vector2 couplingRange = new Vector2(1, 10); // coupling range to have
    [SerializeField]
    private Vector2 noiseSclRange = new Vector2(0.01f, 0.5f); // noise Scl to have
    [SerializeField]
    private Vector2 couplingSclRange = new Vector2(0.2f, 10f); // coupling scl

    [HideInInspector]
    public GameObject[] plastics; //list to hold the sentinels

    public GPUData[] GPUStruct; // list of struct ot hold data, maybe for gpu acceleration

   // public List<GenVel> GenVelLib; // lib to hold the gene move data

    public List<GenKurmto> GenKurLib; // lib to hold gene kurmto data

    [SerializeField]
    private float age = 1000; // age limit to kill sentinels

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




        public void SetFromKuramoto(KuramotoPlasticAgent kuramoto)
        {


            speed = kuramoto.speed;
            phase = kuramoto.phase;
            //cohPhi = kuramoto.cohPhi;
            coherenceRadius = kuramoto.coherenceRadius;
            couplingRange = kuramoto.couplingRange;
            noiseScl = kuramoto.noiseScl;
            coupling = kuramoto.coupling;
            //connections = kuramoto.Connections;

        }





    }
    /*
    // struct to hold the genetic move data
    public struct GenVel
    {

        public GenVel(Vector3[] vels, float fit=0)
        {
            Vels = vels;
            fitness = fit;
        }

        public Vector3[] BlendAttributes(Vector3[] otherVels)
        {
            Vector3[] newVels = new Vector3[Vels.Length];
             for(int i=0; i < newVels.Length; i++)
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
                }

            }

            

            return newVels;
        }

        public Vector3[] Vels;
        public float fitness;

    }
    */

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
        // create list to hold object
        plastics = new GameObject[nPlastic];
        // create list to hold data structs
        GPUStruct = new GPUData[nPlastic];
        // create the two lib lists
        GenKurLib = new List<GenKurmto>();
        // GenVelLib = new List<GenVel>();

        // loop over the nsentinels
        for (int i = 0; i < nPlastic; i++)
        {
            // set rand pos
            float x = UnityEngine.Random.Range(-spawnArea, spawnArea);
            float y = 0;
            float z = UnityEngine.Random.Range(-spawnArea, spawnArea);

            Vector3 pos = new Vector3(x, y, z);

            // instantiate a new sentinel as child and at pos
            GameObject thisSentinel = Instantiate(plastic, pos, Quaternion.identity, this.transform);

            // get its kurmto component
            KuramotoPlasticAgent kuramoto = thisSentinel.GetComponent<KuramotoPlasticAgent>();
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, 0.2f);// setup its setting to randomize them

            // add the object to the list
            plastics[i] = thisSentinel;

            // set data in the struct
            GPUStruct[i].SetFromKuramoto(kuramoto);
            GPUStruct[i].pos = plastics[i].transform.position;
        }


    }

    private void Update()
    {
        // loop over the n sentinels
        for (int i = 0; i < nPlastic; i++)
        {
            // get the kurmto
            KuramotoPlasticAgent kuramoto = plastics[i].GetComponent<KuramotoPlasticAgent>();
            kuramoto.phase = GPUStruct[i].phase;

            // plastics[i].GetComponent<Rigidbody>().velocity += GPUStruct[i].vel* Time.deltaTime * speedScl;
            GPUStruct[i].pos = plastics[i].transform.position;

            // if older than age 
            /*
            if (kuramoto.dead || kuramoto.age > age) {
                // add data to lib
                GenKurmto genKurm = new GenKurmto(kuramoto.speedBPM, kuramoto.noiseScl, kuramoto.coupling, kuramoto.couplingRange, kuramoto.fitness);
                GenKurLib.Add(genKurm);
                GenVel vels = new GenVel(plastics[i].GetComponent<GeneticMovementBiome>().geneticMovement, kuramoto.fitness);
                GenVelLib.Add(vels);
                // reset its values
                ResetSentinel(i);

                plasticData[i].SetFromKuramoto(kuramoto);
                plasticData[i].pos = plastics[i].transform.position;
            }
            
            // if the lib is greater than ...
            if (GenVelLib.Count > 1000)
            {
                // reorder the lib by fitness
                
                int indx = GenVelLib.Count;
                GenVelLib.Sort(SortByScore);
                GenKurLib.Sort(SortByScore);
                // remove the first 250
                GenVelLib.RemoveRange(0, 250);
                GenKurLib.RemoveRange(0, 250);


            }
            */
        }

    }

}
