using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAntigenKey : MonoBehaviour, ITargetable
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
    public Vector3 targetPoint => origin;

    void Start()
    {
        origin = transform.position;
    }

    public void Reset()
    {
        origin = transform.position;
        antigen = new Genetics.Antigen(keyLength);
    }

    public void TimeOut()
    {
        IEnumerator coroutine= TimedDisolve(fadeTime);
        StartCoroutine(coroutine);
    }

    private IEnumerator TimedDisolve(float waitTime)
    {
        float finish = Time.time + waitTime;
        float step = transform.localScale.x / waitTime;
        float scl = transform.localScale.x;

        while (Time.time<finish)
        {
            scl -= step;
            if (scl <= step*2) { Destroy(gameObject); }
            transform.localScale = new Vector3(scl, scl, scl);
            yield return new WaitForSeconds(waitTime / 10);
        }
    }
}
