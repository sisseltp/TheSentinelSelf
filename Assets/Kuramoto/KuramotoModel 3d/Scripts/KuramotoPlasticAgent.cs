using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KuramotoPlasticAgent : MonoBehaviour
{
    private const float CIRCLE_IN_RADIAN = 2f * Mathf.PI; //2* pi
    private const float RADIAN_TO_NORMALIZED = 1f / CIRCLE_IN_RADIAN;

    
    public float speed; // driving force for the phase
    
    public float phase; // holds the phase position
    
    public float cohPhi; // angle to positive a-xis
    
    public float coherenceRadius; //holds the phase distance to 0,0
    public float couplingRange = 1; // holds the distance to the coupling range
    public float noiseScl = 1; // scales the noise added
    public float coupling = 0.5f; // scales the coupling effect
    public float speedVariation = 0.1f; // variation to randomise speed
    public int counter = 0; // counts how many links it has within the range
    public bool dead = false;// dead trigger
    public float fitness = 0;// fitness rating for the agent

    public Vector2 rbEffectsRange = new Vector2(0.5f, 1.5f);
   
    // holds the rendr
    Renderer rendr;

    // two colours to lerp between
    [SerializeField]
    private Color col0;
    [SerializeField]
    private Color col1;

    //holds the sentinel manager
    public SentinelManager sentinelManager;
    // holds the sentinels
    private GameObject[] sentinels;

    private bool played = false;
    public int age=0;


    public void Setup(Vector2 noiseRange, Vector2 couplingRanges, Vector2 SpeedRange, Vector2 couplingScl, float thisSpeedVariation = 0.1f)
    {
        speed = UnityEngine.Random.Range(SpeedRange.x,SpeedRange.y);
        phase = speed * UnityEngine.Random.Range(1f - thisSpeedVariation, 1f + thisSpeedVariation);
        noiseScl = UnityEngine.Random.Range(noiseRange.x, noiseRange.y);
        coupling = UnityEngine.Random.Range(couplingScl.x, couplingScl.y);
        couplingRange = UnityEngine.Random.Range(couplingRanges.x, couplingRanges.y);
        fitness = 0;
        age = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        // hook up rendr component
        rendr = GetComponentInChildren<Renderer>();
        
        
        // link the sentinels as a list
        sentinels = sentinelManager.sentinels ;
        
    }

    // Update is called once per frame
    void Update()
    {
        // if the num of sentinels changes relink them
        if (sentinelManager.nSentinels != sentinels.Length)
        {
            sentinels = sentinelManager.sentinels;
        }
        //run coherence function
        Coherence();

        // get the time between this frame and the last
        var dt = Time.deltaTime;


        //get the phase
        var p = phase;
        // get the its current angle to positive x axis
        var cphi = cohPhi;
        // Get its distance to 0
        var crad = coherenceRadius;
        // get the noise
        float thisNoise = noiseScl * Noise();
        // not sure but im guessing the main distance to phase function sin((angleDistToX-phase) * (2*Pi))
        float calc = Mathf.Sin((cphi - p) * CIRCLE_IN_RADIAN);
        // second phase (scaler * distTo0 * lastClac)
        calc = coupling * crad * calc;
        // add the speed noise and calc together and times by delta time, and add to P
        p += dt * (speed + thisNoise + calc);
        // subtract its intiger to leave it as a 0. someting
        p -= (int)p;
        // set the new phase value
        phase = p;

        // ad the amount of partners * sclr to the fitness
        fitness += counter * 0.2f;

        //float oscil = Mathf.Sin((cohPhi - phase) * (2 * Mathf.PI));
        rendr.material.color = Color.Lerp(col0, col1, phase);

        // if its been interacted with by the players
        if (played)
        {
            age = 0;// reset age
            played = false; // reset played gate
        }
        else { age++; } // else add to the age each frame
    }

    // main comparing function 
    protected void Coherence()
    {


        // variables to hold oscilation totals
        var sumx = 0f;
        var sumy = 0f;
        // times the points value by 2*Pi
        var theta = phase * CIRCLE_IN_RADIAN;

        // get the phase pos on the circle
        var phaseX = Mathf.Cos(theta);
        var phaseY = Mathf.Sin(theta);
        var phasePos = new Vector2(phaseX, phaseY);

        // reset the counter
        counter = 0;

        // loop over the sentinels
        for (var y = 0; y < sentinels.Length; y++)
        {
            // find the distnace to this agent
            float distance = Vector3.Distance(sentinels[y].transform.position, transform.position);
            if (distance < couplingRange)// if less than coupling range
            {
                // get the kuramoto component
                KuramotoSentinelAgent sentinel = sentinels[y].GetComponent<KuramotoSentinelAgent>();
                // times the points value by 2*Pi
                theta = sentinel.phase * CIRCLE_IN_RADIAN;
                // get this sentinels x,y pos
                float thisX = Mathf.Cos(theta);
                float thisY = Mathf.Sin(theta);
                // add to oscilation values to sums (total) 
                sumx += thisX;
                sumy += thisY;

                sentinel.AddOsiclation(phaseX, phaseY);

                // add the 1 to the counter
                counter++;

                // get the oscilation distance (0-2 as sin )
                float sigDst = Vector2.Distance(phasePos, new Vector2(thisX, thisY));

                float normDist = distance / couplingRange;

                sigDst -= 1; ;

                normDist *= (rbEffectsRange.y- rbEffectsRange.x);
                normDist += rbEffectsRange.x;

                // get the vector between the two, scale it by the oscilation difference and add to the rb velocity;
                //sentinels[y].GetComponent<Rigidbody>().velocity += (transform.position - sentinels[y].transform.position) * sigDst;
                sentinels[y].GetComponent<Rigidbody>().mass = normDist;
                sentinels[y].GetComponent<Rigidbody>().drag = normDist;
                sentinel.noiseScl = normDist / 100;
                // draw a line for the connection
                Debug.DrawLine(sentinels[y].transform.position, transform.position, Color.red);

                played = true;
            
            }

        }
        // if there have been connections
        if (counter != 0)
        {

            // average the values over total
            sumx /= counter;
            sumy /= counter;

        }

        // angle to x,y pos to positive x axis
        cohPhi = Mathf.Atan2(sumy, sumx) * RADIAN_TO_NORMALIZED;
        // distance to 0
        coherenceRadius = Mathf.Sqrt(sumx * sumx + sumy * sumy);

    }
    // simple noise function
    protected float Noise()
    {
        return 2f * Random.value - 1f;
    }
    // if it collides with the surrounding area it dies
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag != "Terrain" && collision.gameObject.tag != "Player" && collision.gameObject.tag != "Sentinel")
        {
            dead = true;
        }
    }


    
    // resets and randomizes the base parameter
    internal void Reset()
    {
        speed = UnityEngine.Random.value;
        phase = speed * UnityEngine.Random.Range(1f - speedVariation, 1f + speedVariation);
        noiseScl = UnityEngine.Random.value/2;
        coupling = UnityEngine.Random.Range(2, 10);
        couplingRange = UnityEngine.Random.Range(4, 20);
        fitness = 0;
        age = 0;
    }
}
