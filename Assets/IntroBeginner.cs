using UnityEngine;
using UnityEngine.UI;

public class IntroBeginner : MonoBehaviour
{
    public static IntroBeginner Instance;
    [SerializeField]
    CameraTracker camTrack;
    [SerializeField]
    private SoundFXManager soundFx;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    public Romi.PathTools.MoveAlongPath alongPath;
    [SerializeField]
    private Image faderImage;
    [SerializeField]
    private Transform look;

    [SerializeField]
    private bool useMouseToTransition;
    
    [HideInInspector]
    public bool floating = true;

    [SerializeField]
    private float driftPower = 3.5f;
    
    [SerializeField]
    private float nScale = 3.5f;

    [SerializeField]
    private float movementScl = 3f;
    private Vector3 origin;

    [SerializeField]
    private float rotSpeed = 1;

    [HideInInspector]
    public bool sensorConnected;
    [HideInInspector]
    public bool sensorHasValue;

    private bool shouldChange;

    private bool doingIntro;
    private bool doingOutro;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Color fixedColor = faderImage.color;
        fixedColor.a = 1f;
        faderImage.color = fixedColor;
        faderImage.CrossFadeAlpha(0f, 2f, true);

        origin = transform.position;

        if (!useMouseToTransition)
            Begin();
    }

    void Update()
    {
        if (useMouseToTransition && Input.GetMouseButtonDown(0))
            ChangeStates();

        if (sensorConnected)
        {
            if (camTrack.tracking && !sensorHasValue || !camTrack.tracking && sensorHasValue)
                shouldChange = true;
            else
                shouldChange = false;
        }
        
        if (shouldChange)
            ChangeStates();

        if (floating)
        {
            float x = Mathf.PerlinNoise(Time.time * nScale + 1, Time.time * nScale + 2);
            float y = Mathf.PerlinNoise(Time.time * nScale + 2, Time.time * nScale + 3);
            float z = Mathf.PerlinNoise(Time.time * nScale + 3, Time.time * nScale + 4);
            rb.position = origin + new Vector3(x, y, z) * movementScl;

            Vector3 lookDif = look.position - transform.position;
            var targetRotation = Quaternion.LookRotation(lookDif);
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
        if (camTrack.doingIntro || camTrack.doingOutro) 
            return;
        
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

    public void SetDoingIntro(bool state)
    {
        doingIntro = state;
    }

    public void SetDoingOutro(bool state)
    {
        doingOutro = state;
    }
}
