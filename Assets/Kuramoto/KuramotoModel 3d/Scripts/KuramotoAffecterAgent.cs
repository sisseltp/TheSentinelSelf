using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KuramotoAffecterAgent : MonoBehaviour
{
 

    [Header("Debugging Atrributes (just for looking)")]
    public float speedBPM; // driving force for the phase
    public float speed; // driving force for the phase
    public float phase; // holds the phase position
    public float couplingRange = 1; // holds the distance to the coupling range
    public float noiseScl = 1; // scales the noise added
    public float coupling = 0.5f; // scales the coupling effect
    public float speedVariation = 0.1f; // variation to randomise speed
    public float attractionSclr = 0.5f;
    public bool dead = false;// dead trigger
    public float fitness = 0;// fitness rating for the agent
    public int played = 0;
    public float age = 0;


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
        speed = speedBPM / 60;
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





}
