using System.Collections;
using UnityEngine;

public class AutomateUser : MonoBehaviour
{
    [SerializeField]
    private float changeTimeS = 120;
    private IntroBeginner introBeginner;

    private void Start()
    {
        StartCoroutine(UserChanger(changeTimeS));
    }

    private IEnumerator UserChanger(float delay)
    {
        yield return new WaitForSeconds(2);
        introBeginner = GetComponent<IntroBeginner>();

        // TODO: Code smell fix this
        while (true)
        {
            introBeginner.ChangeStates();
            yield return new WaitForSeconds(delay);
        }
    }
}
