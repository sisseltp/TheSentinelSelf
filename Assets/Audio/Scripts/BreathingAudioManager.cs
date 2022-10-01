using System.Collections.Generic;
using UnityEngine;

public class BreathingAudioManager : MonoBehaviour
{
    public AudioSource[] breathingAudioSources;

    [Range(0.0f, 1.0f)]
    public float pitchRandomization = 0.0f;

    // Source for the phase parameter
    public BreathingObjects breathingScript;

    void Awake() {
        foreach(AudioSource asrc in breathingAudioSources) {
            asrc.pitch = asrc.pitch + Random.Range(-pitchRandomization, pitchRandomization);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach(AudioSource asrc in breathingAudioSources) {
            asrc.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach(AudioSource asrc in breathingAudioSources) {
            asrc.volume = breathingScript.phase;
        }

        
    }
}
