using System.Collections;
using Script.CameraSystem;
using UnityEngine;

public class Fosilising : MonoBehaviour
{
    [SerializeField]
    private float fadeTime = 2;

    [SerializeField]
    private GameObject fosil;

    private bool collided = false;

    private Rigidbody rb;

    private float originalDrag;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalDrag = rb.drag;
        enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Terrain") && !collided && this.enabled)
        {
            TimeOut();
            collided = true;
        }
    }

    Bounds GetMaxBounds(GameObject g)
    {
        Renderer[] renderers = g.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0) 
            return new Bounds(g.transform.position, Vector3.zero);

        var b = renderers[0].bounds;
        foreach (Renderer r in renderers)
            b.Encapsulate(r.bounds);

        return b;
    }

    public void TimeOut()
    {
        IEnumerator coroutine = TimedDisolve(fadeTime);
        StartCoroutine(coroutine);
    }

    private IEnumerator TimedDisolve(float waitTime)
    {
        // TODO: @Neander: This is where the Sentinel becomes an egg
        CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.SentinelBecomesEgg, transform, null));
        
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        Bounds B = GetMaxBounds(gameObject);
        GameObject thisFosil = Instantiate(fosil, B.center - new Vector3(0, -0.5f, 0), new Quaternion(0, 0, 0, 0));
        thisFosil.transform.localScale = B.size * 1.2f;

        yield return new WaitForSeconds(waitTime);
        
        foreach (Transform p in GetComponent<GeneticMovementSentinel>().plastics)
            Destroy(p.gameObject);

        GetComponent<GeneticMovementSentinel>().plastics.Clear();

        GeneticAntigenKey[] antigens = GetComponentsInChildren<GeneticAntigenKey>();

        foreach (GeneticAntigenKey K in GetComponent<GeneticMovementSentinel>().digestAntigens)
            Destroy(K.gameObject);

        GetComponent<GeneticMovementSentinel>().digestAntigens.Clear();

        gameObject.SetActive(false);
        GetComponent<KuramotoAffectedAgent>().dead = true;
        rb.useGravity = false;
        rb.drag = originalDrag;
        this.enabled = false;
        collided = false;
        rb.isKinematic = false;
    }
}
