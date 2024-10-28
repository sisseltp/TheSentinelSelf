using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TCellManager : MonoBehaviour
{
    public AgentsManagerParameters parameters;

    [SerializeField]
    private GameObject[] prefabsTCell;
    
    private int MaxTCells = 15;
    [HideInInspector]
    public int RealAmountTCells = 0;
    [HideInInspector]
    public TCell[] TCells;
    [HideInInspector]
    public GPUCompute.GPUData[] GPUStruct; // list of struct ot hold data, maybe for gpu acceleration
    public GPUCompute.GPUOutput[] GPUOutput;

    private List<Genetics.GenVel> GenVelLib; // lib to hold the gene move data
    private List<Genetics.GenKurmto> GenKurLib; // lib to hold gene kurmto data

    [SerializeField]
    private float emitionRate = 4;

    void Start()
    {
        MaxTCells = Mathf.FloorToInt(parameters.amongAgentsAtStart * 1.5f);
        TCells = new TCell[MaxTCells];

        // create list to hold data structs
        GPUStruct = new GPUCompute.GPUData[MaxTCells];
        // create the two lib lists
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();


        GPUOutput = new GPUCompute.GPUOutput[MaxTCells];

        for (int i=0; i< parameters.amongAgentsAtStart; i++)
        {
            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere* parameters.spawnArea;
            int randindx = UnityEngine.Random.Range(0, prefabsTCell.Length);
            TCell newTCell = Instantiate(prefabsTCell[randindx], pos, Quaternion.identity, this.transform).GetComponent<TCell>();

            KuramotoAffectedAgent kuramoto = newTCell.kuramotoAffectedAgent;
            kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them

            newTCell.geneticAntigenKey.Reset();

            GPUStruct[i].SetFromKuramoto(kuramoto);
            GPUStruct[i].pos = newTCell.transform.position;
            GPUOutput[i].Setup();

            TCells[i] = newTCell;
        }

        RealAmountTCells = parameters.amongAgentsAtStart;

        StartCoroutine(emition(emitionRate));
    }

    IEnumerator emition( float rate)
    {
        while (true)
        {
            if (CanAddCell())
            {
                Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;
                int randindx = UnityEngine.Random.Range(0, prefabsTCell.Length);
                TCell thisSentinel = Instantiate(prefabsTCell[randindx], pos, Quaternion.identity, this.transform).GetComponent<TCell>();

                thisSentinel.kuramotoAffectedAgent.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them

                AddTCell(thisSentinel);
            }

            yield return new WaitForSeconds(rate);
        }
    }
    private void Update()
    {
        List<int> toRemove = new List<int>();

        for (int i = 0; i < RealAmountTCells; i++)
        {
            if (TCells[i] == null)
                continue;
            
            KuramotoAffectedAgent kuramoto = TCells[i].kuramotoAffectedAgent;
            
            if (kuramoto.dead || kuramoto.age > parameters.MaxAge) 
            {
                if (i > parameters.amongAgentsAtStart)
                {
                    toRemove.Add(i);
                    continue;
                }

                ResetTCell(i);
            }
            else
            {
                if (kuramoto.played == 3)
                    kuramoto.age += Time.deltaTime*10;
                else
                    kuramoto.age += Time.deltaTime;

                kuramoto.phase += GPUOutput[i].phaseAdition * Time.deltaTime;
                if (kuramoto.phase > 1) { kuramoto.phase = kuramoto.phase-1; }
                GPUStruct[i].played = kuramoto.played;
                GPUStruct[i].phase = kuramoto.phase;

                TCells[i].rigidbody.AddForceAtPosition(GPUOutput[i].vel * parameters.speedScl * Time.deltaTime * kuramoto.phase, TCells[i].transform.position + TCells[i].transform.up);

                GPUStruct[i].pos = TCells[i].transform.position;

            }

            if (TCells[i].renderer.isVisible)
                TCells[i].renderer.material.SetFloat("Phase", kuramoto.phase);
        }

        int nxtIndx = -1;
        
        for ( int i=0; i<toRemove.Count; i++)
        {
            int indx = toRemove[i];
            Destroy(TCells[indx]);

            if (i != toRemove.Count-1)
                 nxtIndx = toRemove[i+1];
            else
                nxtIndx = RealAmountTCells;

            for (int p = indx+1; p <= nxtIndx ; p++)
            {
                GPUStruct[p - (i+1)] = GPUStruct[p];
                TCells[p - (i+1)] = TCells[p];
            }            
        }
        
        RealAmountTCells -= toRemove.Count;
        
        if (nxtIndx != -1) {
            GPUStruct[nxtIndx] = new GPUCompute.GPUData();
            TCells[nxtIndx] = null;
        }
    }
   
    public void ResetTCell(int i ,bool genOn = false)
    {
        TCell thisTCell = TCells[i];

        thisTCell.renderer.material.SetFloat("KeyTrigger", 0);

        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;

        thisTCell.transform.position = pos;

        if (!genOn)
        {
            thisTCell.kuramotoAffectedAgent.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);
            thisTCell.geneticMovementTcell.Reset();
            thisTCell.geneticAntigenKey.Reset();
        }
        else if (GenKurLib.Count < 500)
        {
            KuramotoAffectedAgent kuramoto = thisTCell.kuramotoAffectedAgent;

            Genetics.GenKurmto genKurm = new Genetics.GenKurmto(kuramoto.speedBPM, kuramoto.noiseScl, kuramoto.coupling, kuramoto.couplingRange, kuramoto.attractionSclr, kuramoto.fitness);
            GenKurLib.Add(genKurm);
            Genetics.GenVel vels = new Genetics.GenVel(TCells[i].geneticMovementSentinel.geneticMovement, kuramoto.fitness);
            GenVelLib.Add(vels);
            // add random new sentinel
            kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them
            
            thisTCell.geneticMovementTcell.Reset();
            thisTCell.geneticAntigenKey.Reset();
        }
        else
        {
            // add random sentinel from lib
            int rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData1 = GenKurLib[rand];
            rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData2 = GenKurLib[rand];

            float[] Settings = kurData1.BlendAttributes(kurData2.Settings);

            KuramotoAffectedAgent kuramoto = thisTCell.kuramotoAffectedAgent;
            kuramoto.SetupData(Settings);

            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel1 = GenVelLib[rand];
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel2 = GenVelLib[rand];

            Vector3[] Vels = genVel2.BlendAttributes(genVel1.Vels);

            thisTCell.geneticMovementTcell.Reset();
            thisTCell.geneticMovementTcell.geneticMovement = genVel1.BlendAttributes(Vels);
            thisTCell.geneticAntigenKey.Reset();
        }
    }

    public bool CanAddCell() => RealAmountTCells<MaxTCells - 1;

    public void AddTCell(TCell tCell)
    {
        RealAmountTCells++;
        TCells[RealAmountTCells-1] = tCell;
        KuramotoAffectedAgent kuramoto = tCell.kuramotoAffectedAgent;
        tCell.geneticAntigenKey.Reset();

        GPUCompute.GPUData gpuStruct = new GPUCompute.GPUData();
        gpuStruct.SetFromKuramoto(kuramoto);
        gpuStruct.pos = tCell.transform.position;
        GPUStruct[RealAmountTCells] = gpuStruct;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 1f);
    }
#endif
}
