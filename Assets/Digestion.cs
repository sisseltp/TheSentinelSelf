using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Digestion : MonoBehaviour
{
    [SerializeField]
    private float digestTime = 1;
    [SerializeField]
    private float digestSphere = 0.2f;
    [SerializeField]
    private float swallowTimer = 1;
    [SerializeField]
    private int swallowSteps = 50;

    private Vector2Int boneSelection = new Vector2Int(2, 4);

    [HideInInspector]
    public Vector3 origin; 
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DigestTimer(digestTime, swallowTimer));
        origin = transform.position;
    }



    IEnumerator DigestTimer(float dtime, float swallowTime)
    {
        yield return new WaitForSeconds(dtime);

        Transform rootBone = GetComponentInParent<GeneticMovementSentinel>().rootBone;

        Transform newBone = rootBone;

        int randBone = Random.Range(boneSelection.x, boneSelection.y);

        for (int i = 1; i < randBone; i++)
        {
            newBone = newBone.GetChild(0);
        }

        float t = 0;

        Vector3 origPos = transform.position - rootBone.position;

        Vector3 newPos = Random.insideUnitSphere * digestSphere;

        while (t < 1)
        {
            t += 1 / (float)swallowSteps;
            Vector3 nextPos = Vector3.Lerp(rootBone.position + origPos, newBone.position + newPos, t);
            transform.position = nextPos;
            yield return new WaitForSeconds(swallowTime/ (float)swallowSteps);

        }
        
        transform.parent = newBone;

        this.enabled = false;
    }
}
