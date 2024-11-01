using UnityEngine;
using UnityEngine.UI;

public class IntroBeginner : MonoBehaviour
{
    // [SerializeField] 
    // private GameObject underWaterScene;
    //
    // [SerializeField] 
    // private GameObject simulationScene;
    
    CameraTracker camTrack;

    private SoundFXManager soundFx;

    [SerializeField]
    private bool useMouseToTransition;
    
    [HideInInspector]
    public bool floating = true;

    [SerializeField]
    private float driftPower = 3.5f;
    
    [SerializeField]
    private float nScale = 3.5f;
    private Rigidbody rb;

    [SerializeField]
    private float movementScl = 3f;
    private Vector3 origin;

    [SerializeField]
    private Transform look;

    [SerializeField]
    private float rotSpeed = 1;

    [HideInInspector]
    public Romi.PathTools.MoveAlongPath alongPath;
    public Image faderImage;

    private bool sensorConnected;
    private bool sensorHasValue;
    private bool shouldChange;
    
    // Start is called before the first frame update
    void Start()
    {
        camTrack = GetComponent<CameraTracker>();
        soundFx = GetComponent<SoundFXManager>();
       
        //Make the alpha 1
        Color fixedColor = faderImage.color;
        fixedColor.a = 1;
        faderImage.color = fixedColor;

        //Set the 0 to zero then duration to 0
        faderImage.CrossFadeAlpha(0f, 2, true);

        rb = GetComponent<Rigidbody>();
        origin = transform.position;

        alongPath = GetComponent<Romi.PathTools.MoveAlongPath>();

        if (!useMouseToTransition)
        {
            Begin();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (useMouseToTransition && Input.GetMouseButtonDown(0))
        {
            ChangeStates();
        }

        if (sensorConnected)
        {
            if (camTrack.tracking && !sensorHasValue || !camTrack.tracking && sensorHasValue)
            {
                shouldChange = true;
            } 
            else
            {
                shouldChange = false;
            }
        }
        
        if (shouldChange)
        {
            ChangeStates();
        }

        if (floating)
        {
            /*
            float x =  Mathf.PerlinNoise(transform.position.x* nScale +1, transform.position.y* nScale +1);
            float y = Mathf.PerlinNoise(transform.position.x*nScale + 2, transform.position.y* nScale + 2);
            float z = Mathf.PerlinNoise(transform.position.x* nScale, transform.position.y* nScale);
            rb.AddForce(new Vector3(x, y, z)* driftPower * Time.deltaTime);
            */
            float x = Mathf.PerlinNoise(Time.time * nScale + 1, Time.time * nScale + 2);
            float y = Mathf.PerlinNoise(Time.time * nScale + 2, Time.time * nScale + 3);
            float z = Mathf.PerlinNoise(Time.time * nScale + 3, Time.time * nScale + 4);
            rb.position = origin + new Vector3(x, y, z) * movementScl;

            Vector3 lookDif = look.position - transform.position;

            var targetRotation = Quaternion.LookRotation(lookDif);

            // Smoothly rotate towards the target point.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);
        }
    }

    public void SetSensorConnected(bool isConnected, bool hasValue = false)
    {
        sensorConnected = isConnected;
        sensorHasValue = hasValue;
    }

    public void ChangeStates()
    {
        if (camTrack.doingIntro || camTrack.doingOutro) return;
        
        if (camTrack.tracking)
        {
            Debug.Log($"Change state, go back to body");
            Restart();
        }
        else
        {
            Debug.Log($"Change state, go into the scene");
            Begin();
        }
    }

    // Called when visitor touches the biosensor, begins approach to body.
    private void Begin()
    {
        shouldChange = false;
        soundFx.Play("ApproachBody");

        camTrack.BeginTracking();
        camTrack.enabled = true;
        floating = false;
        alongPath.enabled = false;
    }

    // Called when visitor disconnects, return to the outer world.
    private void Restart()
    {
        shouldChange = false;

        soundFx.Play("ExitBody");

        // TODO: Maybe nicer to delay this or fade in?
        soundFx.Play("VoiceOver");

        camTrack.ReturnToOrigin();
    }

    private bool doingIntro;
    private bool doingOutro;
    
    public void SetDoingIntro(bool state)
    {
        // if (!state && doingIntro)
        // {
        //     underWaterScene.SetActive(false);
        // }
        // else if (state && !doingIntro)
        // {
        //     simulationScene.SetActive(true);
        // }
        
        doingIntro = state;
    }

    public void SetDoingOutro(bool state)
    {
        // if (state && !doingOutro)
        // {
        //     underWaterScene.SetActive(true);
        // } 
        // else if (!state && doingOutro)
        // {
        //     simulationScene.SetActive(false);
        // }
        
        doingOutro = state;
    }
}
