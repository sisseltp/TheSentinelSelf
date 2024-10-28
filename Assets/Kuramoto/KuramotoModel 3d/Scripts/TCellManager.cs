using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TCellManager : MonoBehaviour
{
    public AgentsManagerParameters parameters;

    [Tooltip("The gameobject for each agent in this manager")]
    [SerializeField]
    private GameObject[] tCell; // holds the sentinel prefab
    
    private int MaxSentinels = 15;
    [HideInInspector]
    public int RealNumSentinels = 0;
    [HideInInspector]
    public GameObject[] sentinels; //list to hold the sentinels
    [HideInInspector]
    public GPUCompute.GPUData[] GPUStruct; // list of struct ot hold data, maybe for gpu acceleration
    public GPUCompute.GPUOutput[] GPUOutput;

    private List<Genetics.GenVel> GenVelLib; // lib to hold the gene move data

    private List<Genetics.GenKurmto> GenKurLib; // lib to hold gene kurmto data

    private List<Genetics.Antigen> antigenLib;

    [SerializeField]
    private float emitionRate = 4;

    void Start()
    {
        MaxSentinels = Mathf.FloorToInt(parameters.nSentinels * 1.5f);


        // create list to hold object
        sentinels = new GameObject[MaxSentinels];
        // create list to hold data structs
        GPUStruct = new GPUCompute.GPUData[MaxSentinels];
        // create the two lib lists
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();
        antigenLib = new List<Genetics.Antigen>();

        GPUOutput = new GPUCompute.GPUOutput[MaxSentinels];

        // loop over the nsentinels
        for (int i=0; i< parameters.nSentinels; i++)
        {

            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere* parameters.spawnArea;

            int randindx = UnityEngine.Random.Range(0, tCell.Length);

            // instantiate a new sentinel as child and at pos
            GameObject thisSentinel = Instantiate(tCell[randindx], pos, Quaternion.identity, this.transform);

            // get its kurmto component
            KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
            kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them

            thisSentinel.GetComponent<GeneticAntigenKey>().Reset();

            // add the object to the list
            sentinels[i] = thisSentinel;

            // set data in the struct
            GPUStruct[i].SetFromKuramoto(kuramoto);
            GPUStruct[i].pos = sentinels[i].transform.position;
            GPUOutput[i].Setup();


        }
        RealNumSentinels = parameters.nSentinels;

        StartCoroutine(emition(emitionRate));
    }
    IEnumerator emition( float rate)
    {
        while (true)
        {
            if (CanAddCell())
            {
                Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;

                int randindx = UnityEngine.Random.Range(0, tCell.Length);

                // instantiate a new sentinel as child and at pos
                GameObject thisSentinel = Instantiate(tCell[randindx], pos, Quaternion.identity, this.transform);

                // get its kurmto component
                KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
                kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them

                AddTCell(thisSentinel);
            }

            yield return new WaitForSeconds(rate);

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
            KuramotoAffectedAgent kuramoto = sentinels[i].GetComponent<KuramotoAffectedAgent>();
            
            // if older than age 
            if (kuramoto.dead || kuramoto.age > parameters.MaxAge) {
                if (i > parameters.nSentinels)
                {

                    toRemove.Add(i);
                    
                    continue;
                }


                // reset its values
                ResetSentinel(i);
                

            }
            else
            {
                if (kuramoto.played == 3)
                {
                    kuramoto.age += Time.deltaTime*10;

                }
                else
                {
                    kuramoto.age += Time.deltaTime;
                }
                kuramoto.phase += GPUOutput[i].phaseAdition * Time.deltaTime;
                if (kuramoto.phase > 1) { kuramoto.phase = kuramoto.phase-1; }
                GPUStruct[i].played = kuramoto.played;
                GPUStruct[i].phase = kuramoto.phase;

                sentinels[i].GetComponent<Rigidbody>().AddForceAtPosition(GPUOutput[i].vel * parameters.speedScl * Time.deltaTime * kuramoto.phase, sentinels[i].transform.position + sentinels[i].transform.up);

                GPUStruct[i].pos = sentinels[i].transform.position;

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
        
        for ( int i=0; i<toRemove.Count; i++)
        {
            int indx = toRemove[i];
            Destroy(sentinels[indx]);

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
            GPUStruct[nxtIndx] = new GPUCompute.GPUData();
            sentinels[nxtIndx] = null;

        }

    }
   
    public void ResetSentinel(int i ,bool genOn = false)
    {

        // get i sentinel
        GameObject thisSentinel = sentinels[i];

        thisSentinel.GetComponent<Renderer>().material.SetFloat("KeyTrigger", 0);

        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;

        thisSentinel.transform.position = pos;

        if (!genOn)
        {
            // reset bothe genetic values to random
            KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
            kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);

            GeneticMovementTcell genVel = thisSentinel.GetComponent<GeneticMovementTcell>();
            genVel.Reset();

            thisSentinel.GetComponent<GeneticAntigenKey>().Reset();

        }
        else if (GenKurLib.Count < 500)
        {
            KuramotoAffecterAgent kuramoto = thisSentinel.GetComponent<KuramotoAffecterAgent>();


            // add it settings to the librarys
            Genetics.GenKurmto genKurm = new Genetics.GenKurmto(kuramoto.speedBPM, kuramoto.noiseScl, kuramoto.coupling, kuramoto.couplingRange, kuramoto.attractionSclr, kuramoto.fitness);
            GenKurLib.Add(genKurm);
            Genetics.GenVel vels = new Genetics.GenVel(sentinels[i].GetComponent<GeneticMovementSentinel>().geneticMovement, kuramoto.fitness);
            GenVelLib.Add(vels);
            // add random new sentinel
            kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them
            
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

            KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
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

    public bool CanAddCell()
    {
        if (RealNumSentinels < MaxSentinels - 1)
        {
            return true;
        }
        return false;
    }

    public void AddTCell(GameObject TCell)
    {
        
            RealNumSentinels++;


            // add the object to the list
            sentinels[RealNumSentinels-1] = TCell;
            KuramotoAffectedAgent kuramoto = TCell.GetComponent<KuramotoAffectedAgent>();
            TCell.GetComponent<GeneticAntigenKey>().Reset();
            // set data in the struct
            GPUCompute.GPUData gpuStruct = new GPUCompute.GPUData();
            gpuStruct.SetFromKuramoto(kuramoto);
            gpuStruct.pos = TCell.transform.position;
            GPUStruct[RealNumSentinels] = gpuStruct;
        

    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 1f);
    }
#endif

}
