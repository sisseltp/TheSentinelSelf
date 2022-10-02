using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KuramotoPlasticAgent : MonoBehaviour
{


    [Header("Debugging Atrributes (just for looking)")]
    public float speed; // driving force for the phase
    public float speedBPM;
    public float phase; // holds the phase position
    public float coherenceRadius; //holds the phase distance to 0,0
    public float couplingRange = 1; // holds the distance to the coupling range
    public float noiseScl = 1; // scales the noise added
    public float coupling = 0.5f; // scales the coupling effect
    public float speedVariation = 0.1f; // variation to randomise speed
    public float attractionSclr = 0.5f;// Scales Attraction
    public bool dead = false;
    public float age = 0;
    public float fitness;



    public void Setup(Vector2 noiseRange, Vector2 couplingRanges, Vector2 SpeedRange, Vector2 couplingScl, Vector2 attractionSclRange, float thisSpeedVariation = 0.1f)
    {
        speedBPM = UnityEngine.Random.Range(SpeedRange.x, SpeedRange.y);
        speed = speedBPM/60;
        phase = speed * UnityEngine.Random.Range(1f - thisSpeedVariation, 1f + thisSpeedVariation);
        noiseScl = UnityEngine.Random.Range(noiseRange.x, noiseRange.y);
        coupling = UnityEngine.Random.Range(couplingScl.x, couplingScl.y);
        couplingRange = UnityEngine.Random.Range(couplingRanges.x, couplingRanges.y);
        attractionSclr = UnityEngine.Random.Range(attractionSclRange.x, attractionSclRange.y);

        age = 0;
        dead = false;
    }

}
