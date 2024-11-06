using System;
using System.IO;
using System.IO.Ports;
using UnityEngine;

public class SerialCOM : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField]
    private string port = "COM3";
    
    [SerializeField]
    private int baudRate = 115200;
    
    [Header("References")]
    [SerializeField]
    private EthernetValues ethernetValues;
    
    private SerialPort serialPort;

    private bool isStreaming;
    
    private void Start()
    {
        Open();
    }

    private void OnDestroy()
    {
        Close();
    }

    private void OnDisable()
    {
        Close();
    }

    private void Open()
    {
        isStreaming = true;
        serialPort = new SerialPort(port, baudRate);
        serialPort.ReadTimeout = 50;

        try
        {
            serialPort.Open();
            Debug.Log("COM port opened successfully");
        }
        catch (IOException e)
        {
            Debug.Log(e.Message);
        }  
    }

    private void Close()
    {
        serialPort?.Close();
    }
    
    private void Update()
    {
        if (!isStreaming) 
            return;
        if (!serialPort.IsOpen)
            return;

        // Make sure we read all the data else it will stack up and give a delay
        while (serialPort.BytesToRead > 0)
        {
            var value = ReadSerialPort();
            if (value != null )
            {
                ParseMessage(value); // -> 0,600,0
            }
        }
    }

    private void ParseMessage(string message)
    {
        var split = message.Split(',');

        if (split.Length != 3) return;

        var bpm = 0;
        var interval = 0;
        var pulse = 0;

        var success = true;
        
        for (var i = 0; i < split.Length; i++)
        {
            if (int.TryParse(split[i], out var value))
            {
                switch (i)
                {
                    case 0:
                        bpm = value;
                        break;
                    case 1:
                        interval = value;
                        break;
                    case 2:
                        pulse = value;
                        break;
                }
            }
            else
            {
                success = false;
                break;
            }
        }

        if (!success) return;
        
        // Debug.Log($"Heart rate data. BPM: {bpm}, Interval : {interval}, Pulse : {pulse}");
        
        ethernetValues.SetGlobalHeartBeatRateValue(bpm);
        ethernetValues.SetGlobalHeartBeatIntervalValue(interval);
        ethernetValues.SetGlobalHeartBeatPulseValue(pulse);
    }

    private string ReadSerialPort(int timeout = 50)
    {
        serialPort.ReadTimeout = timeout;

        try
        {
            if (serialPort.IsOpen)
                return serialPort.ReadLine();
            
            return "ERROR: Serial Port Not Open";

        }
        catch (TimeoutException e)
        {
            Debug.LogWarning(e.Message);
            return null;
        }
    }
}
