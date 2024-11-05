using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

public class GPUCompute : MonoBehaviour
{
    [SerializeField]
    private ComputeShader shader;

    private int TexResolution;

    private RenderTexture rt;

    private GPUData[] sentinelData;
    private GPUData[] pathogenAndTCellData;
    private GPUData[] plasticData;

    private GPUOutput[] sentinelDataOut;
    private GPUOutput[] pathogenAndTCellDataOut;
    private GPUOutput[] plasticDataOut;

    private bool plasticDataOutComputed = false;
    private bool sentinelDataOutComputed = false;
    private bool pathogenAndTCellDataOutComputed = false;

    private ComputeBuffer sentinelBuffer;
    private ComputeBuffer BiomeBuffer;
    private ComputeBuffer plasticBuffer;

    private ComputeBuffer sentinelBufferOut;
    private ComputeBuffer BiomeBufferOut;
    private ComputeBuffer plasticBufferOut;

    public struct GPUData
    {
        public float connections;
        public int played;
        public float speed;
        public float phase;
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
            phase = kuramoto.phase;
            couplingRange = kuramoto.couplingRange;
            noiseScl = kuramoto.noiseScl;
            coupling = kuramoto.coupling;
            attractionScl = kuramoto.attractionSclr;
            pos = kuramoto.transform.position;
            played = 1;
        }
    }

    public struct GPUOutput
    {
        public Vector3 vel;
        public float phaseAdition;

        public void Setup()
        {
            vel = Vector3.zero;
            phaseAdition = 1f;
        }
    }

    private void Start()
    {
        StartCoroutine(UpdateTextureFromComputeASync());
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

            sentinelBufferOut = new ComputeBuffer(sentinelData.Length, Marshal.SizeOf(typeof(GPUOutput)));
            sentinelBufferOut.SetData(new GPUOutput[sentinelData.Length]);

            BiomeBufferOut = new ComputeBuffer(pathogenAndTCellData.Length, Marshal.SizeOf(typeof(GPUOutput)));
            BiomeBufferOut.SetData(new GPUOutput[pathogenAndTCellData.Length]);

            plasticBufferOut = new ComputeBuffer(plasticData.Length, Marshal.SizeOf(typeof(GPUOutput)));
            plasticBufferOut.SetData(new GPUOutput[plasticData.Length]);

            int UpdateBiome = shader.FindKernel("BiomeUpdate");

            shader.SetTexture(UpdateBiome, "Result", rt);
            shader.SetBuffer(UpdateBiome, "sentinelData", sentinelBuffer);
            shader.SetBuffer(UpdateBiome, "biomeData", BiomeBuffer);
            shader.SetBuffer(UpdateBiome, "plasticData", plasticBuffer);

            shader.SetBuffer(UpdateBiome, "sentinelDataOut", sentinelBufferOut);
            shader.SetBuffer(UpdateBiome, "biomeDataOut", BiomeBufferOut);
            shader.SetBuffer(UpdateBiome, "plasticDataOut", plasticBufferOut);

            shader.Dispatch(UpdateBiome, TexResolution, 1, 1);

            AsyncGPUReadback.Request(plasticBufferOut, POnCompleteReadBack);
            AsyncGPUReadback.Request(sentinelBufferOut, SOnCompleteReadBack);
            AsyncGPUReadback.Request(BiomeBufferOut, BOnCompleteReadBack);

            yield return new WaitUntil(() => pathogenAndTCellDataOutComputed && sentinelDataOutComputed && plasticDataOutComputed);

            pathogenAndTCellDataOutComputed = false;
            sentinelDataOutComputed = false;
            plasticDataOutComputed = false;

            AsyncSetData();

            BiomeBuffer.Release(); ;
            sentinelBuffer.Release();
            plasticBuffer.Release();
            BiomeBufferOut.Release();
            sentinelBufferOut.Release();
            plasticBufferOut.Release();
        }
    }

    private void LinkData()
    {
        List<GPUData> sentData = new List<GPUData>();
        List<GPUOutput> sentDataOut = new List<GPUOutput>();
        for (int i = 0; i < GameManager.Instance.sentinelsManagers.Count; i++)
        {
            sentData.AddRange(GameManager.Instance.sentinelsManagers[i].GPUStruct);
            sentDataOut.AddRange(GameManager.Instance.sentinelsManagers[i].GPUOutput);
        }
                
        sentinelData = sentData.ToArray();
        sentinelDataOut = sentDataOut.ToArray();

        List<GPUData> bioData = new List<GPUData>();
        List<GPUOutput> bioDataOut = new List<GPUOutput>();
        for (int i = 0; i < GameManager.Instance.pathogensManagers.Count; i++)
        {
            bioData.AddRange(GameManager.Instance.pathogensManagers[i].GPUStruct);
            bioDataOut.AddRange(GameManager.Instance.pathogensManagers[i].GPUOutput);
        }

        for (int i = 0; i < GameManager.Instance.tCellsManagers.Count; i++)
        {
            bioData.AddRange(GameManager.Instance.tCellsManagers[i].GPUStruct);
            bioDataOut.AddRange(GameManager.Instance.tCellsManagers[i].GPUOutput);
        }
         
        pathogenAndTCellData = bioData.ToArray();
        pathogenAndTCellDataOut = bioDataOut.ToArray();

        List<GPUData> plasData = new List<GPUData>();
        List<GPUOutput> plasDataOut = new List<GPUOutput>();
        for (int i = 0; i < GameManager.Instance.plasticsManagers.Count; i++)     
        {
            plasData.AddRange(GameManager.Instance.plasticsManagers[i].GPUStruct);
            plasDataOut.AddRange(GameManager.Instance.plasticsManagers[i].GPUOutput);
        }
                

        plasticData = plasData.ToArray();
        plasticDataOut = plasDataOut.ToArray();

        TexResolution = pathogenAndTCellData.Length + sentinelData.Length + plasticData.Length;
    }

    void POnCompleteReadBack(AsyncGPUReadbackRequest request)
    {
        if (!request.hasError)
        {
            plasticDataOut = request.GetData<GPUOutput>().ToArray();
            plasticDataOutComputed = true;
        }
        
    }

    void SOnCompleteReadBack(AsyncGPUReadbackRequest request)
    {
        if (!request.hasError)
        {
            sentinelDataOut = request.GetData<GPUOutput>().ToArray();
            sentinelDataOutComputed = true;
        }
    }

    void BOnCompleteReadBack(AsyncGPUReadbackRequest request)
    {
        if (!request.hasError)
        {
            pathogenAndTCellDataOut = request.GetData<GPUOutput>().ToArray();
            pathogenAndTCellDataOutComputed = true;
        }  
    }

    private void AsyncSetData()
    {
        int pathoAndTCellOffset = 0;
        foreach (PathogensManager manager in GameManager.Instance.pathogensManagers)
        {
            manager.GPUOutput = Extensions.SubArray(pathogenAndTCellDataOut, pathoAndTCellOffset, manager.GPUOutput.Length);
            pathoAndTCellOffset += manager.GPUOutput.Length;
        }

        foreach (TCellsManager manager in GameManager.Instance.tCellsManagers)
        {
            manager.GPUOutput = Extensions.SubArray(pathogenAndTCellDataOut, pathoAndTCellOffset, manager.GPUOutput.Length);
            pathoAndTCellOffset += manager.GPUOutput.Length;
        }

        int sentOffset = 0;
        foreach (SentinelsManager manager in GameManager.Instance.sentinelsManagers)
        {
            manager.GPUOutput = Extensions.SubArray(sentinelDataOut, sentOffset, manager.GPUOutput.Length);
            sentOffset += manager.GPUOutput.Length;
        }

        int plasticOffset = 0;
        foreach (PlasticsManager manager in GameManager.Instance.plasticsManagers)
        {
            manager.GPUOutput = Extensions.SubArray(plasticDataOut, plasticOffset, manager.GPUOutput.Length);
            plasticOffset += manager.GPUOutput.Length;
        }
    }

    private void OnDisable()
    {
        BiomeBuffer?.Release();
        sentinelBuffer?.Release();
        plasticBuffer?.Release();
        BiomeBufferOut?.Release();
        sentinelBufferOut?.Release();
        plasticBufferOut?.Release();
    }

    private void OnDestroy()
    {
        BiomeBuffer?.Release();
        sentinelBuffer?.Release();
        plasticBuffer?.Release();
        BiomeBufferOut?.Release();
        sentinelBufferOut?.Release();
        plasticBufferOut?.Release();
    }
}

public static class Extensions
{
    public static T[] SubArray<T>(this T[] array, int offset, int length)
    {
        return array.Skip(offset).Take(length).ToArray();
    } 
}

