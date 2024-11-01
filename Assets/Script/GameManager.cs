using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<SentinelsManager> sentinelsManagers = new List<SentinelsManager>();
    public List<PathogensManager> pathogensManagers = new List<PathogensManager>();
    public List<PlasticsManager> plasticsManagers = new List<PlasticsManager>();
    public List<TCellsManager> tCellsManagers = new List<TCellsManager>();

    public void Awake()
    {
        Instance = this;
    }
}
