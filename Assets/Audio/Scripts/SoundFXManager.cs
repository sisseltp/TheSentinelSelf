using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{

    [Tooltip("Sources to play when approaching the body")]
    public AudioSource[] approachBodyAudioSources;

    [Tooltip("Sources to play when entering the body")]
    public AudioSource[] enterBodyAudioSources;

    [Tooltip("Sources to play when exiting the body")]
    public AudioSource[] exitBodyAudioSources;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play(string tag) {

        Debug.Log("Play: " + tag);

        switch(tag) {
            case "ApproachBody":
                // In case the exit sources are playing...
                foreach(AudioSource src in exitBodyAudioSources) {
                    src.Play();
                }
                // Play approach sources...
                foreach(AudioSource src in approachBodyAudioSources) {
                    src.Play();            
                }
                break;

            case "EnterBody":

                // Play enter sources...
                foreach(AudioSource src in enterBodyAudioSources) {
                    src.Play();            
                }

                // Stop approach sources after 5 seconds
                Invoke("StopApproachSounds", 5.0f);


                break;
            
            case "ExitBody":
                // In case any of the approach or enter sources are already playing..
                foreach(AudioSource src in approachBodyAudioSources) {
                    src.Stop();            
                }
                foreach(AudioSource src in enterBodyAudioSources) {
                    src.Stop();            
                }

                // Play exit sources
                foreach(AudioSource src in exitBodyAudioSources) {
                    src.Play();
                }
                break;

            default:
                throw new Exception("Unknown Play tag to SoundFXManager: " + tag);
        }
    }

    void StopApproachSounds() {
        foreach(AudioSource src in approachBodyAudioSources) {
            src.Stop();            
        }
    }
}
