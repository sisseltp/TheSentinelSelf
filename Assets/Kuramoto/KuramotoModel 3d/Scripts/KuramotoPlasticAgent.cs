using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KuramotoPlasticAgent : MonoBehaviour
{
    private const float CIRCLE_IN_RADIAN = 2f * Mathf.PI; //2* pi
    
    public float speed; // driving force for the phase
    public float speedBPM;
    public float phase; // holds the phase position
    
    
    public float coherenceRadius; //holds the phase distance to 0,0
    public float couplingRange = 1; // holds the distance to the coupling range
    public float noiseScl = 1; // scales the noise added
    public float coupling = 0.5f; // scales the coupling effect
    public float speedVariation = 0.1f; // variation to randomise speed

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


    public void Setup(Vector2 noiseRange, Vector2 couplingRanges, Vector2 SpeedRange, Vector2 couplingScl, float thisSpeedVariation = 0.1f)
    {
        speedBPM = UnityEngine.Random.Range(SpeedRange.x, SpeedRange.y);
        speed = speedBPM/60;
        phase = speed * UnityEngine.Random.Range(1f - thisSpeedVariation, 1f + thisSpeedVariation);
        noiseScl = UnityEngine.Random.Range(noiseRange.x, noiseRange.y);
        coupling = UnityEngine.Random.Range(couplingScl.x, couplingScl.y);
        couplingRange = UnityEngine.Random.Range(couplingRanges.x, couplingRanges.y);
  
    }

    // Start is called before the first frame update
    void Start()
    {
        // hook up rendr component
        rendr = GetComponentInChildren<Renderer>();
        
        
        // link the sentinels as a list
        sentinels = sentinelManager.sentinels ;
        Setup(new Vector2(0.01f, 0.1f), new Vector2(20, 30), new Vector2(50, 60), new Vector2(4, 20));
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
     
        // add the speed noise and calc together and times by delta time, and add to P
        p += dt * speed;
        // subtract its intiger to leave it as a 0. someting
        p -= (int)p;
        // set the new phase value
        phase = p;


        //float oscil = Mathf.Sin((cohPhi - phase) * (2 * Mathf.PI));
        rendr.material.color = Color.Lerp(col0, col1, phase);

    }

    // main comparing function 
    protected void Coherence()
    {

        // times the points value by 2*Pi
        var theta = phase * CIRCLE_IN_RADIAN;

        // get the phase pos on the circle
        var phaseX = Mathf.Cos(theta);
        var phaseY = Mathf.Sin(theta);
       

        // loop over the sentinels
        for (var y = 0; y < sentinels.Length; y++)
        {
            // find the distnace to this agent
            float distance = Vector3.Distance(sentinels[y].transform.position, transform.position);
            if (distance < couplingRange)// if less than coupling range
            {
                // get the kuramoto component
                KuramotoSentinelAgent sentinel = sentinels[y].GetComponent<KuramotoSentinelAgent>();
               
               // sentinel.AddOsiclation(phaseX, phaseY, 3);

               
                float normDist = distance / couplingRange;


                normDist *= (rbEffectsRange.y- rbEffectsRange.x);
                normDist += rbEffectsRange.x;

                // get the vector between the two, scale it by the oscilation difference and add to the rb velocity;
                //sentinels[y].GetComponent<Rigidbody>().velocity += (transform.position - sentinels[y].transform.position) * sigDst;
                sentinels[y].GetComponent<Rigidbody>().mass = normDist;
                sentinels[y].GetComponent<Rigidbody>().drag = normDist;
                sentinel.noiseScl = normDist ;
                // draw a line for the connection
                Debug.DrawLine(sentinels[y].transform.position, transform.position, Color.red);

                played = true;
            
            }

        }
       

    }
 
}
