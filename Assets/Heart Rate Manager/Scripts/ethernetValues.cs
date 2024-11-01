using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using Freya;

public class EthernetValues : MonoBehaviour
{
    [Header("Sensor Values")]
    public int sentinel1Rate;
    public int sentinel1Interval;
    public float sentinel1Pulse;

    [Header("Kuramoto Values")]
    public float pulseGradient;
    private KuramotoAffectedAgent agent;
    private const float CIRCLE_IN_RADIAN = 2f * Mathf.PI; //2* pi
    public int bias = 3;


    private bool playing = false;
    private bool reading = false;

    [Header("Scene Threshold Values")]
    [SerializeField]
    private float beginTimer = 2;
    [SerializeField]
    private float restartTimer = 5;

    private float TimerGate = 0;

    private IntroBeginner IntroControl;

    [SerializeField]
    private float avrgDrag = 0.6f;
    private float avrgRate = 0;
    private float lastAvrgRate = 0;
    [SerializeField]
    private float changeGate = 10;

    void Start()
    {
        IntroControl = GetComponentInParent<IntroBeginner>();
    }

    void Update()
    {
        avrgRate += (sentinel1Rate - avrgRate) * avrgDrag;
        float avrgChange = Mathf.Abs(sentinel1Rate - lastAvrgRate);

        if (avrgRate > 50 && avrgRate < 150 && (avrgChange < changeGate))
        {
            if (!reading) 
            {
                reading = true;
                TimerGate = Time.time;
            }
            else if (!playing && (TimerGate + beginTimer < Time.time) ) 
            {
                playing = true;
                IntroControl.Begin();
            }

            float step = (float)sentinel1Rate / 30;
            step *= Time.deltaTime;
            pulseGradient += step;// *f; I realised the division above was wron so should be perfect now
            pulseGradient = Mathf.Clamp01(pulseGradient);

            if (sentinel1Pulse == 1)
                pulseGradient = 0;

            /*if (agent != null)
                agent.phase = pulseGradient;*/

            if(!HeartRateManager.Instance.simulateHeartBeat)
                HeartRateManager.Instance.GlobalPhase = pulseGradient;
        } 
        else if (reading) 
        {
            reading = false;
            TimerGate = Time.time;
        } 
        else if (playing && (TimerGate + restartTimer < Time.time))
            if (IntroControl.Restart())
                playing = false;

        lastAvrgRate = avrgRate;
    }

    public void SetSentinelAgent(KuramotoAffectedAgent thisAgent)
    {
        agent = thisAgent;
    }
}