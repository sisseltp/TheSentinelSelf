using OscCore;
using UnityEngine;

public class OSCManager : MonoBehaviour
{
    [SerializeField]
    private bool useCOMPort;
    
    [SerializeField]
    private OscReceiver oscReceiver;

    [SerializeField]
    private SerialCOM comReceiver;

    private void Awake()
    {
        if (useCOMPort)
        {
            oscReceiver.enabled = false;
            comReceiver.enabled = true;
        }
        else
        {
            oscReceiver.enabled = true;
            comReceiver.enabled = false;
        }
    }
}
