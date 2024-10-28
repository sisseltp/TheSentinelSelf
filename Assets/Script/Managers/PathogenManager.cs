using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathogenManager : MonoBehaviour
{
    public AgentsManagerParameters parameters;

    [SerializeField]
    private GameObject prefabPathogen;

    [HideInInspector]
    public Pathogen[] pathogens;

    public int RealAmountPathogens = 0;

    [HideInInspector]
    public GPUCompute.GPUData[] GPUStruct; // list of struct ot hold data, maybe for gpu acceleration
    public GPUCompute.GPUOutput[] GPUOutput;

    private List<Genetics.GenVel> GenVelLib; // lib to hold the gene move data
    private List<Genetics.GenKurmto> GenKurLib; // lib to hold gene kurmto data

    [SerializeField]
    private float emitionTimer = 1.0f;
    private float timeGate = 0;

    void Start()
    {
        pathogens = new Pathogen[parameters.amongAgentsAtStart];

        GPUStruct = new GPUCompute.GPUData[parameters.amongAgentsAtStart];
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();
        GPUOutput = new GPUCompute.GPUOutput[parameters.amongAgentsAtStart];

        for (int i=0; i< parameters.amongAgentsAtStart; i++)
            AddPathogen(i);
    }

    public void AddPathogen(int i)
    {
        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;

        Pathogen thisPathogen = Instantiate(prefabPathogen, pos, Quaternion.identity, this.transform).GetComponent<Pathogen>();

        thisPathogen.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them

        thisPathogen.geneticAntigenKey.Reset();

        pathogens[RealAmountPathogens] = thisPathogen;

        GPUStruct[RealAmountPathogens].SetFromKuramoto(thisPathogen.kuramoto);
        GPUStruct[RealAmountPathogens].pos = pathogens[i].transform.position;
        GPUOutput[RealAmountPathogens].Setup();

        RealAmountPathogens++;
    }

    private void DuplicatePathogen(Pathogen pathogen, int duplications=2)
    {
        if (RealAmountPathogens<parameters.amongAgentsAtStart -2)
        {
            for (int l = 0; l < duplications; l++)
            {
                Pathogen thisPathogen = Instantiate(pathogen, pathogen.transform.position, pathogen.transform.rotation, this.transform).GetComponent<Pathogen>();

                thisPathogen.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them

                pathogens[RealAmountPathogens] = thisPathogen;

                GPUStruct[RealAmountPathogens].SetFromKuramoto(thisPathogen.kuramoto);
                GPUStruct[RealAmountPathogens].pos = pathogens[RealAmountPathogens].transform.position;

                RealAmountPathogens++;

                thisPathogen.geneticAntigenKey.antigen = new Genetics.Antigen();
                thisPathogen.geneticAntigenKey.antigen.Key = pathogen.GetComponentInChildren<GeneticAntigenKey>().antigen.Key;
            }
        }
    }

    private void Update()
    {
        if(Time.time > emitionTimer + timeGate && RealAmountPathogens < parameters.amongAgentsAtStart)
        {
            timeGate = Time.time;
            AddPathogen(RealAmountPathogens);
        }

        List<int> toRemove = new List<int>();

        for (int i = 0; i < RealAmountPathogens; i++)
        {
            if (pathogens[i].kuramoto.dead) 
            {
                toRemove.Add(i);
            }
            else  if (pathogens[i].kuramoto.age > parameters.MaxAge )
            {
                pathogens[i].kuramoto.age = 0;
                DuplicatePathogen(pathogens[i],1);
            }
            else
            {
                pathogens[i].kuramoto.age += Time.deltaTime;
                pathogens[i].kuramoto.phase += GPUOutput[i].phaseAdition * Time.deltaTime;

                if (pathogens[i].kuramoto.phase > 1) 
                    pathogens[i].kuramoto.phase--;

                GPUStruct[i].phase = pathogens[i].kuramoto.phase;

                pathogens[i].rigidBody.AddForceAtPosition(GPUOutput[i].vel * parameters.speedScl * Time.deltaTime * pathogens[i].kuramoto.phase, pathogens[i].transform.position + pathogens[i].transform.up);

                GPUStruct[i].pos = pathogens[i].rigidBody.position;

            }

            if (pathogens[i].renderer.isVisible)
                pathogens[i].renderer.material.SetFloat("Phase", pathogens[i].kuramoto.phase);
        }

        int nextIndex = 0;
        for (int i=0;i<parameters.amongAgentsAtStart;i++)
        {
            if (toRemove.Contains(i))
            {
                if (pathogens[i] != null)
                {
                    Destroy(pathogens[i].gameObject);
                    pathogens[i] = null;
                }
                continue;
            }

            pathogens[nextIndex] = pathogens[i];
            GPUStruct[nextIndex] = GPUStruct[i];

            if (nextIndex != i)
            {
                pathogens[i] = null;
                GPUStruct[i] = new GPUCompute.GPUData();
            }

            nextIndex++;
        }

        RealAmountPathogens -= toRemove.Count;
    }

    public void ResetSentinel(int i, bool genOn = false)
    {
        Pathogen thisPathogen = pathogens[i];

        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;

        thisPathogen.transform.position = pos;

        if (!genOn)
        {
            thisPathogen.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);
            thisPathogen.geneticMovement.Reset();
        }
        else if (GenKurLib.Count < 500)
        {
            Genetics.GenKurmto genKurm = new Genetics.GenKurmto(thisPathogen.kuramoto.speedBPM, thisPathogen.kuramoto.noiseScl, thisPathogen.kuramoto.coupling, thisPathogen.kuramoto.couplingRange, thisPathogen.kuramoto.attractionSclr, thisPathogen.kuramoto.fitness);
            GenKurLib.Add(genKurm);
            Genetics.GenVel vels = new Genetics.GenVel(pathogens[i].geneticMovement.geneticMovement, thisPathogen.kuramoto.fitness);
            GenVelLib.Add(vels);

            thisPathogen.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange, 0.2f);// setup its setting to randomize them

            thisPathogen.geneticMovement.Reset();
        }
        else
        {
            int rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData1 = GenKurLib[rand];
            rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData2 = GenKurLib[rand];

            float[] Settings = kurData1.BlendAttributes(kurData2.Settings);

            thisPathogen.kuramoto.SetupData(Settings);

            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel1 = GenVelLib[rand];
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel2 = GenVelLib[rand];

            Vector3[] Vels = genVel2.BlendAttributes(genVel1.Vels);

            thisPathogen.geneticMovement.Reset();
            thisPathogen.geneticMovement.geneticMovement = genVel1.BlendAttributes(Vels);
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
