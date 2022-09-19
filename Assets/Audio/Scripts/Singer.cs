using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Singer
{
    public GameObject gameObject;

    public List<AudioSource> sources = new List<AudioSource>();

    private int activeSource = -1;

    public AudioSource GetActiveSource() {
        return sources[activeSource];
    }


    // Play the next AudioSource in the sequence of AudioSources
    // This becomes the current active AudioSource.
    public void PlayNextSource() {
        int nextSource = activeSource + 1;
        if(nextSource >= sources.Count) {
            nextSource = 0;
        }

        Debug.Log("Play Source: nextSource -- " + gameObject);
        sources[nextSource].Play();
        activeSource = nextSource;
    }

    // Load the AudioClip into the next inactive AudioSource
    // in the sequence.
    public void CueClip(AudioClip ac) {
        int nextSource = activeSource + 1;
        if(nextSource >= sources.Count) {
            nextSource = 0;
        }
        sources[nextSource].clip = ac;
    }

    public AudioSource AddSource(
        AudioClip clip=null, 
        float spatialBlend=1.0f, 
        bool loop=false, 
        float doppler=0.2f, 
        float maxDistance=300.0f, 
        float minDistance=10.0f,
        AudioRolloffMode rolloff=AudioRolloffMode.Logarithmic
        ) {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.clip = clip;
            newSource.spatialBlend = spatialBlend;
            newSource.loop = loop; 
            newSource.maxDistance = maxDistance;
            newSource.minDistance = minDistance;
            newSource.dopplerLevel = doppler;
            sources.Add(newSource);

            return newSource;
    }

}
