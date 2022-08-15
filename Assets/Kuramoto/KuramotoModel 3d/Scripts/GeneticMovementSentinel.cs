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

    private Rigidbody rb;// rigidbody

    private int step = 0;// to hold the steps number

    private float lastPhase = 0;// holds the last phase

    private Vector3 target;

    private SentinelManager manager;

    // Start is called before the first frame update
    void Start()
    {
        // gets the sentinels kurmto
        sentinel = GetComponent<KuramotoAffecterAgent>();
        // gets this rb
        rb = GetComponent<Rigidbody>();
        // sets it to a new vec3 list for vels
        geneticMovement = new Vector3[cycleLength];

        // set the vels of the list
        for(int i=0; i<cycleLength; i++)
        {
            // random vec
            geneticMovement[i] = Random.insideUnitSphere;
           

        }


        manager = GetComponentInParent<SentinelManager>();
        int indx = Random.Range(0, manager.PathogenEmitters.Length);
        
        target = manager.PathogenEmitters[indx];

       

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
        }

        thisGenVel = geneticMovement[step];

         // get vel from this steps genmov, mult by phase and scl
        Vector3 vel =   thisGenVel * sentinel.phase * genSpeedScl;
        vel += Vector3.Normalize(target - transform.position) * sentinel.phase* targetSpeedScl;

        // more than one sentinel contact scl it up
        //if (sentinel.Connections > 2) { vel*=Mathf.Sqrt(sentinel.Connections)*0.6f; }
        
        // add the vel to the rb
        rb.AddForceAtPosition(vel * Time.deltaTime, transform.position +transform.forward);

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
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Lymphonde")
        {
            int indx = Random.Range(0, manager.PathogenEmitters.Length);

            target = manager.PathogenEmitters[indx];


            int numChild = transform.childCount;

            for(int i=0; i<numChild; i++)
            {
                Transform child = transform.GetChild(i);
                GeneticAntigenKey key = child.GetComponent<GeneticAntigenKey>();
                if (key != null)
                {
                    key.TimeOut();
                }
            }
            

         
        }
        else if (collision.gameObject.tag == "PathogenEmitter")
        {
            int indx = Random.Range(0, manager.Lymphondes.Length);

            target = manager.Lymphondes[indx];
        }
        else if (collision.gameObject.tag == "Terrain" && rb.useGravity)
        {
            GetComponent<Fosilising>().enabled = true;
        }
    }

}
