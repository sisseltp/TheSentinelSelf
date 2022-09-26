using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public enum APCState { SeekingPathogens, CarryingAntigens }    

public class APCSong : MonoBehaviour
{
    
    [SerializeField]
    private string seekingClipsPath;

    [SerializeField]
    private string carryingClipsPath;

    public float minDistance = 1.0f;
    public float maxDistance = 50.0f;
    public AudioRolloffMode rolloff = AudioRolloffMode.Logarithmic;

    private Singer singer;

    public APCState state = APCState.SeekingPathogens;

    private List<AudioClip> seekingPathogenClips = new List<AudioClip>();
    private List<AudioClip> carryingAntigenClips = new List<AudioClip>();
    
    private List<AudioClip> currentClipSource;

    public AudioMixerGroup audioMixerGroup;

    // Use for initialization...
    // Called before Start (e.g. before the first frame will be run)
    void Awake() {
        singer = new Singer(gameObject);

        // TODO: randomly choose 3 from each category
        foreach(AudioClip ac in Resources.LoadAll(seekingClipsPath, typeof(AudioClip))) {
            seekingPathogenClips.Add(ac);
        }
        foreach(AudioClip ac in Resources.LoadAll(carryingClipsPath, typeof(AudioClip))) {
            carryingAntigenClips.Add(ac);
        }

        Debug.Assert(seekingPathogenClips.Count > 0, "No audio files found in seeking path!");
        Debug.Assert(carryingAntigenClips.Count > 0, "No audio files found in carrying path!");


        currentClipSource = seekingPathogenClips;
        // Initialize 2x AudioSource components on APC game object.
        singer.AddSource(
            clip: Singer.GetRandomClip(currentClipSource), 
            spatialBlend: 1.0f, 
            loop: false, 
            maxDistance: maxDistance, 
            minDistance: minDistance, 
            rolloff: rolloff,
            audioMixerGroup: audioMixerGroup
        );
        singer.AddSource(
            clip: Singer.GetRandomClip(currentClipSource), 
            spatialBlend: 1.0f, 
            loop: false, 
            maxDistance: maxDistance, 
            minDistance: minDistance, 
            rolloff: rolloff,
            audioMixerGroup: audioMixerGroup
        );
    }

    // Start is called before the first frame update
    void Start() {
        singer.PlayNextSource(); // start playing...
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void setState(APCState newState) {

        singer.GetActiveSource().Stop();

        switch(newState) {

            case APCState.SeekingPathogens:
                currentClipSource = seekingPathogenClips;
                break;
            case APCState.CarryingAntigens:
                currentClipSource = carryingAntigenClips;
                break;
            default:
                Debug.LogError("Unknown APCState: " + newState);
                break;
        }
        singer.CueClip(Singer.GetRandomClip(currentClipSource));
        PlayNext();
        Debug.Log("Change State: " + newState + " in " + gameObject);
        state = newState;
    }

    // Called every ~20ms
    // See: https://gamedevplanet.com/the-difference-between-update-and-fixedupdate-in-unity/
    void FixedUpdate() {
        // Check if audio is playing
        // if not, play next clip and cue following clip in audiosource
        if (! singer.GetActiveSource().isPlaying ) {
            PlayNext();
        }
    }

    void PlayNext() {
        singer.PlayNextSource();
        singer.CueClip(Singer.GetRandomClip(currentClipSource));
    }
  
}
