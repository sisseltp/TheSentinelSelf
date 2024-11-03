using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class AgentsManager : MonoBehaviour
{
    public AgentsManagerParameters parameters;
    public GameObject[] prefabsAgents;
    [HideInInspector]
    public Agent[] agents;
    [HideInInspector]
    public int realAmountAgents = 0;

    [HideInInspector]
    public GPUCompute.GPUOutput[] GPUOutput; // list of agent struct, that will hold the data for gpu compute
    [HideInInspector]
    public GPUCompute.GPUData[] GPUStruct; // list of agent struct, that will hold the data for gpu compute
    [HideInInspector]
    public List<Genetics.GenVel> GenVelLib; // lib to hold the gene move data
    [HideInInspector]
    public List<Genetics.GenKurmto> GenKurLib; // lib to hold gene kurmto data

    public bool CanAddCell => realAmountAgents < parameters.maxAmountAgents;

    public bool emitsContinuously = false;

    [ShowIf("@emitsContinuously == true")]
    public float delayBetweenEmissions = 1f;

    public List<int> toRemove = new List<int>();

    public virtual void Start()
    {
        agents = new Agent[parameters.maxAmountAgents];
        GPUOutput = new GPUCompute.GPUOutput[parameters.maxAmountAgents];
        GPUStruct = new GPUCompute.GPUData[parameters.maxAmountAgents];
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();

        int amountAgentsAtStart = (int)(parameters.maxAmountAgents * parameters.percentageMaxAmountAgentsAtStart / 100f);

        for (int i = 0; i < amountAgentsAtStart; i++)
            SetNewAgentAtIndex(i);

        realAmountAgents = amountAgentsAtStart;

        if (emitsContinuously)
            StartCoroutine(Emission());
    }

    public virtual void Update()
    {
        toRemove = new List<int>();

        for (int i = 0; i < parameters.maxAmountAgents; i++)
        {
            if (agents[i] == null)
                continue;

            if (agents[i].kuramoto.dead)
            {
                OnAgentDead(i);
            }
            else if (agents[i].kuramoto.age > parameters.MaxAge)
            {
                OnAgentAged(i);
            }
            else
            {
                agents[i].kuramoto.age += Time.deltaTime;
                GPUStruct[i].played = agents[i].kuramoto.played;
                GPUStruct[i].speed = agents[i].kuramoto.speed;
                GPUStruct[i].pos = agents[i].rigidBody.position;
            }
        }

        RemoveAgentsAtIndexes(toRemove);
    }

    IEnumerator Emission()
    {
        while (true)
        {
            if (CanAddCell && emitsContinuously)
                AddNewAgentAtTop();

            yield return new WaitForSeconds(delayBetweenEmissions);
        }
    }

    public virtual void AddNewAgentAtTop(Agent newAgent = null)
    {
        if(realAmountAgents<parameters.maxAmountAgents)
        {
            SetNewAgentAtIndex(realAmountAgents, newAgent);
            realAmountAgents++;
        }
    }

    public virtual void SetNewAgentAtIndex(int i, Agent newAgent = null)
    {
        if (i < 0 || i > parameters.maxAmountAgents - 1)
            return;

        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;

        if(newAgent==null)
            newAgent = Instantiate(prefabsAgents[UnityEngine.Random.Range(0, prefabsAgents.Length)], pos, Quaternion.identity, this.transform).GetComponent<Agent>();
        newAgent.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange);// setup its setting to randomize them

        if(newAgent.geneticAntigenKey!=null)
            newAgent.geneticAntigenKey.Reset();

        GPUStruct[i].SetFromKuramoto(newAgent.kuramoto);
        GPUStruct[i].pos = newAgent.transform.position;
        GPUOutput[i].Setup();

        agents[i] = newAgent;
    }

    public virtual void RemoveAgentsAtIndexes(List<int> toRemove)
    {
        int nextIndex = 0;
        for (int i = 0; i < parameters.maxAmountAgents; i++)
        {
            if (toRemove.Contains(i))
            {
                if (agents[i] != null)
                {
                    Destroy(agents[i].gameObject);
                    agents[i] = null;
                }
                continue;
            }

            agents[nextIndex] = agents[i];
            GPUStruct[nextIndex] = GPUStruct[i];

            if (nextIndex != i)
            {
                agents[i] = null;
                GPUStruct[i] = new GPUCompute.GPUData();
            }

            nextIndex++;
        }

        realAmountAgents -= toRemove.Count;
    }

    public virtual void ResetAgentAtIndex(int i, bool genOn = false)
    {
        Agent thisAgent = agents[i];

        thisAgent.gameObject.SetActive(false);

        thisAgent.transform.position = transform.position + UnityEngine.Random.insideUnitSphere * parameters.spawnArea;

        bool cond1 = genOn;
        bool cond2 = GenKurLib.Count < 500;

        if (!cond1 && cond2)
        {
            Genetics.GenKurmto genKurm = new Genetics.GenKurmto(thisAgent.kuramoto.speedBPM, thisAgent.kuramoto.noiseScl, thisAgent.kuramoto.coupling, thisAgent.kuramoto.couplingRange, thisAgent.kuramoto.attractionSclr, thisAgent.kuramoto.fitness);
            GenKurLib.Add(genKurm);
            Genetics.GenVel vels = new Genetics.GenVel(agents[i].geneticMovement.geneticMovement, thisAgent.kuramoto.fitness);
            GenVelLib.Add(vels);
        }

        if (!cond1 || cond2)
        {
            thisAgent.kuramoto.Setup(parameters.noiseSclRange, parameters.couplingRange, parameters.speedRange, parameters.couplingSclRange, parameters.attractionSclRange);
        }
        else
        {
            int rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData1 = GenKurLib[rand];
            rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData2 = GenKurLib[rand];

            float[] Settings = kurData1.BlendAttributes(kurData2.Settings);

            thisAgent.kuramoto.SetupData(Settings);
        }

        thisAgent.geneticMovement.Reset();

        if (cond1 && !cond2)
        {
            int rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel1 = GenVelLib[rand];
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel2 = GenVelLib[rand];
            Vector3[] Vels = genVel2.BlendAttributes(genVel1.Vels);
            thisAgent.geneticMovement.geneticMovement = Vels;
        }

        if (thisAgent.geneticAntigenKey != null)
            thisAgent.geneticAntigenKey.Reset();

        if (thisAgent.fosilising != null)
            thisAgent.fosilising.enabled = false;

        if (thisAgent is TCell)
            thisAgent.renderer.material.SetFloat("KeyTrigger", 0);

        thisAgent.gameObject.SetActive(true);
    }

    public virtual void OnAgentDead(int index)
    {

    }

    public virtual void OnAgentAged(int index)
    {

    }
}
