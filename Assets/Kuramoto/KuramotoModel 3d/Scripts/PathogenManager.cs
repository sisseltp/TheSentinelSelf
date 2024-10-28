using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PathogenManager : MonoBehaviour
{
    [Tooltip("The gameobject for each agent in this manager")]
    [SerializeField]
    private GameObject prefabPathogen;
    [Tooltip("Number of the agents to be produced by this manager")]
    [Range(1, 3000)]
    [SerializeField]
    public int nSentinels = 10; // number of them to be made

    [Tooltip("radius to be spawned in from this obects transform")]
    [Range(0.1f, 1000f)]
    [SerializeField]
    private float spawnArea = 1.0f; // area to spawn in 
    [Tooltip("Kuramoto speed, measured in bpm, x=min y=max")]
    [SerializeField]
    private Vector2 speedRange = new Vector2(90, 100); // variation of speed for them to have
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
    [Tooltip("Max age the agents will reach")]
    [SerializeField]
    private float MaxAge = 1000; // age limit to kill sentinels

    [Tooltip("Kuramoto Speed Scaler")]
    [SerializeField]
    private float speedScl = 3f;

    //[HideInInspector]
    public int RealNumPathogens = 0;
    [SerializeField]
    private float emitionTimer = 1.0f;
    private float timeGate = 0;

    // struct to hold data maybe for gpu acceleration
    
    // Start is called before the first frame update
    void Start()
    {
        // create list to hold object
        sentinels = new GameObject[nSentinels];
        // create list to hold data structs
        GPUStruct = new GPUCompute.GPUData[nSentinels];
        // create the two lib lists
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();
        GPUOutput = new GPUCompute.GPUOutput[nSentinels];

        // loop over the nsentinels
        for (int i=0; i< nSentinels; i++)
            AddPathogen(i);
    }

    public void AddPathogen(int i)
    {
        if (nSentinels-2 > RealNumPathogens)
        {
            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnArea;

            // instantiate a new sentinel as child and at pos
            GameObject thisSentinel = Instantiate(prefabPathogen, pos, Quaternion.identity, this.transform);

            // get its kurmto component
            KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them

            thisSentinel.GetComponentInChildren<GeneticAntigenKey>().Reset();

            // add the object to the list
            sentinels[RealNumPathogens] = thisSentinel;

            // set data in the struct
            GPUStruct[RealNumPathogens].SetFromKuramoto(kuramoto);
            GPUStruct[RealNumPathogens].pos = sentinels[i].transform.position;
            GPUOutput[RealNumPathogens].Setup();

            RealNumPathogens++;

        }
    }

    private void DuplicatePathogen( GameObject pathogen, int duplications=2)
    {
        if (nSentinels-2 > RealNumPathogens)
        {
            for (int l = 0; l < duplications; l++)
            {
                // instantiate a new sentinel as child and at pos
                GameObject thisSentinel = Instantiate(pathogen, pathogen.transform.position, pathogen.transform.rotation, this.transform);

                // get its kurmto component
                KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
                kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them

                // add the object to the list
                sentinels[RealNumPathogens] = thisSentinel;

                // set data in the struct
                GPUStruct[RealNumPathogens].SetFromKuramoto(kuramoto);
                GPUStruct[RealNumPathogens].pos = sentinels[RealNumPathogens].transform.position;

                RealNumPathogens++;


                thisSentinel.GetComponentInChildren<GeneticAntigenKey>().antigen = new Genetics.Antigen();

                thisSentinel.GetComponentInChildren<GeneticAntigenKey>().antigen.Key = pathogen.GetComponentInChildren<GeneticAntigenKey>().antigen.Key;


            }
        }
    }

    private void Update()
    {
        if(Time.time > emitionTimer + timeGate && RealNumPathogens < nSentinels)
        {

            timeGate = Time.time;
            AddPathogen(RealNumPathogens);


        }

        List<int> toRemove = new List<int>();

        // loop over the n sentinels
        for (int i = 0; i < RealNumPathogens; i++)
        {
            
            // get the kurmto
            KuramotoAffectedAgent kuramoto = sentinels[i].GetComponent<KuramotoAffectedAgent>();
            
            // if dead remove
            if (kuramoto.dead ) {

                toRemove.Add(i);
            }
            else  if (kuramoto.age > MaxAge )
            {

                kuramoto.age = 0;

                DuplicatePathogen( sentinels[i],1);
                    
               
            }
            else
            {
                
                kuramoto.age += Time.deltaTime;
                kuramoto.phase += GPUOutput[i].phaseAdition * Time.deltaTime;
                if (kuramoto.phase > 1) { kuramoto.phase = kuramoto.phase - 1; }
                GPUStruct[i].phase = kuramoto.phase;

                sentinels[i].GetComponent<Rigidbody>().AddForceAtPosition(GPUOutput[i].vel * speedScl * Time.deltaTime * kuramoto.phase, sentinels[i].transform.position + sentinels[i].transform.up);

                GPUStruct[i].pos = sentinels[i].GetComponent<Rigidbody>().position;

            }
            Renderer rendr = sentinels[i].GetComponent<Renderer>();
            if (rendr.isVisible)
            {
                //float oscil = Mathf.Sin((cohPhi - phase) * (2 * Mathf.PI));
                //rendr.material.color = Color.Lerp(col0, col1, phase);
                rendr.material.SetFloat("Phase", kuramoto.phase);
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

        //loops over agents to remove
        for (int i = 0; i < toRemove.Count; i++)
        {
            // index of the agent to remove
            int indx = toRemove[i];
            Destroy(sentinels[indx]);
            // if its not the last agent in the remove list
            if (i != toRemove.Count - 1)
            {
                // set the next index from the remove list
                nxtIndx = toRemove[i + 1];
            }
            else
            {
                // set the next indx with the limit of agents
                nxtIndx = Mathf.Clamp( RealNumPathogens,0, nSentinels);
            }
            // loop from this indx+1 to the next index  
            for (int p = indx + 1; p <= nxtIndx; p++)
            {
                GPUStruct[p - (i + 1)] = GPUStruct[p];
                sentinels[p - (i + 1)] = sentinels[p];

            }

        }

        RealNumPathogens -= toRemove.Count;
       
        if (nxtIndx != -1)
        {
            GPUStruct[nxtIndx] = new GPUCompute.GPUData();
            sentinels[nxtIndx] = null;

        }
    }

   
   
    public void ResetSentinel(int i, bool genOn = false)
    {

        // get i sentinel
        GameObject thisSentinel = sentinels[i];



        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnArea;

        thisSentinel.transform.position = pos;

        if (!genOn)
        {
            // reset bothe genetic values to random
            KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);

            GeneticMovementPathogen genVel = thisSentinel.GetComponent<GeneticMovementPathogen>();
            genVel.Reset();
        }
        else if (GenKurLib.Count < 500)
        {
            KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();

            // add it settings to the librarys
            Genetics.GenKurmto genKurm = new Genetics.GenKurmto(kuramoto.speedBPM, kuramoto.noiseScl, kuramoto.coupling, kuramoto.couplingRange, kuramoto.attractionSclr, kuramoto.fitness);
            GenKurLib.Add(genKurm);
            Genetics.GenVel vels = new Genetics.GenVel(sentinels[i].GetComponent<GeneticMovementSentinel>().geneticMovement, kuramoto.fitness);
            GenVelLib.Add(vels);

            // add random new sentinel
    
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them

            GeneticMovementPathogen genVel = thisSentinel.GetComponent<GeneticMovementPathogen>();
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

            KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
            kuramoto.SetupData(Settings);

            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel1 = GenVelLib[rand];
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel2 = GenVelLib[rand];

            Vector3[] Vels = genVel2.BlendAttributes(genVel1.Vels);

            GeneticMovementPathogen genMov = thisSentinel.GetComponent<GeneticMovementPathogen>();
            genMov.Reset();
            genMov.geneticMovement = genVel1.BlendAttributes(Vels);


        }
        
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 1f);
    }
#endif


}
