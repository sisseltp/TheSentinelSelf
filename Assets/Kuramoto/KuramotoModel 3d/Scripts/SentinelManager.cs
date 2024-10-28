using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentinelManager : MonoBehaviour
{
    public AgentsManagerParameters parameters;

    [Tooltip("Main sentinel agent")]
    [SerializeField]
    private GameObject prefabSentinel;
   
    [Tooltip("Number of agents to produce")]
    [Range(1, 3000)]
    [SerializeField]
    public int nSentinels = 10; // number of sentinels to be made
    [Tooltip("radius of area to be produced in")]
    [Range(0.1f, 100f)]
    [SerializeField]
    private float spawnArea = 1.0f; // area to be spawned in

    [Range(0f, 20f)]
    [SerializeField]
    private float speedScl = 2;

    [Tooltip("Kuramoto speed, measured in bpm, x=min y=max")]
    [SerializeField]
    private Vector2 speedRange = new Vector2(0, 1); // variation of speed for them to have
    [Tooltip("Kuramoto, range for the max distance for the effect, x=min y=max")]
    [SerializeField]
    private Vector2 couplingRange = new Vector2(1, 10); // coupling range to have
    [Tooltip("Kuramoto, range for noise effect, x=min y=max")]
    [SerializeField]
    private Vector2 noiseSclRange = new Vector2( 0.01f, 0.5f); // noise Scl to have
    [Tooltip("Kuramoto, range for the strength of the coupling effect, x=min y=max")]
    [SerializeField]
    private Vector2 couplingSclRange = new Vector2(0.2f,10f); // coupling scl
    [Tooltip("Kuramoto, range for the scaling the clustering/attraction effect, x=min y=max")]
    [SerializeField]
    private Vector2 attractionSclRange = new Vector2(0.2f, 1f); // coupling scl

    [HideInInspector]
    public GameObject[] sentinels; // list of the sentinel object
    [HideInInspector]
    public GPUCompute.GPUData[] GPUStruct; // list of sentinel struct, that will hold the data for gpu compute
    public GPUCompute.GPUOutput[] GPUOutput;

    private List<Genetics.GenVel> GenVelLib; // list of the GenVel data to act as the library

    private List<Genetics.GenKurmto> GenKurLib;// list of the GenKurmto data to act as the library
    [Tooltip("Max age the agents will reach")]
    [SerializeField]
    private float MaxAge = 1000; // age limit to kill sentinels

    [HideInInspector]
    public Vector3[] Lymphondes;
    [HideInInspector]
    public Vector3[] PathogenEmitters;
    [HideInInspector]
    public PathogenManager[] pathogenManagers;
    [HideInInspector]
    public TCellManager[] tcellManagers;


    // struct to hold all the sentinels data potential gpu compute
   


    // Start is called before the first frame update
    void Start()
    {
        sentinels = new GameObject[nSentinels];
        GPUStruct = new GPUCompute.GPUData[nSentinels];
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();
        GPUOutput = new GPUCompute.GPUOutput[nSentinels];

        for (int i=0; i<nSentinels; i++)
        {

            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnArea;

            GameObject newSentinel = Instantiate(prefabSentinel, pos, Quaternion.identity, this.transform);
            
            // get the kuramoto sentinel
            KuramotoAffecterAgent kuramoto = newSentinel.GetComponent<KuramotoAffecterAgent>();
            kuramoto.Setup(noiseSclRange,couplingRange,speedRange, couplingSclRange, attractionSclRange,  0.2f);// setup its setting to randomize them

            sentinels[i] = newSentinel; // set the sentinel object to the list


            GPUStruct[i].SetFromKuramoto(kuramoto);
            GPUStruct[i].pos = sentinels[i].transform.position;

            GPUOutput[i].Setup();

        }

        GameObject[] lymphs = GameObject.FindGameObjectsWithTag("Lymphonde");
        Lymphondes = new Vector3[lymphs.Length];
        tcellManagers = new TCellManager[lymphs.Length];
        for (int i = 0; i < lymphs.Length; i++)
        {
            tcellManagers[i] = lymphs[i].GetComponent<TCellManager>();
            Lymphondes[i] = lymphs[i].transform.position;
        }

        GameObject[] pathogens = GameObject.FindGameObjectsWithTag("PathogenEmitter");
        PathogenEmitters = new Vector3[pathogens.Length];
        pathogenManagers = new PathogenManager[pathogens.Length];

        for (int i = 0; i < pathogens.Length; i++)
        {
            PathogenEmitters[i] = pathogens[i].transform.position;
            pathogenManagers[i] = pathogens[i].GetComponent<PathogenManager>();
        }


    }

    private void Update()
    {
        
        for (int i = 0; i < nSentinels; i++)
        {
            // get the components kuramoto component
            KuramotoAffecterAgent kuramoto = sentinels[i].GetComponent<KuramotoAffecterAgent>();
            

            // if the agent is dead
            if (kuramoto.dead || kuramoto.age> MaxAge) {

                
                // call the reset function
                ResetSentinel(i);

                GPUStruct[i].SetFromKuramoto(kuramoto);
                GPUStruct[i].pos = sentinels[i].GetComponent<Rigidbody>().position;
            }
            else
            {
                kuramoto.age += Time.deltaTime;
                kuramoto.phase += GPUOutput[i].phaseAdition * Time.deltaTime;
                if (kuramoto.phase > 1) { kuramoto.phase = kuramoto.phase - 1; }
                GPUStruct[i].phase = kuramoto.phase;

                sentinels[i].GetComponent<Rigidbody>().AddForceAtPosition(GPUOutput[i].vel * speedScl * Time.deltaTime * kuramoto.phase, sentinels[i].transform.position + sentinels[i].transform.up);


                GPUStruct[i].speed = sentinels[i].GetComponent<KuramotoAffecterAgent>().speed;
                GPUStruct[i].pos = sentinels[i].GetComponent<Rigidbody>().position;
            }

            Renderer rendr = sentinels[i].GetComponentInChildren<Renderer>();
            if (rendr.isVisible)
            {
                //float oscil = Mathf.Sin((cohPhi - phase) * (2 * Mathf.PI));
                //rendr.material.color = Color.Lerp(col0, col1, phase);
                rendr.material.SetFloat("Phase", kuramoto.phase);
            }
        }

        /*
        // if lib is greater than ...
        if (GenVelLib.Count > 1000)
        {
            // reorder on fitness
            
            // negative selection
            GenVelLib = Genetics.NegativeSelection(GenVelLib);
            GenKurLib = Genetics.NegativeSelection(GenKurLib);
            

        }
        */
    }

    // resets the sentinel
    public void ResetSentinel(int i, bool genOn= false)
    {

        // get the sentinel
        GameObject thisSentinel = sentinels[i];

        

        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnArea;

        thisSentinel.transform.position = pos;

        thisSentinel.GetComponent<Fosilising>().enabled = false;
        thisSentinel.SetActive(true);

        if (!genOn)
        {
            // reset bothe genetic values to random
            KuramotoAffecterAgent kuramoto = thisSentinel.GetComponent<KuramotoAffecterAgent>();
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);

            GeneticMovementSentinel genVel = thisSentinel.GetComponent<GeneticMovementSentinel>();
            genVel.Reset();
        }
        else if (GenKurLib.Count < 500)
        {
            KuramotoAffecterAgent kuramoto = thisSentinel.GetComponent<KuramotoAffecterAgent>();


            // add it settings to the librarys
            Genetics.GenKurmto genKurm = new Genetics.GenKurmto(kuramoto.speedBPM, kuramoto.noiseScl, kuramoto.coupling, kuramoto.couplingRange, kuramoto.attractionSclr, kuramoto.fitness);
            GenKurLib.Add(genKurm);
            Genetics.GenVel vels = new Genetics.GenVel(sentinels[i].GetComponent<GeneticMovementSentinel>().geneticMovement, kuramoto.fitness);
            GenVelLib.Add(vels);

            // reset bothe genetic values to random
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

            KuramotoAffecterAgent kuramoto = thisSentinel.GetComponent<KuramotoAffecterAgent>();
            kuramoto.SetupData(Settings);
           

            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel1 = GenVelLib[rand];
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel2 = GenVelLib[rand];

            Vector3[] Vels = genVel2.BlendAttributes(genVel1.Vels);

            GeneticMovementSentinel genMov = thisSentinel.GetComponent<GeneticMovementSentinel>();
            genMov.Reset();
            genMov.geneticMovement = Vels;

            
        }
        
    }

    // all bellow is for ui to change all sentinels values 
    public void setRange(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoAffecterAgent kuramoto = sentinels[i].GetComponent<KuramotoAffecterAgent>();
            kuramoto.couplingRange = range;

        }
    }

    public void setCoupling(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoAffecterAgent kuramoto = sentinels[i].GetComponent<KuramotoAffecterAgent>();
            kuramoto.coupling = range;

        }
    }

    public void setNoise(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoAffecterAgent kuramoto = sentinels[i].GetComponent<KuramotoAffecterAgent>();
            kuramoto.noiseScl = range;

        }

        JsonUtility.ToJson(GenVelLib);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 1f);
    }
#endif
}
