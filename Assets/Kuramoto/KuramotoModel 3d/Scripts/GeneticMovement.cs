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
        if (agent.kuramoto.phase > lastPhase)
            step = (step + 1) % cycleLength;

        Vector3 vel = geneticMovement[step] * agent.kuramoto.phase * speedScl;
        agent.rigidBody.AddForceAtPosition(vel * Time.deltaTime, transform.position + transform.up);

        lastPhase = agent.kuramoto.phase;
    }

    public virtual void OnCollisionEnter(Collision collision)
    {
        if (!(this is GeneticMovementSentinel))
        {
            if (collision.gameObject.CompareTag("Kill"))
                OnCollisionEnterKill(collision);
            else if (collision.gameObject.CompareTag("Player"))
                OnCollisionEnterPlayer(collision);
            else if (collision.gameObject.CompareTag("Pathogen") && this is GeneticMovementTcell)
                OnCollisionEnterPathogen(collision);
        }
        else
        {
            if (collision.gameObject.CompareTag("Tcell"))
                OnCollisionEnterTCell(collision);
            else if (collision.gameObject.CompareTag("Terrain") && agent.rigidBody.useGravity)
                OnCollisionEnterTerrain(collision);
        }
    }

    public virtual void OnCollisionEnterKill(Collision collision)
    {
        agent.kuramoto.dead = true;
    }

    public virtual void OnCollisionEnterPlayer(Collision collision) { }
    public virtual void OnCollisionEnterTCell(Collision collision) { }
    public virtual void OnCollisionEnterPathogen(Collision collision) { }
    public virtual void OnCollisionEnterTerrain(Collision collision) { }

    public virtual void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Lymphonde"))
            OnTriggerEnterLymphonde(collider);
        else if (collider.gameObject.CompareTag("PathogenEmitter"))
            OnTriggerEnterPathogenEmitter(collider);
    }

    public virtual void OnTriggerEnterLymphonde(Collider collider) { }
    public virtual void OnTriggerEnterPathogenEmitter(Collider collider) { }

    public virtual void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.CompareTag("Lymphonde"))
            OnTriggerStayLymphonde(collider);
        else if (collider.gameObject.CompareTag("PathogenEmitter"))
            OnTriggerStayPathogenEmitter(collider);
        else if (collider.gameObject.CompareTag("PlasticMover"))
            OnTriggerStayPlasticMover(collider);
    }

    public virtual void OnTriggerStayLymphonde(Collider collider) { }
    public virtual void OnTriggerStayPathogenEmitter(Collider collider) { }
    public virtual void OnTriggerStayPlasticMover(Collider collider) { }

    public virtual void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.CompareTag("LymphOuter"))
            OnTriggerExitLymphOuter(collider);
        else if (collider.gameObject.CompareTag("PathogenEmitter"))
            OnTriggerExitPathogenEmitter(collider);
        else
            OnTriggerExitAnything(collider);
    }

    public virtual void OnTriggerExitLymphOuter(Collider collider) { }
    public virtual void OnTriggerExitPathogenEmitter(Collider collider) { }
    public virtual void OnTriggerExitAnything(Collider collider) { }
}