using System.Collections;
using System.Collections.Generic;
using Script.CameraSystem;
using UnityEngine;

public class GeneticMovementPlastic : GeneticMovement
{
    [SerializeField]
    private GameObject attachedGO;

    [HideInInspector]
    public Vector3 origin;

    [SerializeField]
    private int numToKill = 10;

    [SerializeField]
    private float maxDrag = 0.4f;

    private float dragItter = 0;

    private bool full = false;

    public override void Start()
    {
        base.Start();
        dragItter = (maxDrag - agent.rigidBody.drag) / (float)numToKill;
    }

    public override void Reset()
    {
        base.Reset();
        origin = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Kill"))
        {
            agent.kuramoto.dead = true;
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            agent.kuramoto.dead = true;

            GameObject plast =  Instantiate(attachedGO, collision.GetContact(0).point, transform.rotation, collision.transform);

            collision.gameObject.GetComponent<GeneticMovementSentinel>().plastics.Add(plast.transform);

            if (!full)
            {
                collision.gameObject.GetComponent<KuramotoAffectedAgent>().speed *= 0.9f;

                Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

                var drag = rb.drag;
                drag += dragItter;
                rb.drag = drag;
                
                if (drag > maxDrag)
                {
                    // TODO: @Neander: This is where the sentinel dies and falls to the ground
                    CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.SentinelDies, collision.transform));
                    
                    rb.useGravity = true;
                    full = true;
                }
                else
                {
                    // TODO: @Neander: Check the max drag and see if we are close, keep following this sentinel because it is about to die
                    CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.SentinelAtePlastic, collision.transform, new EventData(drag, maxDrag)));
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.transform.CompareTag("PlasticMover") && agent.rigidBody != null)
            agent.rigidBody.AddForceAtPosition(Vector3.down * Time.deltaTime * 10, transform.position + transform.up);
    }
}
