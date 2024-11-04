using Script.CameraSystem;
using UnityEngine;

public class GeneticMovementPathogen : GeneticMovement
{
    [SerializeField]
    private int maxKeys = 10;

    public override void OnCollisionEnterPlayer(Collision collision)
    {
        int numkeys = collision.gameObject.GetComponentsInChildren<GeneticAntigenKey>().Length;
        if (numkeys < collision.gameObject.GetComponentInChildren<GeneticMovementSentinel>().NumKeysToCollect + maxKeys)// if less than max num, pick up key
        {
            Quaternion rot = Quaternion.LookRotation(collision.transform.position, transform.up);

            GameObject newObj = transform.GetChild(0).gameObject;

            var scale = newObj.transform.localScale / 100;
            newObj = Instantiate(newObj, collision.GetContact(0).point, rot, collision.collider.transform);
            newObj.transform.localScale = scale;
            
            newObj.GetComponent<Digestion>().enabled = true;
            
            collision.gameObject.GetComponent<GeneticMovementSentinel>().keys++;
            collision.gameObject.GetComponent<GeneticMovementSentinel>().digestAntigens.Add(newObj.GetComponent<GeneticAntigenKey>());
            agent.kuramoto.dead = true;

            CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.SentinelAteAntigen, collision.transform, new EventData(numkeys, collision.gameObject.GetComponentInChildren<GeneticMovementSentinel>().NumKeysToCollect + maxKeys)));
        }
    }

    public override void OnTriggerExitPathogenEmitter(Collider collider)
    {
        transform.position = transform.parent.position+ Random.insideUnitSphere;
    }
}
