Shader "Texel-Lit with Palette"
{
    Properties
    {
        [NoScaleOffset]_BaseMap("Diffuse", 2D) = "white" {}
        [HDR]_BaseColor("Base Diffuse", Color) = (0, 0, 0, 0)
        [NoScaleOffset]_NormalMap("Normal", 2D) = "bump" {}
        [NoScaleOffset]_MetallicGlossMap("Metallic", 2D) = "black" {}
        baseMetallic("Base Metallic", Range(0, 1)) = 0
        baseSmoothness("Base Smoothness", Range(0, 1)) = 0
        baseDitherWeight("Base Dither Weight", Range(0, 1)) = 0
        [NoScaleOffset]_EmissionMap("Emission Map", 2D) = "black" {}
        _BaseEmissionColor("Base Emissive Color", Color) = (0, 0, 0, 0)
        emissiveIntensity("Emissive Intensity", Float) = 1
        shadowColor("Shadow Color", Color) = (0, 0, 0, 0)
        overallTransparency("Transparency", Range(0, 1)) = 1
        [NoScaleOffset]paletteTexture("Palette Lookup Texture", 2D) = "white" {}
        paletteCellCount("Palette Cell Count", Int) = 16
        paletteEffect("Palette Effect", Range(0, 1)) = 0
        lightmapWeight("Lightmap Weight", Range(0, 1)) = 1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "UniversalMaterialType" = "Unlit"
            "Queue"="AlphaTest"
        }
        Pass
        {
            Name "Pass"
            Tags
            {
                // LightMode: <None>
            }

            // Render State
            Cull Off
        Blend One Zero
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
            #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma shader_feature _ _SAMPLE_GI
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
        #pragma multi_compile _ _SHADOWS_SOFT



            // Defines
            #define _AlphaClip 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_TEXCOORD2
            #define ATTRIBUTES_NEED_TEXCOORD3
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD1
            #define VARYINGS_NEED_TEXCOORD2
            #define VARYINGS_NEED_TEXCOORD3
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_UNLIT
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
            float4 uv2 : TEXCOORD2;
            float4 uv3 : TEXCOORD3;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float3 normalWS;
            float4 texCoord0;
            float4 texCoord1;
            float4 texCoord2;
            float4 texCoord3;
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
            float3 ObjectSpaceViewDirection;
            float3 WorldSpaceViewDirection;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 uv0;
            float4 uv1;
            float4 uv2;
            float4 uv3;
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
            float4 interp4 : TEXCOORD4;
            float4 interp5 : TEXCOORD5;
            float3 interp6 : TEXCOORD6;
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
            output.interp2.xyzw =  input.texCoord0;
            output.interp3.xyzw =  input.texCoord1;
            output.interp4.xyzw =  input.texCoord2;
            output.interp5.xyzw =  input.texCoord3;
            output.interp6.xyz =  input.viewDirectionWS;
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
            output.texCoord0 = input.interp2.xyzw;
            output.texCoord1 = input.interp3.xyzw;
            output.texCoord2 = input.interp4.xyzw;
            output.texCoord3 = input.interp5.xyzw;
            output.viewDirectionWS = input.interp6.xyz;
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
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float4 _NormalMap_TexelSize;
        float4 _MetallicGlossMap_TexelSize;
        float baseMetallic;
        float baseSmoothness;
        float baseDitherWeight;
        float4 _EmissionMap_TexelSize;
        float4 _BaseEmissionColor;
        float emissiveIntensity;
        float4 shadowColor;
        float overallTransparency;
        float4 paletteTexture_TexelSize;
        float paletteCellCount;
        float paletteEffect;
        float lightmapWeight;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_NormalMap);
        SAMPLER(sampler_NormalMap);
        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_EmissionMap);
        SAMPLER(sampler_EmissionMap);
        TEXTURE2D(paletteTexture);
        SAMPLER(samplerpaletteTexture);

            // Graph Functions

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }

        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }

        // ae574a596f27ccc1c05e27ac40f795d7
        #include "Assets/Banchou/Shaders/Subgraphs/SubgraphFunctions.hlsl"

        struct Bindings_TexelSnap_a0da51191bd8bb4419c891a18487e25b
        {
            half4 uv0;
            half4 uv1;
            half4 uv2;
            half4 uv3;
        };

        void SG_TexelSnap_a0da51191bd8bb4419c891a18487e25b(float4 Vector4_19dcf0963d8d462598f362cf77471f80, UnityTexture2D Texture2D_7b88a15474c645e6b5163ef5300931d5, Bindings_TexelSnap_a0da51191bd8bb4419c891a18487e25b IN, out float4 snapped_1)
        {
            float4 _Property_3463cefa50804f4eb376808e0343f01c_Out_0 = Vector4_19dcf0963d8d462598f362cf77471f80;
            float4 _UV_f620b3918f5f41a4bdb2669862b09dc0_Out_0 = IN.uv0;
            UnityTexture2D _Property_05d17b1534134d50be9ab9b75cbbda55_Out_0 = Texture2D_7b88a15474c645e6b5163ef5300931d5;
            float _TexelSize_59b7504c2b6f448fb221cd456846b96a_Width_0 = _Property_05d17b1534134d50be9ab9b75cbbda55_Out_0.texelSize.z;
            float _TexelSize_59b7504c2b6f448fb221cd456846b96a_Height_2 = _Property_05d17b1534134d50be9ab9b75cbbda55_Out_0.texelSize.w;
            float2 _Vector2_eb58a01f34ed40d4a868b1877bd2c496_Out_0 = float2(_TexelSize_59b7504c2b6f448fb221cd456846b96a_Width_0, _TexelSize_59b7504c2b6f448fb221cd456846b96a_Height_2);
            float4 _TexelSnapCustomFunction_d150667e7dc74ebba201834086a417bf_snapped_2;
            TexelSnap_float(_Property_3463cefa50804f4eb376808e0343f01c_Out_0, (_UV_f620b3918f5f41a4bdb2669862b09dc0_Out_0.xy), _Vector2_eb58a01f34ed40d4a868b1877bd2c496_Out_0, _TexelSnapCustomFunction_d150667e7dc74ebba201834086a417bf_snapped_2);
            snapped_1 = _TexelSnapCustomFunction_d150667e7dc74ebba201834086a417bf_snapped_2;
        }

        void Unity_NormalUnpack_float(float4 In, out float3 Out)
        {
                        Out = UnpackNormal(In);
                    }

        void Unity_NormalBlend_float(float3 A, float3 B, out float3 Out)
        {
            Out = SafeNormalize(float3(A.rg + B.rg, A.b * B.b));
        }

        struct Bindings_TexelSnappedNormals_05d618dcee8af6942adba1f3f4b1fcc3
        {
            float3 WorldSpaceNormal;
            half4 uv0;
            half4 uv1;
            half4 uv2;
            half4 uv3;
        };

        void SG_TexelSnappedNormals_05d618dcee8af6942adba1f3f4b1fcc3(UnityTexture2D Texture2D_d7cdf2ee12034e1ab25769cae5275851, Bindings_TexelSnappedNormals_05d618dcee8af6942adba1f3f4b1fcc3 IN, out float4 Normals_1, out float3 Geometry_Normals_2, out float3 Normal_Map_3)
        {
            UnityTexture2D _Property_14a34ba4a9f64f798e55fe269d5161c4_Out_0 = Texture2D_d7cdf2ee12034e1ab25769cae5275851;
            float4 _SampleTexture2D_be27b957c2724b828474571419e845ec_RGBA_0 = SAMPLE_TEXTURE2D(_Property_14a34ba4a9f64f798e55fe269d5161c4_Out_0.tex, _Property_14a34ba4a9f64f798e55fe269d5161c4_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_be27b957c2724b828474571419e845ec_R_4 = _SampleTexture2D_be27b957c2724b828474571419e845ec_RGBA_0.r;
            float _SampleTexture2D_be27b957c2724b828474571419e845ec_G_5 = _SampleTexture2D_be27b957c2724b828474571419e845ec_RGBA_0.g;
            float _SampleTexture2D_be27b957c2724b828474571419e845ec_B_6 = _SampleTexture2D_be27b957c2724b828474571419e845ec_RGBA_0.b;
            float _SampleTexture2D_be27b957c2724b828474571419e845ec_A_7 = _SampleTexture2D_be27b957c2724b828474571419e845ec_RGBA_0.a;
            float3 _NormalUnpack_d0e3596e75a741d0aa76a62e977ffc80_Out_1;
            Unity_NormalUnpack_float(_SampleTexture2D_be27b957c2724b828474571419e845ec_RGBA_0, _NormalUnpack_d0e3596e75a741d0aa76a62e977ffc80_Out_1);
            Bindings_TexelSnap_a0da51191bd8bb4419c891a18487e25b _TexelSnap_1f984dc819214c2988b4c6fbcd015440;
            _TexelSnap_1f984dc819214c2988b4c6fbcd015440.uv0 = IN.uv0;
            _TexelSnap_1f984dc819214c2988b4c6fbcd015440.uv1 = IN.uv1;
            _TexelSnap_1f984dc819214c2988b4c6fbcd015440.uv2 = IN.uv2;
            _TexelSnap_1f984dc819214c2988b4c6fbcd015440.uv3 = IN.uv3;
            float4 _TexelSnap_1f984dc819214c2988b4c6fbcd015440_snapped_1;
            SG_TexelSnap_a0da51191bd8bb4419c891a18487e25b((float4(IN.WorldSpaceNormal, 1.0)), _Property_14a34ba4a9f64f798e55fe269d5161c4_Out_0, _TexelSnap_1f984dc819214c2988b4c6fbcd015440, _TexelSnap_1f984dc819214c2988b4c6fbcd015440_snapped_1);
            float3 _NormalBlend_2a939f795c0347b98e8e35ec8dd9e751_Out_2;
            Unity_NormalBlend_float(_NormalUnpack_d0e3596e75a741d0aa76a62e977ffc80_Out_1, (_TexelSnap_1f984dc819214c2988b4c6fbcd015440_snapped_1.xyz), _NormalBlend_2a939f795c0347b98e8e35ec8dd9e751_Out_2);
            Normals_1 = (float4(_NormalBlend_2a939f795c0347b98e8e35ec8dd9e751_Out_2, 1.0));
            Geometry_Normals_2 = (_TexelSnap_1f984dc819214c2988b4c6fbcd015440_snapped_1.xyz);
            Normal_Map_3 = _NormalUnpack_d0e3596e75a741d0aa76a62e977ffc80_Out_1;
        }

        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }

        void Unity_ReflectionProbe_float(float3 ViewDir, float3 Normal, float LOD, out float3 Out)
        {
            Out = SHADERGRAPH_REFLECTION_PROBE(ViewDir, Normal, LOD);
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_Saturate_float4(float4 In, out float4 Out)
        {
            Out = saturate(In);
        }

        void Grade_float(float4 color, float cellCount, float2 texelSize, float weight, UnityTexture2D lut, out float4 graded){
            float maxColor = cellCount - 1.0;
            float2 halfCol = texelSize / 2.0;
            float threshold = maxColor / cellCount;

            float cell = floor(color.b * maxColor);
            float cellOffset = cell / cellCount;

            float2 lutPos = float2(
                (halfCol.x + color.r * threshold / cellCount) + cellOffset,
                halfCol.y + (1.0 - color.g) * threshold
            );
            float4 gradedCol = tex2D(lut, lutPos);

            graded = lerp(color, gradedCol, weight);
        }

        struct Bindings_ColorLookupTable_bb55662be3239744dbc2caf2e0d0fa31
        {
            half4 uv0;
            half4 uv1;
            half4 uv2;
            half4 uv3;
        };

        void SG_ColorLookupTable_bb55662be3239744dbc2caf2e0d0fa31(float4 Color_3a07a9fcd2b048c98880eb8b9024f77a, UnityTexture2D Texture2D_568122734b48489bb9691b81b31a52c8, float Vector1_9c1cd85c43a143bbab914e09b14c62c4, float Vector1_cfacc6c500824d0c83c4cd468cbcfc42, Bindings_ColorLookupTable_bb55662be3239744dbc2caf2e0d0fa31 IN, out float4 OutVector4_1)
        {
            float4 _Property_24d737b9799e454091a3bfdfb9944dfd_Out_0 = Color_3a07a9fcd2b048c98880eb8b9024f77a;
            float4 _Saturate_0462de655f384991900a7e13218f40a0_Out_1;
            Unity_Saturate_float4(_Property_24d737b9799e454091a3bfdfb9944dfd_Out_0, _Saturate_0462de655f384991900a7e13218f40a0_Out_1);
            float _Property_460f410c054b4de6ad54e8b704ec7bd6_Out_0 = Vector1_9c1cd85c43a143bbab914e09b14c62c4;
            UnityTexture2D _Property_c850f4a2502440e68975b6d03ec5d0e1_Out_0 = Texture2D_568122734b48489bb9691b81b31a52c8;
            float _TexelSize_38265d2262e843ba8d05c5483ad905eb_Width_0 = _Property_c850f4a2502440e68975b6d03ec5d0e1_Out_0.texelSize.z;
            float _TexelSize_38265d2262e843ba8d05c5483ad905eb_Height_2 = _Property_c850f4a2502440e68975b6d03ec5d0e1_Out_0.texelSize.w;
            float2 _Vector2_08272d93803d437ea839b5433bdbb5ec_Out_0 = float2(_TexelSize_38265d2262e843ba8d05c5483ad905eb_Width_0, _TexelSize_38265d2262e843ba8d05c5483ad905eb_Height_2);
            float _Property_0f290b42b9f54cb494ccd3d49b477537_Out_0 = Vector1_cfacc6c500824d0c83c4cd468cbcfc42;
            UnityTexture2D _Property_db9c7f6f55e9408f91370081d3522824_Out_0 = Texture2D_568122734b48489bb9691b81b31a52c8;
            float4 _GradeCustomFunction_dbfd2c9de66448fbae499c9ecd8432c2_graded_4;
            Grade_float(_Saturate_0462de655f384991900a7e13218f40a0_Out_1, _Property_460f410c054b4de6ad54e8b704ec7bd6_Out_0, _Vector2_08272d93803d437ea839b5433bdbb5ec_Out_0, _Property_0f290b42b9f54cb494ccd3d49b477537_Out_0, _Property_db9c7f6f55e9408f91370081d3522824_Out_0, _GradeCustomFunction_dbfd2c9de66448fbae499c9ecd8432c2_graded_4);
            OutVector4_1 = _GradeCustomFunction_dbfd2c9de66448fbae499c9ecd8432c2_graded_4;
        }

        struct Bindings_AdditionalLightsDithered_b549abda3241b7e4cb2999b9e86a4b7d
        {
            half4 uv0;
            half4 uv1;
            half4 uv2;
            half4 uv3;
        };

        void SG_AdditionalLightsDithered_b549abda3241b7e4cb2999b9e86a4b7d(float3 Vector3_542a1ca5e5fa4171b36867f259f2bf72, float3 Vector3_407f0568a1084b2ab93786202016f883, float3 Vector3_cbe39d4b369d47a7ba967c528ab585ef, float3 Vector3_6b00abb3883949efb3d1cff03e3d2bc0, float3 Vector3_0205087d2b4045cdb72369018e961b68, float Vector1_31e6c70ab6c34008a1f142a4b08a1fb2, UnityTexture2D Texture2D_45cfedd0c0e143b7834ef738470d6fa1, Bindings_AdditionalLightsDithered_b549abda3241b7e4cb2999b9e86a4b7d IN, out float3 Diffuse_1, out float Specular_2)
        {
            float3 _Property_1e6d382292c94d22b134c6a1a124dbab_Out_0 = Vector3_cbe39d4b369d47a7ba967c528ab585ef;
            float3 _Property_e2fe7a9c1e6443e0a249d3723cc14d07_Out_0 = Vector3_6b00abb3883949efb3d1cff03e3d2bc0;
            float3 _Property_7324b45f25e442819db1f29856027828_Out_0 = Vector3_542a1ca5e5fa4171b36867f259f2bf72;
            float3 _Property_f6b309e5102b47faa51a583a60d14f4b_Out_0 = Vector3_0205087d2b4045cdb72369018e961b68;
            float3 _Property_1a7a7948c8df47828b863f27477e1445_Out_0 = Vector3_407f0568a1084b2ab93786202016f883;
            float _Property_786a144483a845c0916637fc2153a443_Out_0 = Vector1_31e6c70ab6c34008a1f142a4b08a1fb2;
            float4 _UV_b2b8022d733d4b248e912ed465a47a80_Out_0 = IN.uv0;
            UnityTexture2D _Property_ef0bde442bb14f089ca5f5f392b09102_Out_0 = Texture2D_45cfedd0c0e143b7834ef738470d6fa1;
            float _TexelSize_8867cd14544246a687e1f264c3d7bc66_Width_0 = _Property_ef0bde442bb14f089ca5f5f392b09102_Out_0.texelSize.z;
            float _TexelSize_8867cd14544246a687e1f264c3d7bc66_Height_2 = _Property_ef0bde442bb14f089ca5f5f392b09102_Out_0.texelSize.w;
            float2 _Vector2_221f2b4cc427423a9cd601acd58553ba_Out_0 = float2(_TexelSize_8867cd14544246a687e1f264c3d7bc66_Width_0, _TexelSize_8867cd14544246a687e1f264c3d7bc66_Height_2);
            float3 _AdditionalLightsDitheredCustomFunction_1ae3189ec194405da505c54ab7247a03_Diffuse_8;
            float3 _AdditionalLightsDitheredCustomFunction_1ae3189ec194405da505c54ab7247a03_Specular_9;
            AdditionalLightsDithered_float(_Property_1e6d382292c94d22b134c6a1a124dbab_Out_0, (_Property_e2fe7a9c1e6443e0a249d3723cc14d07_Out_0).x, _Property_7324b45f25e442819db1f29856027828_Out_0, _Property_f6b309e5102b47faa51a583a60d14f4b_Out_0, _Property_1a7a7948c8df47828b863f27477e1445_Out_0, _Property_786a144483a845c0916637fc2153a443_Out_0, (_UV_b2b8022d733d4b248e912ed465a47a80_Out_0.xy), _Vector2_221f2b4cc427423a9cd601acd58553ba_Out_0, _AdditionalLightsDitheredCustomFunction_1ae3189ec194405da505c54ab7247a03_Diffuse_8, _AdditionalLightsDitheredCustomFunction_1ae3189ec194405da505c54ab7247a03_Specular_9);
            Diffuse_1 = _AdditionalLightsDitheredCustomFunction_1ae3189ec194405da505c54ab7247a03_Diffuse_8;
            Specular_2 = (_AdditionalLightsDitheredCustomFunction_1ae3189ec194405da505c54ab7247a03_Specular_9).x;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        struct Bindings_TextureSpaceDither_05c0e565c3b6982409fa16d8ee961c0a
        {
            half4 uv0;
            half4 uv1;
            half4 uv2;
            half4 uv3;
        };

        void SG_TextureSpaceDither_05c0e565c3b6982409fa16d8ee961c0a(UnityTexture2D Texture2D_5b68cb0f5cdb4882b0d68224e476ff0f, float Vector1_282560b21d2c42fdbba565c24e2e4af7, Bindings_TextureSpaceDither_05c0e565c3b6982409fa16d8ee961c0a IN, out float Out_1)
        {
            float _Property_593fb36f8b80480283a04de54afb7942_Out_0 = Vector1_282560b21d2c42fdbba565c24e2e4af7;
            float4 _UV_a99585cb3fbb4fb8bd5de40b08842806_Out_0 = IN.uv0;
            UnityTexture2D _Property_6ce295d4120540a3a5c6a9b8d1c5aa8f_Out_0 = Texture2D_5b68cb0f5cdb4882b0d68224e476ff0f;
            float _TexelSize_b8b5449fa4f84fc283d231c9caf49c71_Width_0 = _Property_6ce295d4120540a3a5c6a9b8d1c5aa8f_Out_0.texelSize.z;
            float _TexelSize_b8b5449fa4f84fc283d231c9caf49c71_Height_2 = _Property_6ce295d4120540a3a5c6a9b8d1c5aa8f_Out_0.texelSize.w;
            float2 _Vector2_e337668c8b8d4a47b907cbf62fd784e9_Out_0 = float2(_TexelSize_b8b5449fa4f84fc283d231c9caf49c71_Width_0, _TexelSize_b8b5449fa4f84fc283d231c9caf49c71_Height_2);
            float _TextureSpaceDitherCustomFunction_7ee59ad6f36a4b90a1035511300cb0fe_Out_2;
            TextureSpaceDither_float(1, (_UV_a99585cb3fbb4fb8bd5de40b08842806_Out_0.xy), _Vector2_e337668c8b8d4a47b907cbf62fd784e9_Out_0, _TextureSpaceDitherCustomFunction_7ee59ad6f36a4b90a1035511300cb0fe_Out_2);
            float _Multiply_e8862e10087041d9a3ad1b5a01e20fef_Out_2;
            Unity_Multiply_float(_Property_593fb36f8b80480283a04de54afb7942_Out_0, _TextureSpaceDitherCustomFunction_7ee59ad6f36a4b90a1035511300cb0fe_Out_2, _Multiply_e8862e10087041d9a3ad1b5a01e20fef_Out_2);
            Out_1 = _Multiply_e8862e10087041d9a3ad1b5a01e20fef_Out_2;
        }

        void Unity_OneMinus_float3(float3 In, out float3 Out)
        {
            Out = 1 - In;
        }

        void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A - B;
        }

        void Unity_Saturate_float3(float3 In, out float3 Out)
        {
            Out = saturate(In);
        }

        // 4c09902e3d8141dc3cfb16f6bca6ce7f
        #include "Assets/URP_ShaderGraphCustomLighting/CustomLighting.hlsl"

        struct Bindings_MainLight_970d962ef25c3084987fd10643902eed
        {
        };

        void SG_MainLight_970d962ef25c3084987fd10643902eed(Bindings_MainLight_970d962ef25c3084987fd10643902eed IN, out float3 Direction_1, out float3 Colour_2, out float DistanceAtten_3)
        {
            float3 _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_Direction_1;
            float3 _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_Colour_2;
            float _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_DistanceAtten_3;
            MainLight_float(_MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_Direction_1, _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_Colour_2, _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_DistanceAtten_3);
            Direction_1 = _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_Direction_1;
            Colour_2 = _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_Colour_2;
            DistanceAtten_3 = _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_DistanceAtten_3;
        }

        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }

        void Unity_Negate_float(float In, out float Out)
        {
            Out = -1 * In;
        }

        struct Bindings_MainLightShadows_f3ba9248037f69f429d9e52bc2e30940
        {
        };

        void SG_MainLightShadows_f3ba9248037f69f429d9e52bc2e30940(float3 Vector3_B87D7B6B, Bindings_MainLightShadows_f3ba9248037f69f429d9e52bc2e30940 IN, out float Atten_4)
        {
            float3 _Property_65e443cb15a9fb859a50ebae06e81569_Out_0 = Vector3_B87D7B6B;
            float _MainLightShadowsCustomFunction_3be2118d2270ff818ec5f0f1353e249f_ShadowAtten_4;
            MainLightShadows_float(_Property_65e443cb15a9fb859a50ebae06e81569_Out_0, _MainLightShadowsCustomFunction_3be2118d2270ff818ec5f0f1353e249f_ShadowAtten_4);
            Atten_4 = _MainLightShadowsCustomFunction_3be2118d2270ff818ec5f0f1353e249f_ShadowAtten_4;
        }

        void Unity_SampleGradient_float(Gradient Gradient, float Time, out float4 Out)
        {
            float3 color = Gradient.colors[0].rgb;
            [unroll]
            for (int c = 1; c < 8; c++)
            {
                float colorPos = saturate((Time - Gradient.colors[c-1].w) / (Gradient.colors[c].w - Gradient.colors[c-1].w)) * step(c, Gradient.colorsLength-1);
                color = lerp(color, Gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), Gradient.type));
            }
        #ifndef UNITY_COLORSPACE_GAMMA
            color = SRGBToLinear(color);
        #endif
            float alpha = Gradient.alphas[0].x;
            [unroll]
            for (int a = 1; a < 8; a++)
            {
                float alphaPos = saturate((Time - Gradient.alphas[a-1].y) / (Gradient.alphas[a].y - Gradient.alphas[a-1].y)) * step(a, Gradient.alphasLength-1);
                alpha = lerp(alpha, Gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), Gradient.type));
            }
            Out = float4(color, alpha);
        }

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void Unity_Subtract_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A - B;
        }

        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_BakedGIScale_float(float3 Normal, out float3 Out, float3 Position, float2 StaticUV, float2 DynamicUV)
        {
            Out = SHADERGRAPH_BAKED_GI(Position, Normal, StaticUV, DynamicUV, true);
        }

        void Unity_Length_float3(float3 In, out float Out)
        {
            Out = length(In);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Blend_Multiply_float3(float3 Base, float3 Blend, out float3 Out, float Opacity)
        {
            Out = Base * Blend;
            Out = lerp(Base, Out, Opacity);
        }

        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
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
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_c941884277494a37ae26164ab00da274_Out_0 = _BaseEmissionColor;
            UnityTexture2D _Property_2de01fbcbbb442089dc993965eb98f9b_Out_0 = UnityBuildTexture2DStructNoScale(_EmissionMap);
            float4 _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_RGBA_0 = SAMPLE_TEXTURE2D(_Property_2de01fbcbbb442089dc993965eb98f9b_Out_0.tex, _Property_2de01fbcbbb442089dc993965eb98f9b_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_R_4 = _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_RGBA_0.r;
            float _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_G_5 = _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_RGBA_0.g;
            float _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_B_6 = _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_RGBA_0.b;
            float _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_A_7 = _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_RGBA_0.a;
            float4 _Add_8f89962facb2434a9bad34d1238c2d85_Out_2;
            Unity_Add_float4(_Property_c941884277494a37ae26164ab00da274_Out_0, _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_RGBA_0, _Add_8f89962facb2434a9bad34d1238c2d85_Out_2);
            float _Property_20a4da244ff6473e9171c2056c88cd51_Out_0 = emissiveIntensity;
            float4 _Multiply_d15b022c7eb748a1b886125924916898_Out_2;
            Unity_Multiply_float(_Add_8f89962facb2434a9bad34d1238c2d85_Out_2, (_Property_20a4da244ff6473e9171c2056c88cd51_Out_0.xxxx), _Multiply_d15b022c7eb748a1b886125924916898_Out_2);
            UnityTexture2D _Property_2f39c220461b4adeb7b10ed612b4b849_Out_0 = UnityBuildTexture2DStructNoScale(_MetallicGlossMap);
            float4 _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_RGBA_0 = SAMPLE_TEXTURE2D(_Property_2f39c220461b4adeb7b10ed612b4b849_Out_0.tex, _Property_2f39c220461b4adeb7b10ed612b4b849_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_R_4 = _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_RGBA_0.r;
            float _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_G_5 = _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_RGBA_0.g;
            float _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_B_6 = _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_RGBA_0.b;
            float _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_A_7 = _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_RGBA_0.a;
            float _Property_5be87ab580e648609124b7a24d526ca7_Out_0 = baseMetallic;
            float _Add_6b1d5ca901cd4df187d07b47abd03a95_Out_2;
            Unity_Add_float(_SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_R_4, _Property_5be87ab580e648609124b7a24d526ca7_Out_0, _Add_6b1d5ca901cd4df187d07b47abd03a95_Out_2);
            float _Saturate_868d213e7ced43218e451d4326c337ff_Out_1;
            Unity_Saturate_float(_Add_6b1d5ca901cd4df187d07b47abd03a95_Out_2, _Saturate_868d213e7ced43218e451d4326c337ff_Out_1);
            UnityTexture2D _Property_bbab3daac9f3429cb25f1cede3896799_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            Bindings_TexelSnap_a0da51191bd8bb4419c891a18487e25b _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d;
            _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d.uv0 = IN.uv0;
            _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d.uv1 = IN.uv1;
            _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d.uv2 = IN.uv2;
            _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d.uv3 = IN.uv3;
            float4 _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d_snapped_1;
            SG_TexelSnap_a0da51191bd8bb4419c891a18487e25b((float4(IN.ObjectSpaceViewDirection, 1.0)), _Property_bbab3daac9f3429cb25f1cede3896799_Out_0, _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d, _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d_snapped_1);
            UnityTexture2D _Property_88bb11857beb4ceba985dba1f663cd93_Out_0 = UnityBuildTexture2DStructNoScale(_NormalMap);
            Bindings_TexelSnappedNormals_05d618dcee8af6942adba1f3f4b1fcc3 _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb;
            _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb.WorldSpaceNormal = IN.WorldSpaceNormal;
            _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb.uv0 = IN.uv0;
            _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb.uv1 = IN.uv1;
            _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb.uv2 = IN.uv2;
            _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb.uv3 = IN.uv3;
            float4 _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_Normals_1;
            float3 _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_GeometryNormals_2;
            float3 _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_NormalMap_3;
            SG_TexelSnappedNormals_05d618dcee8af6942adba1f3f4b1fcc3(_Property_88bb11857beb4ceba985dba1f663cd93_Out_0, _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb, _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_Normals_1, _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_GeometryNormals_2, _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_NormalMap_3);
            float _Property_7bfcc3514a6a49d081a1cc0da7052616_Out_0 = baseSmoothness;
            float _Add_b75b350c99524afa9674c3eb95d85c1f_Out_2;
            Unity_Add_float(_SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_A_7, _Property_7bfcc3514a6a49d081a1cc0da7052616_Out_0, _Add_b75b350c99524afa9674c3eb95d85c1f_Out_2);
            float _Saturate_e72819e5013242f4b13f6eccfbbd188b_Out_1;
            Unity_Saturate_float(_Add_b75b350c99524afa9674c3eb95d85c1f_Out_2, _Saturate_e72819e5013242f4b13f6eccfbbd188b_Out_1);
            float _OneMinus_b006d55dc8f944e9b6ea1eca594b665a_Out_1;
            Unity_OneMinus_float(_Saturate_e72819e5013242f4b13f6eccfbbd188b_Out_1, _OneMinus_b006d55dc8f944e9b6ea1eca594b665a_Out_1);
            float3 _ReflectionProbe_0fbedfcd963141519565e3cbce08ddcd_Out_3;
            Unity_ReflectionProbe_float((_TexelSnap_1aa8132355fb4782bd63c3e3aba4887d_snapped_1.xyz), (_TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_Normals_1.xyz), _OneMinus_b006d55dc8f944e9b6ea1eca594b665a_Out_1, _ReflectionProbe_0fbedfcd963141519565e3cbce08ddcd_Out_3);
            float3 _Multiply_7deba2adf29a4631bc6985a49f1f9f1d_Out_2;
            Unity_Multiply_float((_Saturate_868d213e7ced43218e451d4326c337ff_Out_1.xxx), _ReflectionProbe_0fbedfcd963141519565e3cbce08ddcd_Out_3, _Multiply_7deba2adf29a4631bc6985a49f1f9f1d_Out_2);
            UnityTexture2D _Property_b86e47fb642747279c931dddb61a2232_Out_0 = UnityBuildTexture2DStructNoScale(paletteTexture);
            float _Property_1c1b9ac30fa44b29b79f161d13393f9f_Out_0 = paletteCellCount;
            float _Property_fabaa22253e941ab9c264648b09fe453_Out_0 = paletteEffect;
            Bindings_ColorLookupTable_bb55662be3239744dbc2caf2e0d0fa31 _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef;
            _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef.uv0 = IN.uv0;
            _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef.uv1 = IN.uv1;
            _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef.uv2 = IN.uv2;
            _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef.uv3 = IN.uv3;
            float4 _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef_OutVector4_1;
            SG_ColorLookupTable_bb55662be3239744dbc2caf2e0d0fa31((float4(_Multiply_7deba2adf29a4631bc6985a49f1f9f1d_Out_2, 1.0)), _Property_b86e47fb642747279c931dddb61a2232_Out_0, _Property_1c1b9ac30fa44b29b79f161d13393f9f_Out_0, _Property_fabaa22253e941ab9c264648b09fe453_Out_0, _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef, _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef_OutVector4_1);
            UnityTexture2D _Property_fd99d55ef8ed46e89b2b23af9b98788f_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            Bindings_TexelSnap_a0da51191bd8bb4419c891a18487e25b _TexelSnap_704d249c9dcd429aaa3348a761777794;
            _TexelSnap_704d249c9dcd429aaa3348a761777794.uv0 = IN.uv0;
            _TexelSnap_704d249c9dcd429aaa3348a761777794.uv1 = IN.uv1;
            _TexelSnap_704d249c9dcd429aaa3348a761777794.uv2 = IN.uv2;
            _TexelSnap_704d249c9dcd429aaa3348a761777794.uv3 = IN.uv3;
            float4 _TexelSnap_704d249c9dcd429aaa3348a761777794_snapped_1;
            SG_TexelSnap_a0da51191bd8bb4419c891a18487e25b((float4(IN.WorldSpacePosition, 1.0)), _Property_fd99d55ef8ed46e89b2b23af9b98788f_Out_0, _TexelSnap_704d249c9dcd429aaa3348a761777794, _TexelSnap_704d249c9dcd429aaa3348a761777794_snapped_1);
            UnityTexture2D _Property_db3e1603d31147d79ce721863ac82072_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            Bindings_TexelSnap_a0da51191bd8bb4419c891a18487e25b _TexelSnap_a97232639f604377b93579cb8f5695c9;
            _TexelSnap_a97232639f604377b93579cb8f5695c9.uv0 = IN.uv0;
            _TexelSnap_a97232639f604377b93579cb8f5695c9.uv1 = IN.uv1;
            _TexelSnap_a97232639f604377b93579cb8f5695c9.uv2 = IN.uv2;
            _TexelSnap_a97232639f604377b93579cb8f5695c9.uv3 = IN.uv3;
            float4 _TexelSnap_a97232639f604377b93579cb8f5695c9_snapped_1;
            SG_TexelSnap_a0da51191bd8bb4419c891a18487e25b((float4(IN.WorldSpaceViewDirection, 1.0)), _Property_db3e1603d31147d79ce721863ac82072_Out_0, _TexelSnap_a97232639f604377b93579cb8f5695c9, _TexelSnap_a97232639f604377b93579cb8f5695c9_snapped_1);
            float4 Color_04682488ec7a4191ba35d99753cc2f4b = IsGammaSpace() ? float4(1, 1, 1, 0) : float4(SRGBToLinear(float3(1, 1, 1)), 0);
            float _Property_b10828a656a641b3baed474ddd99707e_Out_0 = baseDitherWeight;
            float _Add_48670c9162604dc994d2e17304275c38_Out_2;
            Unity_Add_float(_SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_G_5, _Property_b10828a656a641b3baed474ddd99707e_Out_0, _Add_48670c9162604dc994d2e17304275c38_Out_2);
            float _Saturate_615c2206e6494cb99dc7427db55e03a8_Out_1;
            Unity_Saturate_float(_Add_48670c9162604dc994d2e17304275c38_Out_2, _Saturate_615c2206e6494cb99dc7427db55e03a8_Out_1);
            Bindings_AdditionalLightsDithered_b549abda3241b7e4cb2999b9e86a4b7d _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce;
            _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce.uv0 = IN.uv0;
            _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce.uv1 = IN.uv1;
            _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce.uv2 = IN.uv2;
            _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce.uv3 = IN.uv3;
            float3 _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce_Diffuse_1;
            float _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce_Specular_2;
            SG_AdditionalLightsDithered_b549abda3241b7e4cb2999b9e86a4b7d((_TexelSnap_704d249c9dcd429aaa3348a761777794_snapped_1.xyz), (_TexelSnap_a97232639f604377b93579cb8f5695c9_snapped_1.xyz), (Color_04682488ec7a4191ba35d99753cc2f4b.xyz), (_OneMinus_b006d55dc8f944e9b6ea1eca594b665a_Out_1.xxx), (_TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_Normals_1.xyz), _Saturate_615c2206e6494cb99dc7427db55e03a8_Out_1, _Property_db3e1603d31147d79ce721863ac82072_Out_0, _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce, _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce_Diffuse_1, _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce_Specular_2);
            float3 _Add_18327d860c784ad8be78e5ad2b148094_Out_2;
            Unity_Add_float3(_AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce_Diffuse_1, (_AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce_Specular_2.xxx), _Add_18327d860c784ad8be78e5ad2b148094_Out_2);
            UnityTexture2D _Property_e1eacbdca5bf4749aaa4888d9bada48a_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            Bindings_TextureSpaceDither_05c0e565c3b6982409fa16d8ee961c0a _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597;
            _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597.uv0 = IN.uv0;
            _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597.uv1 = IN.uv1;
            _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597.uv2 = IN.uv2;
            _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597.uv3 = IN.uv3;
            float _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597_Out_1;
            SG_TextureSpaceDither_05c0e565c3b6982409fa16d8ee961c0a(_Property_e1eacbdca5bf4749aaa4888d9bada48a_Out_0, _Saturate_615c2206e6494cb99dc7427db55e03a8_Out_1, _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597, _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597_Out_1);
            float3 _OneMinus_e2f59e0a622243c2ba647060620fd9e8_Out_1;
            Unity_OneMinus_float3(_Add_18327d860c784ad8be78e5ad2b148094_Out_2, _OneMinus_e2f59e0a622243c2ba647060620fd9e8_Out_1);
            float3 _Multiply_531d81b4443e4f40b7826a66217e7a3c_Out_2;
            Unity_Multiply_float((_TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597_Out_1.xxx), _OneMinus_e2f59e0a622243c2ba647060620fd9e8_Out_1, _Multiply_531d81b4443e4f40b7826a66217e7a3c_Out_2);
            float3 _Subtract_30d76b96f1d2410794631360f9092eb4_Out_2;
            Unity_Subtract_float3(_Add_18327d860c784ad8be78e5ad2b148094_Out_2, _Multiply_531d81b4443e4f40b7826a66217e7a3c_Out_2, _Subtract_30d76b96f1d2410794631360f9092eb4_Out_2);
            float3 _Saturate_38e6b7a29c3142fcaf2e87384db2933b_Out_1;
            Unity_Saturate_float3(_Subtract_30d76b96f1d2410794631360f9092eb4_Out_2, _Saturate_38e6b7a29c3142fcaf2e87384db2933b_Out_1);
            Bindings_MainLight_970d962ef25c3084987fd10643902eed _MainLight_9871f869140e4d26ac0824e8758383bf;
            float3 _MainLight_9871f869140e4d26ac0824e8758383bf_Direction_1;
            float3 _MainLight_9871f869140e4d26ac0824e8758383bf_Colour_2;
            float _MainLight_9871f869140e4d26ac0824e8758383bf_DistanceAtten_3;
            SG_MainLight_970d962ef25c3084987fd10643902eed(_MainLight_9871f869140e4d26ac0824e8758383bf, _MainLight_9871f869140e4d26ac0824e8758383bf_Direction_1, _MainLight_9871f869140e4d26ac0824e8758383bf_Colour_2, _MainLight_9871f869140e4d26ac0824e8758383bf_DistanceAtten_3);
            float _DotProduct_ea443ec73d4c4497adaee6c764034864_Out_2;
            Unity_DotProduct_float3((_TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_Normals_1.xyz), _MainLight_9871f869140e4d26ac0824e8758383bf_Direction_1, _DotProduct_ea443ec73d4c4497adaee6c764034864_Out_2);
            float _Negate_1be95d99f91f4fdeb6ae39448ab5420c_Out_1;
            Unity_Negate_float(_DotProduct_ea443ec73d4c4497adaee6c764034864_Out_2, _Negate_1be95d99f91f4fdeb6ae39448ab5420c_Out_1);
            float _OneMinus_692a0d5845704c5c89da0d5336eb151d_Out_1;
            Unity_OneMinus_float(_Negate_1be95d99f91f4fdeb6ae39448ab5420c_Out_1, _OneMinus_692a0d5845704c5c89da0d5336eb151d_Out_1);
            float3 _Multiply_9f158e5167dd456db56e79b69fca6ed8_Out_2;
            Unity_Multiply_float(_Saturate_38e6b7a29c3142fcaf2e87384db2933b_Out_1, (_OneMinus_692a0d5845704c5c89da0d5336eb151d_Out_1.xxx), _Multiply_9f158e5167dd456db56e79b69fca6ed8_Out_2);
            float4 _Property_d9517f2134134053b6484ca147ee80ad_Out_0 = shadowColor;
            Bindings_MainLightShadows_f3ba9248037f69f429d9e52bc2e30940 _MainLightShadows_3b258aa64ee94a0eb9831fdd16e12468;
            float _MainLightShadows_3b258aa64ee94a0eb9831fdd16e12468_Atten_4;
            SG_MainLightShadows_f3ba9248037f69f429d9e52bc2e30940((_TexelSnap_704d249c9dcd429aaa3348a761777794_snapped_1.xyz), _MainLightShadows_3b258aa64ee94a0eb9831fdd16e12468, _MainLightShadows_3b258aa64ee94a0eb9831fdd16e12468_Atten_4);
            float4 _SampleGradient_f0fd477e2e5e456c9f261f785dba3234_Out_2;
            Unity_SampleGradient_float(NewGradient(0, 2, 2, float4(0, 0, 0, 0),float4(1, 1, 1, 0.4),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0), float2(1, 0),float2(1, 1),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0)), _DotProduct_ea443ec73d4c4497adaee6c764034864_Out_2, _SampleGradient_f0fd477e2e5e456c9f261f785dba3234_Out_2);
            float4 _Multiply_c56132f6a8004be09e7d68721849ba3c_Out_2;
            Unity_Multiply_float((_MainLightShadows_3b258aa64ee94a0eb9831fdd16e12468_Atten_4.xxxx), _SampleGradient_f0fd477e2e5e456c9f261f785dba3234_Out_2, _Multiply_c56132f6a8004be09e7d68721849ba3c_Out_2);
            float4 _OneMinus_e183316756e04a9aa6eb88789810f1a9_Out_1;
            Unity_OneMinus_float4(_Multiply_c56132f6a8004be09e7d68721849ba3c_Out_2, _OneMinus_e183316756e04a9aa6eb88789810f1a9_Out_1);
            float4 _Multiply_b37c707241e643bdb9d50d1d8f3bfc58_Out_2;
            Unity_Multiply_float((_TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597_Out_1.xxxx), _OneMinus_e183316756e04a9aa6eb88789810f1a9_Out_1, _Multiply_b37c707241e643bdb9d50d1d8f3bfc58_Out_2);
            float4 _Subtract_68ee4abf5e8441adb7160e46ac7c878c_Out_2;
            Unity_Subtract_float4(_Multiply_c56132f6a8004be09e7d68721849ba3c_Out_2, _Multiply_b37c707241e643bdb9d50d1d8f3bfc58_Out_2, _Subtract_68ee4abf5e8441adb7160e46ac7c878c_Out_2);
            float4 _Saturate_519033122f9b4762a794ebb97f82fb71_Out_1;
            Unity_Saturate_float4(_Subtract_68ee4abf5e8441adb7160e46ac7c878c_Out_2, _Saturate_519033122f9b4762a794ebb97f82fb71_Out_1);
            float3 _Lerp_70199ec603d14a9fb09f0f7a2a3702ba_Out_3;
            Unity_Lerp_float3((_Property_d9517f2134134053b6484ca147ee80ad_Out_0.xyz), _MainLight_9871f869140e4d26ac0824e8758383bf_Colour_2, (_Saturate_519033122f9b4762a794ebb97f82fb71_Out_1.xyz), _Lerp_70199ec603d14a9fb09f0f7a2a3702ba_Out_3);
            float4 _UV_f363edf81e644ad4aa7551177bd1ab9f_Out_0 = IN.uv1;
            UnityTexture2D _Property_44b2115ff3d1423a8e162102bac7d0d6_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            Bindings_TexelSnap_a0da51191bd8bb4419c891a18487e25b _TexelSnap_9ef883d11b974d7fac11c413d42fccad;
            _TexelSnap_9ef883d11b974d7fac11c413d42fccad.uv0 = IN.uv0;
            _TexelSnap_9ef883d11b974d7fac11c413d42fccad.uv1 = IN.uv1;
            _TexelSnap_9ef883d11b974d7fac11c413d42fccad.uv2 = IN.uv2;
            _TexelSnap_9ef883d11b974d7fac11c413d42fccad.uv3 = IN.uv3;
            float4 _TexelSnap_9ef883d11b974d7fac11c413d42fccad_snapped_1;
            SG_TexelSnap_a0da51191bd8bb4419c891a18487e25b(_UV_f363edf81e644ad4aa7551177bd1ab9f_Out_0, _Property_44b2115ff3d1423a8e162102bac7d0d6_Out_0, _TexelSnap_9ef883d11b974d7fac11c413d42fccad, _TexelSnap_9ef883d11b974d7fac11c413d42fccad_snapped_1);
            float3 _BakedGI_2c079a9783da4e6887c52dfc2eef4632_Out_1;
            Unity_BakedGIScale_float(IN.WorldSpaceNormal, _BakedGI_2c079a9783da4e6887c52dfc2eef4632_Out_1, IN.WorldSpacePosition, (_TexelSnap_9ef883d11b974d7fac11c413d42fccad_snapped_1.xy), IN.uv2.xy);
            float3 _OneMinus_4c9db1307c0a44518f09efaffdc9bf02_Out_1;
            Unity_OneMinus_float3(_BakedGI_2c079a9783da4e6887c52dfc2eef4632_Out_1, _OneMinus_4c9db1307c0a44518f09efaffdc9bf02_Out_1);
            float _Length_336e3d70970f4fa9acd463d7ea614895_Out_1;
            Unity_Length_float3(_OneMinus_4c9db1307c0a44518f09efaffdc9bf02_Out_1, _Length_336e3d70970f4fa9acd463d7ea614895_Out_1);
            float _OneMinus_9ce836eb4a0645cb857fe80311fc34e5_Out_1;
            Unity_OneMinus_float(_Length_336e3d70970f4fa9acd463d7ea614895_Out_1, _OneMinus_9ce836eb4a0645cb857fe80311fc34e5_Out_1);
            Bindings_TextureSpaceDither_05c0e565c3b6982409fa16d8ee961c0a _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc;
            _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc.uv0 = IN.uv0;
            _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc.uv1 = IN.uv1;
            _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc.uv2 = IN.uv2;
            _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc.uv3 = IN.uv3;
            float _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc_Out_1;
            SG_TextureSpaceDither_05c0e565c3b6982409fa16d8ee961c0a(_Property_44b2115ff3d1423a8e162102bac7d0d6_Out_0, 1, _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc, _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc_Out_1);
            float _Multiply_9aad79adaeaf43d3a02ed5b58036a37e_Out_2;
            Unity_Multiply_float(_OneMinus_9ce836eb4a0645cb857fe80311fc34e5_Out_1, _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc_Out_1, _Multiply_9aad79adaeaf43d3a02ed5b58036a37e_Out_2);
            float _Subtract_82035a96c026427a8d0bd10d7c050c7e_Out_2;
            Unity_Subtract_float(_Length_336e3d70970f4fa9acd463d7ea614895_Out_1, _Multiply_9aad79adaeaf43d3a02ed5b58036a37e_Out_2, _Subtract_82035a96c026427a8d0bd10d7c050c7e_Out_2);
            float _Saturate_fbc01d3577304322abbf653d1699857b_Out_1;
            Unity_Saturate_float(_Subtract_82035a96c026427a8d0bd10d7c050c7e_Out_2, _Saturate_fbc01d3577304322abbf653d1699857b_Out_1);
            float _Property_83c9c4586edc461da172390db6696a94_Out_0 = lightmapWeight;
            float _Multiply_b73b7f8e106943ed949a9613e74fb8df_Out_2;
            Unity_Multiply_float(_Saturate_fbc01d3577304322abbf653d1699857b_Out_1, _Property_83c9c4586edc461da172390db6696a94_Out_0, _Multiply_b73b7f8e106943ed949a9613e74fb8df_Out_2);
            float3 _Lerp_2b4003eaf1f54c089a66fe31f1147af2_Out_3;
            Unity_Lerp_float3(float3(1, 1, 1), _BakedGI_2c079a9783da4e6887c52dfc2eef4632_Out_1, (_Multiply_b73b7f8e106943ed949a9613e74fb8df_Out_2.xxx), _Lerp_2b4003eaf1f54c089a66fe31f1147af2_Out_3);
            UnityTexture2D _Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0 = SAMPLE_TEXTURE2D(_Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0.tex, _Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_R_4 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.r;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_G_5 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.g;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_B_6 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.b;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_A_7 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.a;
            float3 _Blend_74a05fcecbd04f5ead24cf34660db158_Out_2;
            Unity_Blend_Multiply_float3(_Lerp_2b4003eaf1f54c089a66fe31f1147af2_Out_3, (_SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.xyz), _Blend_74a05fcecbd04f5ead24cf34660db158_Out_2, 1);
            float3 _Multiply_60c62bf144dc4730b4e2315c99e301bb_Out_2;
            Unity_Multiply_float(_Lerp_70199ec603d14a9fb09f0f7a2a3702ba_Out_3, _Blend_74a05fcecbd04f5ead24cf34660db158_Out_2, _Multiply_60c62bf144dc4730b4e2315c99e301bb_Out_2);
            float3 _Add_84c961f5681f408793466db5a074ea33_Out_2;
            Unity_Add_float3(_Multiply_9f158e5167dd456db56e79b69fca6ed8_Out_2, _Multiply_60c62bf144dc4730b4e2315c99e301bb_Out_2, _Add_84c961f5681f408793466db5a074ea33_Out_2);
            UnityTexture2D _Property_4868b464b51048cc8a3ba7793391d3e8_Out_0 = UnityBuildTexture2DStructNoScale(paletteTexture);
            float _Property_cac7a5b1a01949b8b6e92baf790c327e_Out_0 = paletteCellCount;
            float _Property_b806675d29b2457bb30a72c5221c4ec3_Out_0 = paletteEffect;
            Bindings_ColorLookupTable_bb55662be3239744dbc2caf2e0d0fa31 _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51;
            _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51.uv0 = IN.uv0;
            _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51.uv1 = IN.uv1;
            _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51.uv2 = IN.uv2;
            _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51.uv3 = IN.uv3;
            float4 _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51_OutVector4_1;
            SG_ColorLookupTable_bb55662be3239744dbc2caf2e0d0fa31((float4(_Add_84c961f5681f408793466db5a074ea33_Out_2, 1.0)), _Property_4868b464b51048cc8a3ba7793391d3e8_Out_0, _Property_cac7a5b1a01949b8b6e92baf790c327e_Out_0, _Property_b806675d29b2457bb30a72c5221c4ec3_Out_0, _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51, _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51_OutVector4_1);
            float4 _Add_20f7a826a6ad4252935c81469c664fdf_Out_2;
            Unity_Add_float4(_ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef_OutVector4_1, _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51_OutVector4_1, _Add_20f7a826a6ad4252935c81469c664fdf_Out_2);
            float4 _Add_7b66094e58664d8086b450e780d230ad_Out_2;
            Unity_Add_float4(_Multiply_d15b022c7eb748a1b886125924916898_Out_2, _Add_20f7a826a6ad4252935c81469c664fdf_Out_2, _Add_7b66094e58664d8086b450e780d230ad_Out_2);
            float _Property_9706510f749941aa816114d2f229b348_Out_0 = overallTransparency;
            float _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2;
            Unity_Multiply_float(_Property_9706510f749941aa816114d2f229b348_Out_0, _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_A_7, _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2);
            float _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2;
            Unity_Dither_float(1, float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2);
            surface.BaseColor = (_Add_7b66094e58664d8086b450e780d230ad_Out_2.xyz);
            surface.Alpha = _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2;
            surface.AlphaClipThreshold = _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2;
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
            output.ObjectSpaceViewDirection =    TransformWorldToObjectDir(output.WorldSpaceViewDirection);
            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                         input.texCoord0;
            output.uv1 =                         input.texCoord1;
            output.uv2 =                         input.texCoord2;
            output.uv3 =                         input.texCoord3;
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
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"

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
            Cull Off
        Blend One Zero
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
            #define _AlphaClip 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
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
            float3 positionWS;
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
            float3 WorldSpacePosition;
            float4 ScreenPosition;
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
            output.interp0.xyz =  input.positionWS;
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
            output.positionWS = input.interp0.xyz;
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
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float4 _NormalMap_TexelSize;
        float4 _MetallicGlossMap_TexelSize;
        float baseMetallic;
        float baseSmoothness;
        float baseDitherWeight;
        float4 _EmissionMap_TexelSize;
        float4 _BaseEmissionColor;
        float emissiveIntensity;
        float4 shadowColor;
        float overallTransparency;
        float4 paletteTexture_TexelSize;
        float paletteCellCount;
        float paletteEffect;
        float lightmapWeight;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_NormalMap);
        SAMPLER(sampler_NormalMap);
        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_EmissionMap);
        SAMPLER(sampler_EmissionMap);
        TEXTURE2D(paletteTexture);
        SAMPLER(samplerpaletteTexture);

            // Graph Functions

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
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
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_9706510f749941aa816114d2f229b348_Out_0 = overallTransparency;
            UnityTexture2D _Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0 = SAMPLE_TEXTURE2D(_Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0.tex, _Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_R_4 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.r;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_G_5 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.g;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_B_6 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.b;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_A_7 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.a;
            float _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2;
            Unity_Multiply_float(_Property_9706510f749941aa816114d2f229b348_Out_0, _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_A_7, _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2);
            float _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2;
            Unity_Dither_float(1, float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2);
            surface.Alpha = _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2;
            surface.AlphaClipThreshold = _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2;
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





            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
            Cull Off
        Blend One Zero
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
            #define _AlphaClip 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
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
            float3 positionWS;
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
            float3 WorldSpacePosition;
            float4 ScreenPosition;
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
            output.interp0.xyz =  input.positionWS;
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
            output.positionWS = input.interp0.xyz;
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
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float4 _NormalMap_TexelSize;
        float4 _MetallicGlossMap_TexelSize;
        float baseMetallic;
        float baseSmoothness;
        float baseDitherWeight;
        float4 _EmissionMap_TexelSize;
        float4 _BaseEmissionColor;
        float emissiveIntensity;
        float4 shadowColor;
        float overallTransparency;
        float4 paletteTexture_TexelSize;
        float paletteCellCount;
        float paletteEffect;
        float lightmapWeight;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_NormalMap);
        SAMPLER(sampler_NormalMap);
        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_EmissionMap);
        SAMPLER(sampler_EmissionMap);
        TEXTURE2D(paletteTexture);
        SAMPLER(samplerpaletteTexture);

            // Graph Functions

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
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
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_9706510f749941aa816114d2f229b348_Out_0 = overallTransparency;
            UnityTexture2D _Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0 = SAMPLE_TEXTURE2D(_Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0.tex, _Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_R_4 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.r;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_G_5 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.g;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_B_6 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.b;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_A_7 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.a;
            float _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2;
            Unity_Multiply_float(_Property_9706510f749941aa816114d2f229b348_Out_0, _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_A_7, _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2);
            float _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2;
            Unity_Dither_float(1, float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2);
            surface.Alpha = _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2;
            surface.AlphaClipThreshold = _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2;
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





            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 2.0

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaUnlit

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitMetaPass.hlsl"

            ENDHLSL
        }
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "UniversalMaterialType" = "Unlit"
            "Queue"="AlphaTest"
        }
        Pass
        {
            Name "Pass"
            Tags
            {
                // LightMode: <None>
            }

            // Render State
            Cull Off
        Blend One Zero
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
        #pragma shader_feature _ _SAMPLE_GI
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
        #pragma multi_compile _ _SHADOWS_SOFT



            // Defines
            #define _AlphaClip 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_TEXCOORD2
            #define ATTRIBUTES_NEED_TEXCOORD3
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD1
            #define VARYINGS_NEED_TEXCOORD2
            #define VARYINGS_NEED_TEXCOORD3
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_UNLIT
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
            float4 uv2 : TEXCOORD2;
            float4 uv3 : TEXCOORD3;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float3 normalWS;
            float4 texCoord0;
            float4 texCoord1;
            float4 texCoord2;
            float4 texCoord3;
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
            float3 ObjectSpaceViewDirection;
            float3 WorldSpaceViewDirection;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 uv0;
            float4 uv1;
            float4 uv2;
            float4 uv3;
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
            float4 interp4 : TEXCOORD4;
            float4 interp5 : TEXCOORD5;
            float3 interp6 : TEXCOORD6;
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
            output.interp2.xyzw =  input.texCoord0;
            output.interp3.xyzw =  input.texCoord1;
            output.interp4.xyzw =  input.texCoord2;
            output.interp5.xyzw =  input.texCoord3;
            output.interp6.xyz =  input.viewDirectionWS;
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
            output.texCoord0 = input.interp2.xyzw;
            output.texCoord1 = input.interp3.xyzw;
            output.texCoord2 = input.interp4.xyzw;
            output.texCoord3 = input.interp5.xyzw;
            output.viewDirectionWS = input.interp6.xyz;
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
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float4 _NormalMap_TexelSize;
        float4 _MetallicGlossMap_TexelSize;
        float baseMetallic;
        float baseSmoothness;
        float baseDitherWeight;
        float4 _EmissionMap_TexelSize;
        float4 _BaseEmissionColor;
        float emissiveIntensity;
        float4 shadowColor;
        float overallTransparency;
        float4 paletteTexture_TexelSize;
        float paletteCellCount;
        float paletteEffect;
        float lightmapWeight;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_NormalMap);
        SAMPLER(sampler_NormalMap);
        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_EmissionMap);
        SAMPLER(sampler_EmissionMap);
        TEXTURE2D(paletteTexture);
        SAMPLER(samplerpaletteTexture);

            // Graph Functions

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }

        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }

        // ae574a596f27ccc1c05e27ac40f795d7
        #include "Assets/Banchou/Shaders/Subgraphs/SubgraphFunctions.hlsl"

        struct Bindings_TexelSnap_a0da51191bd8bb4419c891a18487e25b
        {
            half4 uv0;
            half4 uv1;
            half4 uv2;
            half4 uv3;
        };

        void SG_TexelSnap_a0da51191bd8bb4419c891a18487e25b(float4 Vector4_19dcf0963d8d462598f362cf77471f80, UnityTexture2D Texture2D_7b88a15474c645e6b5163ef5300931d5, Bindings_TexelSnap_a0da51191bd8bb4419c891a18487e25b IN, out float4 snapped_1)
        {
            float4 _Property_3463cefa50804f4eb376808e0343f01c_Out_0 = Vector4_19dcf0963d8d462598f362cf77471f80;
            float4 _UV_f620b3918f5f41a4bdb2669862b09dc0_Out_0 = IN.uv0;
            UnityTexture2D _Property_05d17b1534134d50be9ab9b75cbbda55_Out_0 = Texture2D_7b88a15474c645e6b5163ef5300931d5;
            float _TexelSize_59b7504c2b6f448fb221cd456846b96a_Width_0 = _Property_05d17b1534134d50be9ab9b75cbbda55_Out_0.texelSize.z;
            float _TexelSize_59b7504c2b6f448fb221cd456846b96a_Height_2 = _Property_05d17b1534134d50be9ab9b75cbbda55_Out_0.texelSize.w;
            float2 _Vector2_eb58a01f34ed40d4a868b1877bd2c496_Out_0 = float2(_TexelSize_59b7504c2b6f448fb221cd456846b96a_Width_0, _TexelSize_59b7504c2b6f448fb221cd456846b96a_Height_2);
            float4 _TexelSnapCustomFunction_d150667e7dc74ebba201834086a417bf_snapped_2;
            TexelSnap_float(_Property_3463cefa50804f4eb376808e0343f01c_Out_0, (_UV_f620b3918f5f41a4bdb2669862b09dc0_Out_0.xy), _Vector2_eb58a01f34ed40d4a868b1877bd2c496_Out_0, _TexelSnapCustomFunction_d150667e7dc74ebba201834086a417bf_snapped_2);
            snapped_1 = _TexelSnapCustomFunction_d150667e7dc74ebba201834086a417bf_snapped_2;
        }

        void Unity_NormalUnpack_float(float4 In, out float3 Out)
        {
                        Out = UnpackNormal(In);
                    }

        void Unity_NormalBlend_float(float3 A, float3 B, out float3 Out)
        {
            Out = SafeNormalize(float3(A.rg + B.rg, A.b * B.b));
        }

        struct Bindings_TexelSnappedNormals_05d618dcee8af6942adba1f3f4b1fcc3
        {
            float3 WorldSpaceNormal;
            half4 uv0;
            half4 uv1;
            half4 uv2;
            half4 uv3;
        };

        void SG_TexelSnappedNormals_05d618dcee8af6942adba1f3f4b1fcc3(UnityTexture2D Texture2D_d7cdf2ee12034e1ab25769cae5275851, Bindings_TexelSnappedNormals_05d618dcee8af6942adba1f3f4b1fcc3 IN, out float4 Normals_1, out float3 Geometry_Normals_2, out float3 Normal_Map_3)
        {
            UnityTexture2D _Property_14a34ba4a9f64f798e55fe269d5161c4_Out_0 = Texture2D_d7cdf2ee12034e1ab25769cae5275851;
            float4 _SampleTexture2D_be27b957c2724b828474571419e845ec_RGBA_0 = SAMPLE_TEXTURE2D(_Property_14a34ba4a9f64f798e55fe269d5161c4_Out_0.tex, _Property_14a34ba4a9f64f798e55fe269d5161c4_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_be27b957c2724b828474571419e845ec_R_4 = _SampleTexture2D_be27b957c2724b828474571419e845ec_RGBA_0.r;
            float _SampleTexture2D_be27b957c2724b828474571419e845ec_G_5 = _SampleTexture2D_be27b957c2724b828474571419e845ec_RGBA_0.g;
            float _SampleTexture2D_be27b957c2724b828474571419e845ec_B_6 = _SampleTexture2D_be27b957c2724b828474571419e845ec_RGBA_0.b;
            float _SampleTexture2D_be27b957c2724b828474571419e845ec_A_7 = _SampleTexture2D_be27b957c2724b828474571419e845ec_RGBA_0.a;
            float3 _NormalUnpack_d0e3596e75a741d0aa76a62e977ffc80_Out_1;
            Unity_NormalUnpack_float(_SampleTexture2D_be27b957c2724b828474571419e845ec_RGBA_0, _NormalUnpack_d0e3596e75a741d0aa76a62e977ffc80_Out_1);
            Bindings_TexelSnap_a0da51191bd8bb4419c891a18487e25b _TexelSnap_1f984dc819214c2988b4c6fbcd015440;
            _TexelSnap_1f984dc819214c2988b4c6fbcd015440.uv0 = IN.uv0;
            _TexelSnap_1f984dc819214c2988b4c6fbcd015440.uv1 = IN.uv1;
            _TexelSnap_1f984dc819214c2988b4c6fbcd015440.uv2 = IN.uv2;
            _TexelSnap_1f984dc819214c2988b4c6fbcd015440.uv3 = IN.uv3;
            float4 _TexelSnap_1f984dc819214c2988b4c6fbcd015440_snapped_1;
            SG_TexelSnap_a0da51191bd8bb4419c891a18487e25b((float4(IN.WorldSpaceNormal, 1.0)), _Property_14a34ba4a9f64f798e55fe269d5161c4_Out_0, _TexelSnap_1f984dc819214c2988b4c6fbcd015440, _TexelSnap_1f984dc819214c2988b4c6fbcd015440_snapped_1);
            float3 _NormalBlend_2a939f795c0347b98e8e35ec8dd9e751_Out_2;
            Unity_NormalBlend_float(_NormalUnpack_d0e3596e75a741d0aa76a62e977ffc80_Out_1, (_TexelSnap_1f984dc819214c2988b4c6fbcd015440_snapped_1.xyz), _NormalBlend_2a939f795c0347b98e8e35ec8dd9e751_Out_2);
            Normals_1 = (float4(_NormalBlend_2a939f795c0347b98e8e35ec8dd9e751_Out_2, 1.0));
            Geometry_Normals_2 = (_TexelSnap_1f984dc819214c2988b4c6fbcd015440_snapped_1.xyz);
            Normal_Map_3 = _NormalUnpack_d0e3596e75a741d0aa76a62e977ffc80_Out_1;
        }

        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }

        void Unity_ReflectionProbe_float(float3 ViewDir, float3 Normal, float LOD, out float3 Out)
        {
            Out = SHADERGRAPH_REFLECTION_PROBE(ViewDir, Normal, LOD);
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_Saturate_float4(float4 In, out float4 Out)
        {
            Out = saturate(In);
        }

        void Grade_float(float4 color, float cellCount, float2 texelSize, float weight, UnityTexture2D lut, out float4 graded){
            float maxColor = cellCount - 1.0;
            float2 halfCol = texelSize / 2.0;
            float threshold = maxColor / cellCount;

            float cell = floor(color.b * maxColor);
            float cellOffset = cell / cellCount;

            float2 lutPos = float2(
                (halfCol.x + color.r * threshold / cellCount) + cellOffset,
                halfCol.y + (1.0 - color.g) * threshold
            );
            float4 gradedCol = tex2D(lut, lutPos);

            graded = lerp(color, gradedCol, weight);
        }

        struct Bindings_ColorLookupTable_bb55662be3239744dbc2caf2e0d0fa31
        {
            half4 uv0;
            half4 uv1;
            half4 uv2;
            half4 uv3;
        };

        void SG_ColorLookupTable_bb55662be3239744dbc2caf2e0d0fa31(float4 Color_3a07a9fcd2b048c98880eb8b9024f77a, UnityTexture2D Texture2D_568122734b48489bb9691b81b31a52c8, float Vector1_9c1cd85c43a143bbab914e09b14c62c4, float Vector1_cfacc6c500824d0c83c4cd468cbcfc42, Bindings_ColorLookupTable_bb55662be3239744dbc2caf2e0d0fa31 IN, out float4 OutVector4_1)
        {
            float4 _Property_24d737b9799e454091a3bfdfb9944dfd_Out_0 = Color_3a07a9fcd2b048c98880eb8b9024f77a;
            float4 _Saturate_0462de655f384991900a7e13218f40a0_Out_1;
            Unity_Saturate_float4(_Property_24d737b9799e454091a3bfdfb9944dfd_Out_0, _Saturate_0462de655f384991900a7e13218f40a0_Out_1);
            float _Property_460f410c054b4de6ad54e8b704ec7bd6_Out_0 = Vector1_9c1cd85c43a143bbab914e09b14c62c4;
            UnityTexture2D _Property_c850f4a2502440e68975b6d03ec5d0e1_Out_0 = Texture2D_568122734b48489bb9691b81b31a52c8;
            float _TexelSize_38265d2262e843ba8d05c5483ad905eb_Width_0 = _Property_c850f4a2502440e68975b6d03ec5d0e1_Out_0.texelSize.z;
            float _TexelSize_38265d2262e843ba8d05c5483ad905eb_Height_2 = _Property_c850f4a2502440e68975b6d03ec5d0e1_Out_0.texelSize.w;
            float2 _Vector2_08272d93803d437ea839b5433bdbb5ec_Out_0 = float2(_TexelSize_38265d2262e843ba8d05c5483ad905eb_Width_0, _TexelSize_38265d2262e843ba8d05c5483ad905eb_Height_2);
            float _Property_0f290b42b9f54cb494ccd3d49b477537_Out_0 = Vector1_cfacc6c500824d0c83c4cd468cbcfc42;
            UnityTexture2D _Property_db9c7f6f55e9408f91370081d3522824_Out_0 = Texture2D_568122734b48489bb9691b81b31a52c8;
            float4 _GradeCustomFunction_dbfd2c9de66448fbae499c9ecd8432c2_graded_4;
            Grade_float(_Saturate_0462de655f384991900a7e13218f40a0_Out_1, _Property_460f410c054b4de6ad54e8b704ec7bd6_Out_0, _Vector2_08272d93803d437ea839b5433bdbb5ec_Out_0, _Property_0f290b42b9f54cb494ccd3d49b477537_Out_0, _Property_db9c7f6f55e9408f91370081d3522824_Out_0, _GradeCustomFunction_dbfd2c9de66448fbae499c9ecd8432c2_graded_4);
            OutVector4_1 = _GradeCustomFunction_dbfd2c9de66448fbae499c9ecd8432c2_graded_4;
        }

        struct Bindings_AdditionalLightsDithered_b549abda3241b7e4cb2999b9e86a4b7d
        {
            half4 uv0;
            half4 uv1;
            half4 uv2;
            half4 uv3;
        };

        void SG_AdditionalLightsDithered_b549abda3241b7e4cb2999b9e86a4b7d(float3 Vector3_542a1ca5e5fa4171b36867f259f2bf72, float3 Vector3_407f0568a1084b2ab93786202016f883, float3 Vector3_cbe39d4b369d47a7ba967c528ab585ef, float3 Vector3_6b00abb3883949efb3d1cff03e3d2bc0, float3 Vector3_0205087d2b4045cdb72369018e961b68, float Vector1_31e6c70ab6c34008a1f142a4b08a1fb2, UnityTexture2D Texture2D_45cfedd0c0e143b7834ef738470d6fa1, Bindings_AdditionalLightsDithered_b549abda3241b7e4cb2999b9e86a4b7d IN, out float3 Diffuse_1, out float Specular_2)
        {
            float3 _Property_1e6d382292c94d22b134c6a1a124dbab_Out_0 = Vector3_cbe39d4b369d47a7ba967c528ab585ef;
            float3 _Property_e2fe7a9c1e6443e0a249d3723cc14d07_Out_0 = Vector3_6b00abb3883949efb3d1cff03e3d2bc0;
            float3 _Property_7324b45f25e442819db1f29856027828_Out_0 = Vector3_542a1ca5e5fa4171b36867f259f2bf72;
            float3 _Property_f6b309e5102b47faa51a583a60d14f4b_Out_0 = Vector3_0205087d2b4045cdb72369018e961b68;
            float3 _Property_1a7a7948c8df47828b863f27477e1445_Out_0 = Vector3_407f0568a1084b2ab93786202016f883;
            float _Property_786a144483a845c0916637fc2153a443_Out_0 = Vector1_31e6c70ab6c34008a1f142a4b08a1fb2;
            float4 _UV_b2b8022d733d4b248e912ed465a47a80_Out_0 = IN.uv0;
            UnityTexture2D _Property_ef0bde442bb14f089ca5f5f392b09102_Out_0 = Texture2D_45cfedd0c0e143b7834ef738470d6fa1;
            float _TexelSize_8867cd14544246a687e1f264c3d7bc66_Width_0 = _Property_ef0bde442bb14f089ca5f5f392b09102_Out_0.texelSize.z;
            float _TexelSize_8867cd14544246a687e1f264c3d7bc66_Height_2 = _Property_ef0bde442bb14f089ca5f5f392b09102_Out_0.texelSize.w;
            float2 _Vector2_221f2b4cc427423a9cd601acd58553ba_Out_0 = float2(_TexelSize_8867cd14544246a687e1f264c3d7bc66_Width_0, _TexelSize_8867cd14544246a687e1f264c3d7bc66_Height_2);
            float3 _AdditionalLightsDitheredCustomFunction_1ae3189ec194405da505c54ab7247a03_Diffuse_8;
            float3 _AdditionalLightsDitheredCustomFunction_1ae3189ec194405da505c54ab7247a03_Specular_9;
            AdditionalLightsDithered_float(_Property_1e6d382292c94d22b134c6a1a124dbab_Out_0, (_Property_e2fe7a9c1e6443e0a249d3723cc14d07_Out_0).x, _Property_7324b45f25e442819db1f29856027828_Out_0, _Property_f6b309e5102b47faa51a583a60d14f4b_Out_0, _Property_1a7a7948c8df47828b863f27477e1445_Out_0, _Property_786a144483a845c0916637fc2153a443_Out_0, (_UV_b2b8022d733d4b248e912ed465a47a80_Out_0.xy), _Vector2_221f2b4cc427423a9cd601acd58553ba_Out_0, _AdditionalLightsDitheredCustomFunction_1ae3189ec194405da505c54ab7247a03_Diffuse_8, _AdditionalLightsDitheredCustomFunction_1ae3189ec194405da505c54ab7247a03_Specular_9);
            Diffuse_1 = _AdditionalLightsDitheredCustomFunction_1ae3189ec194405da505c54ab7247a03_Diffuse_8;
            Specular_2 = (_AdditionalLightsDitheredCustomFunction_1ae3189ec194405da505c54ab7247a03_Specular_9).x;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        struct Bindings_TextureSpaceDither_05c0e565c3b6982409fa16d8ee961c0a
        {
            half4 uv0;
            half4 uv1;
            half4 uv2;
            half4 uv3;
        };

        void SG_TextureSpaceDither_05c0e565c3b6982409fa16d8ee961c0a(UnityTexture2D Texture2D_5b68cb0f5cdb4882b0d68224e476ff0f, float Vector1_282560b21d2c42fdbba565c24e2e4af7, Bindings_TextureSpaceDither_05c0e565c3b6982409fa16d8ee961c0a IN, out float Out_1)
        {
            float _Property_593fb36f8b80480283a04de54afb7942_Out_0 = Vector1_282560b21d2c42fdbba565c24e2e4af7;
            float4 _UV_a99585cb3fbb4fb8bd5de40b08842806_Out_0 = IN.uv0;
            UnityTexture2D _Property_6ce295d4120540a3a5c6a9b8d1c5aa8f_Out_0 = Texture2D_5b68cb0f5cdb4882b0d68224e476ff0f;
            float _TexelSize_b8b5449fa4f84fc283d231c9caf49c71_Width_0 = _Property_6ce295d4120540a3a5c6a9b8d1c5aa8f_Out_0.texelSize.z;
            float _TexelSize_b8b5449fa4f84fc283d231c9caf49c71_Height_2 = _Property_6ce295d4120540a3a5c6a9b8d1c5aa8f_Out_0.texelSize.w;
            float2 _Vector2_e337668c8b8d4a47b907cbf62fd784e9_Out_0 = float2(_TexelSize_b8b5449fa4f84fc283d231c9caf49c71_Width_0, _TexelSize_b8b5449fa4f84fc283d231c9caf49c71_Height_2);
            float _TextureSpaceDitherCustomFunction_7ee59ad6f36a4b90a1035511300cb0fe_Out_2;
            TextureSpaceDither_float(1, (_UV_a99585cb3fbb4fb8bd5de40b08842806_Out_0.xy), _Vector2_e337668c8b8d4a47b907cbf62fd784e9_Out_0, _TextureSpaceDitherCustomFunction_7ee59ad6f36a4b90a1035511300cb0fe_Out_2);
            float _Multiply_e8862e10087041d9a3ad1b5a01e20fef_Out_2;
            Unity_Multiply_float(_Property_593fb36f8b80480283a04de54afb7942_Out_0, _TextureSpaceDitherCustomFunction_7ee59ad6f36a4b90a1035511300cb0fe_Out_2, _Multiply_e8862e10087041d9a3ad1b5a01e20fef_Out_2);
            Out_1 = _Multiply_e8862e10087041d9a3ad1b5a01e20fef_Out_2;
        }

        void Unity_OneMinus_float3(float3 In, out float3 Out)
        {
            Out = 1 - In;
        }

        void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A - B;
        }

        void Unity_Saturate_float3(float3 In, out float3 Out)
        {
            Out = saturate(In);
        }

        // 4c09902e3d8141dc3cfb16f6bca6ce7f
        #include "Assets/URP_ShaderGraphCustomLighting/CustomLighting.hlsl"

        struct Bindings_MainLight_970d962ef25c3084987fd10643902eed
        {
        };

        void SG_MainLight_970d962ef25c3084987fd10643902eed(Bindings_MainLight_970d962ef25c3084987fd10643902eed IN, out float3 Direction_1, out float3 Colour_2, out float DistanceAtten_3)
        {
            float3 _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_Direction_1;
            float3 _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_Colour_2;
            float _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_DistanceAtten_3;
            MainLight_float(_MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_Direction_1, _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_Colour_2, _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_DistanceAtten_3);
            Direction_1 = _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_Direction_1;
            Colour_2 = _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_Colour_2;
            DistanceAtten_3 = _MainLightCustomFunction_3be2118d2270ff818ec5f0f1353e249f_DistanceAtten_3;
        }

        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }

        void Unity_Negate_float(float In, out float Out)
        {
            Out = -1 * In;
        }

        struct Bindings_MainLightShadows_f3ba9248037f69f429d9e52bc2e30940
        {
        };

        void SG_MainLightShadows_f3ba9248037f69f429d9e52bc2e30940(float3 Vector3_B87D7B6B, Bindings_MainLightShadows_f3ba9248037f69f429d9e52bc2e30940 IN, out float Atten_4)
        {
            float3 _Property_65e443cb15a9fb859a50ebae06e81569_Out_0 = Vector3_B87D7B6B;
            float _MainLightShadowsCustomFunction_3be2118d2270ff818ec5f0f1353e249f_ShadowAtten_4;
            MainLightShadows_float(_Property_65e443cb15a9fb859a50ebae06e81569_Out_0, _MainLightShadowsCustomFunction_3be2118d2270ff818ec5f0f1353e249f_ShadowAtten_4);
            Atten_4 = _MainLightShadowsCustomFunction_3be2118d2270ff818ec5f0f1353e249f_ShadowAtten_4;
        }

        void Unity_SampleGradient_float(Gradient Gradient, float Time, out float4 Out)
        {
            float3 color = Gradient.colors[0].rgb;
            [unroll]
            for (int c = 1; c < 8; c++)
            {
                float colorPos = saturate((Time - Gradient.colors[c-1].w) / (Gradient.colors[c].w - Gradient.colors[c-1].w)) * step(c, Gradient.colorsLength-1);
                color = lerp(color, Gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), Gradient.type));
            }
        #ifndef UNITY_COLORSPACE_GAMMA
            color = SRGBToLinear(color);
        #endif
            float alpha = Gradient.alphas[0].x;
            [unroll]
            for (int a = 1; a < 8; a++)
            {
                float alphaPos = saturate((Time - Gradient.alphas[a-1].y) / (Gradient.alphas[a].y - Gradient.alphas[a-1].y)) * step(a, Gradient.alphasLength-1);
                alpha = lerp(alpha, Gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), Gradient.type));
            }
            Out = float4(color, alpha);
        }

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void Unity_Subtract_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A - B;
        }

        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_BakedGIScale_float(float3 Normal, out float3 Out, float3 Position, float2 StaticUV, float2 DynamicUV)
        {
            Out = SHADERGRAPH_BAKED_GI(Position, Normal, StaticUV, DynamicUV, true);
        }

        void Unity_Length_float3(float3 In, out float Out)
        {
            Out = length(In);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Blend_Multiply_float3(float3 Base, float3 Blend, out float3 Out, float Opacity)
        {
            Out = Base * Blend;
            Out = lerp(Base, Out, Opacity);
        }

        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
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
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_c941884277494a37ae26164ab00da274_Out_0 = _BaseEmissionColor;
            UnityTexture2D _Property_2de01fbcbbb442089dc993965eb98f9b_Out_0 = UnityBuildTexture2DStructNoScale(_EmissionMap);
            float4 _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_RGBA_0 = SAMPLE_TEXTURE2D(_Property_2de01fbcbbb442089dc993965eb98f9b_Out_0.tex, _Property_2de01fbcbbb442089dc993965eb98f9b_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_R_4 = _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_RGBA_0.r;
            float _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_G_5 = _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_RGBA_0.g;
            float _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_B_6 = _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_RGBA_0.b;
            float _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_A_7 = _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_RGBA_0.a;
            float4 _Add_8f89962facb2434a9bad34d1238c2d85_Out_2;
            Unity_Add_float4(_Property_c941884277494a37ae26164ab00da274_Out_0, _SampleTexture2D_7df32d63b36244dbb52f0ff5025b6d71_RGBA_0, _Add_8f89962facb2434a9bad34d1238c2d85_Out_2);
            float _Property_20a4da244ff6473e9171c2056c88cd51_Out_0 = emissiveIntensity;
            float4 _Multiply_d15b022c7eb748a1b886125924916898_Out_2;
            Unity_Multiply_float(_Add_8f89962facb2434a9bad34d1238c2d85_Out_2, (_Property_20a4da244ff6473e9171c2056c88cd51_Out_0.xxxx), _Multiply_d15b022c7eb748a1b886125924916898_Out_2);
            UnityTexture2D _Property_2f39c220461b4adeb7b10ed612b4b849_Out_0 = UnityBuildTexture2DStructNoScale(_MetallicGlossMap);
            float4 _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_RGBA_0 = SAMPLE_TEXTURE2D(_Property_2f39c220461b4adeb7b10ed612b4b849_Out_0.tex, _Property_2f39c220461b4adeb7b10ed612b4b849_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_R_4 = _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_RGBA_0.r;
            float _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_G_5 = _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_RGBA_0.g;
            float _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_B_6 = _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_RGBA_0.b;
            float _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_A_7 = _SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_RGBA_0.a;
            float _Property_5be87ab580e648609124b7a24d526ca7_Out_0 = baseMetallic;
            float _Add_6b1d5ca901cd4df187d07b47abd03a95_Out_2;
            Unity_Add_float(_SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_R_4, _Property_5be87ab580e648609124b7a24d526ca7_Out_0, _Add_6b1d5ca901cd4df187d07b47abd03a95_Out_2);
            float _Saturate_868d213e7ced43218e451d4326c337ff_Out_1;
            Unity_Saturate_float(_Add_6b1d5ca901cd4df187d07b47abd03a95_Out_2, _Saturate_868d213e7ced43218e451d4326c337ff_Out_1);
            UnityTexture2D _Property_bbab3daac9f3429cb25f1cede3896799_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            Bindings_TexelSnap_a0da51191bd8bb4419c891a18487e25b _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d;
            _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d.uv0 = IN.uv0;
            _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d.uv1 = IN.uv1;
            _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d.uv2 = IN.uv2;
            _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d.uv3 = IN.uv3;
            float4 _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d_snapped_1;
            SG_TexelSnap_a0da51191bd8bb4419c891a18487e25b((float4(IN.ObjectSpaceViewDirection, 1.0)), _Property_bbab3daac9f3429cb25f1cede3896799_Out_0, _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d, _TexelSnap_1aa8132355fb4782bd63c3e3aba4887d_snapped_1);
            UnityTexture2D _Property_88bb11857beb4ceba985dba1f663cd93_Out_0 = UnityBuildTexture2DStructNoScale(_NormalMap);
            Bindings_TexelSnappedNormals_05d618dcee8af6942adba1f3f4b1fcc3 _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb;
            _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb.WorldSpaceNormal = IN.WorldSpaceNormal;
            _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb.uv0 = IN.uv0;
            _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb.uv1 = IN.uv1;
            _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb.uv2 = IN.uv2;
            _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb.uv3 = IN.uv3;
            float4 _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_Normals_1;
            float3 _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_GeometryNormals_2;
            float3 _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_NormalMap_3;
            SG_TexelSnappedNormals_05d618dcee8af6942adba1f3f4b1fcc3(_Property_88bb11857beb4ceba985dba1f663cd93_Out_0, _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb, _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_Normals_1, _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_GeometryNormals_2, _TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_NormalMap_3);
            float _Property_7bfcc3514a6a49d081a1cc0da7052616_Out_0 = baseSmoothness;
            float _Add_b75b350c99524afa9674c3eb95d85c1f_Out_2;
            Unity_Add_float(_SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_A_7, _Property_7bfcc3514a6a49d081a1cc0da7052616_Out_0, _Add_b75b350c99524afa9674c3eb95d85c1f_Out_2);
            float _Saturate_e72819e5013242f4b13f6eccfbbd188b_Out_1;
            Unity_Saturate_float(_Add_b75b350c99524afa9674c3eb95d85c1f_Out_2, _Saturate_e72819e5013242f4b13f6eccfbbd188b_Out_1);
            float _OneMinus_b006d55dc8f944e9b6ea1eca594b665a_Out_1;
            Unity_OneMinus_float(_Saturate_e72819e5013242f4b13f6eccfbbd188b_Out_1, _OneMinus_b006d55dc8f944e9b6ea1eca594b665a_Out_1);
            float3 _ReflectionProbe_0fbedfcd963141519565e3cbce08ddcd_Out_3;
            Unity_ReflectionProbe_float((_TexelSnap_1aa8132355fb4782bd63c3e3aba4887d_snapped_1.xyz), (_TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_Normals_1.xyz), _OneMinus_b006d55dc8f944e9b6ea1eca594b665a_Out_1, _ReflectionProbe_0fbedfcd963141519565e3cbce08ddcd_Out_3);
            float3 _Multiply_7deba2adf29a4631bc6985a49f1f9f1d_Out_2;
            Unity_Multiply_float((_Saturate_868d213e7ced43218e451d4326c337ff_Out_1.xxx), _ReflectionProbe_0fbedfcd963141519565e3cbce08ddcd_Out_3, _Multiply_7deba2adf29a4631bc6985a49f1f9f1d_Out_2);
            UnityTexture2D _Property_b86e47fb642747279c931dddb61a2232_Out_0 = UnityBuildTexture2DStructNoScale(paletteTexture);
            float _Property_1c1b9ac30fa44b29b79f161d13393f9f_Out_0 = paletteCellCount;
            float _Property_fabaa22253e941ab9c264648b09fe453_Out_0 = paletteEffect;
            Bindings_ColorLookupTable_bb55662be3239744dbc2caf2e0d0fa31 _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef;
            _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef.uv0 = IN.uv0;
            _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef.uv1 = IN.uv1;
            _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef.uv2 = IN.uv2;
            _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef.uv3 = IN.uv3;
            float4 _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef_OutVector4_1;
            SG_ColorLookupTable_bb55662be3239744dbc2caf2e0d0fa31((float4(_Multiply_7deba2adf29a4631bc6985a49f1f9f1d_Out_2, 1.0)), _Property_b86e47fb642747279c931dddb61a2232_Out_0, _Property_1c1b9ac30fa44b29b79f161d13393f9f_Out_0, _Property_fabaa22253e941ab9c264648b09fe453_Out_0, _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef, _ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef_OutVector4_1);
            UnityTexture2D _Property_fd99d55ef8ed46e89b2b23af9b98788f_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            Bindings_TexelSnap_a0da51191bd8bb4419c891a18487e25b _TexelSnap_704d249c9dcd429aaa3348a761777794;
            _TexelSnap_704d249c9dcd429aaa3348a761777794.uv0 = IN.uv0;
            _TexelSnap_704d249c9dcd429aaa3348a761777794.uv1 = IN.uv1;
            _TexelSnap_704d249c9dcd429aaa3348a761777794.uv2 = IN.uv2;
            _TexelSnap_704d249c9dcd429aaa3348a761777794.uv3 = IN.uv3;
            float4 _TexelSnap_704d249c9dcd429aaa3348a761777794_snapped_1;
            SG_TexelSnap_a0da51191bd8bb4419c891a18487e25b((float4(IN.WorldSpacePosition, 1.0)), _Property_fd99d55ef8ed46e89b2b23af9b98788f_Out_0, _TexelSnap_704d249c9dcd429aaa3348a761777794, _TexelSnap_704d249c9dcd429aaa3348a761777794_snapped_1);
            UnityTexture2D _Property_db3e1603d31147d79ce721863ac82072_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            Bindings_TexelSnap_a0da51191bd8bb4419c891a18487e25b _TexelSnap_a97232639f604377b93579cb8f5695c9;
            _TexelSnap_a97232639f604377b93579cb8f5695c9.uv0 = IN.uv0;
            _TexelSnap_a97232639f604377b93579cb8f5695c9.uv1 = IN.uv1;
            _TexelSnap_a97232639f604377b93579cb8f5695c9.uv2 = IN.uv2;
            _TexelSnap_a97232639f604377b93579cb8f5695c9.uv3 = IN.uv3;
            float4 _TexelSnap_a97232639f604377b93579cb8f5695c9_snapped_1;
            SG_TexelSnap_a0da51191bd8bb4419c891a18487e25b((float4(IN.WorldSpaceViewDirection, 1.0)), _Property_db3e1603d31147d79ce721863ac82072_Out_0, _TexelSnap_a97232639f604377b93579cb8f5695c9, _TexelSnap_a97232639f604377b93579cb8f5695c9_snapped_1);
            float4 Color_04682488ec7a4191ba35d99753cc2f4b = IsGammaSpace() ? float4(1, 1, 1, 0) : float4(SRGBToLinear(float3(1, 1, 1)), 0);
            float _Property_b10828a656a641b3baed474ddd99707e_Out_0 = baseDitherWeight;
            float _Add_48670c9162604dc994d2e17304275c38_Out_2;
            Unity_Add_float(_SampleTexture2D_5d29dbd821394f3d8b230aefc471c102_G_5, _Property_b10828a656a641b3baed474ddd99707e_Out_0, _Add_48670c9162604dc994d2e17304275c38_Out_2);
            float _Saturate_615c2206e6494cb99dc7427db55e03a8_Out_1;
            Unity_Saturate_float(_Add_48670c9162604dc994d2e17304275c38_Out_2, _Saturate_615c2206e6494cb99dc7427db55e03a8_Out_1);
            Bindings_AdditionalLightsDithered_b549abda3241b7e4cb2999b9e86a4b7d _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce;
            _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce.uv0 = IN.uv0;
            _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce.uv1 = IN.uv1;
            _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce.uv2 = IN.uv2;
            _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce.uv3 = IN.uv3;
            float3 _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce_Diffuse_1;
            float _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce_Specular_2;
            SG_AdditionalLightsDithered_b549abda3241b7e4cb2999b9e86a4b7d((_TexelSnap_704d249c9dcd429aaa3348a761777794_snapped_1.xyz), (_TexelSnap_a97232639f604377b93579cb8f5695c9_snapped_1.xyz), (Color_04682488ec7a4191ba35d99753cc2f4b.xyz), (_OneMinus_b006d55dc8f944e9b6ea1eca594b665a_Out_1.xxx), (_TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_Normals_1.xyz), _Saturate_615c2206e6494cb99dc7427db55e03a8_Out_1, _Property_db3e1603d31147d79ce721863ac82072_Out_0, _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce, _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce_Diffuse_1, _AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce_Specular_2);
            float3 _Add_18327d860c784ad8be78e5ad2b148094_Out_2;
            Unity_Add_float3(_AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce_Diffuse_1, (_AdditionalLightsDithered_8c7ab766547d41068afd190f80d540ce_Specular_2.xxx), _Add_18327d860c784ad8be78e5ad2b148094_Out_2);
            UnityTexture2D _Property_e1eacbdca5bf4749aaa4888d9bada48a_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            Bindings_TextureSpaceDither_05c0e565c3b6982409fa16d8ee961c0a _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597;
            _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597.uv0 = IN.uv0;
            _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597.uv1 = IN.uv1;
            _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597.uv2 = IN.uv2;
            _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597.uv3 = IN.uv3;
            float _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597_Out_1;
            SG_TextureSpaceDither_05c0e565c3b6982409fa16d8ee961c0a(_Property_e1eacbdca5bf4749aaa4888d9bada48a_Out_0, _Saturate_615c2206e6494cb99dc7427db55e03a8_Out_1, _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597, _TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597_Out_1);
            float3 _OneMinus_e2f59e0a622243c2ba647060620fd9e8_Out_1;
            Unity_OneMinus_float3(_Add_18327d860c784ad8be78e5ad2b148094_Out_2, _OneMinus_e2f59e0a622243c2ba647060620fd9e8_Out_1);
            float3 _Multiply_531d81b4443e4f40b7826a66217e7a3c_Out_2;
            Unity_Multiply_float((_TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597_Out_1.xxx), _OneMinus_e2f59e0a622243c2ba647060620fd9e8_Out_1, _Multiply_531d81b4443e4f40b7826a66217e7a3c_Out_2);
            float3 _Subtract_30d76b96f1d2410794631360f9092eb4_Out_2;
            Unity_Subtract_float3(_Add_18327d860c784ad8be78e5ad2b148094_Out_2, _Multiply_531d81b4443e4f40b7826a66217e7a3c_Out_2, _Subtract_30d76b96f1d2410794631360f9092eb4_Out_2);
            float3 _Saturate_38e6b7a29c3142fcaf2e87384db2933b_Out_1;
            Unity_Saturate_float3(_Subtract_30d76b96f1d2410794631360f9092eb4_Out_2, _Saturate_38e6b7a29c3142fcaf2e87384db2933b_Out_1);
            Bindings_MainLight_970d962ef25c3084987fd10643902eed _MainLight_9871f869140e4d26ac0824e8758383bf;
            float3 _MainLight_9871f869140e4d26ac0824e8758383bf_Direction_1;
            float3 _MainLight_9871f869140e4d26ac0824e8758383bf_Colour_2;
            float _MainLight_9871f869140e4d26ac0824e8758383bf_DistanceAtten_3;
            SG_MainLight_970d962ef25c3084987fd10643902eed(_MainLight_9871f869140e4d26ac0824e8758383bf, _MainLight_9871f869140e4d26ac0824e8758383bf_Direction_1, _MainLight_9871f869140e4d26ac0824e8758383bf_Colour_2, _MainLight_9871f869140e4d26ac0824e8758383bf_DistanceAtten_3);
            float _DotProduct_ea443ec73d4c4497adaee6c764034864_Out_2;
            Unity_DotProduct_float3((_TexelSnappedNormals_d0a5f1d21e6441989c2852d9132714bb_Normals_1.xyz), _MainLight_9871f869140e4d26ac0824e8758383bf_Direction_1, _DotProduct_ea443ec73d4c4497adaee6c764034864_Out_2);
            float _Negate_1be95d99f91f4fdeb6ae39448ab5420c_Out_1;
            Unity_Negate_float(_DotProduct_ea443ec73d4c4497adaee6c764034864_Out_2, _Negate_1be95d99f91f4fdeb6ae39448ab5420c_Out_1);
            float _OneMinus_692a0d5845704c5c89da0d5336eb151d_Out_1;
            Unity_OneMinus_float(_Negate_1be95d99f91f4fdeb6ae39448ab5420c_Out_1, _OneMinus_692a0d5845704c5c89da0d5336eb151d_Out_1);
            float3 _Multiply_9f158e5167dd456db56e79b69fca6ed8_Out_2;
            Unity_Multiply_float(_Saturate_38e6b7a29c3142fcaf2e87384db2933b_Out_1, (_OneMinus_692a0d5845704c5c89da0d5336eb151d_Out_1.xxx), _Multiply_9f158e5167dd456db56e79b69fca6ed8_Out_2);
            float4 _Property_d9517f2134134053b6484ca147ee80ad_Out_0 = shadowColor;
            Bindings_MainLightShadows_f3ba9248037f69f429d9e52bc2e30940 _MainLightShadows_3b258aa64ee94a0eb9831fdd16e12468;
            float _MainLightShadows_3b258aa64ee94a0eb9831fdd16e12468_Atten_4;
            SG_MainLightShadows_f3ba9248037f69f429d9e52bc2e30940((_TexelSnap_704d249c9dcd429aaa3348a761777794_snapped_1.xyz), _MainLightShadows_3b258aa64ee94a0eb9831fdd16e12468, _MainLightShadows_3b258aa64ee94a0eb9831fdd16e12468_Atten_4);
            float4 _SampleGradient_f0fd477e2e5e456c9f261f785dba3234_Out_2;
            Unity_SampleGradient_float(NewGradient(0, 2, 2, float4(0, 0, 0, 0),float4(1, 1, 1, 0.4),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0), float2(1, 0),float2(1, 1),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0)), _DotProduct_ea443ec73d4c4497adaee6c764034864_Out_2, _SampleGradient_f0fd477e2e5e456c9f261f785dba3234_Out_2);
            float4 _Multiply_c56132f6a8004be09e7d68721849ba3c_Out_2;
            Unity_Multiply_float((_MainLightShadows_3b258aa64ee94a0eb9831fdd16e12468_Atten_4.xxxx), _SampleGradient_f0fd477e2e5e456c9f261f785dba3234_Out_2, _Multiply_c56132f6a8004be09e7d68721849ba3c_Out_2);
            float4 _OneMinus_e183316756e04a9aa6eb88789810f1a9_Out_1;
            Unity_OneMinus_float4(_Multiply_c56132f6a8004be09e7d68721849ba3c_Out_2, _OneMinus_e183316756e04a9aa6eb88789810f1a9_Out_1);
            float4 _Multiply_b37c707241e643bdb9d50d1d8f3bfc58_Out_2;
            Unity_Multiply_float((_TextureSpaceDither_cff1be69f01c4e8e830d999336f9b597_Out_1.xxxx), _OneMinus_e183316756e04a9aa6eb88789810f1a9_Out_1, _Multiply_b37c707241e643bdb9d50d1d8f3bfc58_Out_2);
            float4 _Subtract_68ee4abf5e8441adb7160e46ac7c878c_Out_2;
            Unity_Subtract_float4(_Multiply_c56132f6a8004be09e7d68721849ba3c_Out_2, _Multiply_b37c707241e643bdb9d50d1d8f3bfc58_Out_2, _Subtract_68ee4abf5e8441adb7160e46ac7c878c_Out_2);
            float4 _Saturate_519033122f9b4762a794ebb97f82fb71_Out_1;
            Unity_Saturate_float4(_Subtract_68ee4abf5e8441adb7160e46ac7c878c_Out_2, _Saturate_519033122f9b4762a794ebb97f82fb71_Out_1);
            float3 _Lerp_70199ec603d14a9fb09f0f7a2a3702ba_Out_3;
            Unity_Lerp_float3((_Property_d9517f2134134053b6484ca147ee80ad_Out_0.xyz), _MainLight_9871f869140e4d26ac0824e8758383bf_Colour_2, (_Saturate_519033122f9b4762a794ebb97f82fb71_Out_1.xyz), _Lerp_70199ec603d14a9fb09f0f7a2a3702ba_Out_3);
            float4 _UV_f363edf81e644ad4aa7551177bd1ab9f_Out_0 = IN.uv1;
            UnityTexture2D _Property_44b2115ff3d1423a8e162102bac7d0d6_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            Bindings_TexelSnap_a0da51191bd8bb4419c891a18487e25b _TexelSnap_9ef883d11b974d7fac11c413d42fccad;
            _TexelSnap_9ef883d11b974d7fac11c413d42fccad.uv0 = IN.uv0;
            _TexelSnap_9ef883d11b974d7fac11c413d42fccad.uv1 = IN.uv1;
            _TexelSnap_9ef883d11b974d7fac11c413d42fccad.uv2 = IN.uv2;
            _TexelSnap_9ef883d11b974d7fac11c413d42fccad.uv3 = IN.uv3;
            float4 _TexelSnap_9ef883d11b974d7fac11c413d42fccad_snapped_1;
            SG_TexelSnap_a0da51191bd8bb4419c891a18487e25b(_UV_f363edf81e644ad4aa7551177bd1ab9f_Out_0, _Property_44b2115ff3d1423a8e162102bac7d0d6_Out_0, _TexelSnap_9ef883d11b974d7fac11c413d42fccad, _TexelSnap_9ef883d11b974d7fac11c413d42fccad_snapped_1);
            float3 _BakedGI_2c079a9783da4e6887c52dfc2eef4632_Out_1;
            Unity_BakedGIScale_float(IN.WorldSpaceNormal, _BakedGI_2c079a9783da4e6887c52dfc2eef4632_Out_1, IN.WorldSpacePosition, (_TexelSnap_9ef883d11b974d7fac11c413d42fccad_snapped_1.xy), IN.uv2.xy);
            float3 _OneMinus_4c9db1307c0a44518f09efaffdc9bf02_Out_1;
            Unity_OneMinus_float3(_BakedGI_2c079a9783da4e6887c52dfc2eef4632_Out_1, _OneMinus_4c9db1307c0a44518f09efaffdc9bf02_Out_1);
            float _Length_336e3d70970f4fa9acd463d7ea614895_Out_1;
            Unity_Length_float3(_OneMinus_4c9db1307c0a44518f09efaffdc9bf02_Out_1, _Length_336e3d70970f4fa9acd463d7ea614895_Out_1);
            float _OneMinus_9ce836eb4a0645cb857fe80311fc34e5_Out_1;
            Unity_OneMinus_float(_Length_336e3d70970f4fa9acd463d7ea614895_Out_1, _OneMinus_9ce836eb4a0645cb857fe80311fc34e5_Out_1);
            Bindings_TextureSpaceDither_05c0e565c3b6982409fa16d8ee961c0a _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc;
            _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc.uv0 = IN.uv0;
            _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc.uv1 = IN.uv1;
            _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc.uv2 = IN.uv2;
            _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc.uv3 = IN.uv3;
            float _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc_Out_1;
            SG_TextureSpaceDither_05c0e565c3b6982409fa16d8ee961c0a(_Property_44b2115ff3d1423a8e162102bac7d0d6_Out_0, 1, _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc, _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc_Out_1);
            float _Multiply_9aad79adaeaf43d3a02ed5b58036a37e_Out_2;
            Unity_Multiply_float(_OneMinus_9ce836eb4a0645cb857fe80311fc34e5_Out_1, _TextureSpaceDither_82d4c91c87c3432abe72c1cd68757ebc_Out_1, _Multiply_9aad79adaeaf43d3a02ed5b58036a37e_Out_2);
            float _Subtract_82035a96c026427a8d0bd10d7c050c7e_Out_2;
            Unity_Subtract_float(_Length_336e3d70970f4fa9acd463d7ea614895_Out_1, _Multiply_9aad79adaeaf43d3a02ed5b58036a37e_Out_2, _Subtract_82035a96c026427a8d0bd10d7c050c7e_Out_2);
            float _Saturate_fbc01d3577304322abbf653d1699857b_Out_1;
            Unity_Saturate_float(_Subtract_82035a96c026427a8d0bd10d7c050c7e_Out_2, _Saturate_fbc01d3577304322abbf653d1699857b_Out_1);
            float _Property_83c9c4586edc461da172390db6696a94_Out_0 = lightmapWeight;
            float _Multiply_b73b7f8e106943ed949a9613e74fb8df_Out_2;
            Unity_Multiply_float(_Saturate_fbc01d3577304322abbf653d1699857b_Out_1, _Property_83c9c4586edc461da172390db6696a94_Out_0, _Multiply_b73b7f8e106943ed949a9613e74fb8df_Out_2);
            float3 _Lerp_2b4003eaf1f54c089a66fe31f1147af2_Out_3;
            Unity_Lerp_float3(float3(1, 1, 1), _BakedGI_2c079a9783da4e6887c52dfc2eef4632_Out_1, (_Multiply_b73b7f8e106943ed949a9613e74fb8df_Out_2.xxx), _Lerp_2b4003eaf1f54c089a66fe31f1147af2_Out_3);
            UnityTexture2D _Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0 = SAMPLE_TEXTURE2D(_Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0.tex, _Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_R_4 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.r;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_G_5 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.g;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_B_6 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.b;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_A_7 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.a;
            float3 _Blend_74a05fcecbd04f5ead24cf34660db158_Out_2;
            Unity_Blend_Multiply_float3(_Lerp_2b4003eaf1f54c089a66fe31f1147af2_Out_3, (_SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.xyz), _Blend_74a05fcecbd04f5ead24cf34660db158_Out_2, 1);
            float3 _Multiply_60c62bf144dc4730b4e2315c99e301bb_Out_2;
            Unity_Multiply_float(_Lerp_70199ec603d14a9fb09f0f7a2a3702ba_Out_3, _Blend_74a05fcecbd04f5ead24cf34660db158_Out_2, _Multiply_60c62bf144dc4730b4e2315c99e301bb_Out_2);
            float3 _Add_84c961f5681f408793466db5a074ea33_Out_2;
            Unity_Add_float3(_Multiply_9f158e5167dd456db56e79b69fca6ed8_Out_2, _Multiply_60c62bf144dc4730b4e2315c99e301bb_Out_2, _Add_84c961f5681f408793466db5a074ea33_Out_2);
            UnityTexture2D _Property_4868b464b51048cc8a3ba7793391d3e8_Out_0 = UnityBuildTexture2DStructNoScale(paletteTexture);
            float _Property_cac7a5b1a01949b8b6e92baf790c327e_Out_0 = paletteCellCount;
            float _Property_b806675d29b2457bb30a72c5221c4ec3_Out_0 = paletteEffect;
            Bindings_ColorLookupTable_bb55662be3239744dbc2caf2e0d0fa31 _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51;
            _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51.uv0 = IN.uv0;
            _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51.uv1 = IN.uv1;
            _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51.uv2 = IN.uv2;
            _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51.uv3 = IN.uv3;
            float4 _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51_OutVector4_1;
            SG_ColorLookupTable_bb55662be3239744dbc2caf2e0d0fa31((float4(_Add_84c961f5681f408793466db5a074ea33_Out_2, 1.0)), _Property_4868b464b51048cc8a3ba7793391d3e8_Out_0, _Property_cac7a5b1a01949b8b6e92baf790c327e_Out_0, _Property_b806675d29b2457bb30a72c5221c4ec3_Out_0, _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51, _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51_OutVector4_1);
            float4 _Add_20f7a826a6ad4252935c81469c664fdf_Out_2;
            Unity_Add_float4(_ColorLookupTable_2e3ba940b9f94f418da450513c0dc4ef_OutVector4_1, _ColorLookupTable_9fab2f8e8dc7437b8189621ea9d50b51_OutVector4_1, _Add_20f7a826a6ad4252935c81469c664fdf_Out_2);
            float4 _Add_7b66094e58664d8086b450e780d230ad_Out_2;
            Unity_Add_float4(_Multiply_d15b022c7eb748a1b886125924916898_Out_2, _Add_20f7a826a6ad4252935c81469c664fdf_Out_2, _Add_7b66094e58664d8086b450e780d230ad_Out_2);
            float _Property_9706510f749941aa816114d2f229b348_Out_0 = overallTransparency;
            float _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2;
            Unity_Multiply_float(_Property_9706510f749941aa816114d2f229b348_Out_0, _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_A_7, _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2);
            float _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2;
            Unity_Dither_float(1, float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2);
            surface.BaseColor = (_Add_7b66094e58664d8086b450e780d230ad_Out_2.xyz);
            surface.Alpha = _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2;
            surface.AlphaClipThreshold = _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2;
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
            output.ObjectSpaceViewDirection =    TransformWorldToObjectDir(output.WorldSpaceViewDirection);
            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                         input.texCoord0;
            output.uv1 =                         input.texCoord1;
            output.uv2 =                         input.texCoord2;
            output.uv3 =                         input.texCoord3;
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
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"

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
            Cull Off
        Blend One Zero
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
            #define _AlphaClip 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
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
            float3 positionWS;
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
            float3 WorldSpacePosition;
            float4 ScreenPosition;
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
            output.interp0.xyz =  input.positionWS;
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
            output.positionWS = input.interp0.xyz;
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
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float4 _NormalMap_TexelSize;
        float4 _MetallicGlossMap_TexelSize;
        float baseMetallic;
        float baseSmoothness;
        float baseDitherWeight;
        float4 _EmissionMap_TexelSize;
        float4 _BaseEmissionColor;
        float emissiveIntensity;
        float4 shadowColor;
        float overallTransparency;
        float4 paletteTexture_TexelSize;
        float paletteCellCount;
        float paletteEffect;
        float lightmapWeight;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_NormalMap);
        SAMPLER(sampler_NormalMap);
        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_EmissionMap);
        SAMPLER(sampler_EmissionMap);
        TEXTURE2D(paletteTexture);
        SAMPLER(samplerpaletteTexture);

            // Graph Functions

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
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
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_9706510f749941aa816114d2f229b348_Out_0 = overallTransparency;
            UnityTexture2D _Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0 = SAMPLE_TEXTURE2D(_Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0.tex, _Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_R_4 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.r;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_G_5 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.g;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_B_6 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.b;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_A_7 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.a;
            float _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2;
            Unity_Multiply_float(_Property_9706510f749941aa816114d2f229b348_Out_0, _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_A_7, _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2);
            float _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2;
            Unity_Dither_float(1, float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2);
            surface.Alpha = _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2;
            surface.AlphaClipThreshold = _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2;
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





            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
            Cull Off
        Blend One Zero
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
            #define _AlphaClip 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
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
            float3 positionWS;
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
            float3 WorldSpacePosition;
            float4 ScreenPosition;
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
            output.interp0.xyz =  input.positionWS;
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
            output.positionWS = input.interp0.xyz;
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
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float4 _NormalMap_TexelSize;
        float4 _MetallicGlossMap_TexelSize;
        float baseMetallic;
        float baseSmoothness;
        float baseDitherWeight;
        float4 _EmissionMap_TexelSize;
        float4 _BaseEmissionColor;
        float emissiveIntensity;
        float4 shadowColor;
        float overallTransparency;
        float4 paletteTexture_TexelSize;
        float paletteCellCount;
        float paletteEffect;
        float lightmapWeight;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_NormalMap);
        SAMPLER(sampler_NormalMap);
        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_EmissionMap);
        SAMPLER(sampler_EmissionMap);
        TEXTURE2D(paletteTexture);
        SAMPLER(samplerpaletteTexture);

            // Graph Functions

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
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
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_9706510f749941aa816114d2f229b348_Out_0 = overallTransparency;
            UnityTexture2D _Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0 = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0 = SAMPLE_TEXTURE2D(_Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0.tex, _Property_1859fa1e2bf641d2a070c2d7f9484f4a_Out_0.samplerstate, IN.uv0.xy);
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_R_4 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.r;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_G_5 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.g;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_B_6 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.b;
            float _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_A_7 = _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_RGBA_0.a;
            float _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2;
            Unity_Multiply_float(_Property_9706510f749941aa816114d2f229b348_Out_0, _SampleTexture2D_7c485fca343f4a7f8b7228b85db78873_A_7, _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2);
            float _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2;
            Unity_Dither_float(1, float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2);
            surface.Alpha = _Multiply_eddacd9827f54bec97b44df37f698a8a_Out_2;
            surface.AlphaClipThreshold = _Dither_e11df6235fb147f49e28edd3b44499c7_Out_2;
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





            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaUnlit

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitMetaPass.hlsl"

            ENDHLSL
        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}