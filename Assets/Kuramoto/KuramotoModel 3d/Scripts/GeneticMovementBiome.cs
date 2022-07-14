using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticMovementBiome : MonoBehaviour
{
    [Tooltip("How many cycles to contain")]
    [SerializeField]
    private int cycleLength = 10; // length of movment list
    [Tooltip("Scaler for the genetic speed")]
    [SerializeField]
    private float speedScl = 0.5f; // scl for the speed
    [HideInInspector]
    public Vector3[] geneticMovement; // list to hold the vels

    private KuramotoBiomeAgent sentinel; // kurmto to get the phase val

    private Rigidbody rb; // rigidbody to add velocity to

    private int step = 0; // step number

    private float lastPhase = 0; // holds the last phase pos




    // Start is called before the first frame update
    void Start()
    {
        // get the kurmto component
        sentinel = GetComponent<KuramotoBiomeAgent>();
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
        rb.AddForceAtPosition(vel * Time.deltaTime, transform.position + transform.up);

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Kill" )
        {
            sentinel.dead = true;
        }
        else if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Tcell")
        {
            Quaternion rot = Quaternion.LookRotation(collision.transform.position, transform.up);


            GameObject newObj = transform.GetChild(0).gameObject;

            GameObject attached = Instantiate(newObj, newObj.transform.position, rot, collision.transform);
            attached.transform.position -= (newObj.transform.position - collision.transform.position) * 0.3f;
            attached.transform.localScale = transform.localScale;
            attached.transform.parent = collision.transform;
            
            sentinel.dead = true;
        }
    }
}
