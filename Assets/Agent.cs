using UnityEngine;
using Sirenix.OdinInspector;

public class Agent : MonoBehaviour
{
    public new Renderer renderer;
    public Rigidbody rigidBody;
    public KuramotoAffectedAgent kuramoto;
    public GeneticMovement geneticMovement;

    [ShowIf("@this.GetType() == typeof(TCell)")]
    public SkinnedMeshRenderer skinnedMeshRenderer;

    [ShowIf("@this.GetType() == typeof(Sentinel)")]
    public Fosilising fosilising;
    [ShowIf("@this.GetType() == typeof(Sentinel)")]
    public APCSong apcSong;
    [ShowIf("@this.GetType() == typeof(Pathogen) || this.GetType() == typeof(TCell)")]
    public GeneticAntigenKey geneticAntigenKey;
    


   [HideInInspector]
    public AgentsManager manager;

    public void Start()
    {
        manager = GetComponentInParent<AgentsManager>();
    }
}
