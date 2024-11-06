Shader "bubble"
{
    Properties
    {
        [NoScaleOffset]Texture2D_fc19be8c381e40e79af53fac1eb60dc4("Texture", 2D) = "white" {}
        Vector1_0f74eb3108994472ab1520e422afcda5("Texth=ure Speed", Float) = 0.5
        [HDR]Color_8d6013e20be440b18ef88aab5ba53339("Color texture", Color) = (0.3504361, 0.9717637, 0.990566, 0)
        Vector1_253e63c1c178480385125f8ed67ed240("Surface movement speed", Float) = 0.1
        Vector1_79c4e17149f84e29bd0c6f7f584bb560("noise scale", Float) = 0.5
        Vector1_6a079c2d4fae4204986b6b8f277fd7f7("distortion speed", Float) = 0.1
        Vector1_bf2c758a07f64df5b4cc43ebeead739e("Border size", Float) = 2
        [HDR]Color_2d78f1c40ea749229055731cd5a17ceb("border color", Color) = (0.25, 0.425024, 1, 0)
        Vector1_b57099da0f604234b785d35982d3db5a("border power", Float) = 0.54
        Vector1_4f9b47792cda4f22a14538bd7ad50123("smoothness", Float) = 3
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
        #define REQUIRE_OPAQUE_TEXTURE
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
            float3 WorldSpaceTangent;
            float3 WorldSpaceBiTangent;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 uv0;
            float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 WorldSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float3 TimeParameters;
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
        float4 Texture2D_fc19be8c381e40e79af53fac1eb60dc4_TexelSize;
        float Vector1_0f74eb3108994472ab1520e422afcda5;
        float4 Color_8d6013e20be440b18ef88aab5ba53339;
        float Vector1_253e63c1c178480385125f8ed67ed240;
        float Vector1_79c4e17149f84e29bd0c6f7f584bb560;
        float Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
        float Vector1_bf2c758a07f64df5b4cc43ebeead739e;
        float4 Color_2d78f1c40ea749229055731cd5a17ceb;
        float Vector1_b57099da0f604234b785d35982d3db5a;
        float Vector1_4f9b47792cda4f22a14538bd7ad50123;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
        SAMPLER(samplerTexture2D_fc19be8c381e40e79af53fac1eb60dc4);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }


        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
        }

        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }


        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);

            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);

            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;

            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            Out = t;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Twirl_float(float2 UV, float2 Center, float Strength, float2 Offset, out float2 Out)
        {
            float2 delta = UV - Center;
            float angle = Strength * length(delta);
            float x = cos(angle) * delta.x - sin(angle) * delta.y;
            float y = sin(angle) * delta.x + cos(angle) * delta.y;
            Out = float2(x + Center.x + Offset.x, y + Center.y + Offset.y);
        }


        float2 Unity_GradientNoise_Dir_float(float2 p)
        {
            // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
            p = p % 289;
            // need full precision, otherwise half overflows when p > 1
            float x = float(34 * p.x + 1) * p.x % 289 + p.y;
            x = (34 * x + 1) * x % 289;
            x = frac(x / 41) * 2 - 1;
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }

        void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
        { 
            float2 p = UV * Scale;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }

        void Unity_NormalFromHeight_Tangent_float(float In, float Strength, float3 Position, float3x3 TangentMatrix, out float3 Out)
        {
            float3 worldDerivativeX = ddx(Position);
            float3 worldDerivativeY = ddy(Position);

            float3 crossX = cross(TangentMatrix[2].xyz, worldDerivativeX);
            float3 crossY = cross(worldDerivativeY, TangentMatrix[2].xyz);
            float d = dot(worldDerivativeX, crossY);
            float sgn = d < 0.0 ? (-1.0f) : 1.0f;
            float surface = sgn / max(0.000000000000001192093f, abs(d));

            float dHdx = ddx(In);
            float dHdy = ddy(In);
            float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
            Out = SafeNormalize(TangentMatrix[2].xyz - (Strength * surfGrad));
            Out = TransformWorldToTangent(Out, TangentMatrix);
        }

        void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
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
            float _Property_3d54fd98a65941ff860977ace1c646fa_Out_0 = Vector1_253e63c1c178480385125f8ed67ed240;
            float _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_3d54fd98a65941ff860977ace1c646fa_Out_0, _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2);
            float3 _Add_815fddedf3c24649af80cc6b8618e81e_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_e5c6132717e64533ab58b561e437e52b_Out_2.xxx), _Add_815fddedf3c24649af80cc6b8618e81e_Out_2);
            float _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2;
            Unity_SimpleNoise_float((_Add_815fddedf3c24649af80cc6b8618e81e_Out_2.xy), 10.28, _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2);
            float _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0 = Vector1_79c4e17149f84e29bd0c6f7f584bb560;
            float _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2;
            Unity_Multiply_float(_SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2, _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0, _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2);
            float3 _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2;
            Unity_Multiply_float(IN.WorldSpaceNormal, (_Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2.xxx), _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2);
            float3 _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2, _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2);
            description.Position = _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
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
            UnityTexture2D _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
            float _Property_bceb82901b7c4f4b9f2332ed8c2447c4_Out_0 = Vector1_0f74eb3108994472ab1520e422afcda5;
            float _Multiply_3350ef5f73a04b828638600703a4cdba_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_bceb82901b7c4f4b9f2332ed8c2447c4_Out_0, _Multiply_3350ef5f73a04b828638600703a4cdba_Out_2);
            float2 _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_3350ef5f73a04b828638600703a4cdba_Out_2.xx), _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3);
            float4 _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0 = SAMPLE_TEXTURE2D(_Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.tex, _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.samplerstate, _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3);
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_R_4 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.r;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_G_5 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.g;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_B_6 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.b;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_A_7 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.a;
            float _OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1;
            Unity_OneMinus_float(_Multiply_3350ef5f73a04b828638600703a4cdba_Out_2, _OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1);
            float2 _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (3, 3), (_OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1.xx), _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3);
            float4 _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0 = SAMPLE_TEXTURE2D(_Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.tex, _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.samplerstate, _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3);
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_R_4 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.r;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_G_5 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.g;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_B_6 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.b;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_A_7 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.a;
            float4 _Add_c968d8ffb24640dba87de4031f77ce5c_Out_2;
            Unity_Add_float4(_SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0, _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0, _Add_c968d8ffb24640dba87de4031f77ce5c_Out_2);
            float4 _Property_c8a13dbad4de4568886b5ee49562997a_Out_0 = IsGammaSpace() ? LinearToSRGB(Color_8d6013e20be440b18ef88aab5ba53339) : Color_8d6013e20be440b18ef88aab5ba53339;
            float4 _Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2;
            Unity_Multiply_float(_Add_c968d8ffb24640dba87de4031f77ce5c_Out_2, _Property_c8a13dbad4de4568886b5ee49562997a_Out_0, _Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2);
            float4 _ScreenPosition_a78e4614011e4b74b98b8d30703a284a_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float _Property_21ea939bd62a406bb16e9d2c245d51fc_Out_0 = Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
            float _Multiply_05e69bd81c764f2592000c0a11155867_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_21ea939bd62a406bb16e9d2c245d51fc_Out_0, _Multiply_05e69bd81c764f2592000c0a11155867_Out_2);
            float2 _Twirl_5a635ff0e6af4f68b7305bc4038ff89b_Out_4;
            Unity_Twirl_float(IN.uv0.xy, float2 (0.5, 0.5), 10, (_Multiply_05e69bd81c764f2592000c0a11155867_Out_2.xx), _Twirl_5a635ff0e6af4f68b7305bc4038ff89b_Out_4);
            float _GradientNoise_941e9e87112c42af8081a12c720dbace_Out_2;
            Unity_GradientNoise_float(_Twirl_5a635ff0e6af4f68b7305bc4038ff89b_Out_4, 10, _GradientNoise_941e9e87112c42af8081a12c720dbace_Out_2);
            float3 _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Out_1;
            float3x3 _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
            float3 _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Position = IN.WorldSpacePosition;
            Unity_NormalFromHeight_Tangent_float(_GradientNoise_941e9e87112c42af8081a12c720dbace_Out_2,0.01,_NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Position,_NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_TangentMatrix, _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Out_1);
            float3 _Multiply_7bafafdd71c849b3a8a4595a5aa911f8_Out_2;
            Unity_Multiply_float((_ScreenPosition_a78e4614011e4b74b98b8d30703a284a_Out_0.xyz), _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Out_1, _Multiply_7bafafdd71c849b3a8a4595a5aa911f8_Out_2);
            float3 _Add_41e3a2b9d13b4488a01c3ab2010011d1_Out_2;
            Unity_Add_float3((_ScreenPosition_a78e4614011e4b74b98b8d30703a284a_Out_0.xyz), _Multiply_7bafafdd71c849b3a8a4595a5aa911f8_Out_2, _Add_41e3a2b9d13b4488a01c3ab2010011d1_Out_2);
            float3 _SceneColor_0d9a92329d8140c685305039ff806d9d_Out_1;
            Unity_SceneColor_float((float4(_Add_41e3a2b9d13b4488a01c3ab2010011d1_Out_2, 1.0)), _SceneColor_0d9a92329d8140c685305039ff806d9d_Out_1);
            float _Property_253bf9d5f9b442eea6ee935ab691f8b4_Out_0 = Vector1_4f9b47792cda4f22a14538bd7ad50123;
            surface.BaseColor = (_Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2.xyz);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Emission = _SceneColor_0d9a92329d8140c685305039ff806d9d_Out_1;
            surface.Metallic = 0;
            surface.Smoothness = _Property_253bf9d5f9b442eea6ee935ab691f8b4_Out_0;
            surface.Occlusion = 1;
            surface.Alpha = 1;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.TimeParameters =              _TimeParameters.xyz;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

        	// must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
        	float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);

        	// use bitangent on the fly like in hdrp
        	// IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
            float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
        	float3 bitang = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);

            output.WorldSpaceNormal =            renormFactor*input.normalWS.xyz;		// we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);

        	// to preserve mikktspace compliance we use same scale renormFactor as was used on the normal.
        	// This is explained in section 2.2 in "surface gradient based bump mapping framework"
            output.WorldSpaceTangent =           renormFactor*input.tangentWS.xyz;
        	output.WorldSpaceBiTangent =         renormFactor*bitang;

            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                         input.texCoord0;
            output.TimeParameters =              _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
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
        #define REQUIRE_OPAQUE_TEXTURE
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
            float3 WorldSpaceTangent;
            float3 WorldSpaceBiTangent;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 uv0;
            float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 WorldSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float3 TimeParameters;
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
        float4 Texture2D_fc19be8c381e40e79af53fac1eb60dc4_TexelSize;
        float Vector1_0f74eb3108994472ab1520e422afcda5;
        float4 Color_8d6013e20be440b18ef88aab5ba53339;
        float Vector1_253e63c1c178480385125f8ed67ed240;
        float Vector1_79c4e17149f84e29bd0c6f7f584bb560;
        float Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
        float Vector1_bf2c758a07f64df5b4cc43ebeead739e;
        float4 Color_2d78f1c40ea749229055731cd5a17ceb;
        float Vector1_b57099da0f604234b785d35982d3db5a;
        float Vector1_4f9b47792cda4f22a14538bd7ad50123;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
        SAMPLER(samplerTexture2D_fc19be8c381e40e79af53fac1eb60dc4);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }


        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
        }

        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }


        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);

            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);

            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;

            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            Out = t;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Twirl_float(float2 UV, float2 Center, float Strength, float2 Offset, out float2 Out)
        {
            float2 delta = UV - Center;
            float angle = Strength * length(delta);
            float x = cos(angle) * delta.x - sin(angle) * delta.y;
            float y = sin(angle) * delta.x + cos(angle) * delta.y;
            Out = float2(x + Center.x + Offset.x, y + Center.y + Offset.y);
        }


        float2 Unity_GradientNoise_Dir_float(float2 p)
        {
            // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
            p = p % 289;
            // need full precision, otherwise half overflows when p > 1
            float x = float(34 * p.x + 1) * p.x % 289 + p.y;
            x = (34 * x + 1) * x % 289;
            x = frac(x / 41) * 2 - 1;
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }

        void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
        { 
            float2 p = UV * Scale;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }

        void Unity_NormalFromHeight_Tangent_float(float In, float Strength, float3 Position, float3x3 TangentMatrix, out float3 Out)
        {
            float3 worldDerivativeX = ddx(Position);
            float3 worldDerivativeY = ddy(Position);

            float3 crossX = cross(TangentMatrix[2].xyz, worldDerivativeX);
            float3 crossY = cross(worldDerivativeY, TangentMatrix[2].xyz);
            float d = dot(worldDerivativeX, crossY);
            float sgn = d < 0.0 ? (-1.0f) : 1.0f;
            float surface = sgn / max(0.000000000000001192093f, abs(d));

            float dHdx = ddx(In);
            float dHdy = ddy(In);
            float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
            Out = SafeNormalize(TangentMatrix[2].xyz - (Strength * surfGrad));
            Out = TransformWorldToTangent(Out, TangentMatrix);
        }

        void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
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
            float _Property_3d54fd98a65941ff860977ace1c646fa_Out_0 = Vector1_253e63c1c178480385125f8ed67ed240;
            float _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_3d54fd98a65941ff860977ace1c646fa_Out_0, _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2);
            float3 _Add_815fddedf3c24649af80cc6b8618e81e_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_e5c6132717e64533ab58b561e437e52b_Out_2.xxx), _Add_815fddedf3c24649af80cc6b8618e81e_Out_2);
            float _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2;
            Unity_SimpleNoise_float((_Add_815fddedf3c24649af80cc6b8618e81e_Out_2.xy), 10.28, _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2);
            float _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0 = Vector1_79c4e17149f84e29bd0c6f7f584bb560;
            float _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2;
            Unity_Multiply_float(_SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2, _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0, _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2);
            float3 _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2;
            Unity_Multiply_float(IN.WorldSpaceNormal, (_Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2.xxx), _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2);
            float3 _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2, _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2);
            description.Position = _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
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
            UnityTexture2D _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
            float _Property_bceb82901b7c4f4b9f2332ed8c2447c4_Out_0 = Vector1_0f74eb3108994472ab1520e422afcda5;
            float _Multiply_3350ef5f73a04b828638600703a4cdba_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_bceb82901b7c4f4b9f2332ed8c2447c4_Out_0, _Multiply_3350ef5f73a04b828638600703a4cdba_Out_2);
            float2 _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_3350ef5f73a04b828638600703a4cdba_Out_2.xx), _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3);
            float4 _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0 = SAMPLE_TEXTURE2D(_Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.tex, _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.samplerstate, _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3);
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_R_4 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.r;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_G_5 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.g;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_B_6 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.b;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_A_7 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.a;
            float _OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1;
            Unity_OneMinus_float(_Multiply_3350ef5f73a04b828638600703a4cdba_Out_2, _OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1);
            float2 _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (3, 3), (_OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1.xx), _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3);
            float4 _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0 = SAMPLE_TEXTURE2D(_Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.tex, _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.samplerstate, _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3);
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_R_4 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.r;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_G_5 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.g;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_B_6 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.b;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_A_7 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.a;
            float4 _Add_c968d8ffb24640dba87de4031f77ce5c_Out_2;
            Unity_Add_float4(_SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0, _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0, _Add_c968d8ffb24640dba87de4031f77ce5c_Out_2);
            float4 _Property_c8a13dbad4de4568886b5ee49562997a_Out_0 = IsGammaSpace() ? LinearToSRGB(Color_8d6013e20be440b18ef88aab5ba53339) : Color_8d6013e20be440b18ef88aab5ba53339;
            float4 _Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2;
            Unity_Multiply_float(_Add_c968d8ffb24640dba87de4031f77ce5c_Out_2, _Property_c8a13dbad4de4568886b5ee49562997a_Out_0, _Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2);
            float4 _ScreenPosition_a78e4614011e4b74b98b8d30703a284a_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float _Property_21ea939bd62a406bb16e9d2c245d51fc_Out_0 = Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
            float _Multiply_05e69bd81c764f2592000c0a11155867_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_21ea939bd62a406bb16e9d2c245d51fc_Out_0, _Multiply_05e69bd81c764f2592000c0a11155867_Out_2);
            float2 _Twirl_5a635ff0e6af4f68b7305bc4038ff89b_Out_4;
            Unity_Twirl_float(IN.uv0.xy, float2 (0.5, 0.5), 10, (_Multiply_05e69bd81c764f2592000c0a11155867_Out_2.xx), _Twirl_5a635ff0e6af4f68b7305bc4038ff89b_Out_4);
            float _GradientNoise_941e9e87112c42af8081a12c720dbace_Out_2;
            Unity_GradientNoise_float(_Twirl_5a635ff0e6af4f68b7305bc4038ff89b_Out_4, 10, _GradientNoise_941e9e87112c42af8081a12c720dbace_Out_2);
            float3 _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Out_1;
            float3x3 _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
            float3 _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Position = IN.WorldSpacePosition;
            Unity_NormalFromHeight_Tangent_float(_GradientNoise_941e9e87112c42af8081a12c720dbace_Out_2,0.01,_NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Position,_NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_TangentMatrix, _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Out_1);
            float3 _Multiply_7bafafdd71c849b3a8a4595a5aa911f8_Out_2;
            Unity_Multiply_float((_ScreenPosition_a78e4614011e4b74b98b8d30703a284a_Out_0.xyz), _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Out_1, _Multiply_7bafafdd71c849b3a8a4595a5aa911f8_Out_2);
            float3 _Add_41e3a2b9d13b4488a01c3ab2010011d1_Out_2;
            Unity_Add_float3((_ScreenPosition_a78e4614011e4b74b98b8d30703a284a_Out_0.xyz), _Multiply_7bafafdd71c849b3a8a4595a5aa911f8_Out_2, _Add_41e3a2b9d13b4488a01c3ab2010011d1_Out_2);
            float3 _SceneColor_0d9a92329d8140c685305039ff806d9d_Out_1;
            Unity_SceneColor_float((float4(_Add_41e3a2b9d13b4488a01c3ab2010011d1_Out_2, 1.0)), _SceneColor_0d9a92329d8140c685305039ff806d9d_Out_1);
            float _Property_253bf9d5f9b442eea6ee935ab691f8b4_Out_0 = Vector1_4f9b47792cda4f22a14538bd7ad50123;
            surface.BaseColor = (_Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2.xyz);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Emission = _SceneColor_0d9a92329d8140c685305039ff806d9d_Out_1;
            surface.Metallic = 0;
            surface.Smoothness = _Property_253bf9d5f9b442eea6ee935ab691f8b4_Out_0;
            surface.Occlusion = 1;
            surface.Alpha = 1;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.TimeParameters =              _TimeParameters.xyz;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

        	// must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
        	float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);

        	// use bitangent on the fly like in hdrp
        	// IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
            float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
        	float3 bitang = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);

            output.WorldSpaceNormal =            renormFactor*input.normalWS.xyz;		// we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);

        	// to preserve mikktspace compliance we use same scale renormFactor as was used on the normal.
        	// This is explained in section 2.2 in "surface gradient based bump mapping framework"
            output.WorldSpaceTangent =           renormFactor*input.tangentWS.xyz;
        	output.WorldSpaceBiTangent =         renormFactor*bitang;

            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                         input.texCoord0;
            output.TimeParameters =              _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
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
            #define VARYINGS_NEED_NORMAL_WS
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 normalWS;
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
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 WorldSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float3 TimeParameters;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
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
        float4 Texture2D_fc19be8c381e40e79af53fac1eb60dc4_TexelSize;
        float Vector1_0f74eb3108994472ab1520e422afcda5;
        float4 Color_8d6013e20be440b18ef88aab5ba53339;
        float Vector1_253e63c1c178480385125f8ed67ed240;
        float Vector1_79c4e17149f84e29bd0c6f7f584bb560;
        float Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
        float Vector1_bf2c758a07f64df5b4cc43ebeead739e;
        float4 Color_2d78f1c40ea749229055731cd5a17ceb;
        float Vector1_b57099da0f604234b785d35982d3db5a;
        float Vector1_4f9b47792cda4f22a14538bd7ad50123;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
        SAMPLER(samplerTexture2D_fc19be8c381e40e79af53fac1eb60dc4);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }


        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
        }

        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }


        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);

            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);

            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;

            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            Out = t;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
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
            float _Property_3d54fd98a65941ff860977ace1c646fa_Out_0 = Vector1_253e63c1c178480385125f8ed67ed240;
            float _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_3d54fd98a65941ff860977ace1c646fa_Out_0, _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2);
            float3 _Add_815fddedf3c24649af80cc6b8618e81e_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_e5c6132717e64533ab58b561e437e52b_Out_2.xxx), _Add_815fddedf3c24649af80cc6b8618e81e_Out_2);
            float _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2;
            Unity_SimpleNoise_float((_Add_815fddedf3c24649af80cc6b8618e81e_Out_2.xy), 10.28, _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2);
            float _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0 = Vector1_79c4e17149f84e29bd0c6f7f584bb560;
            float _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2;
            Unity_Multiply_float(_SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2, _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0, _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2);
            float3 _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2;
            Unity_Multiply_float(IN.WorldSpaceNormal, (_Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2.xxx), _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2);
            float3 _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2, _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2);
            description.Position = _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
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
            surface.Alpha = 1;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.TimeParameters =              _TimeParameters.xyz;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
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
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 WorldSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float3 TimeParameters;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
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
        float4 Texture2D_fc19be8c381e40e79af53fac1eb60dc4_TexelSize;
        float Vector1_0f74eb3108994472ab1520e422afcda5;
        float4 Color_8d6013e20be440b18ef88aab5ba53339;
        float Vector1_253e63c1c178480385125f8ed67ed240;
        float Vector1_79c4e17149f84e29bd0c6f7f584bb560;
        float Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
        float Vector1_bf2c758a07f64df5b4cc43ebeead739e;
        float4 Color_2d78f1c40ea749229055731cd5a17ceb;
        float Vector1_b57099da0f604234b785d35982d3db5a;
        float Vector1_4f9b47792cda4f22a14538bd7ad50123;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
        SAMPLER(samplerTexture2D_fc19be8c381e40e79af53fac1eb60dc4);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }


        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
        }

        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }


        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);

            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);

            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;

            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            Out = t;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
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
            float _Property_3d54fd98a65941ff860977ace1c646fa_Out_0 = Vector1_253e63c1c178480385125f8ed67ed240;
            float _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_3d54fd98a65941ff860977ace1c646fa_Out_0, _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2);
            float3 _Add_815fddedf3c24649af80cc6b8618e81e_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_e5c6132717e64533ab58b561e437e52b_Out_2.xxx), _Add_815fddedf3c24649af80cc6b8618e81e_Out_2);
            float _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2;
            Unity_SimpleNoise_float((_Add_815fddedf3c24649af80cc6b8618e81e_Out_2.xy), 10.28, _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2);
            float _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0 = Vector1_79c4e17149f84e29bd0c6f7f584bb560;
            float _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2;
            Unity_Multiply_float(_SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2, _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0, _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2);
            float3 _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2;
            Unity_Multiply_float(IN.WorldSpaceNormal, (_Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2.xxx), _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2);
            float3 _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2, _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2);
            description.Position = _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
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
            surface.Alpha = 1;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.TimeParameters =              _TimeParameters.xyz;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





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
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
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
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 WorldSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float3 TimeParameters;
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
            output.interp1.xyzw =  input.tangentWS;
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
        float4 Texture2D_fc19be8c381e40e79af53fac1eb60dc4_TexelSize;
        float Vector1_0f74eb3108994472ab1520e422afcda5;
        float4 Color_8d6013e20be440b18ef88aab5ba53339;
        float Vector1_253e63c1c178480385125f8ed67ed240;
        float Vector1_79c4e17149f84e29bd0c6f7f584bb560;
        float Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
        float Vector1_bf2c758a07f64df5b4cc43ebeead739e;
        float4 Color_2d78f1c40ea749229055731cd5a17ceb;
        float Vector1_b57099da0f604234b785d35982d3db5a;
        float Vector1_4f9b47792cda4f22a14538bd7ad50123;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
        SAMPLER(samplerTexture2D_fc19be8c381e40e79af53fac1eb60dc4);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }


        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
        }

        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }


        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);

            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);

            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;

            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            Out = t;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
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
            float _Property_3d54fd98a65941ff860977ace1c646fa_Out_0 = Vector1_253e63c1c178480385125f8ed67ed240;
            float _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_3d54fd98a65941ff860977ace1c646fa_Out_0, _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2);
            float3 _Add_815fddedf3c24649af80cc6b8618e81e_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_e5c6132717e64533ab58b561e437e52b_Out_2.xxx), _Add_815fddedf3c24649af80cc6b8618e81e_Out_2);
            float _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2;
            Unity_SimpleNoise_float((_Add_815fddedf3c24649af80cc6b8618e81e_Out_2.xy), 10.28, _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2);
            float _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0 = Vector1_79c4e17149f84e29bd0c6f7f584bb560;
            float _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2;
            Unity_Multiply_float(_SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2, _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0, _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2);
            float3 _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2;
            Unity_Multiply_float(IN.WorldSpaceNormal, (_Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2.xxx), _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2);
            float3 _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2, _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2);
            description.Position = _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
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
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Alpha = 1;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.TimeParameters =              _TimeParameters.xyz;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



            output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);


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
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_META
        #define REQUIRE_OPAQUE_TEXTURE
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
            float3 positionWS;
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
            float3 WorldSpaceNormal;
            float3 WorldSpaceTangent;
            float3 WorldSpaceBiTangent;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 uv0;
            float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 WorldSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float3 TimeParameters;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float3 interp1 : TEXCOORD1;
            float4 interp2 : TEXCOORD2;
            float4 interp3 : TEXCOORD3;
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
        float4 Texture2D_fc19be8c381e40e79af53fac1eb60dc4_TexelSize;
        float Vector1_0f74eb3108994472ab1520e422afcda5;
        float4 Color_8d6013e20be440b18ef88aab5ba53339;
        float Vector1_253e63c1c178480385125f8ed67ed240;
        float Vector1_79c4e17149f84e29bd0c6f7f584bb560;
        float Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
        float Vector1_bf2c758a07f64df5b4cc43ebeead739e;
        float4 Color_2d78f1c40ea749229055731cd5a17ceb;
        float Vector1_b57099da0f604234b785d35982d3db5a;
        float Vector1_4f9b47792cda4f22a14538bd7ad50123;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
        SAMPLER(samplerTexture2D_fc19be8c381e40e79af53fac1eb60dc4);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }


        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
        }

        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }


        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);

            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);

            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;

            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            Out = t;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Twirl_float(float2 UV, float2 Center, float Strength, float2 Offset, out float2 Out)
        {
            float2 delta = UV - Center;
            float angle = Strength * length(delta);
            float x = cos(angle) * delta.x - sin(angle) * delta.y;
            float y = sin(angle) * delta.x + cos(angle) * delta.y;
            Out = float2(x + Center.x + Offset.x, y + Center.y + Offset.y);
        }


        float2 Unity_GradientNoise_Dir_float(float2 p)
        {
            // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
            p = p % 289;
            // need full precision, otherwise half overflows when p > 1
            float x = float(34 * p.x + 1) * p.x % 289 + p.y;
            x = (34 * x + 1) * x % 289;
            x = frac(x / 41) * 2 - 1;
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }

        void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
        { 
            float2 p = UV * Scale;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }

        void Unity_NormalFromHeight_Tangent_float(float In, float Strength, float3 Position, float3x3 TangentMatrix, out float3 Out)
        {
            float3 worldDerivativeX = ddx(Position);
            float3 worldDerivativeY = ddy(Position);

            float3 crossX = cross(TangentMatrix[2].xyz, worldDerivativeX);
            float3 crossY = cross(worldDerivativeY, TangentMatrix[2].xyz);
            float d = dot(worldDerivativeX, crossY);
            float sgn = d < 0.0 ? (-1.0f) : 1.0f;
            float surface = sgn / max(0.000000000000001192093f, abs(d));

            float dHdx = ddx(In);
            float dHdy = ddy(In);
            float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
            Out = SafeNormalize(TangentMatrix[2].xyz - (Strength * surfGrad));
            Out = TransformWorldToTangent(Out, TangentMatrix);
        }

        void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
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
            float _Property_3d54fd98a65941ff860977ace1c646fa_Out_0 = Vector1_253e63c1c178480385125f8ed67ed240;
            float _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_3d54fd98a65941ff860977ace1c646fa_Out_0, _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2);
            float3 _Add_815fddedf3c24649af80cc6b8618e81e_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_e5c6132717e64533ab58b561e437e52b_Out_2.xxx), _Add_815fddedf3c24649af80cc6b8618e81e_Out_2);
            float _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2;
            Unity_SimpleNoise_float((_Add_815fddedf3c24649af80cc6b8618e81e_Out_2.xy), 10.28, _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2);
            float _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0 = Vector1_79c4e17149f84e29bd0c6f7f584bb560;
            float _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2;
            Unity_Multiply_float(_SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2, _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0, _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2);
            float3 _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2;
            Unity_Multiply_float(IN.WorldSpaceNormal, (_Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2.xxx), _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2);
            float3 _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2, _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2);
            description.Position = _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
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
            UnityTexture2D _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
            float _Property_bceb82901b7c4f4b9f2332ed8c2447c4_Out_0 = Vector1_0f74eb3108994472ab1520e422afcda5;
            float _Multiply_3350ef5f73a04b828638600703a4cdba_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_bceb82901b7c4f4b9f2332ed8c2447c4_Out_0, _Multiply_3350ef5f73a04b828638600703a4cdba_Out_2);
            float2 _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_3350ef5f73a04b828638600703a4cdba_Out_2.xx), _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3);
            float4 _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0 = SAMPLE_TEXTURE2D(_Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.tex, _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.samplerstate, _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3);
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_R_4 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.r;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_G_5 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.g;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_B_6 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.b;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_A_7 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.a;
            float _OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1;
            Unity_OneMinus_float(_Multiply_3350ef5f73a04b828638600703a4cdba_Out_2, _OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1);
            float2 _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (3, 3), (_OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1.xx), _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3);
            float4 _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0 = SAMPLE_TEXTURE2D(_Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.tex, _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.samplerstate, _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3);
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_R_4 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.r;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_G_5 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.g;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_B_6 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.b;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_A_7 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.a;
            float4 _Add_c968d8ffb24640dba87de4031f77ce5c_Out_2;
            Unity_Add_float4(_SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0, _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0, _Add_c968d8ffb24640dba87de4031f77ce5c_Out_2);
            float4 _Property_c8a13dbad4de4568886b5ee49562997a_Out_0 = IsGammaSpace() ? LinearToSRGB(Color_8d6013e20be440b18ef88aab5ba53339) : Color_8d6013e20be440b18ef88aab5ba53339;
            float4 _Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2;
            Unity_Multiply_float(_Add_c968d8ffb24640dba87de4031f77ce5c_Out_2, _Property_c8a13dbad4de4568886b5ee49562997a_Out_0, _Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2);
            float4 _ScreenPosition_a78e4614011e4b74b98b8d30703a284a_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float _Property_21ea939bd62a406bb16e9d2c245d51fc_Out_0 = Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
            float _Multiply_05e69bd81c764f2592000c0a11155867_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_21ea939bd62a406bb16e9d2c245d51fc_Out_0, _Multiply_05e69bd81c764f2592000c0a11155867_Out_2);
            float2 _Twirl_5a635ff0e6af4f68b7305bc4038ff89b_Out_4;
            Unity_Twirl_float(IN.uv0.xy, float2 (0.5, 0.5), 10, (_Multiply_05e69bd81c764f2592000c0a11155867_Out_2.xx), _Twirl_5a635ff0e6af4f68b7305bc4038ff89b_Out_4);
            float _GradientNoise_941e9e87112c42af8081a12c720dbace_Out_2;
            Unity_GradientNoise_float(_Twirl_5a635ff0e6af4f68b7305bc4038ff89b_Out_4, 10, _GradientNoise_941e9e87112c42af8081a12c720dbace_Out_2);
            float3 _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Out_1;
            float3x3 _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
            float3 _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Position = IN.WorldSpacePosition;
            Unity_NormalFromHeight_Tangent_float(_GradientNoise_941e9e87112c42af8081a12c720dbace_Out_2,0.01,_NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Position,_NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_TangentMatrix, _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Out_1);
            float3 _Multiply_7bafafdd71c849b3a8a4595a5aa911f8_Out_2;
            Unity_Multiply_float((_ScreenPosition_a78e4614011e4b74b98b8d30703a284a_Out_0.xyz), _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Out_1, _Multiply_7bafafdd71c849b3a8a4595a5aa911f8_Out_2);
            float3 _Add_41e3a2b9d13b4488a01c3ab2010011d1_Out_2;
            Unity_Add_float3((_ScreenPosition_a78e4614011e4b74b98b8d30703a284a_Out_0.xyz), _Multiply_7bafafdd71c849b3a8a4595a5aa911f8_Out_2, _Add_41e3a2b9d13b4488a01c3ab2010011d1_Out_2);
            float3 _SceneColor_0d9a92329d8140c685305039ff806d9d_Out_1;
            Unity_SceneColor_float((float4(_Add_41e3a2b9d13b4488a01c3ab2010011d1_Out_2, 1.0)), _SceneColor_0d9a92329d8140c685305039ff806d9d_Out_1);
            surface.BaseColor = (_Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2.xyz);
            surface.Emission = _SceneColor_0d9a92329d8140c685305039ff806d9d_Out_1;
            surface.Alpha = 1;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.TimeParameters =              _TimeParameters.xyz;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

        	// must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
        	float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);

        	// use bitangent on the fly like in hdrp
        	// IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
            float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
        	float3 bitang = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);

            output.WorldSpaceNormal =            renormFactor*input.normalWS.xyz;		// we want a unit length Normal Vector node in shader graph

        	// to preserve mikktspace compliance we use same scale renormFactor as was used on the normal.
        	// This is explained in section 2.2 in "surface gradient based bump mapping framework"
            output.WorldSpaceTangent =           renormFactor*input.tangentWS.xyz;
        	output.WorldSpaceBiTangent =         renormFactor*bitang;

            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                         input.texCoord0;
            output.TimeParameters =              _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
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
            float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 WorldSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float3 TimeParameters;
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
        float4 Texture2D_fc19be8c381e40e79af53fac1eb60dc4_TexelSize;
        float Vector1_0f74eb3108994472ab1520e422afcda5;
        float4 Color_8d6013e20be440b18ef88aab5ba53339;
        float Vector1_253e63c1c178480385125f8ed67ed240;
        float Vector1_79c4e17149f84e29bd0c6f7f584bb560;
        float Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
        float Vector1_bf2c758a07f64df5b4cc43ebeead739e;
        float4 Color_2d78f1c40ea749229055731cd5a17ceb;
        float Vector1_b57099da0f604234b785d35982d3db5a;
        float Vector1_4f9b47792cda4f22a14538bd7ad50123;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
        SAMPLER(samplerTexture2D_fc19be8c381e40e79af53fac1eb60dc4);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }


        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
        }

        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }


        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);

            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);

            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;

            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            Out = t;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
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
            float _Property_3d54fd98a65941ff860977ace1c646fa_Out_0 = Vector1_253e63c1c178480385125f8ed67ed240;
            float _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_3d54fd98a65941ff860977ace1c646fa_Out_0, _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2);
            float3 _Add_815fddedf3c24649af80cc6b8618e81e_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_e5c6132717e64533ab58b561e437e52b_Out_2.xxx), _Add_815fddedf3c24649af80cc6b8618e81e_Out_2);
            float _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2;
            Unity_SimpleNoise_float((_Add_815fddedf3c24649af80cc6b8618e81e_Out_2.xy), 10.28, _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2);
            float _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0 = Vector1_79c4e17149f84e29bd0c6f7f584bb560;
            float _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2;
            Unity_Multiply_float(_SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2, _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0, _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2);
            float3 _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2;
            Unity_Multiply_float(IN.WorldSpaceNormal, (_Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2.xxx), _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2);
            float3 _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2, _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2);
            description.Position = _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
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
            UnityTexture2D _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
            float _Property_bceb82901b7c4f4b9f2332ed8c2447c4_Out_0 = Vector1_0f74eb3108994472ab1520e422afcda5;
            float _Multiply_3350ef5f73a04b828638600703a4cdba_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_bceb82901b7c4f4b9f2332ed8c2447c4_Out_0, _Multiply_3350ef5f73a04b828638600703a4cdba_Out_2);
            float2 _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_3350ef5f73a04b828638600703a4cdba_Out_2.xx), _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3);
            float4 _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0 = SAMPLE_TEXTURE2D(_Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.tex, _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.samplerstate, _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3);
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_R_4 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.r;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_G_5 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.g;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_B_6 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.b;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_A_7 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.a;
            float _OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1;
            Unity_OneMinus_float(_Multiply_3350ef5f73a04b828638600703a4cdba_Out_2, _OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1);
            float2 _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (3, 3), (_OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1.xx), _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3);
            float4 _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0 = SAMPLE_TEXTURE2D(_Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.tex, _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.samplerstate, _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3);
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_R_4 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.r;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_G_5 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.g;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_B_6 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.b;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_A_7 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.a;
            float4 _Add_c968d8ffb24640dba87de4031f77ce5c_Out_2;
            Unity_Add_float4(_SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0, _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0, _Add_c968d8ffb24640dba87de4031f77ce5c_Out_2);
            float4 _Property_c8a13dbad4de4568886b5ee49562997a_Out_0 = IsGammaSpace() ? LinearToSRGB(Color_8d6013e20be440b18ef88aab5ba53339) : Color_8d6013e20be440b18ef88aab5ba53339;
            float4 _Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2;
            Unity_Multiply_float(_Add_c968d8ffb24640dba87de4031f77ce5c_Out_2, _Property_c8a13dbad4de4568886b5ee49562997a_Out_0, _Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2);
            surface.BaseColor = (_Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2.xyz);
            surface.Alpha = 1;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.TimeParameters =              _TimeParameters.xyz;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.uv0 =                         input.texCoord0;
            output.TimeParameters =              _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
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
        #define REQUIRE_OPAQUE_TEXTURE
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
            float3 WorldSpaceTangent;
            float3 WorldSpaceBiTangent;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 uv0;
            float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 WorldSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float3 TimeParameters;
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
        float4 Texture2D_fc19be8c381e40e79af53fac1eb60dc4_TexelSize;
        float Vector1_0f74eb3108994472ab1520e422afcda5;
        float4 Color_8d6013e20be440b18ef88aab5ba53339;
        float Vector1_253e63c1c178480385125f8ed67ed240;
        float Vector1_79c4e17149f84e29bd0c6f7f584bb560;
        float Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
        float Vector1_bf2c758a07f64df5b4cc43ebeead739e;
        float4 Color_2d78f1c40ea749229055731cd5a17ceb;
        float Vector1_b57099da0f604234b785d35982d3db5a;
        float Vector1_4f9b47792cda4f22a14538bd7ad50123;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
        SAMPLER(samplerTexture2D_fc19be8c381e40e79af53fac1eb60dc4);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }


        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
        }

        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }


        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);

            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);

            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;

            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            Out = t;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Twirl_float(float2 UV, float2 Center, float Strength, float2 Offset, out float2 Out)
        {
            float2 delta = UV - Center;
            float angle = Strength * length(delta);
            float x = cos(angle) * delta.x - sin(angle) * delta.y;
            float y = sin(angle) * delta.x + cos(angle) * delta.y;
            Out = float2(x + Center.x + Offset.x, y + Center.y + Offset.y);
        }


        float2 Unity_GradientNoise_Dir_float(float2 p)
        {
            // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
            p = p % 289;
            // need full precision, otherwise half overflows when p > 1
            float x = float(34 * p.x + 1) * p.x % 289 + p.y;
            x = (34 * x + 1) * x % 289;
            x = frac(x / 41) * 2 - 1;
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }

        void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
        { 
            float2 p = UV * Scale;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }

        void Unity_NormalFromHeight_Tangent_float(float In, float Strength, float3 Position, float3x3 TangentMatrix, out float3 Out)
        {
            float3 worldDerivativeX = ddx(Position);
            float3 worldDerivativeY = ddy(Position);

            float3 crossX = cross(TangentMatrix[2].xyz, worldDerivativeX);
            float3 crossY = cross(worldDerivativeY, TangentMatrix[2].xyz);
            float d = dot(worldDerivativeX, crossY);
            float sgn = d < 0.0 ? (-1.0f) : 1.0f;
            float surface = sgn / max(0.000000000000001192093f, abs(d));

            float dHdx = ddx(In);
            float dHdy = ddy(In);
            float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
            Out = SafeNormalize(TangentMatrix[2].xyz - (Strength * surfGrad));
            Out = TransformWorldToTangent(Out, TangentMatrix);
        }

        void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
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
            float _Property_3d54fd98a65941ff860977ace1c646fa_Out_0 = Vector1_253e63c1c178480385125f8ed67ed240;
            float _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_3d54fd98a65941ff860977ace1c646fa_Out_0, _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2);
            float3 _Add_815fddedf3c24649af80cc6b8618e81e_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_e5c6132717e64533ab58b561e437e52b_Out_2.xxx), _Add_815fddedf3c24649af80cc6b8618e81e_Out_2);
            float _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2;
            Unity_SimpleNoise_float((_Add_815fddedf3c24649af80cc6b8618e81e_Out_2.xy), 10.28, _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2);
            float _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0 = Vector1_79c4e17149f84e29bd0c6f7f584bb560;
            float _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2;
            Unity_Multiply_float(_SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2, _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0, _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2);
            float3 _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2;
            Unity_Multiply_float(IN.WorldSpaceNormal, (_Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2.xxx), _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2);
            float3 _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2, _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2);
            description.Position = _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
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
            UnityTexture2D _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
            float _Property_bceb82901b7c4f4b9f2332ed8c2447c4_Out_0 = Vector1_0f74eb3108994472ab1520e422afcda5;
            float _Multiply_3350ef5f73a04b828638600703a4cdba_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_bceb82901b7c4f4b9f2332ed8c2447c4_Out_0, _Multiply_3350ef5f73a04b828638600703a4cdba_Out_2);
            float2 _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_3350ef5f73a04b828638600703a4cdba_Out_2.xx), _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3);
            float4 _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0 = SAMPLE_TEXTURE2D(_Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.tex, _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.samplerstate, _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3);
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_R_4 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.r;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_G_5 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.g;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_B_6 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.b;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_A_7 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.a;
            float _OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1;
            Unity_OneMinus_float(_Multiply_3350ef5f73a04b828638600703a4cdba_Out_2, _OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1);
            float2 _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (3, 3), (_OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1.xx), _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3);
            float4 _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0 = SAMPLE_TEXTURE2D(_Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.tex, _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.samplerstate, _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3);
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_R_4 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.r;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_G_5 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.g;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_B_6 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.b;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_A_7 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.a;
            float4 _Add_c968d8ffb24640dba87de4031f77ce5c_Out_2;
            Unity_Add_float4(_SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0, _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0, _Add_c968d8ffb24640dba87de4031f77ce5c_Out_2);
            float4 _Property_c8a13dbad4de4568886b5ee49562997a_Out_0 = IsGammaSpace() ? LinearToSRGB(Color_8d6013e20be440b18ef88aab5ba53339) : Color_8d6013e20be440b18ef88aab5ba53339;
            float4 _Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2;
            Unity_Multiply_float(_Add_c968d8ffb24640dba87de4031f77ce5c_Out_2, _Property_c8a13dbad4de4568886b5ee49562997a_Out_0, _Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2);
            float4 _ScreenPosition_a78e4614011e4b74b98b8d30703a284a_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float _Property_21ea939bd62a406bb16e9d2c245d51fc_Out_0 = Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
            float _Multiply_05e69bd81c764f2592000c0a11155867_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_21ea939bd62a406bb16e9d2c245d51fc_Out_0, _Multiply_05e69bd81c764f2592000c0a11155867_Out_2);
            float2 _Twirl_5a635ff0e6af4f68b7305bc4038ff89b_Out_4;
            Unity_Twirl_float(IN.uv0.xy, float2 (0.5, 0.5), 10, (_Multiply_05e69bd81c764f2592000c0a11155867_Out_2.xx), _Twirl_5a635ff0e6af4f68b7305bc4038ff89b_Out_4);
            float _GradientNoise_941e9e87112c42af8081a12c720dbace_Out_2;
            Unity_GradientNoise_float(_Twirl_5a635ff0e6af4f68b7305bc4038ff89b_Out_4, 10, _GradientNoise_941e9e87112c42af8081a12c720dbace_Out_2);
            float3 _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Out_1;
            float3x3 _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
            float3 _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Position = IN.WorldSpacePosition;
            Unity_NormalFromHeight_Tangent_float(_GradientNoise_941e9e87112c42af8081a12c720dbace_Out_2,0.01,_NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Position,_NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_TangentMatrix, _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Out_1);
            float3 _Multiply_7bafafdd71c849b3a8a4595a5aa911f8_Out_2;
            Unity_Multiply_float((_ScreenPosition_a78e4614011e4b74b98b8d30703a284a_Out_0.xyz), _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Out_1, _Multiply_7bafafdd71c849b3a8a4595a5aa911f8_Out_2);
            float3 _Add_41e3a2b9d13b4488a01c3ab2010011d1_Out_2;
            Unity_Add_float3((_ScreenPosition_a78e4614011e4b74b98b8d30703a284a_Out_0.xyz), _Multiply_7bafafdd71c849b3a8a4595a5aa911f8_Out_2, _Add_41e3a2b9d13b4488a01c3ab2010011d1_Out_2);
            float3 _SceneColor_0d9a92329d8140c685305039ff806d9d_Out_1;
            Unity_SceneColor_float((float4(_Add_41e3a2b9d13b4488a01c3ab2010011d1_Out_2, 1.0)), _SceneColor_0d9a92329d8140c685305039ff806d9d_Out_1);
            float _Property_253bf9d5f9b442eea6ee935ab691f8b4_Out_0 = Vector1_4f9b47792cda4f22a14538bd7ad50123;
            surface.BaseColor = (_Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2.xyz);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Emission = _SceneColor_0d9a92329d8140c685305039ff806d9d_Out_1;
            surface.Metallic = 0;
            surface.Smoothness = _Property_253bf9d5f9b442eea6ee935ab691f8b4_Out_0;
            surface.Occlusion = 1;
            surface.Alpha = 1;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.TimeParameters =              _TimeParameters.xyz;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

        	// must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
        	float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);

        	// use bitangent on the fly like in hdrp
        	// IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
            float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
        	float3 bitang = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);

            output.WorldSpaceNormal =            renormFactor*input.normalWS.xyz;		// we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);

        	// to preserve mikktspace compliance we use same scale renormFactor as was used on the normal.
        	// This is explained in section 2.2 in "surface gradient based bump mapping framework"
            output.WorldSpaceTangent =           renormFactor*input.tangentWS.xyz;
        	output.WorldSpaceBiTangent =         renormFactor*bitang;

            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                         input.texCoord0;
            output.TimeParameters =              _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
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
            #define VARYINGS_NEED_NORMAL_WS
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 normalWS;
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
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 WorldSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float3 TimeParameters;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
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
        float4 Texture2D_fc19be8c381e40e79af53fac1eb60dc4_TexelSize;
        float Vector1_0f74eb3108994472ab1520e422afcda5;
        float4 Color_8d6013e20be440b18ef88aab5ba53339;
        float Vector1_253e63c1c178480385125f8ed67ed240;
        float Vector1_79c4e17149f84e29bd0c6f7f584bb560;
        float Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
        float Vector1_bf2c758a07f64df5b4cc43ebeead739e;
        float4 Color_2d78f1c40ea749229055731cd5a17ceb;
        float Vector1_b57099da0f604234b785d35982d3db5a;
        float Vector1_4f9b47792cda4f22a14538bd7ad50123;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
        SAMPLER(samplerTexture2D_fc19be8c381e40e79af53fac1eb60dc4);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }


        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
        }

        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }


        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);

            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);

            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;

            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            Out = t;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
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
            float _Property_3d54fd98a65941ff860977ace1c646fa_Out_0 = Vector1_253e63c1c178480385125f8ed67ed240;
            float _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_3d54fd98a65941ff860977ace1c646fa_Out_0, _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2);
            float3 _Add_815fddedf3c24649af80cc6b8618e81e_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_e5c6132717e64533ab58b561e437e52b_Out_2.xxx), _Add_815fddedf3c24649af80cc6b8618e81e_Out_2);
            float _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2;
            Unity_SimpleNoise_float((_Add_815fddedf3c24649af80cc6b8618e81e_Out_2.xy), 10.28, _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2);
            float _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0 = Vector1_79c4e17149f84e29bd0c6f7f584bb560;
            float _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2;
            Unity_Multiply_float(_SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2, _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0, _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2);
            float3 _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2;
            Unity_Multiply_float(IN.WorldSpaceNormal, (_Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2.xxx), _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2);
            float3 _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2, _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2);
            description.Position = _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
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
            surface.Alpha = 1;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.TimeParameters =              _TimeParameters.xyz;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
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
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 WorldSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float3 TimeParameters;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
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
        float4 Texture2D_fc19be8c381e40e79af53fac1eb60dc4_TexelSize;
        float Vector1_0f74eb3108994472ab1520e422afcda5;
        float4 Color_8d6013e20be440b18ef88aab5ba53339;
        float Vector1_253e63c1c178480385125f8ed67ed240;
        float Vector1_79c4e17149f84e29bd0c6f7f584bb560;
        float Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
        float Vector1_bf2c758a07f64df5b4cc43ebeead739e;
        float4 Color_2d78f1c40ea749229055731cd5a17ceb;
        float Vector1_b57099da0f604234b785d35982d3db5a;
        float Vector1_4f9b47792cda4f22a14538bd7ad50123;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
        SAMPLER(samplerTexture2D_fc19be8c381e40e79af53fac1eb60dc4);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }


        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
        }

        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }


        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);

            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);

            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;

            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            Out = t;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
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
            float _Property_3d54fd98a65941ff860977ace1c646fa_Out_0 = Vector1_253e63c1c178480385125f8ed67ed240;
            float _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_3d54fd98a65941ff860977ace1c646fa_Out_0, _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2);
            float3 _Add_815fddedf3c24649af80cc6b8618e81e_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_e5c6132717e64533ab58b561e437e52b_Out_2.xxx), _Add_815fddedf3c24649af80cc6b8618e81e_Out_2);
            float _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2;
            Unity_SimpleNoise_float((_Add_815fddedf3c24649af80cc6b8618e81e_Out_2.xy), 10.28, _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2);
            float _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0 = Vector1_79c4e17149f84e29bd0c6f7f584bb560;
            float _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2;
            Unity_Multiply_float(_SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2, _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0, _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2);
            float3 _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2;
            Unity_Multiply_float(IN.WorldSpaceNormal, (_Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2.xxx), _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2);
            float3 _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2, _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2);
            description.Position = _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
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
            surface.Alpha = 1;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.TimeParameters =              _TimeParameters.xyz;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





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
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
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
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 WorldSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float3 TimeParameters;
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
            output.interp1.xyzw =  input.tangentWS;
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
        float4 Texture2D_fc19be8c381e40e79af53fac1eb60dc4_TexelSize;
        float Vector1_0f74eb3108994472ab1520e422afcda5;
        float4 Color_8d6013e20be440b18ef88aab5ba53339;
        float Vector1_253e63c1c178480385125f8ed67ed240;
        float Vector1_79c4e17149f84e29bd0c6f7f584bb560;
        float Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
        float Vector1_bf2c758a07f64df5b4cc43ebeead739e;
        float4 Color_2d78f1c40ea749229055731cd5a17ceb;
        float Vector1_b57099da0f604234b785d35982d3db5a;
        float Vector1_4f9b47792cda4f22a14538bd7ad50123;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
        SAMPLER(samplerTexture2D_fc19be8c381e40e79af53fac1eb60dc4);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }


        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
        }

        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }


        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);

            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);

            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;

            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            Out = t;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
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
            float _Property_3d54fd98a65941ff860977ace1c646fa_Out_0 = Vector1_253e63c1c178480385125f8ed67ed240;
            float _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_3d54fd98a65941ff860977ace1c646fa_Out_0, _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2);
            float3 _Add_815fddedf3c24649af80cc6b8618e81e_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_e5c6132717e64533ab58b561e437e52b_Out_2.xxx), _Add_815fddedf3c24649af80cc6b8618e81e_Out_2);
            float _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2;
            Unity_SimpleNoise_float((_Add_815fddedf3c24649af80cc6b8618e81e_Out_2.xy), 10.28, _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2);
            float _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0 = Vector1_79c4e17149f84e29bd0c6f7f584bb560;
            float _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2;
            Unity_Multiply_float(_SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2, _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0, _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2);
            float3 _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2;
            Unity_Multiply_float(IN.WorldSpaceNormal, (_Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2.xxx), _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2);
            float3 _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2, _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2);
            description.Position = _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
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
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Alpha = 1;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.TimeParameters =              _TimeParameters.xyz;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



            output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);


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
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_META
        #define REQUIRE_OPAQUE_TEXTURE
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
            float3 positionWS;
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
            float3 WorldSpaceNormal;
            float3 WorldSpaceTangent;
            float3 WorldSpaceBiTangent;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 uv0;
            float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 WorldSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float3 TimeParameters;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float3 interp1 : TEXCOORD1;
            float4 interp2 : TEXCOORD2;
            float4 interp3 : TEXCOORD3;
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
        float4 Texture2D_fc19be8c381e40e79af53fac1eb60dc4_TexelSize;
        float Vector1_0f74eb3108994472ab1520e422afcda5;
        float4 Color_8d6013e20be440b18ef88aab5ba53339;
        float Vector1_253e63c1c178480385125f8ed67ed240;
        float Vector1_79c4e17149f84e29bd0c6f7f584bb560;
        float Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
        float Vector1_bf2c758a07f64df5b4cc43ebeead739e;
        float4 Color_2d78f1c40ea749229055731cd5a17ceb;
        float Vector1_b57099da0f604234b785d35982d3db5a;
        float Vector1_4f9b47792cda4f22a14538bd7ad50123;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
        SAMPLER(samplerTexture2D_fc19be8c381e40e79af53fac1eb60dc4);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }


        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
        }

        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }


        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);

            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);

            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;

            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            Out = t;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Twirl_float(float2 UV, float2 Center, float Strength, float2 Offset, out float2 Out)
        {
            float2 delta = UV - Center;
            float angle = Strength * length(delta);
            float x = cos(angle) * delta.x - sin(angle) * delta.y;
            float y = sin(angle) * delta.x + cos(angle) * delta.y;
            Out = float2(x + Center.x + Offset.x, y + Center.y + Offset.y);
        }


        float2 Unity_GradientNoise_Dir_float(float2 p)
        {
            // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
            p = p % 289;
            // need full precision, otherwise half overflows when p > 1
            float x = float(34 * p.x + 1) * p.x % 289 + p.y;
            x = (34 * x + 1) * x % 289;
            x = frac(x / 41) * 2 - 1;
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }

        void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
        { 
            float2 p = UV * Scale;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }

        void Unity_NormalFromHeight_Tangent_float(float In, float Strength, float3 Position, float3x3 TangentMatrix, out float3 Out)
        {
            float3 worldDerivativeX = ddx(Position);
            float3 worldDerivativeY = ddy(Position);

            float3 crossX = cross(TangentMatrix[2].xyz, worldDerivativeX);
            float3 crossY = cross(worldDerivativeY, TangentMatrix[2].xyz);
            float d = dot(worldDerivativeX, crossY);
            float sgn = d < 0.0 ? (-1.0f) : 1.0f;
            float surface = sgn / max(0.000000000000001192093f, abs(d));

            float dHdx = ddx(In);
            float dHdy = ddy(In);
            float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
            Out = SafeNormalize(TangentMatrix[2].xyz - (Strength * surfGrad));
            Out = TransformWorldToTangent(Out, TangentMatrix);
        }

        void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
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
            float _Property_3d54fd98a65941ff860977ace1c646fa_Out_0 = Vector1_253e63c1c178480385125f8ed67ed240;
            float _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_3d54fd98a65941ff860977ace1c646fa_Out_0, _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2);
            float3 _Add_815fddedf3c24649af80cc6b8618e81e_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_e5c6132717e64533ab58b561e437e52b_Out_2.xxx), _Add_815fddedf3c24649af80cc6b8618e81e_Out_2);
            float _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2;
            Unity_SimpleNoise_float((_Add_815fddedf3c24649af80cc6b8618e81e_Out_2.xy), 10.28, _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2);
            float _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0 = Vector1_79c4e17149f84e29bd0c6f7f584bb560;
            float _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2;
            Unity_Multiply_float(_SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2, _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0, _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2);
            float3 _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2;
            Unity_Multiply_float(IN.WorldSpaceNormal, (_Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2.xxx), _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2);
            float3 _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2, _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2);
            description.Position = _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
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
            UnityTexture2D _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
            float _Property_bceb82901b7c4f4b9f2332ed8c2447c4_Out_0 = Vector1_0f74eb3108994472ab1520e422afcda5;
            float _Multiply_3350ef5f73a04b828638600703a4cdba_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_bceb82901b7c4f4b9f2332ed8c2447c4_Out_0, _Multiply_3350ef5f73a04b828638600703a4cdba_Out_2);
            float2 _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_3350ef5f73a04b828638600703a4cdba_Out_2.xx), _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3);
            float4 _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0 = SAMPLE_TEXTURE2D(_Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.tex, _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.samplerstate, _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3);
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_R_4 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.r;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_G_5 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.g;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_B_6 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.b;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_A_7 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.a;
            float _OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1;
            Unity_OneMinus_float(_Multiply_3350ef5f73a04b828638600703a4cdba_Out_2, _OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1);
            float2 _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (3, 3), (_OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1.xx), _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3);
            float4 _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0 = SAMPLE_TEXTURE2D(_Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.tex, _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.samplerstate, _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3);
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_R_4 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.r;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_G_5 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.g;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_B_6 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.b;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_A_7 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.a;
            float4 _Add_c968d8ffb24640dba87de4031f77ce5c_Out_2;
            Unity_Add_float4(_SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0, _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0, _Add_c968d8ffb24640dba87de4031f77ce5c_Out_2);
            float4 _Property_c8a13dbad4de4568886b5ee49562997a_Out_0 = IsGammaSpace() ? LinearToSRGB(Color_8d6013e20be440b18ef88aab5ba53339) : Color_8d6013e20be440b18ef88aab5ba53339;
            float4 _Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2;
            Unity_Multiply_float(_Add_c968d8ffb24640dba87de4031f77ce5c_Out_2, _Property_c8a13dbad4de4568886b5ee49562997a_Out_0, _Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2);
            float4 _ScreenPosition_a78e4614011e4b74b98b8d30703a284a_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float _Property_21ea939bd62a406bb16e9d2c245d51fc_Out_0 = Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
            float _Multiply_05e69bd81c764f2592000c0a11155867_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_21ea939bd62a406bb16e9d2c245d51fc_Out_0, _Multiply_05e69bd81c764f2592000c0a11155867_Out_2);
            float2 _Twirl_5a635ff0e6af4f68b7305bc4038ff89b_Out_4;
            Unity_Twirl_float(IN.uv0.xy, float2 (0.5, 0.5), 10, (_Multiply_05e69bd81c764f2592000c0a11155867_Out_2.xx), _Twirl_5a635ff0e6af4f68b7305bc4038ff89b_Out_4);
            float _GradientNoise_941e9e87112c42af8081a12c720dbace_Out_2;
            Unity_GradientNoise_float(_Twirl_5a635ff0e6af4f68b7305bc4038ff89b_Out_4, 10, _GradientNoise_941e9e87112c42af8081a12c720dbace_Out_2);
            float3 _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Out_1;
            float3x3 _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
            float3 _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Position = IN.WorldSpacePosition;
            Unity_NormalFromHeight_Tangent_float(_GradientNoise_941e9e87112c42af8081a12c720dbace_Out_2,0.01,_NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Position,_NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_TangentMatrix, _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Out_1);
            float3 _Multiply_7bafafdd71c849b3a8a4595a5aa911f8_Out_2;
            Unity_Multiply_float((_ScreenPosition_a78e4614011e4b74b98b8d30703a284a_Out_0.xyz), _NormalFromHeight_1d5f5b852a8741f7b6ef661cbdc355f0_Out_1, _Multiply_7bafafdd71c849b3a8a4595a5aa911f8_Out_2);
            float3 _Add_41e3a2b9d13b4488a01c3ab2010011d1_Out_2;
            Unity_Add_float3((_ScreenPosition_a78e4614011e4b74b98b8d30703a284a_Out_0.xyz), _Multiply_7bafafdd71c849b3a8a4595a5aa911f8_Out_2, _Add_41e3a2b9d13b4488a01c3ab2010011d1_Out_2);
            float3 _SceneColor_0d9a92329d8140c685305039ff806d9d_Out_1;
            Unity_SceneColor_float((float4(_Add_41e3a2b9d13b4488a01c3ab2010011d1_Out_2, 1.0)), _SceneColor_0d9a92329d8140c685305039ff806d9d_Out_1);
            surface.BaseColor = (_Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2.xyz);
            surface.Emission = _SceneColor_0d9a92329d8140c685305039ff806d9d_Out_1;
            surface.Alpha = 1;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.TimeParameters =              _TimeParameters.xyz;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

        	// must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
        	float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);

        	// use bitangent on the fly like in hdrp
        	// IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
            float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
        	float3 bitang = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);

            output.WorldSpaceNormal =            renormFactor*input.normalWS.xyz;		// we want a unit length Normal Vector node in shader graph

        	// to preserve mikktspace compliance we use same scale renormFactor as was used on the normal.
        	// This is explained in section 2.2 in "surface gradient based bump mapping framework"
            output.WorldSpaceTangent =           renormFactor*input.tangentWS.xyz;
        	output.WorldSpaceBiTangent =         renormFactor*bitang;

            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                         input.texCoord0;
            output.TimeParameters =              _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
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
            float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 WorldSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float3 TimeParameters;
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
        float4 Texture2D_fc19be8c381e40e79af53fac1eb60dc4_TexelSize;
        float Vector1_0f74eb3108994472ab1520e422afcda5;
        float4 Color_8d6013e20be440b18ef88aab5ba53339;
        float Vector1_253e63c1c178480385125f8ed67ed240;
        float Vector1_79c4e17149f84e29bd0c6f7f584bb560;
        float Vector1_6a079c2d4fae4204986b6b8f277fd7f7;
        float Vector1_bf2c758a07f64df5b4cc43ebeead739e;
        float4 Color_2d78f1c40ea749229055731cd5a17ceb;
        float Vector1_b57099da0f604234b785d35982d3db5a;
        float Vector1_4f9b47792cda4f22a14538bd7ad50123;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
        SAMPLER(samplerTexture2D_fc19be8c381e40e79af53fac1eb60dc4);

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }


        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
        }

        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }


        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);

            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);

            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;

            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

            Out = t;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
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
            float _Property_3d54fd98a65941ff860977ace1c646fa_Out_0 = Vector1_253e63c1c178480385125f8ed67ed240;
            float _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_3d54fd98a65941ff860977ace1c646fa_Out_0, _Multiply_e5c6132717e64533ab58b561e437e52b_Out_2);
            float3 _Add_815fddedf3c24649af80cc6b8618e81e_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_e5c6132717e64533ab58b561e437e52b_Out_2.xxx), _Add_815fddedf3c24649af80cc6b8618e81e_Out_2);
            float _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2;
            Unity_SimpleNoise_float((_Add_815fddedf3c24649af80cc6b8618e81e_Out_2.xy), 10.28, _SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2);
            float _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0 = Vector1_79c4e17149f84e29bd0c6f7f584bb560;
            float _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2;
            Unity_Multiply_float(_SimpleNoise_817a26092f3e44a09cc996b6bb44e539_Out_2, _Property_2644c8afdff94673aa02bfd47dd77e43_Out_0, _Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2);
            float3 _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2;
            Unity_Multiply_float(IN.WorldSpaceNormal, (_Multiply_b255bf09696e4245b72857c2f5b5691f_Out_2.xxx), _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2);
            float3 _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_d1f5bf47942a4b3db3092419db244179_Out_2, _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2);
            description.Position = _Add_9ddf5edf1d974d34854ea6472754cd20_Out_2;
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
            UnityTexture2D _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_fc19be8c381e40e79af53fac1eb60dc4);
            float _Property_bceb82901b7c4f4b9f2332ed8c2447c4_Out_0 = Vector1_0f74eb3108994472ab1520e422afcda5;
            float _Multiply_3350ef5f73a04b828638600703a4cdba_Out_2;
            Unity_Multiply_float(IN.TimeParameters.x, _Property_bceb82901b7c4f4b9f2332ed8c2447c4_Out_0, _Multiply_3350ef5f73a04b828638600703a4cdba_Out_2);
            float2 _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_3350ef5f73a04b828638600703a4cdba_Out_2.xx), _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3);
            float4 _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0 = SAMPLE_TEXTURE2D(_Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.tex, _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.samplerstate, _TilingAndOffset_4cbdfbc523584d5e8ab9c1c67a8e13e9_Out_3);
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_R_4 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.r;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_G_5 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.g;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_B_6 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.b;
            float _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_A_7 = _SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0.a;
            float _OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1;
            Unity_OneMinus_float(_Multiply_3350ef5f73a04b828638600703a4cdba_Out_2, _OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1);
            float2 _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (3, 3), (_OneMinus_8dfd476719c0492cbb565281c888f8b9_Out_1.xx), _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3);
            float4 _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0 = SAMPLE_TEXTURE2D(_Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.tex, _Property_58ecc35f482c45f2a8f5727a3495550b_Out_0.samplerstate, _TilingAndOffset_a5e168b48b544797ab016e1f534293c4_Out_3);
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_R_4 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.r;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_G_5 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.g;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_B_6 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.b;
            float _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_A_7 = _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0.a;
            float4 _Add_c968d8ffb24640dba87de4031f77ce5c_Out_2;
            Unity_Add_float4(_SampleTexture2D_190bdbccb0534760bb58cfbf4ee186fa_RGBA_0, _SampleTexture2D_ebbb3435d87149f386e878260ab894f5_RGBA_0, _Add_c968d8ffb24640dba87de4031f77ce5c_Out_2);
            float4 _Property_c8a13dbad4de4568886b5ee49562997a_Out_0 = IsGammaSpace() ? LinearToSRGB(Color_8d6013e20be440b18ef88aab5ba53339) : Color_8d6013e20be440b18ef88aab5ba53339;
            float4 _Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2;
            Unity_Multiply_float(_Add_c968d8ffb24640dba87de4031f77ce5c_Out_2, _Property_c8a13dbad4de4568886b5ee49562997a_Out_0, _Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2);
            surface.BaseColor = (_Multiply_7645a069e28e4b6eab5c328c2bcb4e3b_Out_2.xyz);
            surface.Alpha = 1;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.TimeParameters =              _TimeParameters.xyz;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.uv0 =                         input.texCoord0;
            output.TimeParameters =              _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
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