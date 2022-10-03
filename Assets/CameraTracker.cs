using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraTracker : MonoBehaviour
{
    public Transform tracked;
    public Transform look;
    
    [Space(10)]
    [Header("Body Entry Animation")]

    [Tooltip("Something to do with the distance to/from a target?")]
    [SerializeField]
    private float setDistance;

    [Tooltip("Something to do with the distance variation to/from a target?")]
    [Range(0.1f, 10f)]
    [SerializeField]
    private float distVariation;
    
    [Range(0f, 20f)]
    [SerializeField]
    private float power;
    
    [Range(0f, 10f)]
    [SerializeField]
    private float rotSpeed;
    
    [Range(5f, 50f)]
    [SerializeField]
    private float distLimit;

    [SerializeField]
    private float ChangeTrackTimer = 10;

    [SerializeField]
    private float underWaterJumpDist = 10;

    [SerializeField]
    private float fadePeriod = 2;

    [SerializeField]
    private float driftPower = 5f;

    [SerializeField]
    private float restReset = 10;

    public float restTimer = 0;

    [SerializeField]
    private bool normalized = true;

    [Space(10)]
    [Header("Debugging")]

    public bool tracking = false;
    public bool Introing = false;

    private float lastChange = 0;    
    private Vector3 origin;
    private Quaternion origRot;
    private Image faderImage;
    private Vector3 lookPos;
    private Vector3 vel;

    [HideInInspector]
    public Rigidbody rb;

    // Heartbeat sensor script component
    private ethernetValues heartbeatSensor;

    // Enter/exit body control script component
    private IntroBeginner introCntrl;

    // Script/component manages FX sound on the camera
    private SoundFXManager soundFx;



    // Start is called before the first frame update
    void Start()
    {
        //tracked = transform.parent;
        //        setDistance = Vector3.Distance(tracked.position, transform.position);
        rb = GetComponent<Rigidbody>();
        soundFx = GetComponent<SoundFXManager>();

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

        if (normalized)
        {
            dif = Vector3.Normalize(dif);
        }

        lookPos += (look.position - lookPos) * 0.2f;
    
        Vector3 lookDif = lookPos - transform.position;

        var targetRotation = Quaternion.LookRotation(lookDif);

        // Smoothly rotate towards the target point.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);

        if (tracking) {
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
                        FindSceneTracked("Player");
                        restTimer = 0;
                    }
                }
                else
                {
                    restTimer = 0;
                }
                return;
            }
          
            rb.AddForce(transform.right * driftPower * Time.deltaTime);

            Ray forward = new Ray(transform.position, Vector3.Normalize(rb.velocity) + Vector3.down * 0.5f);

            RaycastHit hit;
            if (Physics.SphereCast(forward,3, out hit, 20))
            {
                if (hit.transform.tag == "Terrain")
                {
                    vel += Vector3.up * (Vector3.Magnitude(rb.velocity)/3);
                }
            }
        }

        KuramotoAffecterAgent kA = tracked.GetComponent<KuramotoAffecterAgent>();

        vel += dif * power * Time.deltaTime;
        vel *= 0.8f;
        rb.velocity += vel;
    }

    private float AbsMagnitude(Vector3 vec)
    {
        return Mathf.Abs(vec.x) + Mathf.Abs(vec.y)+  Mathf.Abs(vec.z);         
    }

    public void BeginTracking()
    {
        Introing = true;
        FindScreenTracked("BodyAlign");
        FindSceneLook("Body");
    }

    public void ReturnToOrigin()
    {
        if (!Introing)
        {
            faderImage.CrossFadeAlpha(1, fadePeriod, false);
            StartCoroutine(ReturnCallback());
            
        }
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
        GetComponent<SphereCollider>().isTrigger = true;
    }
   
    private void OnTriggerEnter(Collider collision)
    {        
        if (collision.transform.tag == "Body") { // Camera is entering the body, into the inner world.
            soundFx.Play("EnterBody");
            soundFx.Stop("VoiceOver");

            FindSceneTracked("Player");

            tracking = true;
            rb.position -= new Vector3(0, underWaterJumpDist, 0);
            GetComponent<SphereCollider>().isTrigger = false;
            Introing = false;

            StartCoroutine(ChangeCharacter(ChangeTrackTimer));
            StartCoroutine(ChangeOrientation(ChangeTrackTimer * 0.666f));

        } else if (collision.transform.tag == "BodyAlign") { // camera is aligned and looking down at the body
            FindScreenTracked("Body");
        }
    }

    private int lastIndx = -1;
    private void FindSceneTracked(string tag)
    {
        GameObject[] bodies = GameObject.FindGameObjectsWithTag(tag);
        int max = 0;
        int indx = -1;
        for (int i = 0; i < bodies.Length; i++)
        {
            if (i!= lastIndx)
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
            float dist = 0;
            for (int i = 0; i < bodies.Length; i++)
            {
                if (i!= lastIndx)
                {
                    float thisDist = Vector3.Distance(bodies[i].transform.position, transform.position);
                    if (thisDist > dist && bodies[i].GetComponentInChildren<Renderer>().isVisible)
                    {  
                        indx = i;
                        dist = thisDist;
                    }
                }
            }
        }

        if (indx == -1)
        {
            indx = UnityEngine.Random.Range(0, bodies.Length);
        }

        lastIndx = indx;
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

    IEnumerator ChangeCharacter(float Timer)
    {
        while (tracking)
        {
            yield return new WaitForSeconds(Timer);
            if (tracking)
            {
                FindSceneTracked("Player");
            }
        }
    }

    IEnumerator ChangeOrientation(float Timer)
    {
        while (tracking)
        {

            yield return new WaitForSeconds(Timer);

            if (tracking)
            {
                int rand = UnityEngine.Random.Range(0, 4);

                if (rand == 0)
                {
                    driftPower *= -1;
                }
                else if (rand == 1)
                {
                    setDistance = UnityEngine.Random.Range(10, 34);
                }
                else if(rand == 2)
                {
                    power = UnityEngine.Random.Range(0.1f, 0.3f);
                }
                else if (rand == 3)
                {
                    rotSpeed = UnityEngine.Random.Range(0.5f, 1f);
                }
                //Debug.Log("Changed");
            }
        }

    }


}
