using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreathingObjects : MonoBehaviour
{
    [SerializeField]
    private Renderer[] BreathingMaterials;

    [SerializeField]
    private GameObject[] BreathingSentinels;

    [SerializeField]
    private string id;

    [Space(10)]
    [Header("Debug")]
    public float phase;

    private KuramotoAffectedAgent phaseFocus;

    private float phaseSmoothed = 0;

    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        BreathingSentinels = GameObject.FindGameObjectsWithTag("BreathingSentinel");
        if (phaseFocus != null) {
            phase = Mathf.Sin(phaseFocus.phase * Mathf.PI);
            foreach (Renderer breath in BreathingMaterials)
            {
                breath.material.SetFloat(id, phase);
            }

            foreach (GameObject i in BreathingSentinels)
            {
                i.GetComponent<Renderer>().material.SetFloat(id, phase);
            }
        }
    }
    public void SetFocus(Transform focus)
    {
        phaseFocus = focus.GetComponent<KuramotoAffectedAgent>();
    }
}
