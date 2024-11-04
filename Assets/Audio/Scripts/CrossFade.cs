    using UnityEngine;
    using System.Collections;
     
    /*
        Credit Igor Aherne. Feel free to use as you wish, but mention me in credits :)
        www.facebook.com/igor.aherne
     
        audio source which holds a reference to Two audio sources, allowing to transition
        between incoming sound and the previously played one.
     
        1) attach this component onto gameObject.
        2) use GetComponent() to get reference to this DoubleAudioSource,
        3) tell it which AudioClip (.mp3, .wav etc) to play.
     
        No need to attach any clips to the _source0 or _source1
        Just call CrossFade and it will smoothly transition to the clip from the 
        currently played one.
        
        _source0 and _source1 are for the component's internal use
        you don't have to worry about them.
    */
     
    [ExecuteInEditMode]
    public class DoubleAudioSource : MonoBehaviour
    {
        private AudioSource source0;
        private AudioSource source1;
     
        #region internal vars

        private bool currentIsSource0 = true; //is _source0 currently the active AudioSource (plays some sound right now)

        private Coroutine currentSourceFadeRoutine;
        private Coroutine newSourceFadeRoutine;
        #endregion
     
     
        #region internal functionality

        private void Reset()
        {
            Update();
        }


        private void Awake()
        {
            Update();
        }


        private void Update()
        {
            //constantly check if our game object doesn't contain audio sources which we are referencing.
     
            //if the _source0 or _source1 contain obsolete references (most likely 'null'), then
            //we will re-init them:
            if (!source0 || !source1)
            {
                // TODO: Code smell, should be sure they exist
                InitAudioSources();
            }
     
        }
     
     
        //re-establishes references to audio sources on this game object:
        private void InitAudioSources()
        {
            //re-connect _source0 and _source1 to the ones in attachedSources[]
            AudioSource[] audioSources = gameObject.GetComponents<AudioSource>();
     
            if (ReferenceEquals(audioSources, null) || audioSources.Length == 0)
            {
                source0 = gameObject.AddComponent<AudioSource>();
                source1 = gameObject.AddComponent<AudioSource>();
                //DefaultTheSource(_source0);
                // DefaultTheSource(_source1);  //remove? we do this in editor only
                return;
            }
     
            switch (audioSources.Length)
            {
                case 1:
                    {
                        source0 = audioSources[0];
                        source1 = gameObject.AddComponent<AudioSource>();
                        //DefaultTheSource(_source1);  //TODO remove?  we do this in editor only
                    }
                    break;
                default:
                    { //2 and more
                        source0 = audioSources[0];
                        source1 = audioSources[1];
                    }
                    break;
            }//end switch
        }
        #endregion
     
     
        //gradually shifts the sound comming from our audio sources to the this clip:
        // maxVolume should be in 0-to-1 range
        public void CrossFade(AudioClip clipToPlay, float maxVolume, float fadingTime, float delayBeforeCrossFade = 0)
        {
            //var fadeRoutine = StartCoroutine(Fade(clipToPlay, maxVolume, fadingTime, delay_before_crossFade));
            StartCoroutine(Fade(clipToPlay, maxVolume, fadingTime, delayBeforeCrossFade));
     
        }//end CrossFade()


        private IEnumerator Fade(AudioClip playMe, float maxVolume, float fadingTime, float delayBeforeCrossFade = 0)
        {
            if (delayBeforeCrossFade > 0)
            {
                yield return new WaitForSeconds(delayBeforeCrossFade);
            }
     
            AudioSource curActiveSource, newActiveSource;
            if (currentIsSource0)
            {
                //_source0 is currently playing the most recent AudioClip
                curActiveSource = source0;
                //so launch on _source1
                newActiveSource = source1;
            }
            else
            {
                //otherwise, _source1 is currently active
                curActiveSource = source1;
                //so play on _source0
                newActiveSource = source0;
            }
     
            //perform the switching
            newActiveSource.clip = playMe;
            newActiveSource.Play();
            newActiveSource.volume = 0;
     
            if (currentSourceFadeRoutine != null)
            {
                StopCoroutine(currentSourceFadeRoutine);
            }
     
            if (newSourceFadeRoutine != null)
            {
                StopCoroutine(newSourceFadeRoutine);
            }
     
            currentSourceFadeRoutine = StartCoroutine(FadeSource(curActiveSource, curActiveSource.volume, 0, fadingTime));
            newSourceFadeRoutine = StartCoroutine(FadeSource(newActiveSource, newActiveSource.volume, maxVolume, fadingTime));
     
            currentIsSource0 = !currentIsSource0;
        }

        private static IEnumerator FadeSource(AudioSource sourceToFade, float startVolume, float endVolume, float duration)
        {
            float startTime = Time.time;
     
            while (true)
            {
                float elapsed = Time.time - startTime;
     
                sourceToFade.volume = Mathf.Clamp01(Mathf.Lerp(startVolume, endVolume, elapsed / duration));
     
                if (Mathf.Approximately(sourceToFade.volume, endVolume))
                {
                    break;
                }
     
                yield return null;
            }//end while
        }
     
     
        //returns false if BOTH sources are not playing and there are no sounds are staged to be played.
        //also returns false if one of the sources is not yet initialized
        public bool IsPlaying
        {
            get
            {
                if (source0 == null || source1 == null)
                {
                    return false;
                }
     
                //otherwise, both sources are initialized. See if any is playing:
                return source0.isPlaying || source1.isPlaying;
            }//end get
        }
     
    }
