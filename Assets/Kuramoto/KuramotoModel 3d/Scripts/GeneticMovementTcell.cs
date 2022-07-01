using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GeneticMovementTcell : MonoBehaviour
{
    [SerializeField]
    private int cycleLength = 10; // number of steps in cylcle
    [SerializeField]
    private float speedScl = 0.5f; // sclr for the speed

    public Vector3[] geneticMovement; // list to hold vels in

    public Vector3 thisGenVel;

    private KuramotoBiomeAgent sentinel; // sentinel obj

    private Rigidbody rb;// rigidbody

    private int step = 0;// to hold the steps number

    private float lastPhase = 0;// holds the last phase

    public Vector3 target;

    private TCellManager manager;

    [SerializeField]
    private ComputeShader compare;

    private GeneticAntigenKey thisAnti;



    // Start is called before the first frame update
    void Start()
    {
        // gets the sentinels kurmto
        sentinel = GetComponent<KuramotoBiomeAgent>();
        // gets this rb
        rb = GetComponent<Rigidbody>();
        // sets it to a new vec3 list for vels
        geneticMovement = new Vector3[cycleLength];

        // set the vels of the list
        for(int i=0; i<cycleLength; i++)
        {
            // random vec
            geneticMovement[i] = Random.insideUnitSphere;
            // absolute y value so only positive
            geneticMovement[i].y = Mathf.Abs(geneticMovement[i].y);

        }


        manager = GetComponentInParent<TCellManager>();
        target = Vector3.zero;

         thisAnti = gameObject.GetComponent<GeneticAntigenKey>();
        

    }
    
    // Update is called once per frame
    void Update()
    {
        // if phase is less than last phase (back to 0 from 1)
        if (sentinel.phase < lastPhase) { 
            step++;// add a step
            if (step >= cycleLength)// if greater than list length, back to 0
            {
                step = 0;
            }
        }else if(lastPhase== sentinel.phase) { 
            Destroy(gameObject);
            return;
        }

        thisGenVel = geneticMovement[step];

         // get vel from this steps genmov, mult by phase and scl
        Vector3 vel =   thisGenVel * sentinel.phase * speedScl;
        if (target != Vector3.zero) {
            vel += Vector3.Normalize(target - transform.position) * sentinel.phase * speedScl; }

        // more than one sentinel contact scl it up
        //if (sentinel.Connections > 2) { vel*=Mathf.Sqrt(sentinel.Connections)*0.6f; }
        
        // add the vel to the rb
        rb.velocity += vel * Time.deltaTime;

       // set last phase to phase
        lastPhase = sentinel.phase;
    }

    // reset randomizes the list of vels
    public void Reset()
    {
        for (int i = 0; i < geneticMovement.Length; i++)
        {
            geneticMovement[i] = Random.insideUnitSphere;
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.tag == "Kill")
        {
            sentinel.dead = true;
        }
        else if (collision.gameObject.tag == "Player")
        {
            // get keys from children
            GeneticAntigenKey[] Antigens =  collision.gameObject.GetComponentsInChildren<GeneticAntigenKey>();

            // if there are any keys
            if (Antigens.Length > 0)
            {
                // get matches from children
                AntigenKeys[] results = Compare(Antigens);

                // run over results
                for(int i=1; i<results.Length; i++) 
                {
                    // if it has a connection
                    if (results[i].hit > 0)
                    {
                        // add fitness
                        Antigens[i-1].antigen.fitness++;
                        // set the target from the origin
                        target = Antigens[i-1].origin;
                        // create a replica
                        GameObject replica =  Instantiate(this.gameObject, transform.parent);
                        // add new tcell to manager
                        manager.AddTCell(replica);
                        
                    }
                }
            }
           
            
        }
    }
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
        
        
        
        
        keys[0].SetupKey(thisAnti.antigen.Key);

        for (int i=1; i<keys.Length; i++)
        {
            if (antigens[i - 1].antigen.Key != null)
            {
                keys[i].SetupKey(antigens[i - 1].antigen.Key);
            }
        }

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

        keyBuffer.Dispose();

        return  keys;
    }

}
