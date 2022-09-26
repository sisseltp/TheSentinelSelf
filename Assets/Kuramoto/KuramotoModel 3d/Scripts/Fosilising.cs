using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fosilising : MonoBehaviour
{
    [SerializeField]
    private float fadeTime = 2;

    [SerializeField]
    private float steps = 10;

    [SerializeField]
    private GameObject fosil;


    private bool collided = false;

    private Rigidbody rb;

    private float originalDrag;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalDrag = rb.drag;
        this.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
  
        if(collision.gameObject.tag == "Terrain" && !collided)
        {
            TimeOut();
            collided = true;
        }
    }
    Bounds GetMaxBounds(GameObject g)
    {
        var renderers = g.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(g.transform.position, Vector3.zero);
        var b = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }

    public void TimeOut()
    {
        IEnumerator coroutine = TimedDisolve(fadeTime);
        StartCoroutine(coroutine);
        
    }

    private IEnumerator TimedDisolve(float waitTime)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        Bounds B = GetMaxBounds(gameObject);
        GameObject thisFosil = Instantiate(fosil, transform.position - new Vector3(0, -0.75f, 0), new Quaternion(0, 0, 0, 0));
        thisFosil.transform.localScale = B.size * 1.1f;



        yield return new WaitForSeconds(waitTime);
        
       

        foreach (Transform p in GetComponent<GeneticMovementSentinel>().plastics)
        {
            Destroy(p.gameObject);
        }

        GetComponent<GeneticMovementSentinel>().plastics.Clear();

        GeneticAntigenKey[] antigens = GetComponentsInChildren<GeneticAntigenKey>();

        foreach (GeneticAntigenKey K in GetComponent<GeneticMovementSentinel>().digestAntigens)
        {
            Destroy(K.gameObject);
        }

        GetComponent<GeneticMovementSentinel>().digestAntigens.Clear();

        gameObject.SetActive(false);
        GetComponent<KuramotoAffecterAgent>().dead = true;
        rb.useGravity = false;
        rb.drag = originalDrag;
        this.enabled = false;
        collided = false;
        rb.isKinematic = false;

    }


}
