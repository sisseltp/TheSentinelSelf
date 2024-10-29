using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternalVisualizer : MonoBehaviour
{
    private TCellsManager[] tcellManagers;
    private PathogensManager[] pathogensManagers;

    [SerializeField]
    private Renderer rndr;

    [SerializeField]
    private Renderer[] growingObjs;

    private float draggedVarience=0;
    
    void Start()
    {
        tcellManagers = GetComponentsInChildren<TCellsManager>();
        pathogensManagers = GetComponentsInChildren<PathogensManager>();        
    }
    
    void Update()
    {
        int numTCells = 0;
        foreach(TCellsManager manager in tcellManagers)
            numTCells += manager.realAmountAgents;

        int numPathogens = 0;
        foreach (PathogensManager manager in pathogensManagers)
            numPathogens += manager.realAmountAgents;

        float variation = numTCells + numPathogens;
        variation = numTCells / variation;

        draggedVarience += (variation - draggedVarience) * 0.05f;
        rndr.materials[1].SetFloat("ChangeTextures", draggedVarience);

        foreach(Renderer rnd in growingObjs)
            rnd.material.SetFloat("Grow", draggedVarience);
    }
}
