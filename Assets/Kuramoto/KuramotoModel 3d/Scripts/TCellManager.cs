using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TCellManager : MonoBehaviour
{

    [SerializeField]
    private GameObject sentinel; // holds the sentinel prefab
    [Range(1, 3000)]
    [SerializeField]
    public int nSentinels = 10; // number of them to be made
    private int MaxSentinels = 15;
    private int RealNumSentinels = 0;

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

    public BiomeManager.GPUData[] GPUStruct; // list of struct ot hold data, maybe for gpu acceleration
    
    public List<Genetics.GenVel> GenVelLib; // lib to hold the gene move data

    public List<Genetics.GenKurmto> GenKurLib; // lib to hold gene kurmto data

    public List<Genetics.Antigen> antigenLib;

    [SerializeField]
    private float MaxAge = 1000; // age limit to kill sentinels

    [SerializeField]
    private float speedScl = 3f;

    [SerializeField]
    private float yOffset = 5;

    
    // Start is called before the first frame update
    void Start()
    {
        MaxSentinels = Mathf.FloorToInt(nSentinels * 1.5f);

        // create list to hold object
        sentinels = new GameObject[MaxSentinels];
        // create list to hold data structs
        GPUStruct = new BiomeManager.GPUData[MaxSentinels];
        // create the two lib lists
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();
        antigenLib = new List<Genetics.Antigen>();



        // loop over the nsentinels
        for (int i=0; i<nSentinels; i++)
        {

            RealNumSentinels++;

            // set rand pos
            float x = transform.position.x + UnityEngine.Random.Range(-spawnArea, spawnArea);
            float y = transform.position.y + yOffset;
            float z = transform.position.z + UnityEngine.Random.Range(-spawnArea, spawnArea);

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
        List<int> toRemove = new List<int>();
        // loop over the n sentinels
        for (int i = 0; i < RealNumSentinels; i++)
        {

            if(sentinels[i] == null) { continue; }
            // get the kurmto
            KuramotoBiomeAgent kuramoto = sentinels[i].GetComponent<KuramotoBiomeAgent>();
            
            // if older than age 
            if (kuramoto.dead || GPUStruct[i].age > MaxAge) {

                if (i > nSentinels)
                {
                    toRemove.Add(i);
                    
                    continue;
                }

                // add data to lib
                Genetics.GenKurmto genKurm = new Genetics.GenKurmto(kuramoto.speedBPM, kuramoto.noiseScl, kuramoto.coupling, kuramoto.couplingRange, kuramoto.attractionSclr, kuramoto.fitness);
                GenKurLib.Add(genKurm);
                Genetics.GenVel vels = new Genetics.GenVel(sentinels[i].GetComponent<GeneticMovementTcell>().geneticMovement, kuramoto.fitness);
                GenVelLib.Add(vels);

                antigenLib.Add(sentinels[i].GetComponent<GeneticAntigenKey>().antigen);

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
            GPUStruct[nxtIndx] = new BiomeManager.GPUData();
            sentinels[nxtIndx] = null;

        }

    }
    /*
    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        for (int i = 0; i < nSentinels; i++)
        {

            Gizmos.DrawSphere(sentinels[i].transform.position + sentinels[i].transform.up, 0.5f);
        }
    }
    */
    // resets the i sentinel
    public void ResetSentinel(int i)
    {

        // get i sentinel
        GameObject thisSentinel = sentinels[i];

        // set rand pos
        float x = transform.position.x + UnityEngine.Random.Range(-spawnArea, spawnArea);
        float y = transform.position.y + yOffset;
        float z = transform.position.z + UnityEngine.Random.Range(-spawnArea, spawnArea);

        Vector3 pos = new Vector3(x, y, z);

        thisSentinel.transform.position = pos;

        // if lib less than 500
        if (GenKurLib.Count < 500)
        {
            // add random new sentinel
            KuramotoBiomeAgent kuramoto = thisSentinel.GetComponent<KuramotoBiomeAgent>();
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them
            
            GeneticMovementTcell genVel = thisSentinel.GetComponent<GeneticMovementTcell>();
            genVel.Reset();

            thisSentinel.GetComponent<GeneticAntigenKey>().Reset();
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

            GeneticMovementTcell genMov = thisSentinel.GetComponent<GeneticMovementTcell>();
            genMov.Reset();
            genMov.geneticMovement = genVel1.BlendAttributes(Vels);

            GeneticAntigenKey antigenKey = thisSentinel.GetComponent<GeneticAntigenKey>();
            antigenKey.Reset();



        }
        
    }

    public void AddTCell(GameObject TCell)
    {
        if (RealNumSentinels < MaxSentinels)
        {
            RealNumSentinels++;


            // add the object to the list
            sentinels[RealNumSentinels-1] = TCell;
            KuramotoBiomeAgent kuramoto = TCell.GetComponent<KuramotoBiomeAgent>();

            // set data in the struct
            BiomeManager.GPUData gpuStruct = new BiomeManager.GPUData();
            gpuStruct.SetFromKuramoto(kuramoto);
            gpuStruct.SetPos(TCell.transform.position);
            GPUStruct[RealNumSentinels] = gpuStruct;
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