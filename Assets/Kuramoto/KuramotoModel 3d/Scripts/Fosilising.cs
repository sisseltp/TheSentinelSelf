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

        Bounds B =  GetMaxBounds(gameObject);
        fosil.transform.localScale = B.size *1.1f;
        fosil = Instantiate(fosil, transform.position - new Vector3(0,-0.75f,0), new Quaternion(0, 0, 0, 0));
        StartCoroutine(coroutine);
    }

    private IEnumerator TimedDisolve(float waitTime)
    {

        float finish = Time.time + waitTime;

        float step = waitTime / steps;

        while (Time.time <= finish)
        {
           
           

            fadeIn += 1 / steps;
            if (fadeIn > 1 - (1 / steps))
            {
                int numChild = transform.childCount;

                for(int i = 0; i < numChild; i++)
                {
                    Transform child = transform.GetChild(i);
                    if(child.tag == "Plastic")
                    {
                        Destroy(child.gameObject);

                    }

                }
                GetComponent<KuramotoAffecterAgent>().dead = true;
                rb.useGravity = false;
                rb.drag = originalDrag;
            }

            fosil.GetComponent<Renderer>().material.SetFloat("fadeVal", fadeIn);

            yield return new WaitForSeconds(step);
        }
        

    }
}
