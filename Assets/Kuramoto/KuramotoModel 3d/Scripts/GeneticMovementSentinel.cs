using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticMovementSentinel : MonoBehaviour
{
    [Tooltip("How many cycles to contain")]
    [SerializeField]
    private int cycleLength = 10; // number of steps in cylcle
    [Tooltip("Scaler for the genetic speed")]
    [SerializeField]
    private float genSpeedScl = 0.5f; // sclr for the speed
    [Tooltip("Scaler for the target speed")]
    [SerializeField]
    private float targetSpeedScl = 1.5f; // sclr for the speed
    [HideInInspector]
    public Vector3[] geneticMovement; // list to hold vels in

    private Vector3 thisGenVel;

    private KuramotoAffecterAgent sentinel; // sentinel obj

    private APCSong song;

    private Rigidbody rb;// rigidbody

    private int step = 0;// to hold the steps number

    private float lastPhase = 0;// holds the last phase

    private Vector3 target;

    [HideInInspector]
    public bool targeting = true;

    private SentinelManager manager;

    public int keys = 0;

    public int NumKeysToCollect = 4;


    public float origDrag = 0;

    public Transform rootBone;

    [HideInInspector]
    public List<GeneticAntigenKey> digestAntigens;
    
    [HideInInspector]
    public List<Transform> plastics;


    // Start is called before the first frame update
    void Start()
    {
        digestAntigens = new List<GeneticAntigenKey>();
        plastics = new List<Transform>();

        // gets the sentinels kurmto
        sentinel = GetComponent<KuramotoAffecterAgent>();

        // get song manager
        song = GetComponent<APCSong>();

        // gets this rb
        rb = GetComponent<Rigidbody>();
        origDrag = rb.drag;
        // sets it to a new vec3 list for vels
        geneticMovement = new Vector3[cycleLength];

        manager = GetComponentInParent<SentinelManager>();

        Reset();

    }
    Vector3 point;
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
            thisGenVel = geneticMovement[step];
        }

        Vector3 vel = Vector3.zero ;

        // get vel from this steps genmov, mult by phase and scl

        if (targeting)
        {


            // if theres an object in the way add upward force
            vel = Vector3.Normalize(target - transform.position) * targetSpeedScl;
            

        }

        Ray forward = new Ray(transform.position, Vector3.Normalize(rb.velocity) + Vector3.down * 0.5f);

        RaycastHit hit;
        if (Physics.Raycast(forward, out hit, 20))
        {
            if (hit.transform.tag == "Terrain")
            {
                vel += Vector3.up * targetSpeedScl * 3;
            }
        }

        vel += thisGenVel * genSpeedScl;
        vel *= sentinel.phase;

        rb.AddForceAtPosition(vel * Time.deltaTime, transform.position + transform.forward);
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
        rb.drag = origDrag;
        

        int indx = Random.Range(0, manager.PathogenEmitters.Length);

        target = manager.PathogenEmitters[indx];
        targeting = true;

    }
    private int tcellHits = 0;
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Tcell")
        {
            tcellHits++;
        }else if (collision.gameObject.tag == "Terrain" && rb.useGravity)
        {
            GetComponent<Fosilising>().enabled = true;
            Debug.Log("fosil on hit terrain");
        }
    }
    private void OnTriggerEnter(Collider collision)
    {

      

        
         if (collision.gameObject.tag == "PathogenEmitter")
        {
            targeting = false;
        
            //////<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<  reaches the pathogen emitter
        }
        else if (collision.gameObject.tag == "Lymphonde")
        {
            targeting = false;
         
            //////<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<  reaches the lymphonode 

        }

    }

   
    private void OnTriggerStay(Collider collision)
    {

        if (collision.gameObject.tag == "PathogenEmitter")
        {
            if (keys >= NumKeysToCollect && !targeting)
            {
                int indx = 0;
                int length = 0;

                for (int i = 0; i < manager.tcellManagers.Length; i++)
                {
                    int numpathogens = manager.tcellManagers[i].RealNumSentinels;

                    if (numpathogens > length)
                    {
                        length = numpathogens;
                        indx = i;
                    }
                }

                target = manager.Lymphondes[indx];

                targeting = true;
                tcellHits = 0;
          
               
                //song.setState(APCState.CarryingAntigens);
                //////<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< gets enough antigens to leave

            }
            else if( keys< NumKeysToCollect)
            {
                targeting = false;
            }
            
        }
        else if (collision.gameObject.tag == "Lymphonde" && tcellHits > 10 && !targeting)
        {
            int indx = 0;
            int length = 0;

            for (int i = 0; i < manager.pathogenManagers.Length; i++)
            {
                int numpathogens = manager.pathogenManagers[i].RealNumPathogens;

                if (numpathogens > length)
                {
                    length = numpathogens;
                    indx = i;
                }
            }

            targeting = true;

            target = manager.PathogenEmitters[indx];

            foreach(GeneticAntigenKey key in digestAntigens)
            {
                key.TimeOut();
            }

            digestAntigens.Clear();
            keys = 0;
           // song.setState(APCState.SeekingPathogens);
            //////<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< leaving the lymphonode 


        }

    }

    private void OnTriggerExit(Collider other)
    {
        targeting = true;
    }


}
