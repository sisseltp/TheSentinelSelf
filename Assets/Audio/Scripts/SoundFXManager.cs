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

    [Space(10)]
    [Header("Voice Over")]

    [Tooltip("Path to voiceover clips in resources directory")]
    [SerializeField]
    private string voiceoverClipsPath;

    [Tooltip("Voiceover audio source")]
    public AudioSource voiceoverSource;

    [Space(10)]
    [Header("Debug")]

    [SerializeField]
    private bool voiceoverPlaying = false;

    [SerializeField]
    private int currentVoiceoverClip = -1;

    [SerializeField]
    private List<AudioClip> voiceoverClips = new List<AudioClip>();

    void Awake() {
        voiceoverSource.enabled = false;

        foreach(AudioClip ac in Resources.LoadAll(voiceoverClipsPath, typeof(AudioClip))) {
            voiceoverClips.Add(ac);
        }
        Debug.Assert(voiceoverClips.Count > 0, "No audio files found in voiceover clips path!");

    }

    // Start is called before the first frame update
    void Start()
    {
        // Start voiceover...
        Play("VoiceOver");
        
    }

    void FixedUpdate() {
        // Check if audio is playing
        // if not, play next voice clip
        if (voiceoverPlaying && (! voiceoverSource.isPlaying) ) {
            PlayNextVoiceoverClip();
        }
    }

    void PlayNextVoiceoverClip() {
        currentVoiceoverClip++;
        if(currentVoiceoverClip >= voiceoverClips.Count) {
            currentVoiceoverClip=0;
        }
        voiceoverSource.Stop(); // if necessary? maybe fadeout would be nicer...
        // Changing the clip and playing immediately can cause glitches.. ideally do with Invoke
        //  or PlayDelayed...
        // TODO: Probably should wait a moment here!
        voiceoverSource.clip = voiceoverClips[currentVoiceoverClip];
        voiceoverSource.PlayDelayed(0.1f); // play in 100ms
    }

    // Used to play sources.
    public void Play(string tag) {
        Debug.Log("Play: " + tag);
        switch(tag) {
            case "VoiceOver":
                // Start voiceover playback routine...
                voiceoverSource.enabled = true;
                voiceoverPlaying = true;
                break;
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
                Invoke("StopApproachSounds", 2.0f);
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

    // Used to Stop sources
    public void Stop(string tag) {
        Debug.Log("Stop: " + tag);

        switch(tag) {

            case "VoiceOver":
                voiceoverPlaying = false;
                // TODO: Fade out and then disable everything...
                voiceoverSource.Stop();
                voiceoverSource.enabled = false;
                break;

            case "ApproachBody":
                foreach(AudioSource src in approachBodyAudioSources) {
                    src.Stop();            
                }
                break;

            case "EnterBody":
                foreach(AudioSource src in enterBodyAudioSources) {
                    src.Stop();            
                }
                break;
            
            case "ExitBody":
                foreach(AudioSource src in exitBodyAudioSources) {
                    src.Stop();
                }
                break;

            default:
                throw new Exception("Unknown Stop tag to SoundFXManager: " + tag);
        }
    }

    void StopApproachSounds() {
        Stop("ApproachBody");
    }
}
