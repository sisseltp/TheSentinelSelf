using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField]
    private float power = 0.5f;
    [SerializeField]
    private float rotScl = 0.5f;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {

        rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        float V = Input.GetAxis("Vertical");
        //float H = Input.GetAxis("Horizontal");

        Vector2 mousePos =  (Vector2)Input.mousePosition;
        mousePos -= new Vector2(Screen.width / 2, Screen.height / 2);
        mousePos /= new Vector2(Screen.width / 2, Screen.height / 2);
        mousePos *= new Vector2(1, -1);
        

        rb.velocity+= transform.forward * V *power;
        Quaternion rot = rb.rotation * Quaternion.Euler(new Vector3(mousePos.y * rotScl, mousePos.x * rotScl, 0));
        rb.MoveRotation(rot);



    }
}
