
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine.Rendering;
using Unity.Collections;

public class GPUCompute : MonoBehaviour
{
    [SerializeField]
    private ComputeShader shader;


    private int TexResolution;
    
    private RenderTexture rt;

    private SentinelManager.GPUData[] sentinelData;

    private PathogenManager.GPUData[] pathogenData;

    private PlasticManager2.GPUData[] plasticData;

    private SentinelManager[] sentinels;
    private PathogenManager[] pathogen;
    private PlasticManager2[] plastics;
    private TCellManager[] tcells;

    private float cDT = 1;
    private float timer = 0;

    // Start is called before the first frame update

    private void Start()
    {
        sentinels = GetComponentsInChildren<SentinelManager>();

        pathogen = GetComponentsInChildren<PathogenManager>();

        plastics = GetComponentsInChildren<PlasticManager2>();

        tcells = GetComponentsInChildren<TCellManager>();

        cDT = Time.deltaTime;

       
    }

    
    private void Update()
    {
        
            LinkData();

            UpdateTextureFromCompute();

            SetData();
            //timer = Time.realtimeSinceStartup;
            //Debug.Log(pathogenData[0].phase);
            //StartCoroutine(UpdateTextureFromComputeASync1());
            

    }


    private void SetData()
    {
        int bioOffset = 0;
        
        for (int i = 0; i < pathogen.Length; i++)
        {
            if (pathogen[i].GPUStruct != null)
            {
                
                pathogen[i].GPUStruct = Extensions.SubArray(pathogenData, bioOffset,  pathogen[i].GPUStruct.Length);
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

                plastics[i].GPUStruct = Extensions.SubArray(plasticData, plasticOffset,  plastics[i].GPUStruct.Length);
                plasticOffset += plastics[i].GPUStruct.Length;
            }
        }
    }

    private void LinkData()
    {

        List<SentinelManager.GPUData> sentData = new List<SentinelManager.GPUData>();

        for (int i = 0; i < sentinels.Length; i++)
        {
            if (sentinels[i].GPUStruct != null)
            {
                sentData.AddRange(sentinels[i].GPUStruct);
            }
        }


        sentinelData = sentData.ToArray();


        List<PathogenManager.GPUData> bioData = new List<PathogenManager.GPUData>();

        for (int i = 0; i < pathogen.Length; i++)
        {
            if (pathogen[i].GPUStruct != null)
            {
                bioData.AddRange(pathogen[i].GPUStruct);
            }
        }

        for (int i = 0; i < tcells.Length; i++)
        {
            if (tcells[i].GPUStruct != null)
            {
                bioData.AddRange(tcells[i].GPUStruct);
            }
        }

        pathogenData = bioData.ToArray();

        List<PlasticManager2.GPUData> plasData = new List<PlasticManager2.GPUData>();

        for (int i = 0; i < plastics.Length; i++)
        {
            if (plastics[i].GPUStruct != null)
            {
                plasData.AddRange(plastics[i].GPUStruct);
            }
        }

        plasticData = plasData.ToArray();

        TexResolution = pathogenData.Length + sentinelData.Length + plasticData.Length;
        
    }


    private void UpdateTextureFromCompute()
    {

        rt = new RenderTexture(TexResolution, 1, 0);
        rt.enableRandomWrite = true;
        RenderTexture.active = rt;

        ComputeBuffer sentinelBuffer = new ComputeBuffer(sentinelData.Length, Marshal.SizeOf(typeof(SentinelManager.GPUData)));
        sentinelBuffer.SetData(sentinelData);

        ComputeBuffer BiomeBuffer = new ComputeBuffer(pathogenData.Length, Marshal.SizeOf(typeof(PathogenManager.GPUData)));
        BiomeBuffer.SetData(pathogenData);

        ComputeBuffer plasticBuffer = new ComputeBuffer(plasticData.Length, Marshal.SizeOf(typeof(PlasticManager2.GPUData)));
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
        
        shader.Dispatch(UpdateBiome, TexResolution , 1, 1);
        //shader.Dispatch(UpdateSentinel, TexResolution, 1, 1);
        
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




    IEnumerator  UpdateTextureFromComputeASync1()
    {

        rt = new RenderTexture(TexResolution, 1, 0);
        rt.enableRandomWrite = true;
        RenderTexture.active = rt;

        ComputeBuffer sentinelBuffer = new ComputeBuffer(sentinelData.Length, Marshal.SizeOf(typeof(SentinelManager.GPUData)));
        sentinelBuffer.SetData(sentinelData);

        ComputeBuffer BiomeBuffer = new ComputeBuffer(pathogenData.Length, Marshal.SizeOf(typeof(PathogenManager.GPUData)));
        BiomeBuffer.SetData(pathogenData);

        ComputeBuffer plasticBuffer = new ComputeBuffer(plasticData.Length, Marshal.SizeOf(typeof(PlasticManager2.GPUData)));
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

        var Brequest = UnityEngine.Rendering.AsyncGPUReadback.Request(BiomeBuffer, OnCompleteReadback);
        var Srequest = UnityEngine.Rendering.AsyncGPUReadback.Request(sentinelBuffer, OnCompleteReadback);
        var Prequest = UnityEngine.Rendering.AsyncGPUReadback.Request(plasticBuffer, OnCompleteReadback);


        yield return new WaitUntil(() => Brequest.done && Srequest.done && Prequest.done);

        
        pathogenData = Brequest.GetData<PathogenManager.GPUData>().ToArray();
        sentinelData = Srequest.GetData<SentinelManager.GPUData>().ToArray();
        plasticData = Prequest.GetData<PlasticManager2.GPUData>().ToArray();
        
        SetData();

        BiomeBuffer.Release();
        sentinelBuffer.Release();
        plasticBuffer.Release();

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