using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticMovementSentinel : MonoBehaviour
{
    [SerializeField]
    private int cycleLength = 10; // length of movment list

    [SerializeField]
    private float speedScl = 0.5f; // scl for the speed

    public Vector3[] geneticMovement; // list to hold the vels

    private KuramotoSentinel sentinel; // kurmto to get the phase val

    private Rigidbody rb; // rigidbody to add velocity to

    private int step = 0; // step number

    private float lastPhase = 0; // holds the last phase pos


    // Start is called before the first frame update
    void Start()
    {
        // get the kurmto component
        sentinel = GetComponent<KuramotoSentinel>();
        // get the rb component
        rb = GetComponent<Rigidbody>();
        // creat a new list to hold the vels
        geneticMovement = new Vector3[cycleLength];

        //loop over the number of cycles
        for (int i = 0; i < cycleLength; i++)
        {
            // add a random vector3 value
            geneticMovement[i] = Random.insideUnitSphere;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        // if phase is less than last phase (back to 0 from 1)
        if (sentinel.phase > lastPhase) { 
            step++; //go to the next step

            if (step >= cycleLength)// if longer than cycle go back to 0
            {
                step = 0;
            }
        }
        // get this steps vel from the list and mult it by phase and scl it
        Vector3 vel = geneticMovement[step] * sentinel.phase * speedScl;

        // ad it to the rb
        rb.velocity += vel;

        // set last phase to phase
        lastPhase = sentinel.phase;
    }
    // reset randomize the list of vels
    public void Reset()
    {
        for (int i = 0; i < cycleLength; i++)
        {
            geneticMovement[i] = Random.insideUnitSphere;
        }
    }
}
