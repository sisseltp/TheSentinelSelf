
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System;
using System.Linq;

public class GPUCompute : MonoBehaviour
{
    [SerializeField]
    private ComputeShader shader;

    [SerializeField]
    private GameObject Agents;

    private int TexResolution;
    
    private RenderTexture rt;

    private SentinelManager.Sentinel[] sentinelData;

    private BiomeManager.Sentinel[] biomeData;

    private SentinelManager[] sentinels;
    private BiomeManager[] biome;

    // Start is called before the first frame update


    private void Update()
    {
            LinkData();

            UpdateTextureFromCompute();

            SetData();
    }


    private void SetData()
    {
        
        biome = Agents.GetComponentsInChildren<BiomeManager>();

        int bioOffset = 0;

        for (int i = 0; i < biome.Length; i++)
        {
            if (biome[i].sentinelsStruct != null)
            {

                biome[i].sentinelsStruct = Extensions.SubArray(biomeData, bioOffset, bioOffset + biome[i].sentinelsStruct.Length);
                bioOffset += biome[i].sentinelsStruct.Length;
            }
        }

        int sentOffset = 0;

        for (int i = 0; i < sentinels.Length; i++)
        {
            if (sentinels[i].sentinelsStruct != null)
            {

                sentinels[i].sentinelsStruct = Extensions.SubArray(sentinelData, sentOffset, sentOffset+sentinels[i].sentinelsStruct.Length);
                sentOffset += sentinels[i].sentinelsStruct.Length;
            }
        }

    }

    private void LinkData()
    {
        sentinels = Agents.GetComponentsInChildren<SentinelManager>();

        List<SentinelManager.Sentinel> sentData = new List<SentinelManager.Sentinel>();

        for (int i = 0; i < sentinels.Length; i++)
        {
            if (sentinels[i].sentinelsStruct != null)
            {
                sentData.AddRange(sentinels[i].sentinelsStruct);
            }
        }

        sentinelData = sentData.ToArray();

        biome = Agents.GetComponentsInChildren<BiomeManager>();

        List<BiomeManager.Sentinel> bioData = new List<BiomeManager.Sentinel>();

        for (int i = 0; i < biome.Length; i++)
        {
            if (biome[i].sentinelsStruct != null)
            {
                bioData.AddRange(biome[i].sentinelsStruct);
            }
        }

        biomeData = bioData.ToArray();

        TexResolution = biomeData.Length + sentinelData.Length;
        
    }


    private void UpdateTextureFromCompute()
    {

        rt = new RenderTexture(TexResolution, 1, 0);
        rt.enableRandomWrite = true;
        RenderTexture.active = rt;

        ComputeBuffer sentinelBuffer = new ComputeBuffer(sentinelData.Length, Marshal.SizeOf(typeof(SentinelManager.Sentinel)));
        sentinelBuffer.SetData(sentinelData);

        ComputeBuffer BiomeBuffer = new ComputeBuffer(biomeData.Length, Marshal.SizeOf(typeof(BiomeManager.Sentinel)));
        BiomeBuffer.SetData(biomeData);

        Debug.Log("start");
        Debug.Log(biomeData[0].phase);

        int UpdateBiome = shader.FindKernel("BiomeUpdate");
        //int UpdateSentinel = shader.FindKernel("SentinelUpdate");

        shader.SetTexture(UpdateBiome, "Result", rt);
        shader.SetBuffer(UpdateBiome, "sentinelData", sentinelBuffer);
        shader.SetBuffer(UpdateBiome, "biomeData", BiomeBuffer);
        shader.SetFloat("dt", Time.deltaTime);
        
        shader.Dispatch(UpdateBiome, TexResolution , 1 , 1);
        //shader.Dispatch(UpdateSentinel, TexResolution, 1, 1);

        BiomeBuffer.GetData(biomeData);
        sentinelBuffer.GetData(sentinelData);
       // Debug.Log(sentinelData[1].connections);
        Debug.Log(biomeData[0].phase);
       //Debug.Log(biomeData[21].pos);
       // Debug.Log(biomeData[21].played);
       // Debug.Log(biomeData[21].phase);
       // Debug.Log(biomeData[21].speed);
        BiomeBuffer.Dispose();
        sentinelBuffer.Dispose();
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