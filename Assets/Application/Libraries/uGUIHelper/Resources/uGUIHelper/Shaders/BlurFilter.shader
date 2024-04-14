Shader "Hidden/UI/BlurFilter"
{
    Properties
    {
       [PerRendererData]  _MainTex("Main Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "BlurFilter - Sampling"
            ZTest Always
            ZWrite Off
            Cull Back
            Blend One Zero

            HLSLPROGRAM
            #pragma target 2.0

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "BlurFilter.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _SamplingDelta;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;

            	float2 uv0        : TEXCOORD0;
                float4 uv1        : TEXCOORD1;
                float4 uv2        : TEXCOORD2;
                float4 uv3        : TEXCOORD3;
                float4 uv4        : TEXCOORD4;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // position
                const float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);

                // uv
                BoxFilteringUV(
                    input.uv,
                    _SamplingDelta,
                    _MainTex_TexelSize,
                    output.uv0,
                    output.uv1,
                    output.uv2,
                    output.uv3,
                    output.uv4
                );

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return half4(BoxFilteringSampling(
                    input.uv0,
                    input.uv1,
                    input.uv2,
                    input.uv3,
                    input.uv4,
                    TEXTURE2D_ARGS(_MainTex, sampler_MainTex)
                ).rgb, 1.0h);
            }
            ENDHLSL
        }

        Pass
        {
            Name "BlurFilter - Sampling Final"
            ZTest Always
            ZWrite Off
            Cull Back
            Blend One Zero

            HLSLPROGRAM
            #pragma target 2.0

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "BlurFilter.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _SamplingDelta;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;

            	float2 uv0        : TEXCOORD0;
                float4 uv1        : TEXCOORD1;
                float4 uv2        : TEXCOORD2;
                float4 uv3        : TEXCOORD3;
                float4 uv4        : TEXCOORD4;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // position
                const float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);

                // uv
            #if UNITY_UV_STARTS_AT_TOP
                float2 uv;
                uv.x = input.uv.x;
                uv.y = 1.0f - input.uv.y;
            #else
                const float2 uv = input.uv;
            #endif

                BoxFilteringUV(
                    uv,
                    _SamplingDelta,
                    _MainTex_TexelSize,
                    output.uv0,
                    output.uv1,
                    output.uv2,
                    output.uv3,
                    output.uv4
                );

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return half4(BoxFilteringSampling(
                    input.uv0,
                    input.uv1,
                    input.uv2,
                    input.uv3,
                    input.uv4,
                    TEXTURE2D_ARGS(_MainTex, sampler_MainTex)
                ).rgb, 1.0h);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
