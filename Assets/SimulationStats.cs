    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationStats : MonoBehaviour
{
    [Header("Simulation Status")]

    [Tooltip("How often to update the status info (in seconds)")]
    public float updateStatsEvery = 5.0f;

    [Space(10)]
    [Header("Antigen Presenting Cells (APC)")]

    [Tooltip("APCs seeking pathogens")]    
    public int seekingPathogens = 0;

    [Tooltip("APCs carrying antigens to a lymph node")]    
    public int carryingToLymphNode = 0;

    [Tooltip("APCs that are fossilized")]    
    public int fossilized = 0;

    [Space(5)]

    [Tooltip("Total APCs in the simulation")]    
    public int totalApcs = 0;

    [Space(5)]

    [Tooltip("Number of antigens digested by APCs")]    
    public int totalDigestedAntigens = 0;

    [Tooltip("Number of microplastics digested by APCs")]    
    public int totalDigestedPlastics = 0;

    [Tooltip("Number of microplastics currently in the simulation")]    
    public int totalPlastics = 0;



    [Space(10)]
    [Header("Tcell / Pathogens")]

    [Tooltip("Number of naive Tcells")]    
    public int naiveTcells = 0;

    [Tooltip("Number of Tcells that are imprinted with an antigen")]    
    public int imprintedTcells = 0;

    [Tooltip("Number of Tcells that are confused by microplastics")]
    public int confusedTcells = 0;

    [Space(5)]
    [Tooltip("Total Tcells currently in the simulation")]    
    public int totalTcells = 0;

    [Tooltip("Number of pathogens currently in the simulation")]    
    public int totalPathogens = 0;


    [Space(10)]
    [Header("General")]

    [Tooltip("Ratio Pathogens to Tcells -- pathogens / (Tcells + pathogens)")]    
    public float infection = 0.0f;

    [Tooltip("Total agents")]    
    public int totalAgents = 0;

    public float simulationRunningHours = 0.0f;
    public float simulationRunningMinutes = 0.0f;
    public float simulationRunningSeconds = 0.0f;



    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(checkWorld(updateStatsEvery));
    }

    IEnumerator checkWorld(float timetowait)
    {
        while (true)
        {
            totalApcs = 0;
            seekingPathogens = 0;
            carryingToLymphNode = 0;
            totalDigestedAntigens = 0;
            totalDigestedPlastics = 0;
            foreach(GameObject apc in GameObject.FindGameObjectsWithTag("Player")) {
                GeneticMovementSentinel movementScript = apc.GetComponent<GeneticMovementSentinel>();
                switch(movementScript.currentBehavior) {
                    case APCBehavior.SeekingPathogens:
                        seekingPathogens += 1;
                        break;
                    case APCBehavior.CarryingAntigens:
                        carryingToLymphNode += 1;
                        break;
                    default:
                        throw new System.Exception("Unknown APCBehavior: " + movementScript.currentBehavior);
                }
                totalApcs += 1;
                totalDigestedAntigens += movementScript.digestAntigens.Count;
                totalDigestedPlastics += movementScript.plastics.Count;
            }
            
            fossilized = GameObject.FindGameObjectsWithTag("Eggs").Length;

            totalTcells = 0;
            naiveTcells = 0;
            imprintedTcells = 0;
            confusedTcells = 0;
            foreach(GameObject tc in GameObject.FindGameObjectsWithTag("Tcell")) {
                int state = tc.GetComponent<KuramotoAffectedAgent>().played;
                totalTcells += 1;
                switch(state) {
                    case 0: // ignore
                        break;
                    case 1:
                        naiveTcells += 1;
                        break;
                    case 2:
                        imprintedTcells += 1;
                        break;
                    case 3:
                        confusedTcells += 1;
                        break;
                    default:
                        throw new System.Exception("Unknown Tcell behavior state: " + state);
                }
            }
            
            totalPathogens = GameObject.FindGameObjectsWithTag("Pathogen").Length;

            infection = totalTcells + totalPathogens;
            infection = totalPathogens / infection;

            totalPlastics = GameObject.FindGameObjectsWithTag("Plastic").Length;

            totalAgents = totalApcs + totalTcells + totalPathogens + totalPlastics;

            simulationRunningSeconds = Time.time;
            simulationRunningMinutes = simulationRunningSeconds / 60.0f;
            simulationRunningHours = simulationRunningMinutes / 60.0f;

            yield return new WaitForSeconds(timetowait);
        }


    }
    

}
