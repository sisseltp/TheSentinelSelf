using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public enum KuramotoSongBehavior { Swelling, Shouting }

public enum SongScale {Scriabin, Sikah, WholeTone, Lydian, Bartok, Major};




public class PathogenSong : MonoBehaviour
{
    public static Dictionary<SongScale, float[]> SongScaleRatios = new Dictionary<SongScale, float[]>
    {
    {SongScale.Scriabin, new [] {  0.18728838461084f,  0.21022410381474f,  0.25000000000139f,  0.26486577359123f,  0.31498026247517f,  0.37457676922064f,  0.42044820762831f,  0.50000000000139f,  0.52973154718099f,  0.6299605249486f,  0.74915353843921f,  0.8408964152543f,   }}, 
    {SongScale.Sikah, new [] {  0.09364419230568f,  0.11798428908622f,  0.14030775603977f,  0.16685498177245f,  0.21022410381474f,  0.25000000000139f,  0.29730177875212f,  0.37457676922064f,  0.47193715634226f,  0.56123102415598f,  0.6674199270861f,  0.8408964152543f,   }}, 
    {SongScale.WholeTone, new [] {  0.25000000000139f,  0.28061551207877f,  0.31498026247517f,  0.35355339059474f,  0.39685026299352f,  0.44544935907161f,  0.50000000000139f,  0.56123102415598f,  0.6299605249486f,  0.70710678118753f,  0.79370052598483f,  0.89089871814075f,   }}, 
    {SongScale.Lydian, new [] {  0.31498026247517f,  0.35355339059474f,  0.37457676922064f,  0.42044820762831f,  0.47193715634226f,  0.50000000000139f,  0.56123102415598f,  0.6299605249486f,  0.70710678118753f,  0.74915353843921f,  0.8408964152543f,  0.94387431268191f,  }}, 
    {SongScale.Bartok, new [] {  0.31498026247517f,  0.33370996354397f,  0.37457676922064f,  0.39685026299352f,  0.44544935907161f,  0.50000000000139f,  0.56123102415598f,  0.6299605249486f,  0.6674199270861f,  0.74915353843921f,  0.79370052598483f,  0.89089871814075f,  }}, 
    {SongScale.Major, new   [] {  0.31498026247517f,  0.33370996354397f,  0.37457676922064f,  0.42044820762831f,  0.47193715634226f,  0.50000000000139f,  0.56123102415598f,  0.6299605249486f,  0.6674199270861f,  0.74915353843921f,  0.8408964152543f,  0.94387431268191f,  }}, 
    };

    [Tooltip("Kuramoto behavior swelling / shouting")]
    public KuramotoSongBehavior behavior = KuramotoSongBehavior.Shouting;

    [Tooltip("At what value of the kuramoto phase to shout (only if shouting)")]
    [SerializeField]
    private float shoutAtKuramotoPhase = 0.0f;

    [Tooltip("Scale across which voices are distributed")]
    public SongScale scale = SongScale.Lydian;

    [Tooltip("Pitch multipliers based on Scale, one of these is chosen randomly on Awake")]
    [SerializeField]    
    private float[] pitchMultipliers;
    
    [Tooltip("Chance of disabling AudioSource on Awake (useful if there are very many Pathogens singing)")]
    [SerializeField]
    private float percentDisabled = 0.5f;



    [Header("Debugging Attributes (don't edit these, just look!)")]

    [Tooltip("The pitch multiplier assigned to this singer")]    
    [SerializeField]
    private float pitchMultiplier = 1.0f;

    [Tooltip("Maximum volume level per pathogen")]    
    [SerializeField]
    private float maxVolume = 1.0f;

    [Tooltip("Current volume of this pathogen")]    
    [SerializeField]
    private float currentVolume = 1.0f;

    private bool shoutTriggered = false;
    private float lastPhase = 0.0f;
    private AudioSource audioSource;
    private KuramotoAffectedAgent kuramoto;

    // Use for initialization...
    // Called before Start (e.g. before the first frame will be run)
    void Awake() {
        audioSource = GetComponent<AudioSource>();
        maxVolume = audioSource.volume;

        // Set pitchMultipliers based on scale.
        pitchMultipliers = SongScaleRatios[scale];

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
            audioSource.pitch = audioSource.pitch * pitchMultiplier;

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
            currentVolume = 0.5f * (Mathf.Sin(kuramoto.phase * 2.0f * Mathf.PI) + 1.0f);
            audioSource.volume = currentVolume * maxVolume;
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
