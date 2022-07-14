using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationSelector : MonoBehaviour
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

        if (Input.GetMouseButtonDown(0))
        {
            GameObject[] bodies = GameObject.FindGameObjectsWithTag("Player");

            float dist = float.PositiveInfinity;

            int indx = -1;

            for(int i=0; i<bodies.Length; i++)
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(bodies[i].transform.position);
                float thisDist = Vector2.Distance(screenPos, selecterScreenPos);
                if (thisDist < dist)
                {
                    indx = i;
                    dist = thisDist;
                }
            }

            CameraTracker camTrack =  Camera.main.GetComponent<CameraTracker>();

            camTrack.rb = bodies[indx].GetComponent<Rigidbody>();
            camTrack.enabled = true;

            camTrack.tracked = bodies[indx].transform;
        }
    }

    
}
