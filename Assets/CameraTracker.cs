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
                return;
            }
             if (Time.time-lastChange>ChangeTrackTimer)
            {
                lastChange = Time.time;
                FindScreenTracked("Player");
               
            }

            float x = Mathf.PerlinNoise(transform.position.x * nScale + 1, transform.position.y * nScale + 1);
            float y = Mathf.PerlinNoise(transform.position.x * nScale + 2, transform.position.y * nScale + 2);
            float z = Mathf.PerlinNoise(transform.position.x * nScale, transform.position.y * nScale);
            rb.AddForce(transform.right * driftPower * Time.deltaTime);

        }
     

        rb.velocity += dif * power * Time.deltaTime;


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
        
        Debug.Log("fading");
        yield return new WaitForSecondsRealtime(fadePeriod);
        Debug.Log("Faded");

        tracked = null;
        enabled = false;
        introCntrl.floating = true;
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

        tracked = bodies[indx].transform;
        if (heartbeatSensor != null)
        {
            heartbeatSensor.setSentinelAgent(tracked.GetComponent<KuramotoAffecterAgent>());
        }
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

    /*
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
*/
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
    }

    
}
