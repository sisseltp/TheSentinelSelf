using System;
using System.IO.Ports;
using UnityEngine;

public class SerialCOM : MonoBehaviour
{
    [SerializeField]
    private string port = "COM3";
    
    [SerializeField]
    private int baudRate = 115200;
    
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

    public void Open()
    {
        isStreaming = true;
        serialPort = new SerialPort(port, baudRate);
        serialPort.ReadTimeout = 1000;
        serialPort.Open();
        
        Debug.Log("COM port opened successfully");
    }

    public void Close()
    {
        serialPort.Close();
    }

    private void Update()
    {
        if (isStreaming)
        {
            string value = ReadSerialPort();
            if (value != null )
            {
                Debug.Log($"Value: {value}");
            }
        }
    }

    private string ReadSerialPort(int timeout = 50)
    {
        string message;
        
        serialPort.ReadTimeout = timeout;

        try
        {
            message = serialPort.ReadLine();
            return message;
        }
        catch (TimeoutException e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }
}
