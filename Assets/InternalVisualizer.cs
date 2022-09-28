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

    [Tooltip("How often to update the status info (in seconds)")]
    public float updateStatsEvery = 10.0f;

    [Header("Simulation Status")]

    [Tooltip("Ratio Healthy to Infectious agents - Tcells / (Tcells + Pathogens)")]    
    public float health = 1.0f;

    [Tooltip("Number of APCs currently in the simulation")]    
    public int apcs = 0;

    [Tooltip("Number of antigens digested by APCs")]    
    public int digestedAntigens = 0;

    [Tooltip("Number of fossilized APCs currently in the simulation")]    
    public int eggs = 0;


    [Tooltip("Number of Tcells currently in the simulation")]    
    public int tcells = 0;

    [Tooltip("Number of Tcells that are confused")]    
    public int confusedTcells = 0;

    [Tooltip("Number of pathogens currently in the simulation")]    
    public int pathogens = 0;

    [Tooltip("Number of microplastics currently in the simulation")]    
    public int plastics = 0;

    [Tooltip("Number of microplastics digested by APCs")]    
    public int digestedPlastics = 0;

    

    // Start is called before the first frame update
    void Start()
    {
        tcellManagers = GetComponentsInChildren<TCellManager>();
        pathogenManagers = GetComponentsInChildren<PathogenManager>();
        
        StartCoroutine(checkWorld(updateStatsEvery));
    }

        IEnumerator checkWorld(float time)
    {
        while (true)
        {
            apcs = 0;
            digestedAntigens = 0;
            digestedPlastics = 0;
            foreach(GameObject apc in GameObject.FindGameObjectsWithTag("Player")) {
                GeneticMovementSentinel movementScript = apc.GetComponent<GeneticMovementSentinel>();
                apcs += 1;
                digestedAntigens += movementScript.digestAntigens.Count;
                digestedPlastics += movementScript.plastics.Count;
            }
            
            eggs = GameObject.FindGameObjectsWithTag("Eggs").Length;

            tcells = 0;
            confusedTcells = 0;
            foreach(GameObject tc in GameObject.FindGameObjectsWithTag("Tcell")) {
                int state = tc.GetComponent<KuramotoAffectedAgent>().played;
                tcells += 1;
                if(state == 2) {
                    confusedTcells += 1;
                }
            }
            
            pathogens = GameObject.FindGameObjectsWithTag("Pathogen").Length;
            plastics = GameObject.FindGameObjectsWithTag("Plastic").Length;

            yield return new WaitForSeconds(time);
        }


    }
    
    // Update is called once per frame
    void Update()
    {
        int numTCells = 0;
        foreach(TCellManager manager in tcellManagers)
        {
            numTCells += manager.RealNumSentinels;
        }

        int numPathogens = 0;
        foreach (PathogenManager manager in pathogenManagers)
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
