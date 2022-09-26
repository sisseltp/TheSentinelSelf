using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomateUser : MonoBehaviour
{

    [SerializeField]
    private float changeTimeS = 120;

    private bool playing = false;
    private IntroBeginner introBeginner;


   

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UserChanger(changeTimeS));
    }

    // Update is called once per frame
    IEnumerator UserChanger(float delay)
    {
        yield return new WaitForSeconds(2);
        introBeginner = GetComponent<IntroBeginner>();

        while (true)
        {

            introBeginner.ChangeStates();


            yield return new WaitForSeconds(delay);
        }



    }
}
