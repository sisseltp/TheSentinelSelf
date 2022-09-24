using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public enum APCState { SeekingPathogens, CarryingAntigens }    

public class APCSong : MonoBehaviour
{
    
    public string clipsPath;

    private Singer singer;

    private APCState state = APCState.SeekingPathogens;

    public List<AudioClip> seekingPathogenClips = new List<AudioClip>();
    public List<AudioClip> carryingAntigenClips = new List<AudioClip>();
    private List<AudioClip> clipSource;

    // Use for initialization...
    // Called before Start (e.g. before the first frame will be run)
    void Awake() {
        singer = new Singer(gameObject);

        // TODO: randomly choose 3 from each category
        foreach(AudioClip ac in Resources.LoadAll(clipsPath + "/seeking", typeof(AudioClip))) {
            seekingPathogenClips.Add(ac);
        }
        foreach(AudioClip ac in Resources.LoadAll(clipsPath + "/carrying", typeof(AudioClip))) {
            carryingAntigenClips.Add(ac);
        }

        clipSource = seekingPathogenClips;
        // Initialize 2x AudioSource components on APC game object.
        singer.AddSource(clip: Singer.GetRandomClip(clipSource), spatialBlend: 1.0f, loop: false, maxDistance: 50, minDistance: 1, rolloff: AudioRolloffMode.Logarithmic);
        singer.AddSource(clip: Singer.GetRandomClip(clipSource), spatialBlend: 1.0f, loop: false, maxDistance: 50, minDistance: 1, rolloff: AudioRolloffMode.Logarithmic);
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
                clipSource = seekingPathogenClips;
                break;
            case APCState.CarryingAntigens:
                clipSource = carryingAntigenClips;
                break;
            default:
                Debug.LogError("Unknown APCState: " + newState);
                break;
        }
        singer.CueClip(Singer.GetRandomClip(clipSource));
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
        singer.CueClip(Singer.GetRandomClip(clipSource));
    }
  
}
