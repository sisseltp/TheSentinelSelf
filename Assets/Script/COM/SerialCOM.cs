using System;
using System.Collections;
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
    
    [SerializeField]
    private int maxRetryConnectTime = 120;
    
    [Header("References")]
    [SerializeField]
    private EthernetValues ethernetValues;
    
    private SerialPort serialPort;

    private bool isStreaming;
    private int retryTime;
    
    private void Start()
    {
        Open();
    }

    private void Open()
    {
        try
        {
            serialPort = new SerialPort(port, baudRate);
            serialPort.ReadTimeout = 50;
            serialPort.Open();
            isStreaming = true;
            Debug.Log("COM port opened successfully");

            retryTime = 0;
        }
        catch (IOException e)
        {
            // Debug.LogWarning($"Error during opening of port. Message: {e.Message}");
            StartCoroutine(RetryConnect());
        }  
    }

    private void Close()
    {
        isStreaming = false;
        serialPort?.Close();
        
        ethernetValues.SetGlobalHeartBeatRateValue(0);
        ethernetValues.SetGlobalHeartBeatIntervalValue(0);
        ethernetValues.SetGlobalHeartBeatPulseValue(0);
    }

    private void Update()
    {
        if (!isStreaming)
            return;
        if (!serialPort.IsOpen)
            return;

        // Make sure we read all the data else it will stack up and give a delay
        try
        {
            while (serialPort.BytesToRead > 0)
            {
                var value = ReadSerialPort();
                if (value != null)
                {
                    ParseMessage(value); // -> 0,600,0
                }
            }
        }
        catch (Exception e)
        {
            // Debug.LogWarning($"Problem during serial read. Message: {e.Message}");
            Close();
            StartCoroutine(RetryConnect());
        }
    }

    private IEnumerator RetryConnect()
    {
        retryTime += 5; // Every retry wait 5 sec longer

        if (retryTime > maxRetryConnectTime)
        {
            retryTime = maxRetryConnectTime;
        }
        
        Debug.Log($"Waiting for {retryTime} seconds before retry.");
        
        yield return new WaitForSeconds(retryTime);
        
        Open();
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
        
        ethernetValues.SetGlobalHeartBeatRateValue(bpm);
        ethernetValues.SetGlobalHeartBeatIntervalValue(interval);
        ethernetValues.SetGlobalHeartBeatPulseValue(pulse);
    }

    private string ReadSerialPort()
    {
        try
        {
            return serialPort.IsOpen ? serialPort.ReadLine() : null;
        }
        catch
        {
            return null;
        }
    }
    
    private void OnDestroy()
    {
        Close();
    }

    private void OnDisable()
    {
        Close();
    }
}
