using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class AlgyController : MonoBehaviour
{

    private ParticleSystem.MainModule pSyst;
    private ParticleSystem.EmissionModule pSystm;

    [FormerlySerializedAs("MaxSclr")] 
    [SerializeField]
    private int maxSclr = 30;

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

    void Start()
    {
        pSyst = GetComponent<ParticleSystem>().main;
        pSystm = GetComponent<ParticleSystem>().emission;

        StartCoroutine(CheckWorld(10));
    }

    IEnumerator CheckWorld(float time)
    {
        // TODO: This is not good code fix at some point
        while (true)
        {
            GameObject[] eggs = GameObject.FindGameObjectsWithTag("Eggs");
            
            float eggScl = eggs.Length;
            
            eggScl /= maxSclr;

            Color lerp1 = Color.Lerp(healthy1, unhealthy1, eggScl);

            Color lerp2 = Color.Lerp(healthy2, unhealthy2, eggScl);

            pSyst.startColor = new ParticleSystem.MinMaxGradient(lerp1, lerp2);

            float min = Mathf.Lerp(healthySizeMin, unhealthySizeMin, eggScl);
            float max = Mathf.Lerp(healthySizeMax, unhealthySizeMax, eggScl);

            pSyst.startSize = new ParticleSystem.MinMaxCurve(min,max);

            float emission = Mathf.Lerp(healthyEmmisionRate, unhealthyEmmisionRate, eggScl);

            pSystm.rateOverTime = emission;

            yield return new WaitForSeconds(time);
        }
    }
}