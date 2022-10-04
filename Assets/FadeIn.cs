using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    private Renderer rndr;
    [SerializeField]
    private float fadeTime = 10;
    [SerializeField]
    private int steps = 15;
    [SerializeField]
    private float waitTimeS = 360;


    public float fadeVal = 0;
    // Start is called before the first frame update
    void Start()
    {
        rndr = GetComponent<Renderer>();
        StartCoroutine(TimedDisolve(fadeTime, waitTimeS));
    }
    private IEnumerator TimedDisolve(float fade, float waitTime)
    {
        float stepSize = fadeTime / (float)steps;

        float timeGate = Time.time + waitTime;

        while (Time.time < timeGate)
        {
            
            fadeVal += (float)1 / (float)steps;

            rndr.material.SetFloat("fadeVal", fadeVal);
       
            yield return new WaitForSeconds(stepSize);
        }

        yield return new WaitForSeconds(waitTime);

         timeGate = Time.time + waitTime;
        while (Time.time < timeGate)
        {

            fadeVal -= (float)1 / (float)steps;

            rndr.material.SetFloat("fadeVal", fadeVal);

            yield return new WaitForSeconds(stepSize);
        }

        Destroy(this.transform.parent.gameObject);
    }

}
