using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationStats : MonoBehaviour
{
    [Header("Simulation Status")]

    [Tooltip("How often to update the status info (in seconds)")]
    public float updateStatsEvery = 5.0f;

    [Space(10)]
    [Header("APC Status")]

    [Tooltip("APCs seeking pathogens")]    
    public int seekingApcs = 0;

    [Tooltip("APCs carrying antigens to a lymph node")]    
    public int carryingApcs = 0;

    [Tooltip("APCs that are fossilized")]    
    public int fossilApcs = 0;

    [Tooltip("Total APCs in the simulation")]    
    public int totalApcs = 0;


    [Tooltip("Number of antigens digested by APCs")]    
    public int digestedAntigens = 0;

    [Tooltip("Number of microplastics digested by APCs")]    
    public int digestedPlastics = 0;

    [Tooltip("Number of microplastics currently in the simulation")]    
    public int plastics = 0;



    [Space(10)]
    [Header("Tcell / Pathogens")]

    [Tooltip("Number of naive Tcells")]    
    public int naiveTcells = 0;

    [Tooltip("Number of Tcells that are imprinted with an antigen")]    
    public int imprintedTcells = 0;

    [Tooltip("Number of Tcells that are confused by microplastics")]
    public int confusedTcells = 0;

    [Tooltip("Total Tcells currently in the simulation")]    
    public int totalTcells = 0;


    [Tooltip("Number of pathogens currently in the simulation")]    
    public int pathogens = 0;


    [Space(10)]
    [Header("General")]

    [Tooltip("Ratio Healthy to Infectious agents - Tcells / (Tcells + Pathogens)")]    
    public float health = 1.0f;



    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(checkWorld(updateStatsEvery));
    }

    IEnumerator checkWorld(float time)
    {
        while (true)
        {
            totalApcs = 0;
            seekingApcs = 0;
            carryingApcs = 0;
            digestedAntigens = 0;
            digestedPlastics = 0;
            foreach(GameObject apc in GameObject.FindGameObjectsWithTag("Player")) {
                GeneticMovementSentinel movementScript = apc.GetComponent<GeneticMovementSentinel>();
                totalApcs += 1;
                digestedAntigens += movementScript.digestAntigens.Count;
                digestedPlastics += movementScript.plastics.Count;
            }
            
            fossilApcs = GameObject.FindGameObjectsWithTag("Eggs").Length;

            totalTcells = 0;
            naiveTcells = 0;
            imprintedTcells = 0;
            confusedTcells = 0;
            foreach(GameObject tc in GameObject.FindGameObjectsWithTag("Tcell")) {
                int state = tc.GetComponent<KuramotoAffectedAgent>().played;
                totalTcells += 1;
                switch(state) {
                    case 0:
                        naiveTcells += 1;
                        break;
                    case 1:
                        imprintedTcells += 1;
                        break;
                    case 2:
                        confusedTcells += 1;
                        break;
                    default:
                        throw new System.Exception("Unknown Tcell behavior state: " + state);
                }
            }
            
            pathogens = GameObject.FindGameObjectsWithTag("Pathogen").Length;

            health = totalTcells + pathogens;
            health = totalTcells / health;

            plastics = GameObject.FindGameObjectsWithTag("Plastic").Length;

            yield return new WaitForSeconds(time);
        }


    }
    

}
