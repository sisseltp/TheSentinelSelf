using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KuramotoBiomeAgent : MonoBehaviour
{
    private const float CIRCLE_IN_RADIAN = 2f * Mathf.PI;
    private const float RADIAN_TO_NORMALIZED = 1f / CIRCLE_IN_RADIAN;

    public float speedBPM; // speed force of the 
    public float speed;
    public float phase; // the phase its in 
    public float cohPhi; // the agle to positive x-axis
    public float coherenceRadius; // distance to 0
    public float couplingRange = 1; // distance it will couple
    public float noiseScl = 1; // amount of noise
    public float coupling = 0.5f; // coupling scaler
    public float speedVariation = 0.1f; // amount to randomize data
    public int Connections = 0; // num neighbors
    private int newConnections = 0;
    public int played = 0; // if the player has been in contact
    public bool dead = false; // collision killer

    // holds the x,y position of the phase
    public float sumx = 0f; 
    public float sumy = 0f;

    // to hold this object rigid body component
    private Rigidbody rb;
    // to hold a velocity value
    private Vector3 vel = new Vector3(0,0,0);
    // to hold the renderer component
    Renderer rendr;

    [SerializeField]
    private Color col0;// phase col1
    [SerializeField]
    private Color col1; // phase col2

    private Transform[] sentinals; // list of the other sentinels transforms

    public float fitness=0; // holds this agents fitness value

    public int age = 0; // holds this agents age

    // Start is called before the first frame update
    void Start()
    {
        // attach the rigidbody component
        rb = GetComponent<Rigidbody>();

        // attach the renderer component
        rendr = GetComponent<Renderer>();

        // get the parent (sentinel managers) transform
        Transform parent = transform.parent;
        // get the number of children (sentinels)
        int nChild = parent.childCount;
        // create a new list of transforms 1 less than the nChild
        sentinals = new Transform[nChild -1];


        int sub = 0; // Connections to hold indx
        for (int i = 0; i < nChild; i++) // loop for num child
        {
            // get the i child
            Transform child = parent.transform.GetChild(i);
            if ( child.GetInstanceID() != transform.GetInstanceID()) // if the child isnt this object
            {
                sentinals[sub] = child;// set it to the list
                sub++; //add to the indx
            }
            

        }
    }

    public void Setup(Vector2 noiseRange, Vector2 couplingRanges, Vector2 SpeedRange, Vector2 couplingScl, float thisSpeedVariation = 0.1f)
    {
        speedBPM = UnityEngine.Random.Range(SpeedRange.x, SpeedRange.y);
        speed = speedBPM/60;
        phase = speed * UnityEngine.Random.Range(1f - thisSpeedVariation, 1f + thisSpeedVariation);
        noiseScl = UnityEngine.Random.Range(noiseRange.x, noiseRange.y);
        coupling = UnityEngine.Random.Range(couplingScl.x, couplingScl.y);
        couplingRange = UnityEngine.Random.Range(couplingRanges.x, couplingRanges.y);
        fitness = 0;
        age = 0;
        dead = false;
    }

    public void SetupData(float[] settingsData, float thisSpeedVariation = 0.1f)
    {
        speedBPM = settingsData[0];
        noiseScl = settingsData[1];
        couplingRange = settingsData[3];
        coupling = settingsData[2];
        speed = 60 / speedBPM;
        phase = speed * UnityEngine.Random.Range(1f - thisSpeedVariation, 1f + thisSpeedVariation);
        fitness = 0;
        age = 0;
        dead = false;
        rb.velocity = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // call the coherence function
        //Coherence();

        // get the time between this frame and the last
        //var dt = Time.deltaTime;
     

        //get the phase
        //var p = phase;
        // get the its current angle to positive x axis
       // var cphi = cohPhi;
        // Get its distance to 0
        //var crad = coherenceRadius;
        // get the noise
       // float thisNoise = noiseScl * Noise();
        // not sure but im guessing the main distance to phase function sin((angleDistToX-phase) * (2*Pi))
       // float calc = Mathf.Sin((cphi - p) * CIRCLE_IN_RADIAN);
        // second phase (scaler * distTo0 * lastClac)
       // calc = coupling * crad * calc;
        // add the speed noise and calc together and times by delta time, and add to P
      //  p += dt * (speed + thisNoise + calc);
        // subtract its intiger to leave it as a 0. someting
      //  p -= (int)p;
        // set the new phase value
      //  phase = p;

        // add the vel to the rigid body, scaled by the phase to make it pulse movement
        //rb.velocity += vel * 0.1f;

        // set the material to lerp between the the 2 cols by the phase
        rendr.material.color = Color.Lerp(col0, col1, phase);

        // ad the amount of partners * sclr to the fitness
        fitness += Connections * Time.deltaTime;

        // if its been interacted with by the players
        if (played==1) { 
            age = 0;// reset age
            fitness += 10;
        }
        else { age++; } // else add to the age each frame


    }

    // main comparing function 
    protected void Coherence()
    {


        // variables to hold oscilation totals
        
        // times the points value by 2*Pi
        var theta = phase * CIRCLE_IN_RADIAN;
        // get this agents phase x,y
        var phaseX = Mathf.Cos(theta);
        var phaseY = Mathf.Sin(theta);

        var phasePos = new Vector2(phaseX, phaseY);

        Connections = 0; // reset Connections to 0

        vel = Vector3.zero; // reset vel to 0

        // loop over sentinels 
        for (var y = 0; y < sentinals.Length; y++)
        {
            // get the distance between the two agents
            float distance = Vector3.Distance(sentinals[y].position, transform.position);

            // if less than coupling range
            if (distance < couplingRange)
            {
                // get the kuramoto component
                KuramotoBiomeAgent sentinel = sentinals[y].GetComponent<KuramotoBiomeAgent>();
                // times the points value by 2*Pi
                theta = sentinel.phase * CIRCLE_IN_RADIAN;
                // get this phases x,y pos
                float thisX = Mathf.Cos(theta);
                float thisY = Mathf.Sin(theta);
                // add to oscilation values to sums (total) 
                sumx += thisX;
                sumy += thisY;
                // add one to the Connections
                newConnections++;
                // find the distance between the two cycles (between 0-2 as its sin/cos positions)
                float sigDst = Vector2.Distance(phasePos, new Vector2(thisX, thisY));
                // minus 1 so betwenn -1-1
                sigDst -= 1;
                // invert
                sigDst *= -1;
                // add the vector between the two * the distance in cycles
                vel += (sentinals[y].position- transform.position)*sigDst;
                // draw a line if connected
                Debug.DrawLine(sentinals[y].position, transform.position, Color.red);
            }
           
        }

        Connections = newConnections;
        newConnections = 0;

        // if the count is 0 (its connected)
        if (Connections != 0) { 

        // average the values over total num neighbors 
        sumx /= Connections;
        sumy /= Connections;
        vel /= Connections;

        }

        // angle to x,y pos to positive x axis
        cohPhi = Mathf.Atan2(sumy, sumx) * RADIAN_TO_NORMALIZED;
        // distance to 0
        coherenceRadius = Mathf.Sqrt(sumx * sumx + sumy * sumy);

        // reset sum to 0
        sumx = 0f;
        sumy = 0f;

    }
    // simple noise function
    protected float Noise()
    {
        return 2f * UnityEngine.Random.value - 1f;
    }
    // resets values
    internal void Reset()
    {
        speed = UnityEngine.Random.value;
        phase = speed * UnityEngine.Random.Range(1f - speedVariation, 1f + speedVariation);
        noiseScl = UnityEngine.Random.value;
        coupling = UnityEngine.Random.Range(0, 10);
        couplingRange = UnityEngine.Random.Range(1,10);
        age = 0;
        fitness = 0;
    }

  

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Terrain" && collision.gameObject.tag != "Player" && collision.gameObject.tag != "Sentinel")
        {
            dead = true;
        }
    }
}
