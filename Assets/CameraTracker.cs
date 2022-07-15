using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    public Transform tracked;
    private Vector3 offset;
    [SerializeField]
    private float setDistance;

    [Range(0.1f, 10f)]
    [SerializeField]
    private float distVariation;
    [Range(0f, 1f)]
    [SerializeField]
    private float drag;
    [Range(0f, 10f)]
    [SerializeField]
    private float rotSpeed;
    [Range(5f, 50f)]
    [SerializeField]
    private float distLimit;
    [HideInInspector]
    public Rigidbody rb;
    [HideInInspector]
    public bool tracking = false;
    // Start is called before the first frame update
    void Start()
    {
        //tracked = transform.parent;
        offset = transform.position - tracked.position;
//        setDistance = Vector3.Distance(tracked.position, transform.position);
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        

        Vector3 dif = tracked.position - transform.position;
        float dist = Vector3.Distance(tracked.position, transform.position);


        var targetRotation = Quaternion.LookRotation(dif);

        // Smoothly rotate towards the target point.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);

        //rb.MoveRotation(Quaternion.LookRotation(dif*0.1f, transform.up));

        if (tracking)
        {
            if (dist < setDistance - distVariation)
            {
                dif *= -1;
            }
            else if (dist < setDistance + distVariation)
            {
                return;
            }
            else if (dist > distLimit)
            {
                transform.position = tracked.position + offset;
            }
        }
       

        rb.velocity += dif*drag;

     
    }

    private void OnTriggerEnter(Collider collision)
    {
        
        if (collision.transform.tag == "Body") {

            FindTracked("Player");
            tracking = true;
        }

    }

    private void OnTriggerExit(Collider collision)
    {
        Debug.Log(collision.tag);
        if (collision.transform.tag == "Body")
        {
            Transform parent = collision.transform.parent.parent;
            Destroy(parent.gameObject);
       
        }
        else if (collision.transform.tag == "Layer")
        {
            Destroy(collision.gameObject);
        }
    }

    public void FindTracked(string tag)
    {
        GameObject[] bodies = GameObject.FindGameObjectsWithTag(tag);

        float dist = float.PositiveInfinity;

        int indx = -1;

        for (int i = 0; i < bodies.Length; i++)
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(bodies[i].transform.position);
            float thisDist = Vector2.Distance(screenPos, new Vector2(Screen.width/2, Screen.height/2));
            if (thisDist < dist)
            {
                indx = i;
                dist = thisDist;
            }
        }



        tracked = bodies[indx].transform;
    }
}
