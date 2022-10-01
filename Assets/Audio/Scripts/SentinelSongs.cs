using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public class SentinelSongs : MonoBehaviour
{
    [Tooltip("Path to resources folder with sentinel chant audio clips")]
    public string callClipsPath;

    [Space(5)]

    // A general parameter that combines a few different qualities
    // for how "alive" the chanting is... can be used as the world becomes
    // deadened by microplastic intervention..
    [Range(0.0f, 1.0f)]
    public float liveliness = 0.5f;

    // TODO: Add "fear" quality to sentinel chanting
    [Tooltip("Not yet implemented")]
    [Range(0.0f, 1.0f)]
    public float fear = 0.1f;


    [Tooltip("Number of chanting voices")]
    [Range(1, 10)]
    public int numVoices = 4;

    private AudioSource audioSource;


    [Space(10)]
    [Header("Debug")]

    // These values are set indirectly by the liveness
    // parameter.
    public float period = 1.0f;
    public float bpm = 50.0f;


    [SerializeField]
    private bool singing = false; 
    private IEnumerator singingCoroutine;

    public List<AudioClip> callClips = new List<AudioClip>();

    // Use for initialization...
    // Called before Start (e.g. before the first frame will be run)
    void Awake() {

        singingCoroutine = SentinelSinging();
        audioSource = GetComponent<AudioSource>();

        foreach(AudioClip ac in Resources.LoadAll(callClipsPath, typeof(AudioClip))) {
            callClips.Add(ac);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        if(singing) {
            StartCoroutine(singingCoroutine);
        }
    }
   
    private IEnumerator SentinelSinging()
    {
        while (true)
        {
            // Calculate the BPM and Period from liveness
            bpm = (liveliness * 30) + 5;
            period = 60.0f / bpm;


            // Choose n random audioclips
            // Play them one-shot style
            for(int i = 0; i < numVoices; i++) {
                AudioClip clip = callClips[Random.Range(0, callClips.Count)];
                audioSource.PlayOneShot(clip, 1.0f);
            }

            yield return new WaitForSeconds(period); 
        }
    }

}
