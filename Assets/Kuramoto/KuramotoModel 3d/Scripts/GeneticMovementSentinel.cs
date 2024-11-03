using System;
using System.Collections;
using System.Collections.Generic;
using Script.CameraSystem;
using UnityEngine;
using Random = UnityEngine.Random;

public enum APCBehavior {CarryingAntigens, SeekingPathogens}
public class GeneticMovementSentinel : GeneticMovement
{
    public int keys = 0;
    public int NumKeysToCollect = 4;
    public float origDrag = 0;
    public Transform rootBone;

    [HideInInspector]
    public List<GeneticAntigenKey> digestAntigens = new List<GeneticAntigenKey>();

    [HideInInspector]
    public List<Transform> plastics = new List<Transform>();

    [Tooltip("Current stage of the APC in its cycle of activities")]
    public APCBehavior currentBehavior = APCBehavior.SeekingPathogens;

    private int tcellHits = 0;

    public override void Start()
    {
        geneticMovement = new Vector3[cycleLength];
        origDrag = agent.rigidBody.drag;

        // TODO: @Neander: This is where the sentinel starts pathogen hunting
        CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.SentinelGoesToPathogen, transform));
        
        base.Start();
    }

    public override void Reset()
    {
        base.Reset();

        agent.rigidBody.drag = origDrag;

        target = GameManager.Instance.pathogensManagers[Random.Range(0, GameManager.Instance.pathogensManagers.Count)].transform.position;
        targeting = true;
    }

    public override void Update()
    {
        if (HeartRateManager.Instance.GlobalPhaseMod1 < lastPhase) 
            step = (step + 1) % cycleLength;

        Vector3 vel = Vector3.zero;

        if (targeting)  
            vel = Vector3.Normalize(target - transform.position) * speedScl;

        Ray forward = new Ray(transform.position, Vector3.Normalize(agent.rigidBody.velocity) + Vector3.down * 0.5f);

        if (Physics.Raycast(forward, out RaycastHit hit, 20))
            if (hit.transform.CompareTag("Terrain"))
                vel += Vector3.up * speedScl * 3;

        vel += geneticMovement[step] * genSpeedScl;
        vel *= HeartRateManager.Instance.GlobalPhaseMod1;

        agent.rigidBody.AddForceAtPosition(vel * Time.deltaTime, transform.position + transform.forward);
        lastPhase = HeartRateManager.Instance.GlobalPhaseMod1;
    }

    public override void OnCollisionEnterKill(Collision collision)
    {
        //DO NOT APPLY DEAD TO THE SENTINEL
        return;
    }

    public override void OnCollisionEnterTCell(Collision collision)
    {
        tcellHits++;
    }

    public override void OnCollisionEnterTerrain(Collision collision)
    {
        (agent as Sentinel).fosilising.enabled = true;
    }

    public override void OnTriggerEnterLymphonde(Collider collider) 
    {
        targeting = false;
    }

    public override void OnTriggerEnterPathogenEmitter(Collider collider)
    {
        targeting = false;
    }

    public override void OnTriggerStayPathogenEmitter(Collider collider)
    {
        if (keys >= NumKeysToCollect && !targeting)
        {
            int indx = Random.Range(0, GameManager.Instance.tCellsManagers.Count);

            target = GameManager.Instance.tCellsManagers[indx].transform.position;
            targeting = true;
            tcellHits = 0;

            currentBehavior = APCBehavior.CarryingAntigens;

            if (Math.Abs(origDrag - GetComponent<Rigidbody>().drag) < 0.01f)
            {
                // TODO: @Neander: This is where the sentinel has enough antigens
                CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.SentinelGoesToLymphNode, transform));
            }
            else
            {
                // TODO: @Neander: This is where the sentinel has enough antigens and is infected by plastic
                CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.InfectedSentinelGoesToTCell, transform));
            }
        }
        else if (keys < NumKeysToCollect)
            targeting = false;
    }

    public override void OnTriggerStayLymphonde(Collider collider)
    {
        if (tcellHits > 10 && !targeting)
        {
            int indx = Random.Range(0, GameManager.Instance.pathogensManagers.Count);

            target = GameManager.Instance.pathogensManagers[indx].transform.position;
            targeting = true;

            foreach (GeneticAntigenKey key in digestAntigens)
                key.TimeOut();

            digestAntigens.Clear();
            keys = 0;
            currentBehavior = APCBehavior.SeekingPathogens;

            // TODO: @Neander: This is where the sentinel has delivered antigens and goes back to pathogen hunting
            CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.SentinelGoesToPathogen, transform));
            //////<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< leaving the lymphonode 
        }
    }

    public override void OnTriggerExitAnything(Collider collider)
    {
        targeting = true;
    }
}
