
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Rendering;

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

    private SentinelManager[] sentinels;
    private PathogenManager[] pathogen;
    private PlasticManager[] plastics;
    private TCellManager[] tcells;

    private float cDT = 1;
    private float timer = 0;

    private bool Pcomputed = false;
    private bool Scomputed = false;
    private bool Bcomputed = false;
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

        public void SetFromKuramoto(KuramotoAffecterAgent kuramoto)
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

    // Start is called before the first frame update

    private void Start()
    {
        sentinels = GetComponentsInChildren<SentinelManager>();

        pathogen = GetComponentsInChildren<PathogenManager>();

        plastics = GetComponentsInChildren<PlasticManager>();

        tcells = GetComponentsInChildren<TCellManager>();

        cDT = Time.deltaTime;

        StartCoroutine(UpdateTextureFromComputeASync());

    }


    private void Update()
    {

        //LinkData();

        //UpdateTextureFromCompute();

        //SetData();
        //timer = Time.realtimeSinceStartup;
        //Debug.Log(pathogenData[0].phase);


    }

    private void SetData()
    {
        int bioOffset = 0;

        for (int i = 0; i < pathogen.Length; i++)
        {
            if (pathogen[i].GPUStruct != null)
            {

                pathogen[i].GPUStruct = Extensions.SubArray(pathogenData, bioOffset, pathogen[i].GPUStruct.Length);
                bioOffset += pathogen[i].GPUStruct.Length;
            }
        }
        for (int i = 0; i < tcells.Length; i++)
        {
            if (tcells[i].GPUStruct != null)
            {
                tcells[i].GPUStruct = Extensions.SubArray(pathogenData, bioOffset, tcells[i].GPUStruct.Length);

                bioOffset += tcells[i].GPUStruct.Length;
            }
        }


        int sentOffset = 0;

        for (int i = 0; i < sentinels.Length; i++)
        {
            if (sentinels[i].GPUStruct != null)
            {
                sentinels[i].GPUStruct = Extensions.SubArray(sentinelData, sentOffset, sentinels[i].GPUStruct.Length);
                sentOffset += sentinels[i].GPUStruct.Length;
            }
        }

        int plasticOffset = 0;

        for (int i = 0; i < plastics.Length; i++)
        {
            if (plastics[i].GPUStruct != null)
            {

                plastics[i].GPUStruct = Extensions.SubArray(plasticData, plasticOffset, plastics[i].GPUStruct.Length);
                plasticOffset += plastics[i].GPUStruct.Length;
            }
        }
    }

    private void AsncySetData()
    {
        int bioOffset = 0;

        for (int i = 0; i < pathogen.Length; i++)
        {
            if (pathogen[i].GPUStruct != null)
            {

                pathogen[i].GPUOutput = Extensions.SubArray(pathogenDataOut, bioOffset, pathogen[i].GPUStruct.Length);
                bioOffset += pathogen[i].GPUStruct.Length;
            }
        }
        for (int i = 0; i < tcells.Length; i++)
        {
            if (tcells[i].GPUStruct != null)
            {
                tcells[i].GPUOutput = Extensions.SubArray(pathogenDataOut, bioOffset, tcells[i].GPUStruct.Length);

                bioOffset += tcells[i].GPUStruct.Length;
            }
        }


        int sentOffset = 0;

        for (int i = 0; i < sentinels.Length; i++)
        {
            if (sentinels[i].GPUStruct != null)
            {
                sentinels[i].GPUOutput = Extensions.SubArray(sentinelDataOut, sentOffset, sentinels[i].GPUStruct.Length);
                sentOffset += sentinels[i].GPUStruct.Length;
            }
        }

        int plasticOffset = 0;

        for (int i = 0; i < plastics.Length; i++)
        {
            if (plastics[i].GPUStruct != null)
            {

                plastics[i].GPUOutput = Extensions.SubArray(plasticDataOut, plasticOffset, plastics[i].GPUStruct.Length);
                plasticOffset += plastics[i].GPUStruct.Length;
            }
        }
    }

    private void LinkData()
    {

        List<GPUData> sentData = new List<GPUData>();
        List<GPUOutput> sentDataOut = new List<GPUOutput>();

        for (int i = 0; i < sentinels.Length; i++)
        {
            if (sentinels[i].GPUStruct != null)
            {
                sentData.AddRange(sentinels[i].GPUStruct);
                sentDataOut.AddRange(sentinels[i].GPUOutput);
            }
        }


        sentinelData = sentData.ToArray();
        sentinelDataOut = sentDataOut.ToArray();

        List<GPUData> bioData = new List<GPUData>();
        List<GPUOutput> bioDataOut = new List<GPUOutput>();

        for (int i = 0; i < pathogen.Length; i++)
        {
            if (pathogen[i].GPUStruct != null)
            {
                bioData.AddRange(pathogen[i].GPUStruct);
                bioDataOut.AddRange(pathogen[i].GPUOutput);

            }
        }

        for (int i = 0; i < tcells.Length; i++)
        {
            if (tcells[i].GPUStruct != null)
            {
                bioData.AddRange(tcells[i].GPUStruct);
                bioDataOut.AddRange(tcells[i].GPUOutput);
            }
        }

        pathogenData = bioData.ToArray();
        pathogenDataOut = bioDataOut.ToArray();

        List<GPUData> plasData = new List<GPUData>();
        List<GPUOutput> plasDataOut = new List<GPUOutput>();

        for (int i = 0; i < plastics.Length; i++)
        {
            if (plastics[i].GPUStruct != null)
            {
                plasData.AddRange(plastics[i].GPUStruct);
                plasDataOut.AddRange(plastics[i].GPUOutput);
            }
        }

        plasticData = plasData.ToArray();
        plasticDataOut = plasDataOut.ToArray();

        TexResolution = pathogenData.Length + sentinelData.Length + plasticData.Length;

    }


    private void UpdateTextureFromCompute()
    {

        rt = new RenderTexture(TexResolution, 1, 0);
        rt.enableRandomWrite = true;
        RenderTexture.active = rt;

        ComputeBuffer sentinelBuffer = new ComputeBuffer(sentinelData.Length, Marshal.SizeOf(typeof(GPUData)));
        sentinelBuffer.SetData(sentinelData);

        ComputeBuffer BiomeBuffer = new ComputeBuffer(pathogenData.Length, Marshal.SizeOf(typeof(GPUData)));
        BiomeBuffer.SetData(pathogenData);

        ComputeBuffer plasticBuffer = new ComputeBuffer(plasticData.Length, Marshal.SizeOf(typeof(GPUData)));
        plasticBuffer.SetData(plasticData);

        //  Debug.Log("start");
        //  Debug.Log(plasticData[0].phase);

        int UpdateBiome = shader.FindKernel("BiomeUpdate");
        //int UpdateSentinel = shader.FindKernel("SentinelUpdate");

        shader.SetTexture(UpdateBiome, "Result", rt);
        shader.SetBuffer(UpdateBiome, "sentinelData", sentinelBuffer);
        shader.SetBuffer(UpdateBiome, "biomeData", BiomeBuffer);
        shader.SetBuffer(UpdateBiome, "plasticData", plasticBuffer);
        shader.SetFloat("dt", Time.deltaTime);

        shader.Dispatch(UpdateBiome, TexResolution, 1, 1);

        BiomeBuffer.GetData(pathogenData);
        sentinelBuffer.GetData(sentinelData);
        plasticBuffer.GetData(plasticData);
        //  Debug.Log(plasticData[0].phase);

        BiomeBuffer.Release();
        sentinelBuffer.Release();
        plasticBuffer.Release();
        //       print("C");


    }


    void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.Log("GPU readback error detected.");
            return;
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

            ComputeBuffer sentinelBuffer = new ComputeBuffer(sentinelData.Length, Marshal.SizeOf(typeof(GPUData)));
            sentinelBuffer.SetData(sentinelData);

            ComputeBuffer BiomeBuffer = new ComputeBuffer(pathogenData.Length, Marshal.SizeOf(typeof(GPUData)));
            BiomeBuffer.SetData(pathogenData);

            ComputeBuffer plasticBuffer = new ComputeBuffer(plasticData.Length, Marshal.SizeOf(typeof(GPUData)));
            plasticBuffer.SetData(plasticData);

            ComputeBuffer sentinelBufferOut = new ComputeBuffer(sentinelData.Length, Marshal.SizeOf(typeof(GPUOutput)));
            sentinelBufferOut.SetData(new GPUOutput[sentinelData.Length]);

            ComputeBuffer BiomeBufferOut = new ComputeBuffer(pathogenData.Length, Marshal.SizeOf(typeof(GPUOutput)));
            BiomeBufferOut.SetData(new GPUOutput[pathogenData.Length]);

            ComputeBuffer plasticBufferOut = new ComputeBuffer(plasticData.Length, Marshal.SizeOf(typeof(GPUOutput)));
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