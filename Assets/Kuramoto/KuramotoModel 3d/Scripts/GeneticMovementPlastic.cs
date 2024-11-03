using System.Collections;
using System.Collections.Generic;
using Script.CameraSystem;
using UnityEngine;

public class GeneticMovementPlastic : GeneticMovement
{
    [SerializeField]
    private GameObject prefabDigested;

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

    public override void OnCollisionEnterPlayer(Collision collision)
    {
        agent.kuramoto.dead = true;

        GameObject plast = Instantiate(prefabDigested, collision.GetContact(0).point, transform.rotation, collision.transform);

        GeneticMovementSentinel sentinel = collision.gameObject.GetComponent<GeneticMovementSentinel>();
        sentinel.plastics.Add(plast.transform);
        plast.GetComponent<Digestion>().SetRootBoneAndStartDigestion(sentinel.rootBone);

        if (!full)
        {
            sentinel.agent.kuramoto.speed *= 0.9f;

            var drag = sentinel.agent.rigidBody.drag;
            drag += dragItter;
            sentinel.agent.rigidBody.drag = drag;

            if (drag > maxDrag)
            {
                // TODO: @Neander: This is where the sentinel dies and falls to the ground
                CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.SentinelDies, collision.transform));

                sentinel.agent.rigidBody.useGravity = true;
                full = true;
            }
            else
            {
                // TODO: @Neander: Check the max drag and see if we are close, keep following this sentinel because it is about to die
                CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.SentinelAtePlastic, collision.transform, new EventData(drag, maxDrag)));
            }
        }
    }

    public override void OnTriggerStayPlasticMover(Collider collider)
    {
        agent.rigidBody.AddForceAtPosition(Vector3.down * Time.deltaTime * 10, transform.position + transform.up);
    }
}
