using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreathingObjects : MonoBehaviour
{
    [SerializeField]
    private Renderer[] BreathingMaterials;

    [SerializeField]
    private string id;

    [Space(10)]
    [Header("Debug")]
    public float phase;

    private KuramotoAffecterAgent phaseFocus;

    private float phaseSmoothed = 0;

    // Update is called once per frame
    void Update()
    {
        if (phaseFocus != null) {
            phase = Mathf.Sin(phaseFocus.phase * Mathf.PI);
            foreach (Renderer breath in BreathingMaterials)
            {
                breath.material.SetFloat(id, phase);
            }
        }
    }
    public void SetFocus(Transform focus)
    {
        phaseFocus = focus.GetComponent<KuramotoAffecterAgent>();
    }
}
