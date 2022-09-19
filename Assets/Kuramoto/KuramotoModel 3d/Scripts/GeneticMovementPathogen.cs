using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticMovementPathogen : MonoBehaviour
{
    [Tooltip("How many cycles to contain")]
    [SerializeField]
    private int cycleLength = 10; // length of movment list
    [Tooltip("Scaler for the genetic speed")]
    [SerializeField]
    private float speedScl = 0.5f; // scl for the speed
    [HideInInspector]
    public Vector3[] geneticMovement; // list to hold the vels

    private KuramotoAffectedAgent pathogen; // kurmto to get the phase val

    private Rigidbody rb; // rigidbody to add velocity to

    private int step = 0; // step number

    private float lastPhase = 0; // holds the last phase pos

    [SerializeField]
    private int maxKeys = 10;


    // Start is called before the first frame update
    void Start()
    {
        // get the kurmto component
        pathogen = GetComponent<KuramotoAffectedAgent>();
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
        if (pathogen.phase > lastPhase) { 
            step++; //go to the next step

            if (step >= cycleLength)// if longer than cycle go back to 0
            {
                step = 0;
            }
        }
        // get this steps vel from the list and mult it by phase and scl it
        Vector3 vel = geneticMovement[step] * pathogen.phase * speedScl;

        // ad it to the rb
        rb.AddForceAtPosition(vel * Time.deltaTime, transform.position - transform.up);

        // set last phase to phase
        lastPhase = pathogen.phase;
    }
    // reset randomize the list of vels
    public void Reset()
    {
        for (int i = 0; i < cycleLength; i++)
        {
            geneticMovement[i] = Random.insideUnitSphere;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Kill" )
        {
            pathogen.dead = true;
        }
        else if (collision.gameObject.tag == "Player" )// if hit by player
        {
            int numkeys = collision.gameObject.GetComponentsInChildren<GeneticAntigenKey>().Length;
            if (numkeys < collision.gameObject.GetComponentInChildren<GeneticMovementSentinel>().NumKeysToCollect+maxKeys)// if less than max num, pick up key
            {
                Quaternion rot = Quaternion.LookRotation(collision.transform.position, transform.up);

                GameObject newObj = transform.GetChild(0).gameObject;

                newObj = Instantiate(newObj, collision.GetContact(0).point, rot, collision.transform);
                newObj.GetComponent<Digestion>().enabled = true;
                collision.gameObject.GetComponent<GeneticMovementSentinel>().keys++;
                collision.gameObject.GetComponent<GeneticMovementSentinel>().digestAntigens.Add(newObj.GetComponent<GeneticAntigenKey>());
                pathogen.dead = true;
            }
        }
        
        
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "PathogenEmitter")
        {
            transform.position = transform.parent.position;
        }
    }

}
