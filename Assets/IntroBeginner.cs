using UnityEngine;
using UnityEngine.UI;

public class IntroBeginner : MonoBehaviour
{
    public static IntroBeginner Instance;

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
    private float nScale = 3.5f;

    [SerializeField]
    private float movementScl = 3f;
    private Vector3 origin;

    [SerializeField]
    private float rotSpeed = 1;

    private bool shouldChange;

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

        if (HeartRateManager.Instance.sensorConnected)
        {
            if (CameraTracker.Instance.tracking && !HeartRateManager.Instance.sensorHasValue || !CameraTracker.Instance.tracking && HeartRateManager.Instance.sensorHasValue)
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

  

    public void ChangeStates()
    {
        if (CameraTracker.Instance.doingIntro || CameraTracker.Instance.doingOutro) 
            return;
        
        if (CameraTracker.Instance.tracking)
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

        SoundFXManager.Instance.Play("ApproachBody");

        CameraTracker.Instance.BeginTracking();
        CameraTracker.Instance.enabled = true;

        floating = false;
        alongPath.enabled = false;
    }

    // Called when visitor disconnects, return to the outer world.
    private void Restart()
    {
        shouldChange = false;

        SoundFXManager.Instance.Play("ExitBody");
        // TODO: Maybe nicer to delay this or fade in?
        SoundFXManager.Instance.Play("VoiceOver");

        CameraTracker.Instance.ReturnToOrigin();
    }

    public void SetDoingIntro(bool state)
    {
    }

    public void SetDoingOutro(bool state)
    {
    }
}
