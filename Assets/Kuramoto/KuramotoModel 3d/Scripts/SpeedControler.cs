using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedControler : MonoBehaviour
{
    [Range(0, 3)]
    [SerializeField]
    private float timeScl = 1;
    // Update is called once per frame
    void Update()
    {
        if (timeScl != Time.timeScale)
        {

            Time.timeScale = timeScl;

        }
    }
}
