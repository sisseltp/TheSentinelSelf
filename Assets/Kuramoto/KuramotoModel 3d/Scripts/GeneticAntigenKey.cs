using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAntigenKey : MonoBehaviour
{
    [Tooltip("Number of Keys in chain")]
    [SerializeField]
    private int keyLength = 10; // number of steps in cylcle

    public Genetics.Antigen antigen; // list to hold keys in
    [HideInInspector]
    public Vector3 origin;
    [Tooltip("Time it takes to fade away")]
    [SerializeField]
    private float fadeTime = 10;
    [Tooltip("varience in the fade time")]
    [SerializeField]
    private float fadeTimeVarience = 4;

    // Start is called before the first frame update
    void Start()
    {
        // sets it to a new vec3 list for vels
        antigen = new Genetics.Antigen(keyLength);
        origin = transform.position;
    }
    
   
    // reset randomizes the list of vels
    public void Reset()
    {
        
        // sets it to a new vec3 list for vels
        antigen = new Genetics.Antigen(keyLength);

    }

    public void TimeOut()
    {
         IEnumerator coroutine= TimedDisolve(fadeTime);
        StartCoroutine(coroutine);
    }

    private IEnumerator TimedDisolve(float waitTime)
    {
       // waitTime += UnityEngine.Random.Range(-fadeTimeVarience, fadeTimeVarience);
        float finish = Time.time + waitTime;
        float step = transform.localScale.x / waitTime;
        float scl = transform.localScale.x;
        while (Time.time<finish)
        {
            scl -= step;
            if (scl <= step*2) { Destroy(gameObject); }
            transform.localScale = new Vector3(scl, scl, scl);
            yield return new WaitForSeconds(waitTime / 10);
            //print("WaitAndPrint " + Time.time);
        }

    }
}
