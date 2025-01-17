using UnityEngine;

public class HeartLine : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;
     
    public int amountPoints = 2000;
    float[] valuesHeart;
    float[] xs;
    
    public AnimationCurve curvePeak;

    float progressPeak = 0f;

    public float speed = 50f;
    public float height = 5f;

    Vector3[] finalPositions;

    Keyframe[] baseKeysCurve;
    float lastValue;

    void Start()
    {
        baseKeysCurve = curvePeak.keys;
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
        if(HeartRateManager.Instance.GlobalPhaseMod1<lastValue)
        {
            Keyframe[] modifiedKeys = new Keyframe[baseKeysCurve.Length];

            float multHeight = Random.Range(0.8f, 1.2f);
            for (int i= 0;i < baseKeysCurve.Length;i++)
            {
                Keyframe baseK = baseKeysCurve[i];
                modifiedKeys[i] = new Keyframe(baseK.time+Random.Range(-0.02f,0.02f), (baseK.value*Random.Range(0.8f,1.2f))*multHeight, baseK.inTangent, baseK.outTangent, baseK.inWeight, baseK.outWeight);
                curvePeak.keys = modifiedKeys;
            }

            progressPeak = 1f;
        }

        lastValue = HeartRateManager.Instance.GlobalPhaseMod1;

        for (int k=0;k< speed; k++)
        {
            float newValue = 0f;
            if (progressPeak > 0f && (HeartRateManager.Instance.forceSimulateHeartBeat || (HeartRateManager.Instance.sensorConnected && HeartRateManager.Instance.sensorHasValue)))
            {
                progressPeak -= Time.deltaTime * 3f/ speed;
                progressPeak = Mathf.Clamp01(progressPeak);
                newValue += curvePeak.Evaluate(1f - progressPeak);
            }

            for (int i = 0; i < amountPoints - 1; i++)
                valuesHeart[i] = valuesHeart[i + 1];

            valuesHeart[amountPoints - 1] = newValue * height;
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
