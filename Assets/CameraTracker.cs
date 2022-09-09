using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraTracker : MonoBehaviour
{
    public Transform tracked;
    public Transform look;
    [SerializeField]
    private float setDistance;

    [Range(0.1f, 10f)]
    [SerializeField]
    private float distVariation;
    [Range(0f, 10f)]
    [SerializeField]
    private float power;
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

    [SerializeField]
    private float ChangeTrackTimer = 10;
    private float lastChange = 0;

    [SerializeField]
    private float underWaterJumpDist = 10;
    

    private Vector3 origin;
    private Quaternion origRot;
    private Image faderImage;
    [SerializeField]
    private float fadePeriod = 2;

    private ethernetValues heartbeatSensor;
    private IntroBeginner introCntrl;

    [SerializeField]
    private float nScale = 0.1f;
    [SerializeField]
    private float driftPower = 5f;

    [SerializeField]
    private float restReset = 10;

    public float restTimer = 0;

    // Start is called before the first frame update
    void Start()
    {

        //tracked = transform.parent;
        //        setDistance = Vector3.Distance(tracked.position, transform.position);
        rb = GetComponent<Rigidbody>();

        origin = transform.position;
        origRot = transform.rotation;
        faderImage = transform.GetChild(0).GetComponentInChildren<Image>();

        heartbeatSensor = GetComponentInChildren<ethernetValues>();
        introCntrl = GetComponent<IntroBeginner>();
    }



    // Update is called once per frame
    void Update()
    {


        Vector3 dif = tracked.position - transform.position;

        Vector3 lookDif = look.position - transform.position;

        var targetRotation = Quaternion.LookRotation(lookDif);

        // Smoothly rotate towards the target point.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);

        if (tracking)
        {

            float dist = Vector3.Distance(tracked.position, transform.position);

            //rb.MoveRotation(Quaternion.LookRotation(dif*0.1f, transform.up));

            if (dist < setDistance - distVariation)
            {
                dif *= -1;
            }
            else if (dist < setDistance + distVariation)
            {
                if (AbsMagnitude(rb.velocity) < 3)
                {
                    restTimer += Time.deltaTime;
                    if (restTimer > restReset)
                    {
                        
                        FindScreenTracked("Player");
                        restTimer = 0;
                    }
                }
                else
                {
                    restTimer = 0;
                }
                return;
            }
             if (Time.time-lastChange>ChangeTrackTimer)
            {
                lastChange = Time.time;
                FindScreenTracked("Player");
               
            }

           
            rb.AddForce(transform.right * driftPower * Time.deltaTime);

        }
     

        rb.velocity += dif * power * Time.deltaTime;

       

    }

    private float AbsMagnitude(Vector3 vec)
    {
        return Mathf.Abs(vec.x) + Mathf.Abs(vec.y)+  Mathf.Abs(vec.z);

         
    }

    public void ReturnToOrigin()
    {
        faderImage.CrossFadeAlpha(1, fadePeriod, false);
        
        StartCoroutine(ReturnCallback());
    }

    IEnumerator ReturnCallback()
    {
        rb.velocity = Vector3.zero;
        tracking = false;
        
        yield return new WaitForSecondsRealtime(fadePeriod);

        tracked = null;
        enabled = false;
        introCntrl.floating = true;
        introCntrl.alongPath.enabled = true;
        transform.position = origin;
        transform.rotation = origRot;
        faderImage.CrossFadeAlpha(0, fadePeriod, false);
    }
    private void OnTriggerEnter(Collider collision)
    {
        
        if (collision.transform.tag == "Body") {

            FindSceneTracked("Player");
            tracking = true;
            rb.position -= new Vector3(0, underWaterJumpDist, 0);
        } else if (collision.transform.tag == "BodyAlign")
        {
            FindScreenTracked("Body");
        }

    }

    private void FindSceneTracked(string tag)
    {
        GameObject[] bodies = GameObject.FindGameObjectsWithTag(tag);

        int max = 0;

        int indx = -1;

        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i].GetInstanceID() != tracked.GetInstanceID())
            {
                int numPlastics =  bodies[i].GetComponentsInChildren<GeneticMovementPlastic>().Length;

                if (max < numPlastics)
                {
                    indx = i;
                    max = numPlastics;
                }
            }
        }

        if (indx == -1)
        {
            float dist = float.PositiveInfinity;
            for (int i = 0; i < bodies.Length; i++)
            {
                if (bodies[i].GetInstanceID() != tracked.GetInstanceID())
                {
                    float thisDist = Vector3.Distance(bodies[i].transform.position, transform.position);
                    if (thisDist < dist)
                    {
                        indx = i;
                        dist = thisDist;
                    }
                }
            }
        }
        
        look = bodies[indx].transform;

        tracked = bodies[indx].transform;
        if (heartbeatSensor != null)
        {
            heartbeatSensor.setSentinelAgent(tracked.GetComponent<KuramotoAffecterAgent>());
        }
        GetComponent<BreathingObjects>().SetFocus(tracked);
    }

    public void FindSceneLook(string tag)
    {
        GameObject[] bodies = GameObject.FindGameObjectsWithTag(tag);

        float dist = float.PositiveInfinity;

        int indx = -1;

        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i].GetInstanceID() != tracked.GetInstanceID())
            {
                float thisDist = Vector3.Distance(bodies[i].transform.position, transform.position);
                if (thisDist < dist)
                {
                    indx = i;
                    dist = thisDist;
                }
            }
        }

        look = bodies[indx].transform;
    }


    public void FindScreenTracked(string tag)
    {
        GameObject[] bodies = GameObject.FindGameObjectsWithTag(tag);

        float dist = float.PositiveInfinity;

        int indx = -1;

        for (int i = 0; i < bodies.Length; i++)
        {
            if (tracked==null || bodies[i].GetInstanceID() != tracked.GetInstanceID())
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(bodies[i].transform.position);
                float thisDist = Vector2.Distance(screenPos, new Vector2(Screen.width / 2, Screen.height / 2));
                if (thisDist < dist)
                {
                    indx = i;
                    dist = thisDist;
                }
            }
        }

        look = bodies[indx].transform;

        tracked = bodies[indx].transform;

        GetComponent<BreathingObjects>().SetFocus(tracked);
    }

    
}
