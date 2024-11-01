using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class HeartRateManager : MonoBehaviour
{
    public static HeartRateManager Instance;


    public float GlobalPhase = 0f;
    public float GlobalPhaseMod1 => GlobalPhase % 1f;

    public bool simulateHeartBeat = false;

    [ShowIf("@simulateHeartBeat == true")]
    [Range(1,180)]
    public int simulatedBPM = 60;

    float simulatedPhase;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if(simulateHeartBeat)
        {
            simulatedPhase += Time.deltaTime * simulatedBPM / 60f;
            GlobalPhase = simulatedPhase;
        }

        Shader.SetGlobalFloat("_Phase", GlobalPhaseMod1);
    }
}