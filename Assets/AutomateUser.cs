using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomateUser : MonoBehaviour
{
    [SerializeField]
    private float changeTimeS = 120;
    private IntroBeginner introBeginner;

    void Start()
    {
        StartCoroutine(UserChanger(changeTimeS));
    }

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
