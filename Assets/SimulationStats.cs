using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
    RestartScene - hard reboot of the current scene.

    CleanupSimulation - not yet implemented
        the idea is to provide a softer option over a hard reboot
        clean up any possible memory leaks and reduces the current complexity
        of the simulation
*/
public enum SafetyBehavior {RestartScene, CleanupSimulation};

public class SimulationStats : MonoBehaviour
{

    [Tooltip("How often to update the status info (in seconds)")]
    public float updateStatsEvery = 5.0f;

    [Space(10)]
    public float currentFps = 1000.0f;
 
    [Tooltip("Ratio Pathogens to Tcells -- pathogens / (Tcells + pathogens)")]    
    public float infection = 0.0f;

    [Tooltip("Total agents")]    
    public int totalAgents = 0;

    public float runningHours = 0.0f;
    public float runningMinutes = 0.0f;
    public float runningSeconds = 0.0f;


    [Space(10)]
    [Header("Agents")]

    [Tooltip("APCs seeking pathogens")]    
    public int apcSeeking = 0;

    [Tooltip("APCs carrying antigens to a lymph node")]    
    public int apcCarrying = 0;

    [Tooltip("APCs that are apcFossils")]    
    public int apcFossils = 0;

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

    [Space(5)]

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


    [Space(5)]
    [Header("Reboot Timer")]
    public bool rebootTimerEnabled = false;

    [Tooltip("How often to reboot the system (in minutes)")]
    [Range(1, 240)]
    public float rebootEvery = 1;

    [Space(5)]
    [Header("Watchdog")]

    public SafetyBehavior behavior = SafetyBehavior.RestartScene;

    [Tooltip("FPS threshold below which watchdog kicks in")]
    [Range(5.0f, 40.0f)]
    public float fpsThreshold = 15.0f;

    [Tooltip("Minutes below fps threshold when watchdog activates")]
    [Range(0.1f, 30.0f)]
    public float minutesUnderThreshold = 10.0f;




    [Space(10)]
    [Header("Debug")]
    private float lastFps = 1000.0f;
    
    private float timeThreshold; // in seconds, calculated on start

    [SerializeField]
    private bool watchdogTimerRunning = false;

    [SerializeField]
    private float watchdogTimer = 0.0f;

    private float calculateEvery = 1.0f; // in seconds, used to get an avg fps
    private int framesCount = 0;
    private float framesTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        // Init FPS & Watchdog parameers
        currentFps = 1000f;
        lastFps = 1000f;
        timeThreshold = minutesUnderThreshold * 60.0f;

        // Run stats calculation as a coroutine that updates every so many seconds..
        StartCoroutine(checkWorld(updateStatsEvery));
    }

    // Update is called once per frame
    void Update()
    {
        bool triggerWatchdog = false;
        // Calculate FPS & check watchdog conditions...
        if(watchdogTimerRunning) {
            watchdogTimer += Time.unscaledDeltaTime;
        }
        runningSeconds += Time.unscaledDeltaTime;

        framesTime += Time.unscaledDeltaTime;
        framesCount++;

        // Check watchdog conditions...
        // FPS:
        if(watchdogTimerRunning && ( watchdogTimer >= timeThreshold )) {
            watchdogTimerRunning = false;
            watchdogTimer = 0.0f;
            lastFps = 1000.0f;
            triggerWatchdog = true;
        }

        // Reboot Timer:
        if(rebootTimerEnabled && (runningSeconds >= (rebootEvery * 60))) {
            triggerWatchdog = true;
        }

        // Run Watchdog behavior if triggered.
        if(triggerWatchdog) {
            switch(behavior) {
                case SafetyBehavior.RestartScene:
                    Restart();
                    break;
                case SafetyBehavior.CleanupSimulation:
                    Cleanup();
                    break;
                default:
                    Debug.Assert(false, "Unknown Watchdog behavior: " + behavior);
                    break;
            }
        }



        if(framesTime >= calculateEvery) {

            // Calculate avg fps
            lastFps = currentFps;
            currentFps = framesCount / framesTime;
            framesCount = 0;
            framesTime -= calculateEvery;

            if(currentFps <= fpsThreshold) {
                if(lastFps > fpsThreshold) {
                    // Start timer...
                    watchdogTimerRunning = true;
                    watchdogTimer = 0.0f;
                }
            } else {
                if(lastFps <= fpsThreshold) {
                    watchdogTimerRunning = false;
                    watchdogTimer = 0.0f;
                }
            }
        }     
    }

    public void Restart() {
        Debug.Log("REBOOT THE SCENE!");
            
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Cleanup() {
        Debug.Log("CLEAN UP THE SCENE!");
        // TODO: Not implemented
        //       clean up scene resources
        //       & prune unnecessary game objects
    }

    IEnumerator checkWorld(float timetowait)
    {
        while (true)
        {
            totalApcs = 0;
            apcSeeking = 0;
            apcCarrying = 0;
            totalDigestedAntigens = 0;
            totalDigestedPlastics = 0;
            foreach(GameObject apc in GameObject.FindGameObjectsWithTag("Player")) {
                GeneticMovementSentinel movementScript = apc.GetComponent<GeneticMovementSentinel>();
                switch(movementScript.currentBehavior) {
                    case APCBehavior.SeekingPathogens:
                        apcSeeking += 1;
                        break;
                    case APCBehavior.CarryingAntigens:
                        apcCarrying += 1;
                        break;
                    default:
                        throw new System.Exception("Unknown APCBehavior: " + movementScript.currentBehavior);
                }
                totalApcs += 1;
                totalDigestedAntigens += movementScript.digestAntigens.Count;
                totalDigestedPlastics += movementScript.plastics.Count;
            }
            
            apcFossils = GameObject.FindGameObjectsWithTag("Eggs").Length;

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

            runningMinutes = runningSeconds / 60.0f;
            runningHours = runningMinutes / 60.0f;

            yield return new WaitForSeconds(timetowait);
        }


    }


}
