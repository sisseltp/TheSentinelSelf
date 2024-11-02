using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class GPUCompute : MonoBehaviour
{
    [SerializeField]
    private ComputeShader shader;

    private int TexResolution;

    private RenderTexture rt;

    private GPUData[] sentinelData;
    private GPUData[] pathogenAndTCellData;
    private GPUData[] plasticData;

    private bool Pcomputed = false;
    private bool Scomputed = false;
    private bool Bcomputed = false;
    
    private ComputeBuffer sentinelBuffer;
    private ComputeBuffer BiomeBuffer;
    private ComputeBuffer plasticBuffer;
    
    public struct GPUData
    {
        public float connections;
        public int played;
        public float speed;
        public float cohPhi;
        public float coherenceRadius;
        public float couplingRange;
        public float noiseScl;
        public float coupling;
        public float attractionScl;
        public Vector3 pos;

        public void SetFromKuramoto(KuramotoAffectedAgent kuramoto)
        {
            speed = kuramoto.speed;
            couplingRange = kuramoto.couplingRange;
            noiseScl = kuramoto.noiseScl;
            coupling = kuramoto.coupling;
            attractionScl = kuramoto.attractionSclr;
            played = 1;
        }
    }

    private void Start()
    {
        StartCoroutine(UpdateTextureFromComputeASync());
    }

    private void LinkData()
    {
        List<GPUData> sentData = new List<GPUData>();
        for (int i = 0; i < GameManager.Instance.sentinelsManagers.Count; i++)
            if (GameManager.Instance.sentinelsManagers[i].GPUStruct != null)
                sentData.AddRange(GameManager.Instance.sentinelsManagers[i].GPUStruct);

        sentinelData = sentData.ToArray();

        List<GPUData> bioData = new List<GPUData>();
        for (int i = 0; i < GameManager.Instance.pathogensManagers.Count; i++)
            if (GameManager.Instance.pathogensManagers[i].GPUStruct != null)
                bioData.AddRange(GameManager.Instance.pathogensManagers[i].GPUStruct);

        for (int i = 0; i < GameManager.Instance.tCellsManagers.Count; i++)
            if (GameManager.Instance.tCellsManagers[i].GPUStruct != null)
                bioData.AddRange(GameManager.Instance.tCellsManagers[i].GPUStruct);

        pathogenAndTCellData = bioData.ToArray();

        List<GPUData> plasData = new List<GPUData>();
        for (int i = 0; i < GameManager.Instance.plasticsManagers.Count; i++)
            if (GameManager.Instance.plasticsManagers[i].GPUStruct != null)
                plasData.AddRange(GameManager.Instance.plasticsManagers[i].GPUStruct);

        plasticData = plasData.ToArray();

        TexResolution = pathogenAndTCellData.Length + sentinelData.Length + plasticData.Length;
    }

    IEnumerator UpdateTextureFromComputeASync()
    {
        yield return new WaitForSeconds(1);
        while (true)
        {
            LinkData();

            rt = new RenderTexture(TexResolution, 1, 0);
            rt.enableRandomWrite = true;
            RenderTexture.active = rt;

            sentinelBuffer = new ComputeBuffer(sentinelData.Length, Marshal.SizeOf(typeof(GPUData)));
            sentinelBuffer.SetData(sentinelData);

            BiomeBuffer = new ComputeBuffer(pathogenAndTCellData.Length, Marshal.SizeOf(typeof(GPUData)));
            BiomeBuffer.SetData(pathogenAndTCellData);

            plasticBuffer = new ComputeBuffer(plasticData.Length, Marshal.SizeOf(typeof(GPUData)));
            plasticBuffer.SetData(plasticData);

            int UpdateBiome = shader.FindKernel("BiomeUpdate");
            //int UpdateSentinel = shader.FindKernel("SentinelUpdate");

            shader.SetTexture(UpdateBiome, "Result", rt);
            shader.SetBuffer(UpdateBiome, "sentinelData", sentinelBuffer);
            shader.SetBuffer(UpdateBiome, "biomeData", BiomeBuffer);
            shader.SetBuffer(UpdateBiome, "plasticData", plasticBuffer);

            shader.Dispatch(UpdateBiome, TexResolution, 1, 1);

            yield return new WaitUntil(() => Bcomputed && Scomputed && Pcomputed);

            Bcomputed = false;
            Scomputed = false;
            Pcomputed = false;

            BiomeBuffer.Release(); ;
            sentinelBuffer.Release();
            plasticBuffer.Release();
        }
    }

    private void OnDisable()
    {
        BiomeBuffer?.Release();
        sentinelBuffer?.Release();
        plasticBuffer?.Release();
    }

    private void OnDestroy()
    {
        BiomeBuffer?.Release();
        sentinelBuffer?.Release();
        plasticBuffer?.Release();
    }
}