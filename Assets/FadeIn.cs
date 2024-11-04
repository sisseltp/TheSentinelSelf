using System.Collections;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    private static readonly int FadeVal = Shader.PropertyToID("fadeVal");
    
    private new Renderer renderer;
    [SerializeField]
    private float fadeTime = 10;
    [SerializeField]
    private int steps = 15;
    [SerializeField]
    private float waitTimeS = 360;

    public float fadeVal;

    private void Start()
    {
        renderer = GetComponent<Renderer>();
        StartCoroutine(TimedDisolve(fadeTime, waitTimeS));
    }
    
    // TODO: Remove while loop
    private IEnumerator TimedDisolve(float fade, float waitTime)
    {
        float stepSize = fade / steps;

        while (fadeVal<1)
        {
            fadeVal += 1f / steps;

            renderer.material.SetFloat(FadeVal, fadeVal);
       
            yield return new WaitForSeconds(stepSize);
        }

        yield return new WaitForSeconds(waitTime);

        while (fadeVal>0)
        {

            fadeVal -= 1f / steps;

            renderer.material.SetFloat(FadeVal, fadeVal);

            yield return new WaitForSeconds(stepSize);
        }

        Destroy(transform.parent.gameObject);
    }

}
