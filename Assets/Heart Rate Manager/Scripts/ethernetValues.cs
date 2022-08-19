using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using Freya;

public class ethernetValues : MonoBehaviour
{
    [Header("Sensor Values")]
    public int sentinel1Rate;
    public int sentinel1Interval;
    public float sentinel1Pulse;

    //[Header("Sentinel 2 Values")]
    //public int sentinel2Rate;
    //public int sentinel2Interval;
    //public float sentinel2Pulse;

    //[Header("Sentinel 3 Values")]
    //public int sentinel3Rate;
    //public int sentinel3Interval;
    //public float sentinel3Pulse;

    //[Header("Sentinel 4 Values")]
    //public int sentinel4Rate;
    //public int sentinel4Interval;
    //public float sentinel4Pulse;

    [Header("Kuramoto Values")]
    public float pulseGradient;
    private KuramotoAffecterAgent agent;
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


    // Start is called before the first frame update
    void Start()
    {
        IntroControl = GetComponentInParent<IntroBeginner>();
    }

    // Update is called once per frame
    void Update()
    {

        avrgRate += (sentinel1Rate - avrgRate) * avrgDrag;

        float avrgChange = Mathf.Abs(sentinel1Rate - lastAvrgRate);



        if (avrgRate > 50 && avrgRate < 150 && avrgChange < changeGate)
        {
            if (!reading)
            {
                reading = true;
                TimerGate = Time.time;
            }
            else if (!playing && TimerGate + beginTimer < Time.time)
            {
                playing = true;
                IntroControl.Begin();
            }

            float step = (float)sentinel1Rate / 30;
            step *= Time.deltaTime;
            pulseGradient += step;// *f; I realised the division above was wron so should be perfect now
            pulseGradient = Mathf.Clamp01(pulseGradient);

            if (sentinel1Pulse == 1)
            {
                pulseGradient = 0;
            }

            float theta = pulseGradient * CIRCLE_IN_RADIAN;
            // get this sentinels x,y pos
            float thisX = Mathf.Cos(theta);
            float thisY = Mathf.Sin(theta);

            //agent.AddOsiclation(thisX, thisY, bias);
            if (agent != null)
            {
                agent.phase = pulseGradient;
            }
        }
        else if (reading)
        {
            reading = false;
            TimerGate = Time.time;
        }
        else if (playing && TimerGate + restartTimer < Time.time)
        {
            IntroControl.Restart();
            playing = false;
        }

        lastAvrgRate = avrgRate;
    }

    public void setSentinelAgent(KuramotoAffecterAgent thisAgent)
    {
        agent = thisAgent;
    }


    public void setSentinel1Rate(int value)
    {
        sentinel1Rate = value;
    }

    public void setSentinel1Interval(int value)
    {
        sentinel1Interval = value;
    }

    public void setSentinel1Pulse(int value)
    {
        sentinel1Pulse = Mathfs.Remap(0f, 4095f, 0f, 1f, value);
    }
    //public void setSentinel2Rate(int value)
    //{
    //    sentinel2Rate = value;
    //}

    //public void setSentinel2Interval(int value)
    //{
    //    sentinel2Interval = value;
    //}

    //public void setSentinel2Pulse(int value)
    //{
    //    sentinel2Pulse = Mathfs.Remap(0f,4095f,0f,1f,value);
    //}
    //public void setSentinel3Rate(int value)
    //{
    //    sentinel3Rate = value;
    //}

    //public void setSentinel3Interval(int value)
    //{
    //    sentinel3Interval = value;
    //}

    //public void setSentinel3Pulse(int value)
    //{
    //    sentinel3Pulse = Mathfs.Remap(0f,4095f,0f,1f,value);
    //}

    //public void setSentinel4Rate(int value) {
    //    sentinel4Rate = value;
    //}

    //public void setSentinel4Interval(int value) {
    //    sentinel4Interval = value;
    //}

    //public void setSentinel4Pulse(int value) {
    //    sentinel4Pulse = Mathfs.Remap(0f,4095f,0f,1f,value);
    //}

}

