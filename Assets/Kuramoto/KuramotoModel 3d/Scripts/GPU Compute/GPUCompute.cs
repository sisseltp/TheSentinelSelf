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

    private bool Pcomputed = false;
    private bool Scomputed = false;
    private bool Bcomputed = false;

    private float cDT = 1;
    private float timer = 0;

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
        cDT = Time.deltaTime;
        StartCoroutine(UpdateTextureFromComputeASync());
    }

    private void LinkData()
    {
        List<GPUData> sentData = new List<GPUData>();
        List<GPUOutput> sentDataOut = new List<GPUOutput>();
        for (int i = 0; i < GameManager.Instance.sentinelsManagers.Count; i++)
            if (GameManager.Instance.sentinelsManagers[i].GPUStruct != null)
            {
                sentData.AddRange(GameManager.Instance.sentinelsManagers[i].GPUStruct);
                sentDataOut.AddRange(GameManager.Instance.sentinelsManagers[i].GPUOutput);
            }
                

        sentinelData = sentData.ToArray();
        sentinelDataOut = sentDataOut.ToArray();

        List<GPUData> bioData = new List<GPUData>();
        List<GPUOutput> bioDataOut = new List<GPUOutput>();
        for (int i = 0; i < GameManager.Instance.pathogensManagers.Count; i++)
            if (GameManager.Instance.pathogensManagers[i].GPUStruct != null)
            {
                bioData.AddRange(GameManager.Instance.pathogensManagers[i].GPUStruct);
                bioDataOut.AddRange(GameManager.Instance.pathogensManagers[i].GPUOutput);
            }
                
        for (int i = 0; i < GameManager.Instance.tCellsManagers.Count; i++)
            if (GameManager.Instance.tCellsManagers[i].GPUStruct != null)
            {
                bioData.AddRange(GameManager.Instance.tCellsManagers[i].GPUStruct);
                bioDataOut.AddRange(GameManager.Instance.tCellsManagers[i].GPUOutput);
            }
                

        pathogenAndTCellData = bioData.ToArray();
        pathogenAndTCellDataOut = bioDataOut.ToArray();

        List<GPUData> plasData = new List<GPUData>();
        List<GPUOutput> plasDataOut = new List<GPUOutput>();
        for (int i = 0; i < GameManager.Instance.plasticsManagers.Count; i++)
            if (GameManager.Instance.plasticsManagers[i].GPUStruct != null)
            {
                plasData.AddRange(GameManager.Instance.plasticsManagers[i].GPUStruct);
                plasDataOut.AddRange(GameManager.Instance.plasticsManagers[i].GPUOutput);
            }
                

        plasticData = plasData.ToArray();
        plasticDataOut = plasDataOut.ToArray();

        TexResolution = 
            pathogenAndTCellData.Length + 
            sentinelData.Length + 
            plasticData.Length;
    }

    private void SetData()
    {
        int bioOffset = 0;

        for (int i = 0; i < GameManager.Instance.pathogensManagers.Count; i++)
        {
            if (GameManager.Instance.pathogensManagers[i].GPUStruct != null)
            {
                GameManager.Instance.pathogensManagers[i].GPUStruct = Extensions.SubArray(pathogenAndTCellData, bioOffset, GameManager.Instance.pathogensManagers[i].GPUStruct.Length);
                bioOffset += GameManager.Instance.pathogensManagers[i].GPUStruct.Length;
            }
        }

        for (int i = 0; i < GameManager.Instance.tCellsManagers.Count; i++)
        {
            if (GameManager.Instance.tCellsManagers[i].GPUStruct != null)
            {
                GameManager.Instance.tCellsManagers[i].GPUStruct = Extensions.SubArray(pathogenAndTCellData, bioOffset, GameManager.Instance.tCellsManagers[i].GPUStruct.Length);
                bioOffset += GameManager.Instance.tCellsManagers[i].GPUStruct.Length;
            }
        }

        int sentOffset = 0;

        for (int i = 0; i < GameManager.Instance.sentinelsManagers.Count; i++)
        {
            if (GameManager.Instance.sentinelsManagers[i].GPUStruct != null)
            {
                GameManager.Instance.sentinelsManagers[i].GPUStruct = Extensions.SubArray(sentinelData, sentOffset, GameManager.Instance.sentinelsManagers[i].GPUStruct.Length);
                sentOffset += GameManager.Instance.sentinelsManagers[i].GPUStruct.Length;
            }
        }

        int plasticOffset = 0;

        for (int i = 0; i < GameManager.Instance.plasticsManagers.Count; i++)
        {
            if (GameManager.Instance.plasticsManagers[i].GPUStruct != null)
            {

                GameManager.Instance.plasticsManagers[i].GPUStruct = Extensions.SubArray(plasticData, plasticOffset, GameManager.Instance.plasticsManagers[i].GPUStruct.Length);
                plasticOffset += GameManager.Instance.plasticsManagers[i].GPUStruct.Length;
            }
        }
    }

    private void AsyncSetData()
    {
        int bioOffset = 0;

        for (int i = 0; i < GameManager.Instance.pathogensManagers.Count; i++)
        {
            if (GameManager.Instance.pathogensManagers[i].GPUStruct != null)
            {
                GameManager.Instance.pathogensManagers[i].GPUOutput = Extensions.SubArray(pathogenAndTCellDataOut, bioOffset, GameManager.Instance.pathogensManagers[i].GPUStruct.Length);
                bioOffset += GameManager.Instance.pathogensManagers[i].GPUStruct.Length;
            }
        }

        for (int i = 0; i < GameManager.Instance.tCellsManagers.Count; i++)
        {
            if (GameManager.Instance.tCellsManagers[i].GPUStruct != null)
            {
                GameManager.Instance.tCellsManagers[i].GPUOutput = Extensions.SubArray(pathogenAndTCellDataOut, bioOffset, GameManager.Instance.tCellsManagers[i].GPUStruct.Length);

                bioOffset += GameManager.Instance.tCellsManagers[i].GPUStruct.Length;
            }
        }

        int sentOffset = 0;

        for (int i = 0; i < GameManager.Instance.sentinelsManagers.Count; i++)
        {
            if (GameManager.Instance.sentinelsManagers[i].GPUStruct != null)
            {
                GameManager.Instance.sentinelsManagers[i].GPUOutput = Extensions.SubArray(sentinelDataOut, sentOffset, GameManager.Instance.sentinelsManagers[i].GPUStruct.Length);
                sentOffset += GameManager.Instance.sentinelsManagers[i].GPUStruct.Length;
            }
        }

        int plasticOffset = 0;

        for (int i = 0; i < GameManager.Instance.plasticsManagers.Count; i++)
        {
            if (GameManager.Instance.plasticsManagers[i].GPUStruct != null)
            {
                GameManager.Instance.plasticsManagers[i].GPUOutput = Extensions.SubArray(plasticDataOut, plasticOffset, GameManager.Instance.plasticsManagers[i].GPUStruct.Length);
                plasticOffset += GameManager.Instance.plasticsManagers[i].GPUStruct.Length;
            }
        }
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
            //int UpdateSentinel = shader.FindKernel("SentinelUpdate");

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

            yield return new WaitUntil(() => Bcomputed && Scomputed && Pcomputed);

            Bcomputed = false;
            Scomputed = false;
            Pcomputed = false;

            //Debug.Log(plasticDataOut[0].phaseAdition);

            AsyncSetData();

            BiomeBuffer.Release(); ;
            sentinelBuffer.Release();
            plasticBuffer.Release();
            BiomeBufferOut.Release();
            sentinelBufferOut.Release();
            plasticBufferOut.Release();
        }
    }

    void POnCompleteReadBack(AsyncGPUReadbackRequest request)
    {
        if (request.hasError == false)
        {
            plasticDataOut = request.GetData<GPUOutput>().ToArray();
            Pcomputed = true;
        }
    }

    void SOnCompleteReadBack(AsyncGPUReadbackRequest request)
    {
        if (request.hasError == false)
        {
            sentinelDataOut = request.GetData<GPUOutput>().ToArray();
            Scomputed = true;
        }
    }
    void BOnCompleteReadBack(AsyncGPUReadbackRequest request)
    {
        if (request.hasError == false)
        {
            pathogenAndTCellDataOut = request.GetData<GPUOutput>().ToArray();
            Bcomputed = true;
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
        return array.Skip(offset)
                    .Take(length)
                    .ToArray();
    }
}

