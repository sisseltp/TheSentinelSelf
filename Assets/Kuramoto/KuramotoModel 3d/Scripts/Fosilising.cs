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

    private float fadeIn = 0;

    private bool collided = false;

    private Rigidbody rb;

    private float originalDrag;


    private void Start()
    {
        fadeIn = 0;
        rb = GetComponent<Rigidbody>();
        originalDrag = rb.drag;
        this.enabled = false;
    }


    private void OnCollisionStay(Collision collision)
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
        rb.isKinematic = true;
    }

    private IEnumerator TimedDisolve(float waitTime)
    {
        Bounds B = GetMaxBounds(gameObject);
        GameObject thisFosil = Instantiate(fosil, transform.position - new Vector3(0, -0.75f, 0), new Quaternion(0, 0, 0, 0));
        thisFosil.transform.localScale = B.size * 1.1f;

        float finish = Time.time + waitTime;

        float step = waitTime / steps;

        while (Time.time <= finish)
        {
           
           

            fadeIn += 1 / steps;
          

            thisFosil.GetComponentInChildren<Renderer>().material.SetFloat("fadeVal", fadeIn);

            yield return new WaitForSeconds(step);
        }

        GeneticMovementPlastic[] plastics = GetComponentsInChildren<GeneticMovementPlastic>();

        foreach (GeneticMovementPlastic p in plastics)
        {
            Destroy(p.gameObject);
        }

        GeneticAntigenKey[] antigens = GetComponentsInChildren<GeneticAntigenKey>();

        foreach (GeneticAntigenKey K in antigens)
        {
            Destroy(K.gameObject);
        }


        gameObject.SetActive(false);
        GetComponent<KuramotoAffecterAgent>().dead = true;
        rb.useGravity = false;
        rb.drag = originalDrag;
        this.enabled = false;
        collided = false;
        rb.isKinematic = false;

    }


}
