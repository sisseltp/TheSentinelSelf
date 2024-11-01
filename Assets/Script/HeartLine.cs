using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartLine : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;
    
    
    public int amountPoints = 2000;
    float[] valuesHeart;
    float[] xs;

    float currentValue = 0f;

    public AnimationCurve curvePeak;

    bool wasOverThreshold = false;
    float threshold = 0.9f;
    float progressPeak = 0f;

    public float speed = 50f;

    Vector3[] finalPositions;

    float progressPerlin = 0f;
    void Start()
    {

        valuesHeart = new float[amountPoints];
        xs = new float[amountPoints];

        for (int i = 0; i < amountPoints; i++)
        {
            xs[i] = Mathf.Lerp(-10f, 10f, i / (amountPoints-1f));
            valuesHeart[i] = 0f;
        }


        finalPositions = new Vector3[amountPoints];

        for (int i = 0; i < amountPoints; i++)
            finalPositions[i] = new Vector3(xs[i], valuesHeart[i], 0f);

        lineRenderer.positionCount = amountPoints;
        lineRenderer.SetPositions(finalPositions);
    }

    void Update()
    {
        if(HeartRateManager.Instance.GlobalPhaseMod1>threshold && !wasOverThreshold)
        {
            wasOverThreshold = true;
            progressPeak = 1f;
        }

        if (HeartRateManager.Instance.GlobalPhaseMod1 <= threshold && wasOverThreshold)
            wasOverThreshold = false;

        

   
        for (int k=0;k< speed; k++)
        {
            progressPerlin++;


            float newValue = 0f;
            if (progressPeak > 0f)
            {
                progressPeak -= Time.deltaTime * 3f/ speed;
                progressPeak = Mathf.Clamp01(progressPeak);
                newValue += curvePeak.Evaluate(1f - progressPeak);
            }

            for (int i = 0; i < amountPoints - 1; i++)
            {
                valuesHeart[i] = valuesHeart[i + 1];
            }
                

            valuesHeart[amountPoints - 1] = newValue * 5f;
        }

        finalPositions = new Vector3[amountPoints];

        for(int i=0;i<amountPoints;i++)
        {
            float baseValue = (Mathf.PerlinNoise(Time.realtimeSinceStartup * 0.5f + 10f*(float)i / (amountPoints - 1f), Time.realtimeSinceStartup*0.5f) * 2f - 1f) * 0.2f;
            finalPositions[i] = new Vector3(xs[i], baseValue+valuesHeart[i], 0f);
        }
        
        lineRenderer.positionCount = amountPoints;
        lineRenderer.SetPositions(finalPositions);
    }
}
