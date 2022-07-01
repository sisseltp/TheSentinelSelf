
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;

public class GPUCompute : MonoBehaviour
{
    [SerializeField]
    private ComputeShader shader;


    private int TexResolution;
    
    private RenderTexture rt;

    private SentinelManager.GPUData[] sentinelData;

    private BiomeManager.GPUData[] biomeData;

    private PlasticManager.GPUData[] plasticData;

    private SentinelManager[] sentinels;
    private BiomeManager[] biome;
    private PlasticManager[] plastics;
    private TCellManager[] tcells;


    // Start is called before the first frame update

    private void Start()
    {
        sentinels = GetComponentsInChildren<SentinelManager>();

        biome = GetComponentsInChildren<BiomeManager>();

        plastics = GetComponentsInChildren<PlasticManager>();

        tcells = GetComponentsInChildren<TCellManager>();

    }
    private void Update()
    {
            LinkData();

            UpdateTextureFromCompute();

            SetData();
    }


    private void SetData()
    {
        int bioOffset = 0;

        for (int i = 0; i < biome.Length; i++)
        {
            if (biome[i].GPUStruct != null)
            {

                biome[i].GPUStruct = Extensions.SubArray(biomeData, bioOffset, bioOffset + biome[i].GPUStruct.Length);
                bioOffset += biome[i].GPUStruct.Length;
            }
        }

        for (int i = 0; i < tcells.Length; i++)
        {
            if (tcells[i].GPUStruct != null)
            {
                tcells[i].GPUStruct = biomeData.SubArray<BiomeManager.GPUData>(bioOffset, tcells[i].GPUStruct.Length);
                bioOffset += tcells[i].GPUStruct.Length;
            }
        }


        int sentOffset = 0;

        for (int i = 0; i < sentinels.Length; i++)
        {
            if (sentinels[i].GPUStruct != null)
            {
                sentinels[i].GPUStruct = Extensions.SubArray(sentinelData, sentOffset, sentOffset+sentinels[i].GPUStruct.Length);
                sentOffset += sentinels[i].GPUStruct.Length;
            }
        }

        int plasticOffset = 0;

        for (int i = 0; i < plastics.Length; i++)
        {
            if (plastics[i].GPUStruct != null)
            {

                plastics[i].GPUStruct = Extensions.SubArray(plasticData, plasticOffset, plasticOffset + plastics[i].GPUStruct.Length);
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


        List<BiomeManager.GPUData> bioData = new List<BiomeManager.GPUData>();

        for (int i = 0; i < biome.Length; i++)
        {
            if (biome[i].GPUStruct != null)
            {
                bioData.AddRange(biome[i].GPUStruct);
            }
        }

        for (int i = 0; i < tcells.Length; i++)
        {
            if (tcells[i].GPUStruct != null)
            {
                bioData.AddRange(tcells[i].GPUStruct);
            }
        }

        biomeData = bioData.ToArray();


        List<PlasticManager.GPUData> plasData = new List<PlasticManager.GPUData>();

        for (int i = 0; i < plastics.Length; i++)
        {
            if (plastics[i].GPUStruct != null)
            {
                plasData.AddRange(plastics[i].GPUStruct);
            }
        }

        plasticData = plasData.ToArray();

        TexResolution = biomeData.Length + sentinelData.Length + plasticData.Length;
        
    }


    private void UpdateTextureFromCompute()
    {

        rt = new RenderTexture(TexResolution, 1, 0);
        rt.enableRandomWrite = true;
        RenderTexture.active = rt;

        ComputeBuffer sentinelBuffer = new ComputeBuffer(sentinelData.Length, Marshal.SizeOf(typeof(SentinelManager.GPUData)));
        sentinelBuffer.SetData(sentinelData);

        ComputeBuffer BiomeBuffer = new ComputeBuffer(biomeData.Length, Marshal.SizeOf(typeof(BiomeManager.GPUData)));
        BiomeBuffer.SetData(biomeData);

        ComputeBuffer plasticBuffer = new ComputeBuffer(plasticData.Length, Marshal.SizeOf(typeof(PlasticManager.GPUData)));
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
        
        shader.Dispatch(UpdateBiome, TexResolution , 1 , 1);
        //shader.Dispatch(UpdateSentinel, TexResolution, 1, 1);

        BiomeBuffer.GetData(biomeData);
        sentinelBuffer.GetData(sentinelData);
        plasticBuffer.GetData(plasticData);
      //  Debug.Log(plasticData[0].phase);

        BiomeBuffer.Dispose();
        sentinelBuffer.Dispose();
        plasticBuffer.Dispose();
 //       print("C");


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