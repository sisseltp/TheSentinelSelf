using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public enum KuramotoSongBehavior { Swelling, Shouting }    

public class PathogenSong : MonoBehaviour
{
    [Tooltip("Kuramoto behavior swelling / shouting")]
    public KuramotoSongBehavior behavior = KuramotoSongBehavior.Shouting;

    [Tooltip("At what value of the kuramoto phase to shout (only if shouting)")]
    [SerializeField]
    private float shoutAtKuramotoPhase = 0.0f;

    [Tooltip("Possible pitch multipliers, chosen randomly on Awake")]
    [SerializeField]    
    private float[] pitchMultipliers = new float[] { 0.6299605249486f, 0.6674199270861f, 0.74915353843921f, 0.8408964152543f, 0.94387431268191f, 1.0f, 1.1224620483089f, 1.2599210498937f };
    
    [Tooltip("Chance of disabling AudioSource on Awake (useful if there are very many Pathogens singing)")]
    [SerializeField]
    private float percentDisabled = 0.5f;

    /*

    Scale Ratios..

    Scriabin:
    [ 1.0, 1.0594630943591, 1.2599210498937, 1.4983070768743, 1.6817928305039 ]

    Sikah:
    [ 1.0, 1.0905077326649, 1.2240535433037, 1.3739536474563, 1.4983070768743, 1.6339154532379, 1.8340080864049 ]

    Whole Tone:
    [ 1.0, 1.1224620483089, 1.2599210498937, 1.4142135623711, 1.5874010519653, 1.7817974362766 ]

    Lydian:
    [ 1.0, 1.1224620483089, 1.2599210498937, 1.4142135623711, 1.4983070768743, 1.6817928305039, 1.8877486253586 ]

    Bartok:
    [ 1.0, 1.1224620483089, 1.2599210498937, 1.3348398541685, 1.4983070768743, 1.5874010519653, 1.7817974362766 ]
    
    Major:
     0.6299605249486, 0.6674199270861, 0.74915353843921, 0.8408964152543, 0.94387431268191, 1.0, 1.1224620483089, 1.2599210498937, 1.3348398541685, 1.4983070768743, 1.6817928305039, 1.8877486253586 ]

    */

    [Header("Debugging Attributes (don't edit these, just look!)")]

    [Tooltip("The pitch multiplier assigned to this singer")]    
    [SerializeField]
    private float pitchMultiplier = 1.0f;

    [Tooltip("Current gain/volume")]    
    [SerializeField]
    private float volume = 1.0f;

    private bool shoutTriggered = false;
    private float lastPhase = 0.0f;
    private AudioSource audioSource;
    private KuramotoAffectedAgent kuramoto;

    // Use for initialization...
    // Called before Start (e.g. before the first frame will be run)
    void Awake() {
        audioSource = GetComponent<AudioSource>();
        if(Random.value <= percentDisabled) {
            // Disable this singer.
            audioSource.enabled = false;
            this.enabled = false;
        } else {
            // Continue on as if nothing existentially significant had happened.
            kuramoto = GetComponent<KuramotoAffectedAgent>();
            audioSource.loop = false;
            
            // Choose a random pitch multiplier
            pitchMultiplier =  pitchMultipliers[Random.Range(0, pitchMultipliers.Length)];
            audioSource.pitch = pitchMultiplier;

        }
    }

    // Start is called before the first frame update
    void Start() {
        if(behavior == KuramotoSongBehavior.Swelling) {
            // Play and loop.
            audioSource.loop = true;
            audioSource.Play();
        }

    }

    // Update is called once per animation frame
    // Since the kuramoto model is running at frame-rate (I think?)
    // we do sound synchronization with kuramoto oscillation here.
    void Update() {

        if(behavior == KuramotoSongBehavior.Swelling) {
            // Swelling behavior
            volume = 0.5f * (Mathf.Sin(kuramoto.phase * 2.0f * Mathf.PI) + 1.0f);
            audioSource.volume = volume;
        } else { // Shouting behavior
            if( kuramoto.phase - lastPhase < 0.0f  ) { shoutTriggered = false; }

            if(kuramoto.phase >= shoutAtKuramotoPhase && shoutTriggered == false) {
                audioSource.Play();
                shoutTriggered = true;
            }

            lastPhase = kuramoto.phase;
        }

    }


  
}
