using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternalVisualizer : MonoBehaviour
{
    private TCellManager[] tcells;
    private PathogenManager[] pathogens;
    [SerializeField]
    private Renderer rndr;
    [SerializeField]
    private Renderer[] growingObjs;

    private float draggedVarience=0;

    [Tooltip("How often to update the status of the simulation (in seconds)")]
    public float updateStatsEvery = 10.0f;

    [Header("Simulation Status")]

    [Tooltip("Ratio Healthy to Infectious agents > Tcells / (Tcells + Pathogens)")]    
    public float health = 1.0f;

    [Tooltip("Number of APCs currently in the simulation")]    
    public int numApcs = 0;

    [Tooltip("Number of fossilized APCs currently in the simulation")]    
    public int numEggs = 0;

    [Tooltip("Number of Tcells currently in the simulation")]    
    public int numTcells = 0;

    [Tooltip("Number of pathogens currently in the simulation")]    
    public int numPathogens = 0;

    [Tooltip("Number of microplastics currently in the simulation")]    
    public int numPlastics = 0;

    // Start is called before the first frame update
    void Start()
    {
        tcells = GetComponentsInChildren<TCellManager>();
        pathogens = GetComponentsInChildren<PathogenManager>();
        
        StartCoroutine(checkWorld(updateStatsEvery));
    }

        IEnumerator checkWorld(float time)
    {
        while (true)
        {
            numApcs = GameObject.FindGameObjectsWithTag("Player").Length;
            numEggs = GameObject.FindGameObjectsWithTag("Eggs").Length;
            numTcells = GameObject.FindGameObjectsWithTag("Tcell").Length;
            numPathogens = GameObject.FindGameObjectsWithTag("Pathogen").Length;
            numPlastics = GameObject.FindGameObjectsWithTag("Plastic").Length;
            yield return new WaitForSeconds(time);
        }


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
        health = numTCells / variation;

        draggedVarience += (health - draggedVarience) * 0.05f;
        rndr.materials[1].SetFloat("ChangeTextures", draggedVarience);

        foreach(Renderer rnd in growingObjs)
        {
            rnd.material.SetFloat("Grow", draggedVarience);
        }
    }
}
