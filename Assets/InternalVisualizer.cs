using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternalVisualizer : MonoBehaviour
{
    private TCellManager[] tcells;
    private PathogenManager[] pathogens;
    [SerializeField]
    private Renderer rndr;

    private float draggedVarience=0;
    // Start is called before the first frame update
    void Start()
    {

        tcells = GetComponentsInChildren<TCellManager>();
        pathogens = GetComponentsInChildren<PathogenManager>();

    }
    
    // Update is called once per frame
    void Update()
    {
        int numTCells = 0;

        foreach(TCellManager manager in tcells)
        {
            numTCells += manager.RealNumSentinels;
        }

        int numPathogens = 0;

        foreach (PathogenManager manager in pathogens)
        {
            numPathogens += manager.RealNumPathogens;
        }

        float variation = numTCells + numPathogens;
        variation = numPathogens / variation;

        draggedVarience += (variation - draggedVarience) * 0.05f;
        rndr.materials[1].SetFloat("ChangeTextures", draggedVarience);
    }
}
