using UnityEngine;
using Sirenix.OdinInspector;

public class Agent : MonoBehaviour
{
    public Renderer renderer;
    public Rigidbody rigidBody;
    public KuramotoAffectedAgent kuramoto;
    public GeneticMovement geneticMovement;

    [ShowIf("@this.GetType() == typeof(Sentinel)")]
    public Fosilising fosilising = null;
    [ShowIf("@this.GetType() == typeof(Sentinel)")]
    public APCSong apcSong = null;
    [ShowIf("@this.GetType() == typeof(Pathogen) || this.GetType() == typeof(TCell)")]
    public GeneticAntigenKey geneticAntigenKey = null;

    [HideInInspector]
    public AgentsManager manager;

    public void Start()
    {
        manager = GetComponentInParent<AgentsManager>();
    }
}
