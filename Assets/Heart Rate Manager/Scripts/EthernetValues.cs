using UnityEngine;
using UnityEngine.Serialization;

public class EthernetValues : MonoBehaviour
{
    [Header("Sensor Values")]
    [FormerlySerializedAs("GlobalRate")] 
    public int globalRate;
    [FormerlySerializedAs("GlobalInterval")] 
    public int globalInterval;
    [FormerlySerializedAs("GlobalPulse")] 
    public float globalPulse;

    [Header("Kuramoto Values")]
    public float pulseGradient;
    
    private bool reading;

    [Header("Scene Threshold Values")]
    [SerializeField]
    private float beginTimer = 2;
    [SerializeField]
    private float restartTimer = 5;

    private float timerGate;

    [FormerlySerializedAs("avrgDrag")] [SerializeField]
    private float averageDrag = 0.6f;
    private float averageRate;
    private float lastAverageRate;
    [SerializeField]
    private float changeGate = 10;

    void Update()
    {
        if (globalInterval == 0)
            HeartRateManager.Instance.SetSensorConnected(false);

        averageRate += (globalRate - averageRate) * averageDrag;
        float averageChange = Mathf.Abs(globalRate - lastAverageRate);

        if (averageRate > 50 && averageRate < 150 && (averageChange < changeGate))
        {
            if (!reading) 
            {
                reading = true;
                timerGate = Time.time;
            }
            else if (globalInterval > 0 && timerGate + beginTimer < Time.time ) {
                HeartRateManager.Instance.SetSensorConnected(true, true);
            }

            float step = (float)globalRate / 30;
            step *= Time.deltaTime;
            pulseGradient += step;
            pulseGradient = Mathf.Clamp01(pulseGradient);

            if (Mathf.Approximately(globalPulse, 1))
                pulseGradient = 0;

            if(!HeartRateManager.Instance.simulateHeartBeat)
                HeartRateManager.Instance.GlobalPhase = pulseGradient;
        } 
        else if (reading) 
        {
            reading = false;
            timerGate = Time.time;
        } 
        else if (globalInterval > 0 && timerGate + restartTimer < Time.time)
            HeartRateManager.Instance.SetSensorConnected(true);

        lastAverageRate = averageRate;
    }

    public void SetGlobalHeartBeatRateValue(int value)
    {
        globalRate = value;
    }

    public void SetGlobalHeartBeatIntervalValue(int value)
    {
        globalInterval = value;
    }

    public void SetGlobalHeartBeatPulseValue(int value)
    {
        globalPulse = Mathf.Clamp01(value / 4095f);
    }
}