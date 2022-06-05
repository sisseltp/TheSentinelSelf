using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using Freya;

public class serialValues : MonoBehaviour
{
    SerialPort sentinel1Stream = new SerialPort("COM11", 115200);
    //SerialPort sentinel2Stream = new SerialPort("COM11", 115200);
    //SerialPort sentinel3Stream = new SerialPort("COM13", 115200);
    //SerialPort sentinel4Stream = new SerialPort("COM10", 115200);
    [HideInInspector]
    public string sentinel1String;
    [HideInInspector]
    public string sentinel2String;
    [HideInInspector]
    public string sentinel3String;
    [HideInInspector]
    public string sentinel4String;
    [HideInInspector]
    public string[] sentinel1List;
    [HideInInspector]
    public string[] sentinel2List;
    [HideInInspector]
    public string[] sentinel3List;
    [HideInInspector]
    public string[] sentinel4List;

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
        sentinel1Stream.Open();
        //sentinel2Stream.Open();
        //sentinel3Stream.Open();
        //sentinel4Stream.Open();
    }

    // Update is called once per frame
    void Update()
    {
        sentinel1String = sentinel1Stream.ReadLine();
        sentinel1List = sentinel1String.Split(',');
        setSentinel1Rate(int.Parse(sentinel1List[0]));
        setSentinel1Interval(int.Parse(sentinel1List[1]));
        setSentinel1Pulse(float.Parse(sentinel1List[2]));


        //sentinel2String = sentinel2Stream.ReadLine();
        //sentinel2List = sentinel2String.Split(',');
        //setSentinel2Rate(int.Parse(sentinel2List[0]));
        //setSentinel2Interval(int.Parse(sentinel2List[1]));
        //setSentinel2Pulse(int.Parse(sentinel2List[2]));


        //sentinel3String = sentinel3Stream.ReadLine();
        //sentinel3List = sentinel3String.Split(',');
        //setSentinel3Rate(int.Parse(sentinel3List[0]));
        //setSentinel3Interval(int.Parse(sentinel3List[1]));
        //setSentinel3Pulse(int.Parse(sentinel3List[2]));

        //sentinel4String = sentinel4Stream.ReadLine();
        //sentinel4List = sentinel4String.Split(',');
        //setSentinel4Rate(int.Parse(sentinel4List[0]));
        //setSentinel4Interval(int.Parse(sentinel4List[1]));
        //setSentinel4Pulse(int.Parse(sentinel4List[2]));
    }


    public void setSentinel1Rate(int value)
    {
        sentinel1Rate = value;
    }

    public void setSentinel1Interval(int value)
    {
        sentinel1Interval = value;
    }

    public void setSentinel1Pulse(float value)
    {
        sentinel1Pulse = Mathfs.Remap(0f, 4095f, 0f, 1f, value);
    }
    public void setSentinel2Rate(int value)
    {
        sentinel2Rate = value;
    }

    public void setSentinel2Interval(int value)
    {
        sentinel2Interval = value;
    }

    public void setSentinel2Pulse(float value)
    {
        sentinel2Pulse = Mathfs.Remap(0f, 4095f, 0f, 1f, value);
    }
    public void setSentinel3Rate(int value)
    {
        sentinel3Rate = value;
    }

    public void setSentinel3Interval(int value)
    {
        sentinel3Interval = value;
    }

    public void setSentinel3Pulse(float value)
    {
        sentinel3Pulse = Mathfs.Remap(0f, 4095f, 0f, 1f, value);
    }

    public void setSentinel4Rate(int value)
    {
        sentinel4Rate = value;
    }

    public void setSentinel4Interval(int value)
    {
        sentinel4Interval = value;
    }

    public void setSentinel4Pulse(float value)
    {
        sentinel4Pulse = Mathfs.Remap(0f, 4095f, 0f, 1f, value);
    }

}
