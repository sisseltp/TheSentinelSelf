using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public class SentinelSongs : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float liveliness = 0.5f;

    [Range(0.0f, 1.0f)]
    public float fear = 0.1f;

    private AudioSource audioSource;

    public float period = 1.0f;
    public float bpm = 50.0f;

    public string callClipsPath;

    public int numVoices = 4;

    public bool singing = true;

    public List<AudioClip> callClips = new List<AudioClip>();

    // Use for initialization...
    // Called before Start (e.g. before the first frame will be run)
    void Awake() {

        audioSource = GetComponent<AudioSource>();

        foreach(AudioClip ac in Resources.LoadAll(callClipsPath, typeof(AudioClip))) {
            callClips.Add(ac);
        }

        bpm = (liveliness * 120) + 20;
        period = bpm / 60.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SentinelSinging(bpm / 60.0f));
    }
   
    private IEnumerator SentinelSinging(float waitTime)
    {
        while (singing)
        {
            // Choose n random audioclips
            // Play them one-shot style
            for(int i = 0; i < numVoices; i++) {
                AudioClip clip = callClips[Random.Range(0, callClips.Count)];
                audioSource.PlayOneShot(clip, 1.0f);
            }

            yield return new WaitForSeconds(period); 
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
