using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum APCBehavior {CarryingAntigens, SeekingPathogens}
public class GeneticMovementSentinel : GeneticMovement
{
    public int keys = 0;
    public int NumKeysToCollect = 4;
    public float origDrag = 0;
    public Transform rootBone;

    [HideInInspector]
    public List<GeneticAntigenKey> digestAntigens;
    
    [HideInInspector]
    public List<Transform> plastics;

    [Tooltip("Current stage of the APC in its cycle of activities")]
    public APCBehavior currentBehavior = APCBehavior.SeekingPathogens;

    private int tcellHits = 0;

    public override void Start()
    {
        digestAntigens = new List<GeneticAntigenKey>();
        plastics = new List<Transform>();
        geneticMovement = new Vector3[cycleLength];
        origDrag = agent.rigidBody.drag;

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

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Tcell"))
            tcellHits++;
        else if (collision.gameObject.CompareTag("Terrain") && agent.rigidBody.useGravity)
            (agent as Sentinel).fosilising.enabled = true;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("PathogenEmitter") || collision.gameObject.CompareTag("Lymphonde"))
            targeting = false;
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("PathogenEmitter"))
        {
            if (keys >= NumKeysToCollect && !targeting)
            {
                int indx = Random.Range(0, GameManager.Instance.tCellsManagers.Count);
                
                target = GameManager.Instance.tCellsManagers[indx].transform.position;
                targeting = true;
                tcellHits = 0;

                currentBehavior = APCBehavior.CarryingAntigens;
                //////<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< gets enough antigens to leave
            }
            else if( keys< NumKeysToCollect)
                targeting = false;
        }
        else if (collision.gameObject.CompareTag("Lymphonde") && tcellHits > 10 && !targeting)
        {
            int indx = Random.Range(0, GameManager.Instance.pathogensManagers.Count);

            target = GameManager.Instance.pathogensManagers[indx].transform.position;
            targeting = true;

            foreach (GeneticAntigenKey key in digestAntigens)
                key.TimeOut();

            digestAntigens.Clear();
            keys = 0;
            currentBehavior = APCBehavior.SeekingPathogens;
            //////<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< leaving the lymphonode 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        targeting = true;
    }
}
