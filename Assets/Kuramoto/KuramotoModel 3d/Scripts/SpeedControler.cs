using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedControler : MonoBehaviour
{


    [Range(0, 3)]
    [SerializeField]
    private float timeScl = 1; 
    private float fixedDeltaTime;

    void Awake()
    {
        // Make a copy of the fixedDeltaTime, it defaults to 0.02f, but it can be changed in the editor
        fixedDeltaTime = Time.fixedDeltaTime;
    }
    // Update is called once per frame
    void Update()
    {
        if (timeScl != Time.timeScale)
        {

            Time.timeScale = timeScl;
            //Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;

        }
    }
}
