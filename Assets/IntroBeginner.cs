using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroBeginner : MonoBehaviour
{

    CameraTracker camTrack;

    private bool floating = true;

    [SerializeField]
    private float driftPower = 3.5f;
    [SerializeField]
    private float nScale = 3.5f;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        camTrack = GetComponent<CameraTracker>();
        Image  faderImage = transform.GetChild(0).GetComponentInChildren<Image>();
        //Make the alpha 1
        Color fixedColor = faderImage.color;
        fixedColor.a = 1;
        faderImage.color = fixedColor;

        //Set the 0 to zero then duration to 0
        faderImage.CrossFadeAlpha(0f, 2, true);

        rb = GetComponent<Rigidbody>();
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
            float x =  Mathf.PerlinNoise(transform.position.x* nScale +1, transform.position.y* nScale +1);
            float y = Mathf.PerlinNoise(transform.position.x*nScale + 2, transform.position.y* nScale + 2);
            float z = Mathf.PerlinNoise(transform.position.x* nScale, transform.position.y* nScale);
            rb.AddForce(new Vector3(x, y, z)* driftPower * Time.deltaTime);

        }
    }

    public void Begin()
    {
        camTrack.FindScreenTracked("BodyAlign");
        camTrack.enabled = true;
        floating = false;
    }
    public void Restart()
    {
        camTrack.ReturnToOrigin();
        floating = true;
    }
}
