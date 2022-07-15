using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KuramotoAffecterAgent : MonoBehaviour
{
    // holds the rendr
    Renderer rendr;

    [Tooltip("colour 1 to lerp between")]
    [SerializeField]
    private Color col0;// phase col1
    [Tooltip("colour 2 to lerp between")]
    [SerializeField]
    private Color col1; // phase col2


    [Header("Debugging Atrributes (just for looking)")]
    public float speedBPM; // driving force for the phase
    public float speed; // driving force for the phase
    public float phase; // holds the phase position
    public float cohPhi; // angle to positive a-xis
    public float coherenceRadius; //holds the phase distance to 0,0
    public float couplingRange = 1; // holds the distance to the coupling range
    public float noiseScl = 1; // scales the noise added
    public float coupling = 0.5f; // scales the coupling effect
    public float speedVariation = 0.1f; // variation to randomise speed
    public float attractionSclr = 0.5f;
    public int Connections = 0; // counts how many links it has within the range
    public bool dead = false;// dead trigger
    public float fitness = 0;// fitness rating for the agent
    public int played = 0;
    public int age = 0;
    public float sumX = 0f;
    public float sumY = 0f;


    //holds the sentinel manager
    private PathogenManager pathogenManager;
    // holds the sentinels
    private GameObject[] sentinels;

    

    public void Setup(Vector2 noiseRange, Vector2 couplingRanges, Vector2 SpeedRange, Vector2 couplingScl, Vector2 attractionScls, float thisSpeedVariation = 0.1f)
    {
        speedBPM = UnityEngine.Random.Range(SpeedRange.x,SpeedRange.y);
        speed =  speedBPM/60;
        phase = speed * UnityEngine.Random.Range(1f - thisSpeedVariation, 1f + thisSpeedVariation);
        noiseScl = UnityEngine.Random.Range(noiseRange.x, noiseRange.y);
        coupling = UnityEngine.Random.Range(couplingScl.x, couplingScl.y);
        couplingRange = UnityEngine.Random.Range(couplingRanges.x, couplingRanges.y);
        attractionSclr = UnityEngine.Random.Range(attractionScls.x, attractionScls.y);
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
        attractionSclr = settingsData[4];
        speed = 60 / speedBPM;
        phase = speed * UnityEngine.Random.Range(1f - thisSpeedVariation, 1f + thisSpeedVariation);
        fitness = 0;
        age = 0;
        dead = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        // hook up rendr component
        rendr = GetComponent<Renderer>();
        // find the sentinel maker
        pathogenManager = GameObject.FindGameObjectWithTag("PathogenEmitter").GetComponent<PathogenManager>();
        
        // link the sentinels as a list
        sentinels = pathogenManager.sentinels ;
        
    }

    // Update is called once per frame
    void Update()
    {
        // if the num of sentinels changes relink them
        if (pathogenManager.nSentinels != sentinels.Length)
        {
            sentinels = pathogenManager.sentinels;
        }
        

        // ad the amount of partners * sclr to the fitness
        fitness += Connections *Time.deltaTime;

        //float oscil = Mathf.Sin((cohPhi - phase) * (2 * Mathf.PI));
        rendr.material.color = Color.Lerp(col0, col1, phase);

    }

    // if it collides with the surrounding area it dies
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Kill")
        {
            dead = true;
        }
    }

}
