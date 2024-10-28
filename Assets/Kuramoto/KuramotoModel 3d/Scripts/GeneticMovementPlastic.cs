using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticMovementPlastic : MonoBehaviour
{
    [Tooltip("How many cycles to contain")]
    [SerializeField]
    private int cycleLength = 10; // length of movment list
    [Tooltip("Scaler for the genetic speed")]
    [SerializeField]
    private float speedScl = 0.5f; // speed scale factor
    [HideInInspector]
    public Vector3[] geneticMovement; // list to hold the velocities

    private KuramotoPlasticAgent plastic; // kuramoto to get the phase value

    private Rigidbody rb; // rigidbody to add velocity to

    private int step = 0; // step number

    private float lastPhase = 0; // holds the last phase pos

    [SerializeField]
    private GameObject attachedGO;

    [HideInInspector]
    public Vector3 origin;

    [SerializeField]
    private int numToKill = 10;

    [SerializeField]
    private float maxDrag = 0.4f;

    private float dragItter = 0;

    // Start is called before the first frame update
    void Start()
    {
        
        Reset();

        dragItter = (maxDrag - rb.drag) / (float)numToKill;
    }

    // Update is called once per frame
    void Update()
    {
        // if phase is less than last phase (back to 0 from 1)
        if (plastic.phase > lastPhase)
        {
            step++; //go to the next step

            if (step >= cycleLength)// if longer than cycle go back to 0
            {
                step = 0;
            }
        }
        // get this steps vel from the list and mult it by phase and scl it
        Vector3 vel = geneticMovement[step] * plastic.phase * speedScl;

        // add it to the rb
        rb.AddForceAtPosition(vel * Time.deltaTime, transform.position + transform.up);

        // set last phase to phase
        lastPhase = plastic.phase;
    }
    // reset randomize the list of vels
    public void Reset()
    {
        origin = transform.position;
        // get the kurmto component
        plastic = GetComponent<KuramotoPlasticAgent>();
        // get the rb component
        rb = GetComponent<Rigidbody>();
        // creat a new list to hold the vels

        geneticMovement = new Vector3[cycleLength];
        for (int i = 0; i < cycleLength; i++)
        {
            geneticMovement[i] = Random.insideUnitSphere;
        }
    }
    private bool full = false;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Kill")
        {
            plastic.dead = true;
        }
        else if (collision.gameObject.tag == "Player" )
        {
            plastic = GetComponent<KuramotoPlasticAgent>();

            plastic.dead = true;

            GameObject plast =  Instantiate(attachedGO, collision.GetContact(0).point, transform.rotation, collision.transform);

            
            collision.gameObject.GetComponent<GeneticMovementSentinel>().plastics.Add(plast.transform);

            if (!full)
            {
                collision.gameObject.GetComponent<KuramotoAffectedAgent>().speed *= 0.9f;
                collision.gameObject.GetComponent<Rigidbody>().drag += dragItter;

                if (collision.gameObject.GetComponent<Rigidbody>().drag > maxDrag)
                {

                    collision.gameObject.GetComponent<Rigidbody>().useGravity = true;
                    full = true;
                }
            }
        }
       

    }


    private void OnTriggerStay(Collider other)
    {
        if(other.transform.tag == "PlasticMover" && rb != null)
        {
            // add it to the rb
            rb.AddForceAtPosition(Vector3.down * Time.deltaTime * 10, transform.position + transform.up);
        }
    }
}
