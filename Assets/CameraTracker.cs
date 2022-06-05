using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    private Transform tracked;
    private Vector3 offset;
    private float setDistance;

    [Range(0.1f, 10f)]
    [SerializeField]
    private float distVariation;
    [Range(0f, 1f)]
    [SerializeField]
    private float drag;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        tracked = transform.parent;
        offset = tracked.position - transform.position;
        setDistance = Vector3.Distance(tracked.position, transform.position);
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 dif = tracked.position - transform.position;
        float dist = Vector3.Distance(tracked.position, transform.position);

        transform.LookAt(tracked.position);
        //rb.MoveRotation(Quaternion.LookRotation(dif, Vector3.up));

        if (dist < setDistance - distVariation)
        {
            dif *= -1;
        }
        else if(dist < setDistance + distVariation)
        {
            return;
        }

        rb.velocity += dif*drag;

       //

       // rb.MoveRotation

    }
}
