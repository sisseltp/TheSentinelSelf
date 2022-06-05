using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticMovementSentinel : MonoBehaviour
{
    [SerializeField]
    private int cycleLength = 10; // number of steps in cylcle
    [SerializeField]
    private float speedScl = 0.5f; // sclr for the speed

    public Vector3[] geneticMovement; // list to hold vels in

    private KuramotoSentinelAgent sentinel; // sentinel obj

    private Rigidbody rb;// rigidbody

    private int step = 0;// to hold the steps number

    private float lastPhase = 0;// holds the last phase


    // Start is called before the first frame update
    void Start()
    {
        // gets the sentinels kurmto
        sentinel = GetComponent<KuramotoSentinelAgent>();
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

        // get vel from this steps genmov, mult by phase and scl
        Vector3 vel = geneticMovement[step] * sentinel.phase * speedScl;

        // more than one sentinel contact scl it up
       vel*=1 + (sentinel.Connections*0.02f); 
        
        // add the vel to the rb
        rb.velocity += vel* Time.deltaTime;

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
}
