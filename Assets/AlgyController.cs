using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgyController : MonoBehaviour
{

    private ParticleSystem.MainModule PSyst;
    private ParticleSystem.EmissionModule PSystm;

    [SerializeField]
    private int MaxSclr = 30;

    [SerializeField]
    private Color healthy1;
    [SerializeField]
    private Color healthy2;

    [SerializeField]
    private float healthySizeMin;
    [SerializeField]
    private float healthySizeMax;

    [SerializeField]
    private Color unhealthy1;
    [SerializeField]
    private Color unhealthy2;

    [SerializeField]
    private float unhealthySizeMin;
    [SerializeField]
    private float unhealthySizeMax;

    [SerializeField]
    private float healthyEmmisionRate;
    [SerializeField]
    private float unhealthyEmmisionRate;


    // Start is called before the first frame update
    void Start()
    {
        PSyst = GetComponentInChildren<ParticleSystem>().main;
        PSystm = GetComponentInChildren<ParticleSystem>().emission;

        StartCoroutine(checkWorld(10));
    }



    IEnumerator checkWorld(float time)
    {
        while (true)
        {
            GameObject[] eggs = GameObject.FindGameObjectsWithTag("Eggs");
            
            float eggScl = eggs.Length;
            
            eggScl /= (float)MaxSclr;

            Color lerp1 = Color.Lerp(healthy1, unhealthy1, eggScl);

            Color lerp2 = Color.Lerp(healthy2, unhealthy2, eggScl);

            PSyst.startColor = new ParticleSystem.MinMaxGradient(lerp1, lerp2);

            float min = Mathf.Lerp(healthySizeMin, unhealthySizeMin, eggScl);
            float max = Mathf.Lerp(healthySizeMax, unhealthySizeMax, eggScl);

            PSyst.startSize = new ParticleSystem.MinMaxCurve(min,max);

            float emission = Mathf.Lerp(healthyEmmisionRate, unhealthyEmmisionRate, eggScl);

            PSystm.rateOverTime = emission;

            yield return new WaitForSeconds(time);
        }


    }


}



