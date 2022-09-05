using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasticManager : MonoBehaviour
{
    [Tooltip("The gameobject for each agent in this manager")]
    [SerializeField]
    private GameObject plastic; // holds the sentinel prefab
    [Tooltip("Number of the agents to be produced by this manager")]
    [Range(1, 3000)]
    [SerializeField]
    public int nPlastic = 2; // number of them to be made
    
    [Tooltip("radius to be spawned in from this obects transform")]
    [Range(0.1f, 1000f)]
    [SerializeField]
    private float spawnArea = 1.0f; // area to spawn in 
    [Tooltip("Kuramoto speed, measured in bpm, x=min y=max")]
    [SerializeField]
    private Vector2 speedRange = new Vector2(80, 100); // variation of speed for them to have
    [Tooltip("Kuramoto, range for the max distance for the effect, x=min y=max")]
    [SerializeField]
    private Vector2 couplingRange = new Vector2(1, 10); // coupling range to have
    [Tooltip("Kuramoto, range for noise effect, x=min y=max")]
    [SerializeField]
    private Vector2 noiseSclRange = new Vector2(0.01f, 0.5f); // noise Scl to have
    [Tooltip("Kuramoto, range for the strength of the coupling effect, x=min y=max")]
    [SerializeField]
    private Vector2 couplingSclRange = new Vector2(0.2f, 10f); // coupling scl
    [SerializeField]
    private Vector2 attractionSclRange = new Vector2(0.2f, 1f); // coupling scl

    [HideInInspector]
    public GameObject[] plastics; //list to hold the sentinels
    [HideInInspector]
    public GPUData[] GPUStruct; // list of struct ot hold data, maybe for gpu acceleration

   // public List<GenVel> GenVelLib; // lib to hold the gene move data

    private List<Genetics.GenVel> GenVelLib; // list of the GenVel data to act as the library

    private List<Genetics.GenKurmto> GenKurLib;// list of the GenKurmto data to act as the library

    [SerializeField]
    private float speedScl = 3f;

    [SerializeField]
    private float age = 100;

    // struct to hold data maybe for gpu acceleration
    public struct GPUData
    {
        public float age;
        public float connections;
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
            played = 1;
        }





    }


    // resets the sentinel
    public void ResetSentinel(int i)
    {
        // get the sentinel
        GameObject thisSentinel = plastics[i];

        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnArea;

       
        thisSentinel.GetComponent<Rigidbody>().position = pos;

        // lib count is bellow 500
      
            // reset bothe genetic values to random
            KuramotoPlasticAgent kuramoto = thisSentinel.GetComponent<KuramotoPlasticAgent>();
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);

            GeneticMovementPlastic genVel = thisSentinel.GetComponent<GeneticMovementPlastic>();
            genVel.Reset();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        // create list to hold object
        plastics = new GameObject[nPlastic];
        // create list to hold data structs
        GPUStruct = new GPUData[nPlastic];
        // create the two lib lists
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();
        // GenVelLib = new List<GenVel>();

        // loop over the nsentinels
        for (int i = 0; i < nPlastic; i++)
        {
            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnArea;


            // instantiate a new sentinel as child and at pos
            GameObject thisSentinel = Instantiate(plastic, pos, Quaternion.identity, this.transform);

            // get its kurmto component
            KuramotoPlasticAgent kuramoto = thisSentinel.GetComponent<KuramotoPlasticAgent>();
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them

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

            plastics[i].GetComponent<Rigidbody>().velocity += GPUStruct[i].vel * speedScl;
            GPUStruct[i].pos = plastics[i].transform.position;

            // if older than age 
            
            if (kuramoto.dead || kuramoto.age > age) {
                

                // add data to lib
                Genetics.GenKurmto genKurm = new Genetics.GenKurmto(kuramoto.speedBPM, kuramoto.noiseScl, kuramoto.coupling, kuramoto.couplingRange, kuramoto.attractionSclr, kuramoto.fitness);
                GenKurLib.Add(genKurm);
                Genetics.GenVel vels = new Genetics.GenVel(plastics[i].GetComponent<GeneticMovementPlastic>().geneticMovement, kuramoto.fitness);
                GenVelLib.Add(vels);
                
                // reset its values
                ResetSentinel(i);

                GPUStruct[i].SetFromKuramoto(kuramoto);
                GPUStruct[i].pos = plastics[i].transform.position;
            }
            
            // if the lib is greater than ...
            if (GenVelLib.Count > 1000)
            {
                // reorder the lib by fitness

                GenVelLib = Genetics.NegativeSelection(GenVelLib);
                GenKurLib = Genetics.NegativeSelection(GenKurLib);
                // remove the first 250
                GenVelLib.RemoveRange(0, 250);
                GenKurLib.RemoveRange(0, 250);

            }
        }

    }

}
