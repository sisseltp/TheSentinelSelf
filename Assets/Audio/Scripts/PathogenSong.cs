using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public class PathogenSong : MonoBehaviour
{
    private AudioSource audioSource;
    private KuramotoAffectedAgent kuramoto;

    public float gain = 1.0f;

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

    public float[] pitchRange = new float[] { 0.6299605249486f, 0.6674199270861f, 0.74915353843921f, 0.8408964152543f, 0.94387431268191f, 1.0f, 1.1224620483089f, 1.2599210498937f };
    public float pitch = 1.0f;

    // Use for initialization...
    // Called before Start (e.g. before the first frame will be run)
    void Awake() {

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        pitch =  pitchRange[Random.Range(0, pitchRange.Length)];
        audioSource.pitch = pitch;
        kuramoto = GetComponent<KuramotoAffectedAgent>();

    }

    // Start is called before the first frame update
    void Start() {

        audioSource.Play(); // start playing...

    }

    // Update is called once per animation frame
    // Since the kuramoto model is running at frame-rate
    // we do sound synchronization with kuramoto oscillation here.
    void Update() {
        gain = 0.5f * (Mathf.Sin(kuramoto.phase * 2.0f * Mathf.PI) + 1.0f);
        audioSource.volume = gain;

    }


  
}
