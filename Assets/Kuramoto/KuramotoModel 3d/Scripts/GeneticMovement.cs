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
    public int step = 0;// to hold the steps number
    public float lastPhase = 0;// holds the last phase for a gate
    public bool notKeyed = true;
    [Tooltip("Scaler for the genetic speed")]
    [SerializeField]
    private float speedScl = 0.5f;
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
        if (agent.kuramoto.phase > lastPhase)
            step = (step + 1) % cycleLength;

        Vector3 vel = geneticMovement[step] * agent.kuramoto.phase * speedScl;
        agent.rigidBody.AddForceAtPosition(vel * Time.deltaTime, transform.position + transform.up);

        lastPhase = agent.kuramoto.phase;
    }
}
