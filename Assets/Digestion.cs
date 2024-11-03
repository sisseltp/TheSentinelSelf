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

    private Transform rootBone;

    void Start()
    {
        origin = transform.position;
    }

    public void SetRootBoneAndStartDigestion(Transform b)
    {
        rootBone = b;
        StartCoroutine(DigestTimer(digestTime, swallowTimer));
    }

    IEnumerator DigestTimer(float dtime, float swallowTime)
    {
        yield return new WaitForSeconds(dtime);

        Transform newBone = rootBone;

        int randBone = Random.Range(boneSelection.x, boneSelection.y);

        for (int i = 1; i < randBone; i++)
            newBone = newBone.GetChild(0);

        float t = 0;

        Vector3 origPos = transform.position - rootBone.position;

        Vector3 newPos = Random.insideUnitSphere * digestSphere;

        while (t < 1f)
        {
            t += 1f / (float)swallowSteps;
            Vector3 nextPos = Vector3.Lerp(rootBone.position + origPos, newBone.position + newPos, t);
            transform.position = nextPos;
            yield return new WaitForSeconds(swallowTime/ (float)swallowSteps);
        }
        
        transform.parent = newBone;

        this.enabled = false;
    }
}
