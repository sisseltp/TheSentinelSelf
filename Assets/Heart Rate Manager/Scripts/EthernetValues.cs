using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using Freya;

public class EthernetValues : MonoBehaviour
{
    [Header("Sensor Values")]
    public int GlobalRate;
    public int GlobalInterval;
    public float GlobalPulse;

    [Header("Kuramoto Values")]
    public float pulseGradient;
    private const float CIRCLE_IN_RADIAN = 2f * Mathf.PI; //2* pi
    public int bias = 3;
    
    private bool reading = false;

    [Header("Scene Threshold Values")]
    [SerializeField]
    private float beginTimer = 2;
    [SerializeField]
    private float restartTimer = 5;

    private float TimerGate = 0;

    [SerializeField]
    private float avrgDrag = 0.6f;
    private float avrgRate = 0;
    private float lastAvrgRate = 0;
    [SerializeField]
    private float changeGate = 10;

    void Update()
    {
        if (GlobalInterval == 0)
            HeartRateManager.Instance.SetSensorConnected(false);

        avrgRate += (GlobalRate - avrgRate) * avrgDrag;
        float avrgChange = Mathf.Abs(GlobalRate - lastAvrgRate);

        if (avrgRate > 50 && avrgRate < 150 && (avrgChange < changeGate))
        {
            if (!reading) 
            {
                reading = true;
                TimerGate = Time.time;
            }
            else if (GlobalInterval > 0 && TimerGate + beginTimer < Time.time ) {
                HeartRateManager.Instance.SetSensorConnected(true, true);
            }

            float step = (float)GlobalRate / 30;
            step *= Time.deltaTime;
            pulseGradient += step;
            pulseGradient = Mathf.Clamp01(pulseGradient);

            if (GlobalPulse == 1)
                pulseGradient = 0;

            if(!HeartRateManager.Instance.simulateHeartBeat)
                HeartRateManager.Instance.GlobalPhase = pulseGradient;
        } 
        else if (reading) 
        {
            reading = false;
            TimerGate = Time.time;
        } 
        else if (GlobalInterval > 0 && TimerGate + restartTimer < Time.time)
            HeartRateManager.Instance.SetSensorConnected(true);

        lastAvrgRate = avrgRate;
    }

    public void SetGlobalHeartBeatRateValue(int value)
    {
        GlobalRate = value;
    }

    public void SetGlobalHeartBeatIntervalValue(int value)
    {
        GlobalInterval = value;
    }

    public void SetGlobalHeartBeatPulseValue(int value)
    {
        GlobalPulse = Mathf.Clamp01(value / 4095f);
    }
}