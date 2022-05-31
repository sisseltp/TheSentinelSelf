using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using Freya;

public class ethernetValues : MonoBehaviour
{
    [Header("Sentinel 1 Values")]
    public int sentinel1Rate;
    public int sentinel1Interval;
    public float sentinel1Pulse;
    
    [Header("Sentinel 2 Values")]
    public int sentinel2Rate;
    public int sentinel2Interval;
    public float sentinel2Pulse;
    
    [Header("Sentinel 3 Values")]
    public int sentinel3Rate;
    public int sentinel3Interval;
    public float sentinel3Pulse;
    
    [Header("Sentinel 4 Values")]
    public int sentinel4Rate;
    public int sentinel4Interval;
    public float sentinel4Pulse;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
        sentinel1Pulse = Mathfs.Remap(0f,4095f,0f,1f,value);
    }
    public void setSentinel2Rate(int value)
    {
        sentinel2Rate = value;
    }

    public void setSentinel2Interval(int value)
    {
        sentinel2Interval = value;
    }

    public void setSentinel2Pulse(int value)
    {
        sentinel2Pulse = Mathfs.Remap(0f,4095f,0f,1f,value);
    }
    public void setSentinel3Rate(int value)
    {
        sentinel3Rate = value;
    }

    public void setSentinel3Interval(int value)
    {
        sentinel3Interval = value;
    }

    public void setSentinel3Pulse(int value)
    {
        sentinel3Pulse = Mathfs.Remap(0f,4095f,0f,1f,value);
    }

    public void setSentinel4Rate(int value) {
        sentinel4Rate = value;
    }

    public void setSentinel4Interval(int value) {
        sentinel4Interval = value;
    }

    public void setSentinel4Pulse(int value) {
        sentinel4Pulse = Mathfs.Remap(0f,4095f,0f,1f,value);
    }

}
