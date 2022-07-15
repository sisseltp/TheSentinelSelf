using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyMover : MonoBehaviour
{
    [SerializeField]
    private Vector3 move = new Vector3(0, 1, 0);

    [SerializeField]
    private float nScl = 0.5f;

    [SerializeField]
    private float ninfluence = 0.5f;

    private int offset = 0;

    private void Start()
    {
        offset = Random.Range(0, 100000);
    }
    // Update is called once per frame
    void Update()
    {
        transform.position += move*Time.deltaTime;

        transform.position += transform.forward * (Mathf.PerlinNoise(offset+(transform.position.x+Time.time) * nScl, offset+(transform.position.y+Time.time) * nScl)-0.5f)*Time.deltaTime* ninfluence;


    }
}
