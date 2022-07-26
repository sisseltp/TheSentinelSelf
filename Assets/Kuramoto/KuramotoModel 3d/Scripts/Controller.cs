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
        float V = Input.GetAxis("Vertical")*-1;
        float H = Input.GetAxis("Horizontal")*-1;
        float U = 0;

        if (Input.GetMouseButton(0))
        {
            U += 1;
        }
        else if (Input.GetMouseButton(1)){
            U -= 1;
        }
        /*
        Vector2 mousePos =  (Vector2)Input.mousePosition;
        mousePos -= new Vector2(Screen.width / 2, Screen.height / 2);
        mousePos /= new Vector2(Screen.width / 2, Screen.height / 2);
        mousePos *= new Vector2(1, -1);
        */

        Vector3 forward = transform.forward*360;
        Vector3 vel = Vector3.zero;

        // vel = Quaternion.Euler(forward) * vel;

        vel += forward * V;
        vel += transform.right * H;
        vel += transform.up * U;
        vel = Vector3.Normalize(vel);
        vel *= power;

        rb.AddForceAtPosition(vel * Time.deltaTime, transform.position - transform.forward);



    }
}
