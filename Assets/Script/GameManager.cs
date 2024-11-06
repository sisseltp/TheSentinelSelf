using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

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

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape pressed, quitting...");
            Application.Quit();
        }
    }

    public List<SentinelsManager> GetSentinelsManagersSortedByDistance(Vector3 position)
    {
        List<SentinelsManager> sortedList = sentinelsManagers
           .OrderBy(obj => Vector3.SqrMagnitude(obj.transform.position- position))
           .ToList();

        return sortedList;
    }

    public List<PathogensManager> GetPathogensManagersSortedByDistance(Vector3 position)
    {
        List<PathogensManager> sortedList = pathogensManagers
           .OrderBy(obj => Vector3.SqrMagnitude(obj.transform.position - position))
           .ToList();

        return sortedList;
    }

    public List<PlasticsManager> GetPlasticsManagersSortedByDistance(Vector3 position)
    {
        List<PlasticsManager> sortedList = plasticsManagers
           .OrderBy(obj => Vector3.SqrMagnitude(obj.transform.position - position))
           .ToList();

        return sortedList;
    }

    public List<TCellsManager> GetTCellsManagersSortedByDistance(Vector3 position)
    {
        List<TCellsManager> sortedList = tCellsManagers
           .OrderBy(obj => Vector3.SqrMagnitude(obj.transform.position - position))
           .ToList();

        return sortedList;
    }

    public static List<T> GetFirstHalfElements<T>(List<T> list)
    {
        int elementsToTake = Mathf.CeilToInt(list.Count / 2);
        return list.Take(elementsToTake).ToList();
    }

    public List<SentinelsManager> GetFirstHalfClosestSentinelsManagers(Vector3 position) => GetFirstHalfElements(GetSentinelsManagersSortedByDistance(position));
    public List<PathogensManager> GetFirstHalfClosestPathogensManagers(Vector3 position) => GetFirstHalfElements(GetPathogensManagersSortedByDistance(position));
    public List<PlasticsManager> GetFirstHalfClosestPlasticsManagers(Vector3 position) => GetFirstHalfElements(GetPlasticsManagersSortedByDistance(position));
    public List<TCellsManager> GetFirstHalfClosestTCellsManagers(Vector3 position) => GetFirstHalfElements(GetTCellsManagersSortedByDistance(position));

    public SentinelsManager GetRandomSentinelsManagerAmongClosestHalf(Vector3 position) => GetRandomElementFromList(GetFirstHalfClosestSentinelsManagers(position));
    public PathogensManager GetRandomPathogensManagerAmongClosestHalf(Vector3 position) => GetRandomElementFromList(GetFirstHalfClosestPathogensManagers(position));
    public PlasticsManager GetRandomPlasticsManagerAmongClosestHalf(Vector3 position) => GetRandomElementFromList(GetFirstHalfClosestPlasticsManagers(position));
    public TCellsManager GetRandomTCellsManagerAmongClosestHalf(Vector3 position) => GetRandomElementFromList(GetFirstHalfClosestTCellsManagers(position));

    public static T GetRandomElementFromList<T>(List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }
}
