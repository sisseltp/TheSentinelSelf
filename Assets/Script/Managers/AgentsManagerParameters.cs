using UnityEngine;

[CreateAssetMenu(fileName = "AgentsManagerParameters", menuName = "ScriptableObjects/AgentsManagerParameters", order = 1)]
public class AgentsManagerParameters : ScriptableObject
{
    [Tooltip("Number of agents to produce")]
    [Range(1, 3000)]
    public int amongAgentsAtStart = 10; // number of sentinels to be made
    [Tooltip("radius of area to be produced in")]
    [Range(0.1f, 200f)]
    public float spawnArea = 1.0f; // area to be spawned in
    [Tooltip("Kuramoto Speed Scaler")]
    [Range(0f, 20f)]
    public float speedScl = 3f;

    [Tooltip("Kuramoto speed, measured in bpm, x=min y=max")]
    public Vector2 speedRange = new Vector2(0, 1); // variation of speed for them to have
    [Tooltip("Kuramoto, range for the max distance for the effect, x=min y=max")]
    public Vector2 couplingRange = new Vector2(1, 10); // coupling range to have
    [Tooltip("Kuramoto, range for noise effect, x=min y=max")]
    public Vector2 noiseSclRange = new Vector2(0.01f, 0.5f); // noise Scl to have
    [Tooltip("Kuramoto, range for the strength of the coupling effect, x=min y=max")]
    public Vector2 couplingSclRange = new Vector2(0.2f, 10f); // coupling scl
    [Tooltip("Kuramoto, range for the scaling the clustering/attraction effect, x=min y=max")]
    public Vector2 attractionSclRange = new Vector2(0.2f, 1f); // coupling scl

    [Tooltip("Max age the agents will reach")]
    public float MaxAge = 1000; // age limit to kill sentinels

  
}
