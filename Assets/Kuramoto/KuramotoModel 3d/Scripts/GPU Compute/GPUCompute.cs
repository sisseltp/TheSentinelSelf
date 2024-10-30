using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine.Rendering;

public class GPUCompute : MonoBehaviour
{
    [SerializeField]
    private ComputeShader shader;


    private int TexResolution;

    private RenderTexture rt;

    private GPUData[] sentinelData;

    private GPUData[] pathogenData;

    private GPUData[] plasticData;

    private GPUOutput[] sentinelDataOut;

    private GPUOutput[] pathogenDataOut;

    private GPUOutput[] plasticDataOut;

    private SentinelsManager[] sentinelsManagers;
    private PathogensManager[] pathogensManagers;
    private PlasticsManager[] plasticsManagers;
    private TCellsManager[] tcellsManagers;

    private float cDT = 1;
    private float timer = 0;

    private bool Pcomputed = false;
    private bool Scomputed = false;
    private bool Bcomputed = false;
    
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

        public void SetFromKuramoto(KuramotoPlasticAgent kuramoto)
        {

            speed = kuramoto.speed;
            phase = kuramoto.phase;
            couplingRange = kuramoto.couplingRange;
            noiseScl = kuramoto.noiseScl;
            coupling = kuramoto.coupling;
            attractionScl = kuramoto.attractionSclr;
            played = 1;
        }


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
            phaseAdition = 1;
        }
    }

    private void Start()
    {
        sentinelsManagers = GetComponentsInChildren<SentinelsManager>();
        pathogensManagers = GetComponentsInChildren<PathogensManager>();
        plasticsManagers = GetComponentsInChildren<PlasticsManager>();
        tcellsManagers = GetComponentsInChildren<TCellsManager>();

        cDT = Time.deltaTime;

        StartCoroutine(UpdateTextureFromComputeASync());
    }

    private void SetData()
    {
        int bioOffset = 0;

        for (int i = 0; i < pathogensManagers.Length; i++)
        {
            if (pathogensManagers[i].GPUStruct != null)
            {
                pathogensManagers[i].GPUStruct = Extensions.SubArray(pathogenData, bioOffset, pathogensManagers[i].GPUStruct.Length);
                bioOffset += pathogensManagers[i].GPUStruct.Length;
            }
        }

        for (int i = 0; i < tcellsManagers.Length; i++)
        {
            if (tcellsManagers[i].GPUStruct != null)
            {
                tcellsManagers[i].GPUStruct = Extensions.SubArray(pathogenData, bioOffset, tcellsManagers[i].GPUStruct.Length);
                bioOffset += tcellsManagers[i].GPUStruct.Length;
            }
        }

        int sentOffset = 0;

        for (int i = 0; i < sentinelsManagers.Length; i++)
        {
            if (sentinelsManagers[i].GPUStruct != null)
            {
                sentinelsManagers[i].GPUStruct = Extensions.SubArray(sentinelData, sentOffset, sentinelsManagers[i].GPUStruct.Length);
                sentOffset += sentinelsManagers[i].GPUStruct.Length;
            }
        }

        int plasticOffset = 0;

        for (int i = 0; i < plasticsManagers.Length; i++)
        {
            if (plasticsManagers[i].GPUStruct != null)
            {

                plasticsManagers[i].GPUStruct = Extensions.SubArray(plasticData, plasticOffset, plasticsManagers[i].GPUStruct.Length);
                plasticOffset += plasticsManagers[i].GPUStruct.Length;
            }
        }
    }

    private void AsncySetData()
    {
        int bioOffset = 0;

        for (int i = 0; i < pathogensManagers.Length; i++)
        {
            if (pathogensManagers[i].GPUStruct != null)
            {
                pathogensManagers[i].GPUOutput = Extensions.SubArray(pathogenDataOut, bioOffset, pathogensManagers[i].GPUStruct.Length);
                bioOffset += pathogensManagers[i].GPUStruct.Length;
            }
        }

        for (int i = 0; i < tcellsManagers.Length; i++)
        {
            if (tcellsManagers[i].GPUStruct != null)
            {
                tcellsManagers[i].GPUOutput = Extensions.SubArray(pathogenDataOut, bioOffset, tcellsManagers[i].GPUStruct.Length);

                bioOffset += tcellsManagers[i].GPUStruct.Length;
            }
        }

        int sentOffset = 0;

        for (int i = 0; i < sentinelsManagers.Length; i++)
        {
            if (sentinelsManagers[i].GPUStruct != null)
            {
                sentinelsManagers[i].GPUOutput = Extensions.SubArray(sentinelDataOut, sentOffset, sentinelsManagers[i].GPUStruct.Length);
                sentOffset += sentinelsManagers[i].GPUStruct.Length;
            }
        }

        int plasticOffset = 0;

        for (int i = 0; i < plasticsManagers.Length; i++)
        {
            if (plasticsManagers[i].GPUStruct != null)
            {
                plasticsManagers[i].GPUOutput = Extensions.SubArray(plasticDataOut, plasticOffset, plasticsManagers[i].GPUStruct.Length);
                plasticOffset += plasticsManagers[i].GPUStruct.Length;
            }
        }
    }

    private void LinkData()
    {
        List<GPUData> sentData = new List<GPUData>();
        List<GPUOutput> sentDataOut = new List<GPUOutput>();

        for (int i = 0; i < sentinelsManagers.Length; i++)
        {
            if (sentinelsManagers[i].GPUStruct != null)
            {
                sentData.AddRange(sentinelsManagers[i].GPUStruct);
                sentDataOut.AddRange(sentinelsManagers[i].GPUOutput);
            }
        }

        sentinelData = sentData.ToArray();
        sentinelDataOut = sentDataOut.ToArray();

        List<GPUData> bioData = new List<GPUData>();
        List<GPUOutput> bioDataOut = new List<GPUOutput>();

        for (int i = 0; i < pathogensManagers.Length; i++)
        {
            if (pathogensManagers[i].GPUStruct != null)
            {
                bioData.AddRange(pathogensManagers[i].GPUStruct);
                bioDataOut.AddRange(pathogensManagers[i].GPUOutput);
            }
        }

        for (int i = 0; i < tcellsManagers.Length; i++)
        {
            if (tcellsManagers[i].GPUStruct != null)
            {
                bioData.AddRange(tcellsManagers[i].GPUStruct);
                bioDataOut.AddRange(tcellsManagers[i].GPUOutput);
            }
        }

        pathogenData = bioData.ToArray();
        pathogenDataOut = bioDataOut.ToArray();

        List<GPUData> plasData = new List<GPUData>();
        List<GPUOutput> plasDataOut = new List<GPUOutput>();

        for (int i = 0; i < plasticsManagers.Length; i++)
        {
            if (plasticsManagers[i].GPUStruct != null)
            {
                plasData.AddRange(plasticsManagers[i].GPUStruct);
                plasDataOut.AddRange(plasticsManagers[i].GPUOutput);
            }
        }

        plasticData = plasData.ToArray();
        plasticDataOut = plasDataOut.ToArray();

        TexResolution = pathogenData.Length + sentinelData.Length + plasticData.Length;
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

            BiomeBuffer = new ComputeBuffer(pathogenData.Length, Marshal.SizeOf(typeof(GPUData)));
            BiomeBuffer.SetData(pathogenData);

            plasticBuffer = new ComputeBuffer(plasticData.Length, Marshal.SizeOf(typeof(GPUData)));
            plasticBuffer.SetData(plasticData);

            sentinelBufferOut = new ComputeBuffer(sentinelData.Length, Marshal.SizeOf(typeof(GPUOutput)));
            sentinelBufferOut.SetData(new GPUOutput[sentinelData.Length]);

            BiomeBufferOut = new ComputeBuffer(pathogenData.Length, Marshal.SizeOf(typeof(GPUOutput)));
            BiomeBufferOut.SetData(new GPUOutput[pathogenData.Length]);

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

            AsncySetData();

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
            pathogenDataOut = request.GetData<GPUOutput>().ToArray();
            Bcomputed = true;
        }
    }

    private void OnDisable()
    {
        BiomeBuffer?.Release(); ;
        sentinelBuffer?.Release();
        plasticBuffer?.Release();
        BiomeBufferOut?.Release();
        sentinelBufferOut?.Release();
        plasticBufferOut?.Release();
    }

    private void OnDestroy()
    {
        BiomeBuffer?.Release(); ;
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