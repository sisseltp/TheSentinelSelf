using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticMovement : MonoBehaviour
{
    public Agent agent;

    [Tooltip("How many cycles to contain")]
    [SerializeField]
    public int cycleLength = 10; // number of steps in cylcle
    [Tooltip("Scaler for the genetic speed")]
    [SerializeField]
    public float genSpeedScl = 0.5f; // sclr for the speed
    [HideInInspector]
    public Vector3[] geneticMovement; // list to hold vels in
    [HideInInspector]
    public int step = 0;// to hold the steps number
    [HideInInspector]
    public float lastPhase = 0;// holds the last phase for a gate
    public bool notKeyed = true;
    [Tooltip("Scaler for the genetic speed")]
    [SerializeField]
    public float speedScl = 0.5f;

    [HideInInspector]
    public bool targeting = true;
    [HideInInspector]
    public Vector3 target;

    public virtual void Start()
    {
        Reset();
    }

    public virtual void Reset()
    {
        geneticMovement = new Vector3[cycleLength];

        for (int i = 0; i < cycleLength; i++)
            geneticMovement[i] = Random.insideUnitSphere;
    }

    public virtual void Update()
    {
        if (HeartRateManager.Instance.GlobalPhaseMod1 > lastPhase)
            step = (step + 1) % cycleLength;

        Vector3 vel = geneticMovement[step] * HeartRateManager.Instance.GlobalPhaseMod1 * speedScl;
        agent.rigidBody.AddForceAtPosition(vel * Time.deltaTime, transform.position + transform.up);

        lastPhase = HeartRateManager.Instance.GlobalPhaseMod1;
    }
}
