using Script.CameraSystem;
using UnityEngine;

public class GeneticMovementPathogen : GeneticMovement
{
    [SerializeField]
    private int maxKeys = 10;

    public override void Update()
    {
        base.Update();
        if(Vector3.SqrMagnitude(transform.position-agent.manager.transform.position)>15f*15f)
            transform.position = transform.parent.position + Random.insideUnitSphere;
    }

    public override void OnCollisionEnterPlayer(Collision collision)
    {
        GeneticMovementSentinel sentinel = collision.gameObject.GetComponentInChildren<GeneticMovementSentinel>();

        int numkeys = collision.gameObject.GetComponentsInChildren<GeneticAntigenKey>().Length;
        if (numkeys < sentinel.NumKeysToCollect + maxKeys)// if less than max num, pick up key
        {
            Quaternion rot = Quaternion.LookRotation(collision.transform.position, transform.up);

            GameObject newObj = transform.GetChild(0).gameObject;

            var scale = newObj.transform.localScale / 100;
            newObj = Instantiate(newObj, collision.GetContact(0).point, rot, collision.collider.transform);
            newObj.transform.localScale = scale;
            
            newObj.GetComponent<Digestion>().enabled = true;

            sentinel.keys++;
            sentinel.digestAntigens.Add(newObj.GetComponent<GeneticAntigenKey>());
            agent.kuramoto.dead = true;

            CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.SentinelAteAntigen, collision.transform, new EventData(numkeys, sentinel.NumKeysToCollect + maxKeys)));

            if(!sentinel.targeting)
                sentinel.CheckIfEnoughKeys();
        }
    }
}
