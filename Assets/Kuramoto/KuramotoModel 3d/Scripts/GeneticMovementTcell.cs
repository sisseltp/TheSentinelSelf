using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Script.CameraSystem;
using UnityEngine;

public class GeneticMovementTcell : GeneticMovement
{
    private TCellsManager manager;

    [Tooltip("Shader to compare keys")]
    [SerializeField]
    private ComputeShader compare;

    public override void Start()
    {
        base.Start();

        manager = GetComponentInParent<TCellsManager>();

        if (notKeyed)
            target = transform.parent.position;
    }
    
    public override void Update()
    {
        if (agent.kuramoto.phase < lastPhase)
            step = (step + 1) % cycleLength;
        else if (agent.kuramoto.phase == lastPhase)
        {
            Destroy(gameObject);
            return;
        }
            

        Vector3 vel = geneticMovement[step] * genSpeedScl;

        if (targeting)
            vel += Vector3.Normalize(target - transform.position) * speedScl;

        vel *= agent.kuramoto.phase;
        agent.rigidBody.AddForceAtPosition(vel * Time.deltaTime, transform.position + transform.up);

        lastPhase = agent.kuramoto.phase;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Kill"))
        {
            agent.kuramoto.dead = true;
        }
        else if (collision.gameObject.CompareTag("Player"))//if hits player
        {
            // get keys from children
            List<GeneticAntigenKey> Antigens = collision.gameObject.GetComponent<GeneticMovementSentinel>().digestAntigens;
            List<Transform> plastics = collision.gameObject.GetComponent<GeneticMovementSentinel>().plastics;

            if (plastics.Count > 0) 
            {
                int rndIndx = Random.Range(0, plastics.Count);

                target = plastics[rndIndx].GetComponent<Digestion>().origin;
                notKeyed = false;
                GetComponent<Renderer>().material.SetFloat("KeyTrigger", 2);
                GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, 100);
                GetComponent<KuramotoAffectedAgent>().played = 3;
                
                // TODO: @Neander: This is where the TCell gets corrupted by the Sentinel
                CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.TCellIsCorrupted, transform));
                //////<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< hits a plastic agent and gets lost
            } 
            else if (Antigens.Count > 0) 
            { 
                // TODO: @Neander: Do the TCells get replicated? Do they go to the Pathogens? If yes how does it know which ones because the get killed by the sentinels?
                
                // if it had antigens?
                // get matches from children
                AntigenKeys[] results = Compare(Antigens.ToArray());// gpu accelerated key compare

                // run over results
                for (int i = 1; i < results.Length; i++)
                {
                    // if it has a match
                    if (results[i].hit > 0)
                    {
                        // set movement gate to false
                        notKeyed = false;
                        // sett render value
                        GetComponent<Renderer>().material.SetFloat("KeyTrigger", 1);
                        GetComponent<KuramotoAffectedAgent>().played = 2; // I guess this means its looking for pathogens?
                        // add fitness
                        Antigens[i - 1].antigen.fitness++;
                        
                        // @neander: Sets the pathogen emitter as target and copies itself a few times
                        // set the target from the origin
                        target = Antigens[i - 1].origin;
                        
                        // TODO: @Neander: This is where the TCell goes to the pathogen emitter
                        CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.TCellGoesToPathogen, transform));

                        for (int j = 0; j < 2; j++)
                        {
                            if ((agent.manager as TCellsManager).CanAddCell)
                            {
                                // create a replica
                                TCell replica = Instantiate(gameObject, transform.parent).GetComponent<TCell>();
                                replica.geneticMovement.notKeyed = false;
                                replica.geneticMovement.target = target;
                                // add new tcell to manager
                                manager.AddNewAgentAtTop(replica);
                            }
                            else
                            {
                                return;
                            }
                            //////<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< matches a key and replicates
                        }
                    }
                }
            }
        }
        else if (collision.gameObject.CompareTag("Pathogen")) // TODO: Check why this is not firing
        {
            Debug.Log("Hit Pathogen");
            
            // get keys from children
            GeneticAntigenKey[] Antigens = collision.gameObject.GetComponentsInChildren<GeneticAntigenKey>();

            // if there are any keys
            if (Antigens.Length > 0)
            {
                // get matches from children
                AntigenKeys[] results = Compare(Antigens);

                // run over results
                for (int i = 1; i < results.Length; i++)
                {
                    // if it has a connection
                    if (results[i].hit > 0)
                    {
                        // add fitness
                        Antigens[i - 1].antigen.fitness++;
                        collision.gameObject.GetComponent<KuramotoAffectedAgent>().dead = true;
                        
                        // TODO: @Neander: This is where the TCell killed a Pathogen
                        CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.TCellKillsPathogen, transform));
                        //////<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< matches a key and kils pathogen

                    }
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Lymphonde") && notKeyed) 
        {
            target = transform.parent.position+(Random.onUnitSphere * 100);
        }
        else if (other.gameObject.CompareTag("PathogenEmitter")) 
        { 
            StartCoroutine(TargetTimeout(15));
            
            // TODO: @Neander: This is where the TCell reached the pathogen emitter
            CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.TCellReachedPathogenEmitter, transform));
            //////<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< tcell reaches the pathogen emitter
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("LymphOuter") && notKeyed)
        {
            target = transform.parent.position;
        }
        // else if (other.gameObject.CompareTag("LymphOuter") && !notKeyed)
        // {
        //     // TODO: @Neander: This is where the TCell goes to the pathogen emitter
        //     CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.TCellGoesToPathogen, transform));
        // }
    }


    private IEnumerator TargetTimeout(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        targeting = false;
    }

    /// <summary>
    ///  GPu accelerating bits
    /// </summary>
    private struct AntigenKeys
    {
        public int key1;
        public int key2;
        public int key3;
        public int key4;
        public int hit;

        public void SetupKey(int[] key)
        {
            key1 = key[0];
            key2 = key[1];
            key3 = key[2];
            key4 = key[3];
            hit = 0;
        }
    }

    private AntigenKeys[] Compare(GeneticAntigenKey[] antigens)
    {
        AntigenKeys[] keys = new AntigenKeys[antigens.Length + 1];
        
        keys[0].SetupKey((agent as TCell).geneticAntigenKey.antigen.Key);

        for (int i=1; i<keys.Length; i++)
            if (antigens[i - 1].antigen.Key != null)
                keys[i].SetupKey(antigens[i - 1].antigen.Key);

        RenderTexture rt = new RenderTexture(1, 1, 0);
        rt.enableRandomWrite = true;
        RenderTexture.active = rt;

        ComputeBuffer keyBuffer = new ComputeBuffer(keys.Length, Marshal.SizeOf(typeof(AntigenKeys)));
        keyBuffer.SetData(keys);

        int UpdateBiome = compare.FindKernel("KeyCompare");
        //int UpdateSentinel = shader.FindKernel("SentinelUpdate");

        compare.SetTexture(UpdateBiome, "Result", rt);
        compare.SetBuffer(UpdateBiome, "antigenKeys", keyBuffer);

        compare.Dispatch(UpdateBiome, 1, 1, 1);

        keyBuffer.GetData(keys);

        keyBuffer.Release();

        return keys;
    }
}
