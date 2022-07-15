using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyEmitter : MonoBehaviour
{

    [SerializeField]
    private int Step = 10;

    [SerializeField]
    private GameObject Body;


    [SerializeField]
    private Vector2 selecterScreenPos = new Vector2(0.5f, 0.5f);

    [SerializeField]
    private float life = 100;

    private float timeGate = float.NegativeInfinity;


    private void Start()
    {
        selecterScreenPos = new Vector2(selecterScreenPos.x * Screen.width, selecterScreenPos.y * Screen.height);

    }

    // Update is called once per frame
    void Update()
    {
        
        if (Time.time -timeGate> Step)
        {
            GameObject bod =  Instantiate(Body, this.transform);
            timeGate = Time.time;
            Destroy(bod, life);
        }
      
        if (Input.GetMouseButtonDown(0) )
        {
           CameraTracker camTrack =  Camera.main.GetComponent<CameraTracker>();

            camTrack.FindTracked("Body");
            camTrack.enabled = true;

         
        }
    }

}
