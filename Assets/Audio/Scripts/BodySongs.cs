using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public class BodySongs : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float excitement;

    [Range(0.0f, 1.0f)]
    public float shouting;

    public string mellowClipsPath;
    public string excitedClipsPath;
    public string shoutClipsPath;

    public GameObject[] sentinels;

    private List<Singer> singers = new List<Singer>();

    public List<AudioClip> mellowClips = new List<AudioClip>();
    public List<AudioClip> excitedClips = new List<AudioClip>();
    public List<AudioClip> shoutClips = new List<AudioClip>();


    // Use for initialization...
    // Called before Start (e.g. before the first frame will be run)
    void Awake() {
        foreach(AudioClip ac in Resources.LoadAll(mellowClipsPath, typeof(AudioClip))) {
            mellowClips.Add(ac);
        }
        foreach(AudioClip ac in Resources.LoadAll(excitedClipsPath, typeof(AudioClip))) {
            excitedClips.Add(ac);
        }
        foreach(AudioClip ac in Resources.LoadAll(shoutClipsPath, typeof(AudioClip))) {
            shoutClips.Add(ac);
        }

        // Initialize 2x AudioSource components on Sentinel game objects.
        foreach (GameObject s in sentinels) {
            Singer singer = new Singer(s);
            singers.Add(singer);
            singer.AddSource(clip: Singer.GetRandomClip(mellowClips), spatialBlend: 1.0f, loop: false, rolloff: AudioRolloffMode.Linear, maxDistance: 500);
            singer.AddSource(clip: Singer.GetRandomClip(mellowClips), spatialBlend: 1.0f, loop: false, rolloff: AudioRolloffMode.Linear, maxDistance: 500);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (Singer s in singers) {
            s.PlayNextSource(); // start playing...
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Called every ~20ms
    // See: https://gamedevplanet.com/the-difference-between-update-and-fixedupdate-in-unity/
    void FixedUpdate() {
        // Go through each sentinel and check if audio is playing
        // if not, play next clip and cue following clip in audiosource
        foreach(Singer s in singers) {
            if (! s.GetActiveSource().isPlaying ) {
                s.PlayNextSource();

                // Optionally: cue a random clip to play next
                s.CueClip(Singer.GetRandomClip(mellowClips));
            }

        }
    }

}
