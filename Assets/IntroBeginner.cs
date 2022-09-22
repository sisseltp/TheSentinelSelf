using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroBeginner : MonoBehaviour
{

    CameraTracker camTrack;

    AudioSource audioSource;

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

    [SerializeField]
    private AudioClip enterBodyClip;

    [SerializeField]
    private AudioClip exitBodyClip;



    [HideInInspector]
    public Romi.PathTools.MoveAlongPath alongPath;


    // Start is called before the first frame update
    void Start()
    {
        camTrack = GetComponent<CameraTracker>();
        audioSource = GetComponent<AudioSource>();
        Image  faderImage = transform.GetChild(0).GetComponentInChildren<Image>();
        //Make the alpha 1
        Color fixedColor = faderImage.color;
        fixedColor.a = 1;
        faderImage.color = fixedColor;

        //Set the 0 to zero then duration to 0
        faderImage.CrossFadeAlpha(0f, 2, true);

        rb = GetComponent<Rigidbody>();
        origin = transform.position;

        alongPath = GetComponent<Romi.PathTools.MoveAlongPath>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)==true)
        {
            Begin();
        }
        else if( Input.GetMouseButton(1)== true)
        {
            Restart();
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

    public void Begin()
    {
        Debug.Log("BEGIN!");
        audioSource.clip = enterBodyClip;
        audioSource.Play();

        camTrack.BeginTracking();
        camTrack.enabled = true;
        floating = false;
        alongPath.enabled = false;
        
    }
    public bool Restart()
    {
        if (!camTrack.Introing)
        {
            Debug.Log("RESTART!");
            camTrack.ReturnToOrigin();

            audioSource.clip = exitBodyClip;
            audioSource.Play();
            return true;
        }
        else
        {
            return false;
        }
    }
}
