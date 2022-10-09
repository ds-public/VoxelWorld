// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UI/Blur"
{
	Properties
	{
		_MainTex("Diffuse", 2D) = "white" {}

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15
	}

	HLSLINCLUDE
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	ENDHLSL

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"RenderPipeline" = "UniversalPipeline"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off

		Pass
		{
			Name "HorizontalBlur"

			Tags
			{
				"LightMode" = "UniversalForward"
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
			}

			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma vertex vert
			#pragma fragment frag

			struct Attributes
			{
				float3 positionOS : POSITION;
				float4 color      : COLOR;
				float2 uv         : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float4 color      : COLOR;
				float2 uv         : TEXCOORD0;
			};

			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			float4 _MainTex_ST;

			Varyings vert(Attributes i)
			{
				Varyings o = (Varyings)0;

				o.positionCS = TransformObjectToHClip(i.positionOS);
				o.color = i.color;
				o.uv = TRANSFORM_TEX(i.uv, _MainTex);

				return o;
			}

			half4 frag(Varyings i) : SV_Target
			{
				// 通常
//				float4 color = i.color * SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, i.uv ) ;

				//---------------------------------

				float4 color = float4(0, 0, 0, 0);

				float xr = 1.0 / 240.0;
				float yr = 1.0 / 198.0;

				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-xr, -yr)) * 0.0625;
				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, -yr)) * 0.1250;
				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(xr, -yr)) * 0.0625;

				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-xr,    0)) * 0.1250;
				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0,    0)) * 0.2500;
				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(xr,    0)) * 0.1250;

				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-xr,   yr)) * 0.0625;
				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0,   yr)) * 0.1250;
				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(xr,   yr)) * 0.0625;

				color = color * i.color;

				return color;
			}

			ENDHLSL
		}
	}
}
