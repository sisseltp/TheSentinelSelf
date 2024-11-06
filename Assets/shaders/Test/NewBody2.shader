Shader "NewBody2"
{
    Properties
    {
        [NoScaleOffset]Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca("MainTex", 2D) = "white" {}
        Vector1_c59bce75c00040b2866e6bf70aa8b671("emission", Float) = 0
        [NoScaleOffset]Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4("BodyMask", 2D) = "white" {}
        [NoScaleOffset]Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa("Normal", 2D) = "white" {}
        Vector1_80773e26b92d42a291521a504eba9a22("OpacityHoles", Range(0, 1)) = 0.5
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_FORWARD
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float3 normalWS;
            float4 tangentWS;
            float4 texCoord0;
            float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            float2 lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            float3 sh;
            #endif
            float4 fogFactorAndVertexLight;
            float4 shadowCoord;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 WorldSpaceNormal;
            float3 TangentSpaceNormal;
            float3 WorldSpaceViewDirection;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float3 interp1 : TEXCOORD1;
            float4 interp2 : TEXCOORD2;
            float4 interp3 : TEXCOORD3;
            float3 interp4 : TEXCOORD4;
            #if defined(LIGHTMAP_ON)
            float2 interp5 : TEXCOORD5;
            #endif
            #if !defined(LIGHTMAP_ON)
            float3 interp6 : TEXCOORD6;
            #endif
            float4 interp7 : TEXCOORD7;
            float4 interp8 : TEXCOORD8;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
            output.interp3.xyzw =  input.texCoord0;
            output.interp4.xyz =  input.viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            output.interp5.xy =  input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.interp6.xyz =  input.sh;
            #endif
            output.interp7.xyzw =  input.fogFactorAndVertexLight;
            output.interp8.xyzw =  input.shadowCoord;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
            output.texCoord0 = input.interp3.xyzw;
            output.viewDirectionWS = input.interp4.xyz;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.interp5.xy;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.interp6.xyz;
            #endif
            output.fogFactorAndVertexLight = input.interp7.xyzw;
            output.shadowCoord = input.interp8.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca_TexelSize;
        float Vector1_c59bce75c00040b2866e6bf70aa8b671;
        float4 Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4_TexelSize;
        float4 Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa_TexelSize;
        float Vector1_80773e26b92d42a291521a504eba9a22;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        SAMPLER(samplerTexture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        TEXTURE2D(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        SAMPLER(samplerTexture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        TEXTURE2D(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
        SAMPLER(samplerTexture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }

        void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }


        inline float2 Unity_Voronoi_RandomVector_float (float2 UV, float offset)
        {
            float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
            UV = frac(sin(mul(UV, m)));
            return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
        }

        void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
        {
            float2 g = floor(UV * CellDensity);
            float2 f = frac(UV * CellDensity);
            float t = 8.0;
            float3 res = float3(8.0, 0.0, 0.0);

            for(int y=-1; y<=1; y++)
            {
                for(int x=-1; x<=1; x++)
                {
                    float2 lattice = float2(x,y);
                    float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
                    float d = distance(lattice + offset, f);

                    if(d < res.x)
                    {
                        res = float3(d, offset.x, offset.y);
                        Out = res.x;
                        Cells = res.y;
                    }
                }
            }
        }

        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
        {
            Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
        }

        void Unity_Power_float4(float4 A, float4 B, out float4 Out)
        {
            Out = pow(A, B);
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_9f4588603daa48c29cd7283e33d08cbb_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
            float _Float_4e6007641ec04fc09c2b2667999054c6_Out_0 = 0.07;
            float _Multiply_b50e727a155340708816fcdecae86534_Out_2;
            Unity_Multiply_float(0, _Float_4e6007641ec04fc09c2b2667999054c6_Out_0, _Multiply_b50e727a155340708816fcdecae86534_Out_2);
            float4 _UV_adf164f4eb0c448c91e97f9ce69a6e03_Out_0 = IN.uv0;
            float4 _Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2;
            Unity_Add_float4((_Multiply_b50e727a155340708816fcdecae86534_Out_2.xxxx), _UV_adf164f4eb0c448c91e97f9ce69a6e03_Out_0, _Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2);
            float _Float_b1fb926f1b624976b48efa34c9873587_Out_0 = 7.35;
            float4 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGBA_4;
            float3 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGB_5;
            float2 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6;
            Unity_Combine_float(_Float_b1fb926f1b624976b48efa34c9873587_Out_0, _Float_b1fb926f1b624976b48efa34c9873587_Out_0, 0, 0, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGBA_4, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGB_5, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6);
            float2 _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2;
            Unity_Multiply_float((_Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2.xy), _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6, _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2);
            float4 _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9f4588603daa48c29cd7283e33d08cbb_Out_0.tex, _Property_9f4588603daa48c29cd7283e33d08cbb_Out_0.samplerstate, _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2);
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_R_4 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.r;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_G_5 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.g;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_B_6 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.b;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_A_7 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.a;
            float _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3;
            float _Voronoi_182acf2c1d12452b804b1178617f9171_Cells_4;
            Unity_Voronoi_float(IN.uv0.xy, 5.52, 8.21, _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3, _Voronoi_182acf2c1d12452b804b1178617f9171_Cells_4);
            float _Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3;
            Unity_Smoothstep_float(0.12, 0.83, _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3, _Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3);
            float4 Color_93959687abb64037a82971ddccb58ecf = IsGammaSpace() ? LinearToSRGB(float4(0, 0.8589136, 1.498039, 0)) : float4(0, 0.8589136, 1.498039, 0);
            float4 _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2;
            Unity_Multiply_float((_Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3.xxxx), Color_93959687abb64037a82971ddccb58ecf, _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2);
            UnityTexture2D _Property_caa9b98afd86481cb32af9287161a199_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
            float4 _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0 = SAMPLE_TEXTURE2D(_Property_caa9b98afd86481cb32af9287161a199_Out_0.tex, _Property_caa9b98afd86481cb32af9287161a199_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.r;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_G_5 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.g;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_B_6 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.b;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_A_7 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.a;
            float4 _Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3;
            Unity_Lerp_float4(_SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0, _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0, _Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3);
            UnityTexture2D _Property_33e382b27f964d48be83138a780bf2c8_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
            float4 _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0 = SAMPLE_TEXTURE2D(_Property_33e382b27f964d48be83138a780bf2c8_Out_0.tex, _Property_33e382b27f964d48be83138a780bf2c8_Out_0.samplerstate, IN.uv0.xy);
            _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.rgb = UnpackNormal(_SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0);
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_R_4 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.r;
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_G_5 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.g;
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_B_6 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.b;
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_A_7 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.a;
            float _Property_88c5723aa30f430dbcc4feddc549011d_Out_0 = Vector1_c59bce75c00040b2866e6bf70aa8b671;
            float _Float_6a2651b3179d4aeb9ba2d1ae4b865e02_Out_0 = _Property_88c5723aa30f430dbcc4feddc549011d_Out_0;
            float _FresnelEffect_1167d26bace14f0f938c6714d031cc0e_Out_3;
            Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Float_6a2651b3179d4aeb9ba2d1ae4b865e02_Out_0, _FresnelEffect_1167d26bace14f0f938c6714d031cc0e_Out_3);
            float4 Color_4a9fe31cf860438ba5275bb79d68d2ac = IsGammaSpace() ? LinearToSRGB(float4(0.25, 0.425024, 1, 0)) : float4(0.25, 0.425024, 1, 0);
            float4 _Multiply_3ac2cca204a544b6a7740fd8ef35635f_Out_2;
            Unity_Multiply_float((_FresnelEffect_1167d26bace14f0f938c6714d031cc0e_Out_3.xxxx), Color_4a9fe31cf860438ba5275bb79d68d2ac, _Multiply_3ac2cca204a544b6a7740fd8ef35635f_Out_2);
            float _Float_6747dd8afcbd4ae19f2677bb729ab7bc_Out_0 = 0.54;
            float4 _Power_f73caffcbf4f4d97b05f08d8561e5e71_Out_2;
            Unity_Power_float4(_Multiply_3ac2cca204a544b6a7740fd8ef35635f_Out_2, (_Float_6747dd8afcbd4ae19f2677bb729ab7bc_Out_0.xxxx), _Power_f73caffcbf4f4d97b05f08d8561e5e71_Out_2);
            float _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0 = Vector1_80773e26b92d42a291521a504eba9a22;
            float _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            Unity_Lerp_float(1, _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4, _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3);
            surface.BaseColor = (_Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3.xyz);
            surface.NormalTS = (_SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.xyz);
            surface.Emission = (_Power_f73caffcbf4f4d97b05f08d8561e5e71_Out_2.xyz);
            surface.Metallic = 0;
            surface.Smoothness = 0.5;
            surface.Occlusion = 1;
            surface.Alpha = _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

        	// must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
        	float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);


            output.WorldSpaceNormal =            renormFactor*input.normalWS.xyz;		// we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);


            output.WorldSpaceViewDirection =     input.viewDirectionWS; //TODO: by default normalized in HD, but not in universal
            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "GBuffer"
            Tags
            {
                "LightMode" = "UniversalGBuffer"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
        #pragma multi_compile _ _SHADOWS_SOFT
        #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
        #pragma multi_compile _ _GBUFFER_NORMALS_OCT
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_GBUFFER
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float3 normalWS;
            float4 tangentWS;
            float4 texCoord0;
            float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            float2 lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            float3 sh;
            #endif
            float4 fogFactorAndVertexLight;
            float4 shadowCoord;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 WorldSpaceNormal;
            float3 TangentSpaceNormal;
            float3 WorldSpaceViewDirection;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float3 interp1 : TEXCOORD1;
            float4 interp2 : TEXCOORD2;
            float4 interp3 : TEXCOORD3;
            float3 interp4 : TEXCOORD4;
            #if defined(LIGHTMAP_ON)
            float2 interp5 : TEXCOORD5;
            #endif
            #if !defined(LIGHTMAP_ON)
            float3 interp6 : TEXCOORD6;
            #endif
            float4 interp7 : TEXCOORD7;
            float4 interp8 : TEXCOORD8;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
            output.interp3.xyzw =  input.texCoord0;
            output.interp4.xyz =  input.viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            output.interp5.xy =  input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.interp6.xyz =  input.sh;
            #endif
            output.interp7.xyzw =  input.fogFactorAndVertexLight;
            output.interp8.xyzw =  input.shadowCoord;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
            output.texCoord0 = input.interp3.xyzw;
            output.viewDirectionWS = input.interp4.xyz;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.interp5.xy;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.interp6.xyz;
            #endif
            output.fogFactorAndVertexLight = input.interp7.xyzw;
            output.shadowCoord = input.interp8.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca_TexelSize;
        float Vector1_c59bce75c00040b2866e6bf70aa8b671;
        float4 Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4_TexelSize;
        float4 Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa_TexelSize;
        float Vector1_80773e26b92d42a291521a504eba9a22;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        SAMPLER(samplerTexture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        TEXTURE2D(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        SAMPLER(samplerTexture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        TEXTURE2D(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
        SAMPLER(samplerTexture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }

        void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }


        inline float2 Unity_Voronoi_RandomVector_float (float2 UV, float offset)
        {
            float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
            UV = frac(sin(mul(UV, m)));
            return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
        }

        void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
        {
            float2 g = floor(UV * CellDensity);
            float2 f = frac(UV * CellDensity);
            float t = 8.0;
            float3 res = float3(8.0, 0.0, 0.0);

            for(int y=-1; y<=1; y++)
            {
                for(int x=-1; x<=1; x++)
                {
                    float2 lattice = float2(x,y);
                    float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
                    float d = distance(lattice + offset, f);

                    if(d < res.x)
                    {
                        res = float3(d, offset.x, offset.y);
                        Out = res.x;
                        Cells = res.y;
                    }
                }
            }
        }

        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
        {
            Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
        }

        void Unity_Power_float4(float4 A, float4 B, out float4 Out)
        {
            Out = pow(A, B);
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_9f4588603daa48c29cd7283e33d08cbb_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
            float _Float_4e6007641ec04fc09c2b2667999054c6_Out_0 = 0.07;
            float _Multiply_b50e727a155340708816fcdecae86534_Out_2;
            Unity_Multiply_float(0, _Float_4e6007641ec04fc09c2b2667999054c6_Out_0, _Multiply_b50e727a155340708816fcdecae86534_Out_2);
            float4 _UV_adf164f4eb0c448c91e97f9ce69a6e03_Out_0 = IN.uv0;
            float4 _Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2;
            Unity_Add_float4((_Multiply_b50e727a155340708816fcdecae86534_Out_2.xxxx), _UV_adf164f4eb0c448c91e97f9ce69a6e03_Out_0, _Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2);
            float _Float_b1fb926f1b624976b48efa34c9873587_Out_0 = 7.35;
            float4 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGBA_4;
            float3 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGB_5;
            float2 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6;
            Unity_Combine_float(_Float_b1fb926f1b624976b48efa34c9873587_Out_0, _Float_b1fb926f1b624976b48efa34c9873587_Out_0, 0, 0, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGBA_4, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGB_5, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6);
            float2 _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2;
            Unity_Multiply_float((_Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2.xy), _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6, _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2);
            float4 _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9f4588603daa48c29cd7283e33d08cbb_Out_0.tex, _Property_9f4588603daa48c29cd7283e33d08cbb_Out_0.samplerstate, _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2);
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_R_4 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.r;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_G_5 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.g;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_B_6 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.b;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_A_7 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.a;
            float _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3;
            float _Voronoi_182acf2c1d12452b804b1178617f9171_Cells_4;
            Unity_Voronoi_float(IN.uv0.xy, 5.52, 8.21, _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3, _Voronoi_182acf2c1d12452b804b1178617f9171_Cells_4);
            float _Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3;
            Unity_Smoothstep_float(0.12, 0.83, _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3, _Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3);
            float4 Color_93959687abb64037a82971ddccb58ecf = IsGammaSpace() ? LinearToSRGB(float4(0, 0.8589136, 1.498039, 0)) : float4(0, 0.8589136, 1.498039, 0);
            float4 _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2;
            Unity_Multiply_float((_Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3.xxxx), Color_93959687abb64037a82971ddccb58ecf, _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2);
            UnityTexture2D _Property_caa9b98afd86481cb32af9287161a199_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
            float4 _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0 = SAMPLE_TEXTURE2D(_Property_caa9b98afd86481cb32af9287161a199_Out_0.tex, _Property_caa9b98afd86481cb32af9287161a199_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.r;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_G_5 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.g;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_B_6 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.b;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_A_7 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.a;
            float4 _Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3;
            Unity_Lerp_float4(_SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0, _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0, _Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3);
            UnityTexture2D _Property_33e382b27f964d48be83138a780bf2c8_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
            float4 _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0 = SAMPLE_TEXTURE2D(_Property_33e382b27f964d48be83138a780bf2c8_Out_0.tex, _Property_33e382b27f964d48be83138a780bf2c8_Out_0.samplerstate, IN.uv0.xy);
            _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.rgb = UnpackNormal(_SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0);
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_R_4 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.r;
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_G_5 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.g;
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_B_6 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.b;
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_A_7 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.a;
            float _Property_88c5723aa30f430dbcc4feddc549011d_Out_0 = Vector1_c59bce75c00040b2866e6bf70aa8b671;
            float _Float_6a2651b3179d4aeb9ba2d1ae4b865e02_Out_0 = _Property_88c5723aa30f430dbcc4feddc549011d_Out_0;
            float _FresnelEffect_1167d26bace14f0f938c6714d031cc0e_Out_3;
            Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Float_6a2651b3179d4aeb9ba2d1ae4b865e02_Out_0, _FresnelEffect_1167d26bace14f0f938c6714d031cc0e_Out_3);
            float4 Color_4a9fe31cf860438ba5275bb79d68d2ac = IsGammaSpace() ? LinearToSRGB(float4(0.25, 0.425024, 1, 0)) : float4(0.25, 0.425024, 1, 0);
            float4 _Multiply_3ac2cca204a544b6a7740fd8ef35635f_Out_2;
            Unity_Multiply_float((_FresnelEffect_1167d26bace14f0f938c6714d031cc0e_Out_3.xxxx), Color_4a9fe31cf860438ba5275bb79d68d2ac, _Multiply_3ac2cca204a544b6a7740fd8ef35635f_Out_2);
            float _Float_6747dd8afcbd4ae19f2677bb729ab7bc_Out_0 = 0.54;
            float4 _Power_f73caffcbf4f4d97b05f08d8561e5e71_Out_2;
            Unity_Power_float4(_Multiply_3ac2cca204a544b6a7740fd8ef35635f_Out_2, (_Float_6747dd8afcbd4ae19f2677bb729ab7bc_Out_0.xxxx), _Power_f73caffcbf4f4d97b05f08d8561e5e71_Out_2);
            float _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0 = Vector1_80773e26b92d42a291521a504eba9a22;
            float _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            Unity_Lerp_float(1, _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4, _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3);
            surface.BaseColor = (_Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3.xyz);
            surface.NormalTS = (_SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.xyz);
            surface.Emission = (_Power_f73caffcbf4f4d97b05f08d8561e5e71_Out_2.xyz);
            surface.Metallic = 0;
            surface.Smoothness = 0.5;
            surface.Occlusion = 1;
            surface.Alpha = _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

        	// must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
        	float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);


            output.WorldSpaceNormal =            renormFactor*input.normalWS.xyz;		// we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);


            output.WorldSpaceViewDirection =     input.viewDirectionWS; //TODO: by default normalized in HD, but not in universal
            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRGBufferPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SHADOWCASTER
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 normalWS;
            float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.normalWS;
            output.interp1.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.interp0.xyz;
            output.texCoord0 = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca_TexelSize;
        float Vector1_c59bce75c00040b2866e6bf70aa8b671;
        float4 Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4_TexelSize;
        float4 Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa_TexelSize;
        float Vector1_80773e26b92d42a291521a504eba9a22;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        SAMPLER(samplerTexture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        TEXTURE2D(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        SAMPLER(samplerTexture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        TEXTURE2D(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
        SAMPLER(samplerTexture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);

            // Graph Functions
            
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0 = Vector1_80773e26b92d42a291521a504eba9a22;
            UnityTexture2D _Property_caa9b98afd86481cb32af9287161a199_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
            float4 _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0 = SAMPLE_TEXTURE2D(_Property_caa9b98afd86481cb32af9287161a199_Out_0.tex, _Property_caa9b98afd86481cb32af9287161a199_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.r;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_G_5 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.g;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_B_6 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.b;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_A_7 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.a;
            float _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            Unity_Lerp_float(1, _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4, _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3);
            surface.Alpha = _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHONLY
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float4 interp0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca_TexelSize;
        float Vector1_c59bce75c00040b2866e6bf70aa8b671;
        float4 Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4_TexelSize;
        float4 Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa_TexelSize;
        float Vector1_80773e26b92d42a291521a504eba9a22;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        SAMPLER(samplerTexture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        TEXTURE2D(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        SAMPLER(samplerTexture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        TEXTURE2D(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
        SAMPLER(samplerTexture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);

            // Graph Functions
            
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0 = Vector1_80773e26b92d42a291521a504eba9a22;
            UnityTexture2D _Property_caa9b98afd86481cb32af9287161a199_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
            float4 _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0 = SAMPLE_TEXTURE2D(_Property_caa9b98afd86481cb32af9287161a199_Out_0.tex, _Property_caa9b98afd86481cb32af9287161a199_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.r;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_G_5 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.g;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_B_6 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.b;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_A_7 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.a;
            float _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            Unity_Lerp_float(1, _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4, _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3);
            surface.Alpha = _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 normalWS;
            float4 tangentWS;
            float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 TangentSpaceNormal;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            float4 interp2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.normalWS;
            output.interp1.xyzw =  input.tangentWS;
            output.interp2.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.interp0.xyz;
            output.tangentWS = input.interp1.xyzw;
            output.texCoord0 = input.interp2.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca_TexelSize;
        float Vector1_c59bce75c00040b2866e6bf70aa8b671;
        float4 Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4_TexelSize;
        float4 Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa_TexelSize;
        float Vector1_80773e26b92d42a291521a504eba9a22;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        SAMPLER(samplerTexture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        TEXTURE2D(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        SAMPLER(samplerTexture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        TEXTURE2D(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
        SAMPLER(samplerTexture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);

            // Graph Functions
            
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 NormalTS;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_33e382b27f964d48be83138a780bf2c8_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
            float4 _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0 = SAMPLE_TEXTURE2D(_Property_33e382b27f964d48be83138a780bf2c8_Out_0.tex, _Property_33e382b27f964d48be83138a780bf2c8_Out_0.samplerstate, IN.uv0.xy);
            _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.rgb = UnpackNormal(_SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0);
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_R_4 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.r;
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_G_5 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.g;
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_B_6 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.b;
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_A_7 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.a;
            float _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0 = Vector1_80773e26b92d42a291521a504eba9a22;
            UnityTexture2D _Property_caa9b98afd86481cb32af9287161a199_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
            float4 _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0 = SAMPLE_TEXTURE2D(_Property_caa9b98afd86481cb32af9287161a199_Out_0.tex, _Property_caa9b98afd86481cb32af9287161a199_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.r;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_G_5 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.g;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_B_6 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.b;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_A_7 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.a;
            float _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            Unity_Lerp_float(1, _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4, _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3);
            surface.NormalTS = (_SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.xyz);
            surface.Alpha = _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



            output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);


            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }

            // Render State
            Cull Off

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_TEXCOORD2
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_META
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            float4 uv1 : TEXCOORD1;
            float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 normalWS;
            float4 texCoord0;
            float3 viewDirectionWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 WorldSpaceNormal;
            float3 WorldSpaceViewDirection;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            float3 interp2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.normalWS;
            output.interp1.xyzw =  input.texCoord0;
            output.interp2.xyz =  input.viewDirectionWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.interp0.xyz;
            output.texCoord0 = input.interp1.xyzw;
            output.viewDirectionWS = input.interp2.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca_TexelSize;
        float Vector1_c59bce75c00040b2866e6bf70aa8b671;
        float4 Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4_TexelSize;
        float4 Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa_TexelSize;
        float Vector1_80773e26b92d42a291521a504eba9a22;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        SAMPLER(samplerTexture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        TEXTURE2D(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        SAMPLER(samplerTexture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        TEXTURE2D(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
        SAMPLER(samplerTexture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }

        void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }


        inline float2 Unity_Voronoi_RandomVector_float (float2 UV, float offset)
        {
            float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
            UV = frac(sin(mul(UV, m)));
            return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
        }

        void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
        {
            float2 g = floor(UV * CellDensity);
            float2 f = frac(UV * CellDensity);
            float t = 8.0;
            float3 res = float3(8.0, 0.0, 0.0);

            for(int y=-1; y<=1; y++)
            {
                for(int x=-1; x<=1; x++)
                {
                    float2 lattice = float2(x,y);
                    float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
                    float d = distance(lattice + offset, f);

                    if(d < res.x)
                    {
                        res = float3(d, offset.x, offset.y);
                        Out = res.x;
                        Cells = res.y;
                    }
                }
            }
        }

        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
        {
            Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
        }

        void Unity_Power_float4(float4 A, float4 B, out float4 Out)
        {
            Out = pow(A, B);
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float3 Emission;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_9f4588603daa48c29cd7283e33d08cbb_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
            float _Float_4e6007641ec04fc09c2b2667999054c6_Out_0 = 0.07;
            float _Multiply_b50e727a155340708816fcdecae86534_Out_2;
            Unity_Multiply_float(0, _Float_4e6007641ec04fc09c2b2667999054c6_Out_0, _Multiply_b50e727a155340708816fcdecae86534_Out_2);
            float4 _UV_adf164f4eb0c448c91e97f9ce69a6e03_Out_0 = IN.uv0;
            float4 _Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2;
            Unity_Add_float4((_Multiply_b50e727a155340708816fcdecae86534_Out_2.xxxx), _UV_adf164f4eb0c448c91e97f9ce69a6e03_Out_0, _Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2);
            float _Float_b1fb926f1b624976b48efa34c9873587_Out_0 = 7.35;
            float4 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGBA_4;
            float3 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGB_5;
            float2 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6;
            Unity_Combine_float(_Float_b1fb926f1b624976b48efa34c9873587_Out_0, _Float_b1fb926f1b624976b48efa34c9873587_Out_0, 0, 0, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGBA_4, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGB_5, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6);
            float2 _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2;
            Unity_Multiply_float((_Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2.xy), _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6, _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2);
            float4 _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9f4588603daa48c29cd7283e33d08cbb_Out_0.tex, _Property_9f4588603daa48c29cd7283e33d08cbb_Out_0.samplerstate, _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2);
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_R_4 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.r;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_G_5 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.g;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_B_6 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.b;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_A_7 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.a;
            float _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3;
            float _Voronoi_182acf2c1d12452b804b1178617f9171_Cells_4;
            Unity_Voronoi_float(IN.uv0.xy, 5.52, 8.21, _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3, _Voronoi_182acf2c1d12452b804b1178617f9171_Cells_4);
            float _Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3;
            Unity_Smoothstep_float(0.12, 0.83, _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3, _Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3);
            float4 Color_93959687abb64037a82971ddccb58ecf = IsGammaSpace() ? LinearToSRGB(float4(0, 0.8589136, 1.498039, 0)) : float4(0, 0.8589136, 1.498039, 0);
            float4 _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2;
            Unity_Multiply_float((_Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3.xxxx), Color_93959687abb64037a82971ddccb58ecf, _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2);
            UnityTexture2D _Property_caa9b98afd86481cb32af9287161a199_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
            float4 _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0 = SAMPLE_TEXTURE2D(_Property_caa9b98afd86481cb32af9287161a199_Out_0.tex, _Property_caa9b98afd86481cb32af9287161a199_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.r;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_G_5 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.g;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_B_6 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.b;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_A_7 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.a;
            float4 _Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3;
            Unity_Lerp_float4(_SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0, _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0, _Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3);
            float _Property_88c5723aa30f430dbcc4feddc549011d_Out_0 = Vector1_c59bce75c00040b2866e6bf70aa8b671;
            float _Float_6a2651b3179d4aeb9ba2d1ae4b865e02_Out_0 = _Property_88c5723aa30f430dbcc4feddc549011d_Out_0;
            float _FresnelEffect_1167d26bace14f0f938c6714d031cc0e_Out_3;
            Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Float_6a2651b3179d4aeb9ba2d1ae4b865e02_Out_0, _FresnelEffect_1167d26bace14f0f938c6714d031cc0e_Out_3);
            float4 Color_4a9fe31cf860438ba5275bb79d68d2ac = IsGammaSpace() ? LinearToSRGB(float4(0.25, 0.425024, 1, 0)) : float4(0.25, 0.425024, 1, 0);
            float4 _Multiply_3ac2cca204a544b6a7740fd8ef35635f_Out_2;
            Unity_Multiply_float((_FresnelEffect_1167d26bace14f0f938c6714d031cc0e_Out_3.xxxx), Color_4a9fe31cf860438ba5275bb79d68d2ac, _Multiply_3ac2cca204a544b6a7740fd8ef35635f_Out_2);
            float _Float_6747dd8afcbd4ae19f2677bb729ab7bc_Out_0 = 0.54;
            float4 _Power_f73caffcbf4f4d97b05f08d8561e5e71_Out_2;
            Unity_Power_float4(_Multiply_3ac2cca204a544b6a7740fd8ef35635f_Out_2, (_Float_6747dd8afcbd4ae19f2677bb729ab7bc_Out_0.xxxx), _Power_f73caffcbf4f4d97b05f08d8561e5e71_Out_2);
            float _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0 = Vector1_80773e26b92d42a291521a504eba9a22;
            float _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            Unity_Lerp_float(1, _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4, _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3);
            surface.BaseColor = (_Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3.xyz);
            surface.Emission = (_Power_f73caffcbf4f4d97b05f08d8561e5e71_Out_2.xyz);
            surface.Alpha = _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

        	// must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
        	float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);


            output.WorldSpaceNormal =            renormFactor*input.normalWS.xyz;		// we want a unit length Normal Vector node in shader graph


            output.WorldSpaceViewDirection =     input.viewDirectionWS; //TODO: by default normalized in HD, but not in universal
            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            // Name: <None>
            Tags
            {
                "LightMode" = "Universal2D"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_2D
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float4 interp0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca_TexelSize;
        float Vector1_c59bce75c00040b2866e6bf70aa8b671;
        float4 Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4_TexelSize;
        float4 Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa_TexelSize;
        float Vector1_80773e26b92d42a291521a504eba9a22;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        SAMPLER(samplerTexture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        TEXTURE2D(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        SAMPLER(samplerTexture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        TEXTURE2D(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
        SAMPLER(samplerTexture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }

        void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }


        inline float2 Unity_Voronoi_RandomVector_float (float2 UV, float offset)
        {
            float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
            UV = frac(sin(mul(UV, m)));
            return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
        }

        void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
        {
            float2 g = floor(UV * CellDensity);
            float2 f = frac(UV * CellDensity);
            float t = 8.0;
            float3 res = float3(8.0, 0.0, 0.0);

            for(int y=-1; y<=1; y++)
            {
                for(int x=-1; x<=1; x++)
                {
                    float2 lattice = float2(x,y);
                    float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
                    float d = distance(lattice + offset, f);

                    if(d < res.x)
                    {
                        res = float3(d, offset.x, offset.y);
                        Out = res.x;
                        Cells = res.y;
                    }
                }
            }
        }

        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_9f4588603daa48c29cd7283e33d08cbb_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
            float _Float_4e6007641ec04fc09c2b2667999054c6_Out_0 = 0.07;
            float _Multiply_b50e727a155340708816fcdecae86534_Out_2;
            Unity_Multiply_float(0, _Float_4e6007641ec04fc09c2b2667999054c6_Out_0, _Multiply_b50e727a155340708816fcdecae86534_Out_2);
            float4 _UV_adf164f4eb0c448c91e97f9ce69a6e03_Out_0 = IN.uv0;
            float4 _Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2;
            Unity_Add_float4((_Multiply_b50e727a155340708816fcdecae86534_Out_2.xxxx), _UV_adf164f4eb0c448c91e97f9ce69a6e03_Out_0, _Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2);
            float _Float_b1fb926f1b624976b48efa34c9873587_Out_0 = 7.35;
            float4 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGBA_4;
            float3 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGB_5;
            float2 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6;
            Unity_Combine_float(_Float_b1fb926f1b624976b48efa34c9873587_Out_0, _Float_b1fb926f1b624976b48efa34c9873587_Out_0, 0, 0, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGBA_4, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGB_5, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6);
            float2 _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2;
            Unity_Multiply_float((_Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2.xy), _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6, _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2);
            float4 _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9f4588603daa48c29cd7283e33d08cbb_Out_0.tex, _Property_9f4588603daa48c29cd7283e33d08cbb_Out_0.samplerstate, _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2);
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_R_4 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.r;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_G_5 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.g;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_B_6 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.b;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_A_7 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.a;
            float _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3;
            float _Voronoi_182acf2c1d12452b804b1178617f9171_Cells_4;
            Unity_Voronoi_float(IN.uv0.xy, 5.52, 8.21, _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3, _Voronoi_182acf2c1d12452b804b1178617f9171_Cells_4);
            float _Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3;
            Unity_Smoothstep_float(0.12, 0.83, _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3, _Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3);
            float4 Color_93959687abb64037a82971ddccb58ecf = IsGammaSpace() ? LinearToSRGB(float4(0, 0.8589136, 1.498039, 0)) : float4(0, 0.8589136, 1.498039, 0);
            float4 _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2;
            Unity_Multiply_float((_Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3.xxxx), Color_93959687abb64037a82971ddccb58ecf, _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2);
            UnityTexture2D _Property_caa9b98afd86481cb32af9287161a199_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
            float4 _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0 = SAMPLE_TEXTURE2D(_Property_caa9b98afd86481cb32af9287161a199_Out_0.tex, _Property_caa9b98afd86481cb32af9287161a199_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.r;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_G_5 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.g;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_B_6 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.b;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_A_7 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.a;
            float4 _Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3;
            Unity_Lerp_float4(_SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0, _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0, _Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3);
            float _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0 = Vector1_80773e26b92d42a291521a504eba9a22;
            float _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            Unity_Lerp_float(1, _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4, _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3);
            surface.BaseColor = (_Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3.xyz);
            surface.Alpha = _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"

            ENDHLSL
        }
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_FORWARD
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float3 normalWS;
            float4 tangentWS;
            float4 texCoord0;
            float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            float2 lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            float3 sh;
            #endif
            float4 fogFactorAndVertexLight;
            float4 shadowCoord;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 WorldSpaceNormal;
            float3 TangentSpaceNormal;
            float3 WorldSpaceViewDirection;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float3 interp1 : TEXCOORD1;
            float4 interp2 : TEXCOORD2;
            float4 interp3 : TEXCOORD3;
            float3 interp4 : TEXCOORD4;
            #if defined(LIGHTMAP_ON)
            float2 interp5 : TEXCOORD5;
            #endif
            #if !defined(LIGHTMAP_ON)
            float3 interp6 : TEXCOORD6;
            #endif
            float4 interp7 : TEXCOORD7;
            float4 interp8 : TEXCOORD8;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
            output.interp3.xyzw =  input.texCoord0;
            output.interp4.xyz =  input.viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            output.interp5.xy =  input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.interp6.xyz =  input.sh;
            #endif
            output.interp7.xyzw =  input.fogFactorAndVertexLight;
            output.interp8.xyzw =  input.shadowCoord;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
            output.texCoord0 = input.interp3.xyzw;
            output.viewDirectionWS = input.interp4.xyz;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.interp5.xy;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.interp6.xyz;
            #endif
            output.fogFactorAndVertexLight = input.interp7.xyzw;
            output.shadowCoord = input.interp8.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca_TexelSize;
        float Vector1_c59bce75c00040b2866e6bf70aa8b671;
        float4 Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4_TexelSize;
        float4 Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa_TexelSize;
        float Vector1_80773e26b92d42a291521a504eba9a22;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        SAMPLER(samplerTexture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        TEXTURE2D(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        SAMPLER(samplerTexture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        TEXTURE2D(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
        SAMPLER(samplerTexture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }

        void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }


        inline float2 Unity_Voronoi_RandomVector_float (float2 UV, float offset)
        {
            float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
            UV = frac(sin(mul(UV, m)));
            return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
        }

        void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
        {
            float2 g = floor(UV * CellDensity);
            float2 f = frac(UV * CellDensity);
            float t = 8.0;
            float3 res = float3(8.0, 0.0, 0.0);

            for(int y=-1; y<=1; y++)
            {
                for(int x=-1; x<=1; x++)
                {
                    float2 lattice = float2(x,y);
                    float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
                    float d = distance(lattice + offset, f);

                    if(d < res.x)
                    {
                        res = float3(d, offset.x, offset.y);
                        Out = res.x;
                        Cells = res.y;
                    }
                }
            }
        }

        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
        {
            Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
        }

        void Unity_Power_float4(float4 A, float4 B, out float4 Out)
        {
            Out = pow(A, B);
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_9f4588603daa48c29cd7283e33d08cbb_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
            float _Float_4e6007641ec04fc09c2b2667999054c6_Out_0 = 0.07;
            float _Multiply_b50e727a155340708816fcdecae86534_Out_2;
            Unity_Multiply_float(0, _Float_4e6007641ec04fc09c2b2667999054c6_Out_0, _Multiply_b50e727a155340708816fcdecae86534_Out_2);
            float4 _UV_adf164f4eb0c448c91e97f9ce69a6e03_Out_0 = IN.uv0;
            float4 _Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2;
            Unity_Add_float4((_Multiply_b50e727a155340708816fcdecae86534_Out_2.xxxx), _UV_adf164f4eb0c448c91e97f9ce69a6e03_Out_0, _Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2);
            float _Float_b1fb926f1b624976b48efa34c9873587_Out_0 = 7.35;
            float4 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGBA_4;
            float3 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGB_5;
            float2 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6;
            Unity_Combine_float(_Float_b1fb926f1b624976b48efa34c9873587_Out_0, _Float_b1fb926f1b624976b48efa34c9873587_Out_0, 0, 0, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGBA_4, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGB_5, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6);
            float2 _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2;
            Unity_Multiply_float((_Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2.xy), _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6, _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2);
            float4 _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9f4588603daa48c29cd7283e33d08cbb_Out_0.tex, _Property_9f4588603daa48c29cd7283e33d08cbb_Out_0.samplerstate, _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2);
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_R_4 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.r;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_G_5 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.g;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_B_6 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.b;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_A_7 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.a;
            float _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3;
            float _Voronoi_182acf2c1d12452b804b1178617f9171_Cells_4;
            Unity_Voronoi_float(IN.uv0.xy, 5.52, 8.21, _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3, _Voronoi_182acf2c1d12452b804b1178617f9171_Cells_4);
            float _Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3;
            Unity_Smoothstep_float(0.12, 0.83, _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3, _Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3);
            float4 Color_93959687abb64037a82971ddccb58ecf = IsGammaSpace() ? LinearToSRGB(float4(0, 0.8589136, 1.498039, 0)) : float4(0, 0.8589136, 1.498039, 0);
            float4 _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2;
            Unity_Multiply_float((_Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3.xxxx), Color_93959687abb64037a82971ddccb58ecf, _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2);
            UnityTexture2D _Property_caa9b98afd86481cb32af9287161a199_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
            float4 _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0 = SAMPLE_TEXTURE2D(_Property_caa9b98afd86481cb32af9287161a199_Out_0.tex, _Property_caa9b98afd86481cb32af9287161a199_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.r;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_G_5 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.g;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_B_6 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.b;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_A_7 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.a;
            float4 _Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3;
            Unity_Lerp_float4(_SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0, _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0, _Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3);
            UnityTexture2D _Property_33e382b27f964d48be83138a780bf2c8_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
            float4 _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0 = SAMPLE_TEXTURE2D(_Property_33e382b27f964d48be83138a780bf2c8_Out_0.tex, _Property_33e382b27f964d48be83138a780bf2c8_Out_0.samplerstate, IN.uv0.xy);
            _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.rgb = UnpackNormal(_SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0);
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_R_4 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.r;
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_G_5 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.g;
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_B_6 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.b;
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_A_7 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.a;
            float _Property_88c5723aa30f430dbcc4feddc549011d_Out_0 = Vector1_c59bce75c00040b2866e6bf70aa8b671;
            float _Float_6a2651b3179d4aeb9ba2d1ae4b865e02_Out_0 = _Property_88c5723aa30f430dbcc4feddc549011d_Out_0;
            float _FresnelEffect_1167d26bace14f0f938c6714d031cc0e_Out_3;
            Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Float_6a2651b3179d4aeb9ba2d1ae4b865e02_Out_0, _FresnelEffect_1167d26bace14f0f938c6714d031cc0e_Out_3);
            float4 Color_4a9fe31cf860438ba5275bb79d68d2ac = IsGammaSpace() ? LinearToSRGB(float4(0.25, 0.425024, 1, 0)) : float4(0.25, 0.425024, 1, 0);
            float4 _Multiply_3ac2cca204a544b6a7740fd8ef35635f_Out_2;
            Unity_Multiply_float((_FresnelEffect_1167d26bace14f0f938c6714d031cc0e_Out_3.xxxx), Color_4a9fe31cf860438ba5275bb79d68d2ac, _Multiply_3ac2cca204a544b6a7740fd8ef35635f_Out_2);
            float _Float_6747dd8afcbd4ae19f2677bb729ab7bc_Out_0 = 0.54;
            float4 _Power_f73caffcbf4f4d97b05f08d8561e5e71_Out_2;
            Unity_Power_float4(_Multiply_3ac2cca204a544b6a7740fd8ef35635f_Out_2, (_Float_6747dd8afcbd4ae19f2677bb729ab7bc_Out_0.xxxx), _Power_f73caffcbf4f4d97b05f08d8561e5e71_Out_2);
            float _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0 = Vector1_80773e26b92d42a291521a504eba9a22;
            float _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            Unity_Lerp_float(1, _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4, _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3);
            surface.BaseColor = (_Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3.xyz);
            surface.NormalTS = (_SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.xyz);
            surface.Emission = (_Power_f73caffcbf4f4d97b05f08d8561e5e71_Out_2.xyz);
            surface.Metallic = 0;
            surface.Smoothness = 0.5;
            surface.Occlusion = 1;
            surface.Alpha = _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

        	// must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
        	float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);


            output.WorldSpaceNormal =            renormFactor*input.normalWS.xyz;		// we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);


            output.WorldSpaceViewDirection =     input.viewDirectionWS; //TODO: by default normalized in HD, but not in universal
            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SHADOWCASTER
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 normalWS;
            float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.normalWS;
            output.interp1.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.interp0.xyz;
            output.texCoord0 = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca_TexelSize;
        float Vector1_c59bce75c00040b2866e6bf70aa8b671;
        float4 Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4_TexelSize;
        float4 Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa_TexelSize;
        float Vector1_80773e26b92d42a291521a504eba9a22;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        SAMPLER(samplerTexture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        TEXTURE2D(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        SAMPLER(samplerTexture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        TEXTURE2D(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
        SAMPLER(samplerTexture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);

            // Graph Functions
            
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0 = Vector1_80773e26b92d42a291521a504eba9a22;
            UnityTexture2D _Property_caa9b98afd86481cb32af9287161a199_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
            float4 _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0 = SAMPLE_TEXTURE2D(_Property_caa9b98afd86481cb32af9287161a199_Out_0.tex, _Property_caa9b98afd86481cb32af9287161a199_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.r;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_G_5 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.g;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_B_6 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.b;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_A_7 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.a;
            float _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            Unity_Lerp_float(1, _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4, _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3);
            surface.Alpha = _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHONLY
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float4 interp0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca_TexelSize;
        float Vector1_c59bce75c00040b2866e6bf70aa8b671;
        float4 Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4_TexelSize;
        float4 Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa_TexelSize;
        float Vector1_80773e26b92d42a291521a504eba9a22;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        SAMPLER(samplerTexture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        TEXTURE2D(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        SAMPLER(samplerTexture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        TEXTURE2D(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
        SAMPLER(samplerTexture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);

            // Graph Functions
            
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0 = Vector1_80773e26b92d42a291521a504eba9a22;
            UnityTexture2D _Property_caa9b98afd86481cb32af9287161a199_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
            float4 _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0 = SAMPLE_TEXTURE2D(_Property_caa9b98afd86481cb32af9287161a199_Out_0.tex, _Property_caa9b98afd86481cb32af9287161a199_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.r;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_G_5 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.g;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_B_6 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.b;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_A_7 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.a;
            float _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            Unity_Lerp_float(1, _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4, _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3);
            surface.Alpha = _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 normalWS;
            float4 tangentWS;
            float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 TangentSpaceNormal;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            float4 interp2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.normalWS;
            output.interp1.xyzw =  input.tangentWS;
            output.interp2.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.interp0.xyz;
            output.tangentWS = input.interp1.xyzw;
            output.texCoord0 = input.interp2.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca_TexelSize;
        float Vector1_c59bce75c00040b2866e6bf70aa8b671;
        float4 Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4_TexelSize;
        float4 Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa_TexelSize;
        float Vector1_80773e26b92d42a291521a504eba9a22;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        SAMPLER(samplerTexture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        TEXTURE2D(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        SAMPLER(samplerTexture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        TEXTURE2D(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
        SAMPLER(samplerTexture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);

            // Graph Functions
            
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 NormalTS;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_33e382b27f964d48be83138a780bf2c8_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
            float4 _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0 = SAMPLE_TEXTURE2D(_Property_33e382b27f964d48be83138a780bf2c8_Out_0.tex, _Property_33e382b27f964d48be83138a780bf2c8_Out_0.samplerstate, IN.uv0.xy);
            _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.rgb = UnpackNormal(_SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0);
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_R_4 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.r;
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_G_5 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.g;
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_B_6 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.b;
            float _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_A_7 = _SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.a;
            float _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0 = Vector1_80773e26b92d42a291521a504eba9a22;
            UnityTexture2D _Property_caa9b98afd86481cb32af9287161a199_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
            float4 _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0 = SAMPLE_TEXTURE2D(_Property_caa9b98afd86481cb32af9287161a199_Out_0.tex, _Property_caa9b98afd86481cb32af9287161a199_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.r;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_G_5 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.g;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_B_6 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.b;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_A_7 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.a;
            float _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            Unity_Lerp_float(1, _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4, _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3);
            surface.NormalTS = (_SampleTexture2D_c1d5204efd664a8a952b5b4ec80c7698_RGBA_0.xyz);
            surface.Alpha = _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



            output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);


            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }

            // Render State
            Cull Off

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_TEXCOORD2
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_META
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            float4 uv1 : TEXCOORD1;
            float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 normalWS;
            float4 texCoord0;
            float3 viewDirectionWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 WorldSpaceNormal;
            float3 WorldSpaceViewDirection;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            float3 interp2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.normalWS;
            output.interp1.xyzw =  input.texCoord0;
            output.interp2.xyz =  input.viewDirectionWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.interp0.xyz;
            output.texCoord0 = input.interp1.xyzw;
            output.viewDirectionWS = input.interp2.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca_TexelSize;
        float Vector1_c59bce75c00040b2866e6bf70aa8b671;
        float4 Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4_TexelSize;
        float4 Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa_TexelSize;
        float Vector1_80773e26b92d42a291521a504eba9a22;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        SAMPLER(samplerTexture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        TEXTURE2D(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        SAMPLER(samplerTexture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        TEXTURE2D(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
        SAMPLER(samplerTexture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }

        void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }


        inline float2 Unity_Voronoi_RandomVector_float (float2 UV, float offset)
        {
            float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
            UV = frac(sin(mul(UV, m)));
            return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
        }

        void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
        {
            float2 g = floor(UV * CellDensity);
            float2 f = frac(UV * CellDensity);
            float t = 8.0;
            float3 res = float3(8.0, 0.0, 0.0);

            for(int y=-1; y<=1; y++)
            {
                for(int x=-1; x<=1; x++)
                {
                    float2 lattice = float2(x,y);
                    float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
                    float d = distance(lattice + offset, f);

                    if(d < res.x)
                    {
                        res = float3(d, offset.x, offset.y);
                        Out = res.x;
                        Cells = res.y;
                    }
                }
            }
        }

        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
        {
            Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
        }

        void Unity_Power_float4(float4 A, float4 B, out float4 Out)
        {
            Out = pow(A, B);
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float3 Emission;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_9f4588603daa48c29cd7283e33d08cbb_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
            float _Float_4e6007641ec04fc09c2b2667999054c6_Out_0 = 0.07;
            float _Multiply_b50e727a155340708816fcdecae86534_Out_2;
            Unity_Multiply_float(0, _Float_4e6007641ec04fc09c2b2667999054c6_Out_0, _Multiply_b50e727a155340708816fcdecae86534_Out_2);
            float4 _UV_adf164f4eb0c448c91e97f9ce69a6e03_Out_0 = IN.uv0;
            float4 _Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2;
            Unity_Add_float4((_Multiply_b50e727a155340708816fcdecae86534_Out_2.xxxx), _UV_adf164f4eb0c448c91e97f9ce69a6e03_Out_0, _Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2);
            float _Float_b1fb926f1b624976b48efa34c9873587_Out_0 = 7.35;
            float4 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGBA_4;
            float3 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGB_5;
            float2 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6;
            Unity_Combine_float(_Float_b1fb926f1b624976b48efa34c9873587_Out_0, _Float_b1fb926f1b624976b48efa34c9873587_Out_0, 0, 0, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGBA_4, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGB_5, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6);
            float2 _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2;
            Unity_Multiply_float((_Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2.xy), _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6, _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2);
            float4 _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9f4588603daa48c29cd7283e33d08cbb_Out_0.tex, _Property_9f4588603daa48c29cd7283e33d08cbb_Out_0.samplerstate, _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2);
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_R_4 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.r;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_G_5 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.g;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_B_6 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.b;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_A_7 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.a;
            float _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3;
            float _Voronoi_182acf2c1d12452b804b1178617f9171_Cells_4;
            Unity_Voronoi_float(IN.uv0.xy, 5.52, 8.21, _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3, _Voronoi_182acf2c1d12452b804b1178617f9171_Cells_4);
            float _Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3;
            Unity_Smoothstep_float(0.12, 0.83, _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3, _Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3);
            float4 Color_93959687abb64037a82971ddccb58ecf = IsGammaSpace() ? LinearToSRGB(float4(0, 0.8589136, 1.498039, 0)) : float4(0, 0.8589136, 1.498039, 0);
            float4 _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2;
            Unity_Multiply_float((_Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3.xxxx), Color_93959687abb64037a82971ddccb58ecf, _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2);
            UnityTexture2D _Property_caa9b98afd86481cb32af9287161a199_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
            float4 _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0 = SAMPLE_TEXTURE2D(_Property_caa9b98afd86481cb32af9287161a199_Out_0.tex, _Property_caa9b98afd86481cb32af9287161a199_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.r;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_G_5 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.g;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_B_6 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.b;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_A_7 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.a;
            float4 _Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3;
            Unity_Lerp_float4(_SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0, _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0, _Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3);
            float _Property_88c5723aa30f430dbcc4feddc549011d_Out_0 = Vector1_c59bce75c00040b2866e6bf70aa8b671;
            float _Float_6a2651b3179d4aeb9ba2d1ae4b865e02_Out_0 = _Property_88c5723aa30f430dbcc4feddc549011d_Out_0;
            float _FresnelEffect_1167d26bace14f0f938c6714d031cc0e_Out_3;
            Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Float_6a2651b3179d4aeb9ba2d1ae4b865e02_Out_0, _FresnelEffect_1167d26bace14f0f938c6714d031cc0e_Out_3);
            float4 Color_4a9fe31cf860438ba5275bb79d68d2ac = IsGammaSpace() ? LinearToSRGB(float4(0.25, 0.425024, 1, 0)) : float4(0.25, 0.425024, 1, 0);
            float4 _Multiply_3ac2cca204a544b6a7740fd8ef35635f_Out_2;
            Unity_Multiply_float((_FresnelEffect_1167d26bace14f0f938c6714d031cc0e_Out_3.xxxx), Color_4a9fe31cf860438ba5275bb79d68d2ac, _Multiply_3ac2cca204a544b6a7740fd8ef35635f_Out_2);
            float _Float_6747dd8afcbd4ae19f2677bb729ab7bc_Out_0 = 0.54;
            float4 _Power_f73caffcbf4f4d97b05f08d8561e5e71_Out_2;
            Unity_Power_float4(_Multiply_3ac2cca204a544b6a7740fd8ef35635f_Out_2, (_Float_6747dd8afcbd4ae19f2677bb729ab7bc_Out_0.xxxx), _Power_f73caffcbf4f4d97b05f08d8561e5e71_Out_2);
            float _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0 = Vector1_80773e26b92d42a291521a504eba9a22;
            float _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            Unity_Lerp_float(1, _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4, _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3);
            surface.BaseColor = (_Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3.xyz);
            surface.Emission = (_Power_f73caffcbf4f4d97b05f08d8561e5e71_Out_2.xyz);
            surface.Alpha = _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

        	// must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
        	float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);


            output.WorldSpaceNormal =            renormFactor*input.normalWS.xyz;		// we want a unit length Normal Vector node in shader graph


            output.WorldSpaceViewDirection =     input.viewDirectionWS; //TODO: by default normalized in HD, but not in universal
            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            // Name: <None>
            Tags
            {
                "LightMode" = "Universal2D"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_2D
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float4 interp0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca_TexelSize;
        float Vector1_c59bce75c00040b2866e6bf70aa8b671;
        float4 Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4_TexelSize;
        float4 Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa_TexelSize;
        float Vector1_80773e26b92d42a291521a504eba9a22;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        SAMPLER(samplerTexture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
        TEXTURE2D(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        SAMPLER(samplerTexture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
        TEXTURE2D(Texture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);
        SAMPLER(samplerTexture2D_217b1e16b4194f07ad1c0b3b28cfa6fa);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }

        void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }


        inline float2 Unity_Voronoi_RandomVector_float (float2 UV, float offset)
        {
            float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
            UV = frac(sin(mul(UV, m)));
            return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
        }

        void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
        {
            float2 g = floor(UV * CellDensity);
            float2 f = frac(UV * CellDensity);
            float t = 8.0;
            float3 res = float3(8.0, 0.0, 0.0);

            for(int y=-1; y<=1; y++)
            {
                for(int x=-1; x<=1; x++)
                {
                    float2 lattice = float2(x,y);
                    float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
                    float d = distance(lattice + offset, f);

                    if(d < res.x)
                    {
                        res = float3(d, offset.x, offset.y);
                        Out = res.x;
                        Cells = res.y;
                    }
                }
            }
        }

        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_9f4588603daa48c29cd7283e33d08cbb_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_dd8afbdb8cee4d6db706ffae7f8519ca);
            float _Float_4e6007641ec04fc09c2b2667999054c6_Out_0 = 0.07;
            float _Multiply_b50e727a155340708816fcdecae86534_Out_2;
            Unity_Multiply_float(0, _Float_4e6007641ec04fc09c2b2667999054c6_Out_0, _Multiply_b50e727a155340708816fcdecae86534_Out_2);
            float4 _UV_adf164f4eb0c448c91e97f9ce69a6e03_Out_0 = IN.uv0;
            float4 _Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2;
            Unity_Add_float4((_Multiply_b50e727a155340708816fcdecae86534_Out_2.xxxx), _UV_adf164f4eb0c448c91e97f9ce69a6e03_Out_0, _Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2);
            float _Float_b1fb926f1b624976b48efa34c9873587_Out_0 = 7.35;
            float4 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGBA_4;
            float3 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGB_5;
            float2 _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6;
            Unity_Combine_float(_Float_b1fb926f1b624976b48efa34c9873587_Out_0, _Float_b1fb926f1b624976b48efa34c9873587_Out_0, 0, 0, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGBA_4, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RGB_5, _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6);
            float2 _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2;
            Unity_Multiply_float((_Add_9dcc4a7ee35c419485f9e2ff2e7b73de_Out_2.xy), _Combine_cd1599497abb45e7bb0f2b97753ad3ab_RG_6, _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2);
            float4 _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9f4588603daa48c29cd7283e33d08cbb_Out_0.tex, _Property_9f4588603daa48c29cd7283e33d08cbb_Out_0.samplerstate, _Multiply_d6cb0b9c889943c7a29aad58b2513f03_Out_2);
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_R_4 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.r;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_G_5 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.g;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_B_6 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.b;
            float _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_A_7 = _SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0.a;
            float _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3;
            float _Voronoi_182acf2c1d12452b804b1178617f9171_Cells_4;
            Unity_Voronoi_float(IN.uv0.xy, 5.52, 8.21, _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3, _Voronoi_182acf2c1d12452b804b1178617f9171_Cells_4);
            float _Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3;
            Unity_Smoothstep_float(0.12, 0.83, _Voronoi_182acf2c1d12452b804b1178617f9171_Out_3, _Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3);
            float4 Color_93959687abb64037a82971ddccb58ecf = IsGammaSpace() ? LinearToSRGB(float4(0, 0.8589136, 1.498039, 0)) : float4(0, 0.8589136, 1.498039, 0);
            float4 _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2;
            Unity_Multiply_float((_Smoothstep_a5ff1de68b9945819abb154e93fd1068_Out_3.xxxx), Color_93959687abb64037a82971ddccb58ecf, _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2);
            UnityTexture2D _Property_caa9b98afd86481cb32af9287161a199_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_64aa07abf55d4f6e804fffb1f6c3fbe4);
            float4 _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0 = SAMPLE_TEXTURE2D(_Property_caa9b98afd86481cb32af9287161a199_Out_0.tex, _Property_caa9b98afd86481cb32af9287161a199_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.r;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_G_5 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.g;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_B_6 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.b;
            float _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_A_7 = _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0.a;
            float4 _Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3;
            Unity_Lerp_float4(_SampleTexture2D_a8c30a0dd3984407801d2040959a1cb3_RGBA_0, _Multiply_0c6f36b3184944a5899374ba53d1c0dc_Out_2, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_RGBA_0, _Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3);
            float _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0 = Vector1_80773e26b92d42a291521a504eba9a22;
            float _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            Unity_Lerp_float(1, _Property_9b1d43e0d322407192a1a68ef5e1145c_Out_0, _SampleTexture2D_7b8baaa1c29a4d14b0e61fe47acf5c03_R_4, _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3);
            surface.BaseColor = (_Lerp_b0bee16ea2304140b3a0cb201fe1b986_Out_3.xyz);
            surface.Alpha = _Lerp_e63143ebc80346f6832f6f6cfd801adf_Out_3;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"

            ENDHLSL
        }
    }
    CustomEditor "ShaderGraph.PBRMasterGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}