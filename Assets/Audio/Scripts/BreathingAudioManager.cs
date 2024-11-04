using UnityEngine;

public class BreathingAudioManager : MonoBehaviour
{
    public AudioSource[] breathingAudioSources;

    [Range(0.0f, 1.0f)]
    public float pitchRandomization;

    void Awake() 
    {
        foreach(AudioSource asrc in breathingAudioSources)
            asrc.pitch = asrc.pitch + Random.Range(-pitchRandomization, pitchRandomization);
    }

    void Start()
    {
        foreach(AudioSource asrc in breathingAudioSources)
            asrc.Play();
    }

    void Update()
    {
        foreach(AudioSource asrc in breathingAudioSources)
            asrc.volume = HeartRateManager.Instance.GlobalPhaseMod1;
    }
}
