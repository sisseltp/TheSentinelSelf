using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField]
    private float power = 0.5f;
    //[SerializeField]
    //private float rotScl = 0.5f;
    private Rigidbody rb;
    private Transform childCam;

    // Start is called before the first frame update
    void Start()
    {
        childCam = transform.GetChild(0);
        rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        float V = Input.GetAxis("Vertical");
        float H = Input.GetAxis("Horizontal");
        /*
        Vector2 mousePos =  (Vector2)Input.mousePosition;
        mousePos -= new Vector2(Screen.width / 2, Screen.height / 2);
        mousePos /= new Vector2(Screen.width / 2, Screen.height / 2);
        mousePos *= new Vector2(1, -1);
        */

        Vector3 forward = Vector3.Normalize(transform.position - childCam.position);
        forward.y = 0;
        Vector3 right = Quaternion.Euler(0,90,0) * forward;

        rb.velocity+= forward * V *power;
        rb.velocity += right * H * power;
       // Quaternion rot = rb.rotation * Quaternion.Euler(new Vector3(mousePos.y * rotScl, mousePos.x * rotScl, 0));
       // rb.MoveRotation(rot);



    }
}
