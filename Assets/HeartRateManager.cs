using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class HeartRateManager : MonoBehaviour
{
    public static HeartRateManager Instance;

    [HideInInspector]
    public bool sensorConnected;
    [HideInInspector]
    public bool sensorHasValue;

    public float GlobalPhase = 0f;
    public float GlobalPhaseMod1 => GlobalPhase % 1f;

    public bool forceSimulateHeartBeat = false;

    [Range(1,180)]
    public int simulatedBPM = 30;

    float simulatedPhase;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if(!sensorConnected || !sensorHasValue || forceSimulateHeartBeat)
        {
            simulatedPhase += Time.deltaTime * simulatedBPM / 60f;
            GlobalPhase = simulatedPhase;
        }

        Shader.SetGlobalFloat("_Phase", GlobalPhaseMod1);
    }

    public void SetSensorConnected(bool isConnected, bool hasValue = false)
    {
        sensorConnected = isConnected;
        sensorHasValue = hasValue;
    }
}
