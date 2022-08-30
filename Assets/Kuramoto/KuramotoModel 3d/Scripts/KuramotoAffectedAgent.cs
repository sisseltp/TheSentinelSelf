using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KuramotoAffectedAgent : MonoBehaviour
{
    // to hold this object rigid body component
    private Rigidbody rb;
    // to hold the renderer component

    private Renderer rendr;
    [Tooltip("colour 1 to lerp between")]
    [SerializeField]
    private Color col0;// phase col1
    [Tooltip("colour 2 to lerp between")]
    [SerializeField]
    private Color col1; // phase col2

    private Transform[] sentinals; // list of the other sentinels transforms

    [Header("Debugging Atrributes (just for looking)")]
    public float speedBPM; // speed force of the 
    public float speed;
    public float phase; // the phase its in 
    public float cohPhi; // the agle to positive x-axis
    public float coherenceRadius; // distance to 0
    public float couplingRange = 1; // distance it will couple
    public float noiseScl = 1; // amount of noise
    public float coupling = 0.5f; // coupling scaler
    public float speedVariation = 0.1f; // amount to randomize data
    public float attractionSclr = 0.5f;// Scales Attraction
    public float Connections = 0; // num neighbors
    public int played = 0; // if the player has been in contact
    public bool dead = false; // collision killer
    public float fitness=0; // holds this agents fitness value
    public float age = 0; // holds this agents age
    // holds the x,y position of the phase
    public float sumx = 0f;
    public float sumy = 0f;

   

    // Start is called before the first frame update
    void Start()
    {
        // attach the rigidbody component
        rb = GetComponent<Rigidbody>();

        // attach the renderer component
        rendr = GetComponent<Renderer>();


    }

    public void Setup(Vector2 noiseRange, Vector2 couplingRanges, Vector2 SpeedRange, Vector2 couplingScl, Vector2 attractionSclRange, float thisSpeedVariation = 0.1f)
    {
        speedBPM = UnityEngine.Random.Range(SpeedRange.x, SpeedRange.y);
        speed = speedBPM/60;
        phase = speed * UnityEngine.Random.Range(1f - thisSpeedVariation, 1f + thisSpeedVariation);
        noiseScl = UnityEngine.Random.Range(noiseRange.x, noiseRange.y);
        coupling = UnityEngine.Random.Range(couplingScl.x, couplingScl.y);
        couplingRange = UnityEngine.Random.Range(couplingRanges.x, couplingRanges.y);
        attractionSclr = UnityEngine.Random.Range(attractionSclRange.x, attractionSclRange.y);
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
        rb.velocity = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
       

        // set the material to lerp between the the 2 cols by the phase
        rendr.material.color = Color.Lerp(col0, col1, phase);
        rendr.material.SetFloat("Phase", phase);
     

    }

    
}
