using System;
using System.Collections;
using System.Linq;
using Script.CameraSystem;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CameraTracker : MonoBehaviour
{
    public static CameraTracker Instance;

    public event Action OnOutroDone;
    
    [SerializeField]
    public Rigidbody rb;
    
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Image faderImage;
    [SerializeField]
    private SphereCollider sphereCollider;
    [SerializeField] 
    private float maxSpeed = 5;
    
    private Transform tracked;

    private Transform look;

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

    [FormerlySerializedAs("changeTrackTimer")]
    [FormerlySerializedAs("ChangeTrackTimer")] 
    [SerializeField]
    private float switchTargetTime = 10;

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

    public bool tracking;
    [FormerlySerializedAs("Introing")] 
    public bool doingIntro;

    public bool doingOutro;

    [SerializeField] private float targetSwitchTimer;
    [SerializeField] private float orientationSwitchTimer;

    private Transform currentTarget;
    private Transform nextTarget;

    private CameraBrain cameraBrain;
    [SerializeField] private Transform[] pathogenEmitterLocations;
    
    private float lastChange = 0;    
    private Vector3 origin;
    private Quaternion origRot;
    private Vector3 lookPos;
    private Vector3 vel;

    private int lastIndx = -1;

    private void Awake()
    {
        Instance = this;
        cameraBrain = GetComponent<CameraBrain>();
    }

    private void Start()
    {
        origin = transform.position;
        origRot = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (!tracked || !look) 
            return;
        
        Vector3 dif = tracked.position - transform.position;

        if (normalized)
            dif = Vector3.Normalize(dif);

        lookPos += (look.position - lookPos) * 0.2f;
        Vector3 lookDif = lookPos - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(lookDif);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);

        if (tracking)
        {
            targetSwitchTimer += Time.deltaTime;
            orientationSwitchTimer += Time.deltaTime;

            if (orientationSwitchTimer >= switchTargetTime * 0.666f)
            {
                orientationSwitchTimer = 0;
                ChangeOrientation();
            }
                
            if (targetSwitchTimer >= switchTargetTime)
            {
                targetSwitchTimer = 0;
                SwitchTarget(CameraSwitchReason.TimeWasUp);
            }
            
            float dist = Vector3.Distance(tracked.position, transform.position);
            
            if (dist < setDistance - distVariation) 
            {
                dif *= -1;
            }
            else if (dist < setDistance + distVariation)
            {
                if (AbsMagnitude(rb.velocity) < 3)
                {
                    restTimer += Time.deltaTime;
                    
                    if (!(restTimer > restReset)) return;
                    
                    restTimer = 0;
                    SwitchTarget(CameraSwitchReason.CameraWasTooStill);
                }
                else
                {
                    restTimer = 0;
                }

                return;
            }
          
            rb.AddForce(transform.right * (driftPower * Time.deltaTime));

            Ray forward = new Ray(transform.position, Vector3.Normalize(rb.velocity) + Vector3.down * 0.5f);

            if (Physics.SphereCast(forward,3, out RaycastHit hit, 20))
                if (hit.transform.CompareTag("Terrain"))
                    vel += Vector3.up * (Vector3.Magnitude(rb.velocity)/3);
        }
        
        vel += dif * (power * Time.deltaTime);
        vel *= 0.8f;
        rb.velocity += vel;
        
        if(rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    public void OverrideTracked(Transform newTarget)
    {
        if (!tracking) return;
        
        targetSwitchTimer = 0;
        nextTarget = newTarget;

        SwitchTarget(CameraSwitchReason.BetterEvent);
    }

    private void StartTracking()
    {
        tracking = true;
        
        if (tracked.CompareTag("Player")) return;
        
        var trans = FindSceneTracked("Player");
        nextTarget = trans;
        
        SwitchTarget(CameraSwitchReason.Start);
    }
    
    private void SwitchTarget(CameraSwitchReason reason)
    {
        Debug.Log($"<color=orange>Camera System:</color> Camera switched target, reason: {reason}");

        if (nextTarget == null)
        {
            // Try get something interesting
            nextTarget = cameraBrain.GetNextInteresting();

            // Nothing interesting revert back to oldskool target
            if (nextTarget == null)
            {
                Debug.Log("<color=orange>Camera System:</color> No interesting target using oldskool logic.");
                nextTarget = FindSceneTracked("Player");
            }
            else
            {
                Debug.Log("<color=orange>Camera System:</color> Better interesting target was found.");
            }
        }

        currentTarget = nextTarget;
        nextTarget = null;

        if (currentTarget == null) return;

        look = currentTarget;
        tracked = currentTarget;
    }
    
    private Transform FindSceneTracked(string tagToFind)
    {
        GameObject[] bodies = GameObject.FindGameObjectsWithTag(tagToFind);
        int max = 0;
        int indx = -1;
        for (int i = 0; i < bodies.Length; i++)
        {
            if (i == lastIndx)
                continue;

            int numPlastics =  bodies[i].GetComponentsInChildren<GeneticMovementPlastic>().Length;
            if (max < numPlastics)
            {
                indx = i;
                max = numPlastics;
            }
        }

        if (indx == -1)
        {
            // Find a target close to an emmiter
            for (var i = 0; i < bodies.Length; i++)
            {
                if (!pathogenEmitterLocations
                        .Select(pathogenEmitterLocation =>
                            Vector3.Distance(bodies[i].transform.position, pathogenEmitterLocation.position))
                        .Any(dist => dist <= 5)) continue;
                
                indx = i;
                break;
            }
        }
        
        if (indx == -1)
        {
            float dist = 0f;
            for (int i = 0; i < bodies.Length; i++)
            {
                if (i == lastIndx)
                    continue;

                float thisDist = Vector3.Distance(bodies[i].transform.position, transform.position);
                if (thisDist > dist && bodies[i].GetComponentInChildren<Renderer>().isVisible)
                {  
                    indx = i;
                    dist = thisDist;
                }
            }
        }

        if (indx == -1)
        {
            indx = Random.Range(0, bodies.Length);
        }

        lastIndx = indx;
        return bodies[indx].transform;
    }

    private void ChangeOrientation()
    {
        var rand = Random.Range(0, 4);

        switch (rand)
        {
            case 0:
                driftPower *= -1;
                break;
            case 1:
                setDistance = Random.Range(10, 34);
                break;
            case 2:
                power = Random.Range(0.1f, 0.3f);
                break;
            case 3:
                rotSpeed = Random.Range(0.5f, 1f);
                break;
        }
    }
    
    private float AbsMagnitude(Vector3 vec) => Mathf.Abs(vec.x) + Mathf.Abs(vec.y) + Mathf.Abs(vec.z);
    
    //------ BODY SCENE LOGIC ------//
    
    #region BODY SCENE LOGIC
    // Related to the body scene
    public void BeginTracking()
    {
        doingIntro = true;
        IntroBeginner.Instance.SetDoingIntro(true);
        FindScreenTracked("BodyAlign");
        FindSceneLook("Body");
    }

    // Related to the body scene
    public void ReturnToOrigin()
    {
        if (!doingIntro && !doingOutro)
        {
            doingOutro = true;
            IntroBeginner.Instance.SetDoingOutro(true);
            Debug.Log("Starting the outro");
            StartCoroutine(ReturnCallback());     
        }
    }

    // Related to the body scene
    IEnumerator ReturnCallback()
    {
        rb.velocity = Vector3.zero;
        tracking = false;
        
        if(faderImage != null)
            faderImage.CrossFadeAlpha(1, fadePeriod, false);
        
        yield return new WaitForSecondsRealtime(fadePeriod);

        tracked = null;
        enabled = false;
        IntroBeginner.Instance.floating = true;
        IntroBeginner.Instance.alongPath.enabled = true;
        
        var trans = transform;
        trans.position = origin;
        trans.rotation = origRot;
        
        if(faderImage != null)
            faderImage.CrossFadeAlpha(0, fadePeriod, false);
        GetComponent<SphereCollider>().isTrigger = true;

        yield return new WaitForSeconds(2);

        OnOutroDone?.Invoke();
        
        doingOutro = false;
        IntroBeginner.Instance.SetDoingOutro(false);
    }
   
    // Related to the body scene
    private void OnTriggerEnter(Collider collision)
    {        
        if (collision.transform.CompareTag("Body")) { // Camera is entering the body, into the inner world.
            SoundFXManager.Instance.Play("EnterBody");
            SoundFXManager.Instance.Stop("VoiceOver");

            faderImage.StopAllCoroutines();
            faderImage.CrossFadeAlpha(1f, 0f, false);

            // FindSceneTracked("Player");
            StartTracking();
            
            rb.position -= new Vector3(0, underWaterJumpDist, 0);
            // sphereCollider.isTrigger = false;
            doingIntro = false;
            IntroBeginner.Instance.SetDoingIntro(false);
            // StartCoroutine(ChangeCharacter(changeTrackTimer));
            // StartCoroutine(ChangeOrientation(changeTrackTimer * 0.666f));
            
            faderImage.CrossFadeAlpha(0f, 0.25f, false);
        } 
        else if (collision.transform.CompareTag("BodyAlign")) 
        {
            // camera is aligned and looking down at the body
            FindScreenTracked("Body");
        }
        else if (collision.transform.CompareTag("StartFade"))
        {
            faderImage.CrossFadeAlpha(1f, 0.25f, false);
        }

    }

    // Related to the body scene
    private void FindSceneLook(string tagToFind)
    {
        GameObject[] bodies = GameObject.FindGameObjectsWithTag(tagToFind);

        float dist = float.PositiveInfinity;

        int indx = -1;

        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i].GetInstanceID() == tracked.GetInstanceID())
                continue;

            float thisDist = Vector3.Distance(bodies[i].transform.position, transform.position);
            if (thisDist < dist)
            {
                indx = i;
                dist = thisDist;
            }
        }

        look = bodies[indx].transform;
    }

    // Related to the body scene
    public void FindScreenTracked(string tagToFind)
    {
        GameObject[] bodies = GameObject.FindGameObjectsWithTag(tagToFind);

        float dist = float.PositiveInfinity;

        int indx = -1;

        for (int i = 0; i < bodies.Length; i++)
        {
            if (tracked != null && bodies[i].GetInstanceID() == tracked.GetInstanceID())
                continue;

            Vector2 screenPos = mainCamera.WorldToScreenPoint(bodies[i].transform.position);
            float thisDist = Vector2.Distance(screenPos, new Vector2(Screen.width / 2, Screen.height / 2));
            if (thisDist < dist)
            {
                indx = i;
                dist = thisDist;
            }
        }

        look = bodies[indx].transform;
        tracked = bodies[indx].transform;
    }
    #endregion BODY SCENE LOGIC
    
    //------ BODY SCENE LOGIC ------//

    private enum CameraSwitchReason
    {
        None,
        Start,
        TimeWasUp,
        BetterEvent,
        CameraWasTooStill,
        AlreadyFollowing
    }
}