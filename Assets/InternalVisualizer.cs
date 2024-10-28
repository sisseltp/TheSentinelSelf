using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternalVisualizer : MonoBehaviour
{
    private TCellManager[] tcellManagers;
    private PathogenManager[] pathogenManagers;

    [SerializeField]
    private Renderer rndr;

    [SerializeField]
    private Renderer[] growingObjs;

    private float draggedVarience=0;
    
    void Start()
    {
        tcellManagers = GetComponentsInChildren<TCellManager>();
        pathogenManagers = GetComponentsInChildren<PathogenManager>();        
    }
    
    void Update()
    {
        int numTCells = 0;
        foreach(TCellManager manager in tcellManagers)
            numTCells += manager.RealNumSentinels;

        int numPathogens = 0;
        foreach (PathogenManager manager in pathogenManagers)
            numPathogens += manager.RealNumPathogens;

        float variation = numTCells + numPathogens;
        variation = numTCells / variation;

        draggedVarience += (variation - draggedVarience) * 0.05f;
        rndr.materials[1].SetFloat("ChangeTextures", draggedVarience);

        foreach(Renderer rnd in growingObjs)
            rnd.material.SetFloat("Grow", draggedVarience);
    }
}
