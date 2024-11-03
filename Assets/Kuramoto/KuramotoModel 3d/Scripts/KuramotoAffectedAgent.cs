using UnityEngine;

public class KuramotoAffectedAgent : MonoBehaviour
{
    [Header("Debugging Atrributes (just for looking)")]
    public float speedBPM = 60f; // speed force of the 
    public float speed = 1f;
    public float phase = 1f;
    public float couplingRange = 1f; // distance it will couple
    public float noiseScl = 1f; // amount of noise
    public float coupling = 0.5f; // coupling scaler
    public float attractionSclr = 0.5f;// Scales Attraction
    public float fitness = 0f; // holds this agents fitness value
    public int played = 0; // if the player has been in contact
    public float age = 0f; // holds this agents age
    public bool dead = false; // collision killer

    public void Setup(Vector2 noiseRange, Vector2 couplingRanges, Vector2 SpeedRange, Vector2 couplingScl, Vector2 attractionSclRange)
    {
        float[] settings = new float[] {
            UnityEngine.Random.Range(SpeedRange.x, SpeedRange.y),
            UnityEngine.Random.Range(noiseRange.x, noiseRange.y),
            UnityEngine.Random.Range(couplingScl.x, couplingScl.y),
            UnityEngine.Random.Range(couplingRanges.x, couplingRanges.y),
            UnityEngine.Random.Range(attractionSclRange.x, attractionSclRange.y)};

        SetupData(settings);
    }

    public void SetupData(float[] settingsData)
    {
        speedBPM = settingsData[0];
        speed = speedBPM/60f;
        phase = speed;
        noiseScl = settingsData[1];
        coupling = settingsData[2];
        couplingRange = settingsData[3];
        attractionSclr = settingsData[4];

        fitness = 0f;
        age = 0f;
        dead = false;
        played = 1;
    }
}
