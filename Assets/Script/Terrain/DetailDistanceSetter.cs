using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class DetailDistanceSetter : MonoBehaviour
{
    [SerializeField]
    [Range(250, 2000)]
    private int detailDistance = 250;
    
    private Terrain terrain;
    
    private void Awake()
    {
        terrain = GetComponent<Terrain>();
        terrain.detailObjectDistance = detailDistance;
    }
}
