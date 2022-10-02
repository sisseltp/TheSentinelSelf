using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KuramotoAffectedAgent : MonoBehaviour
{


    [Header("Debugging Atrributes (just for looking)")]
    public float speedBPM; // speed force of the 
    public float speed;
    public float phase; // the phase its in 
    public float couplingRange = 1; // distance it will couple
    public float noiseScl = 1; // amount of noise
    public float coupling = 0.5f; // coupling scaler
    public float speedVariation = 0.1f; // amount to randomize data
    public float attractionSclr = 0.5f;// Scales Attraction
    public int played = 0; // if the player has been in contact
    public bool dead = false; // collision killer
    public float fitness=0; // holds this agents fitness value
    public float age = 0; // holds this agents age



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
        played = 1;
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
        played = 1;

    }


    
}
