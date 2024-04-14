// 参考 https://tech.spark-creative.co.jp/entry/2021/01/13/130743

Shader "Custom/VertexColor"
{
	Properties
	{
		_MainTex( "Texture", 2D ) = "white" {}
		_AmbientLight( "Ambient Light", Color ) = ( 0.5, 0.5, 0.5, 1 )
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
		}
		LOD 100

		//-------------------------------------------------------------------------------------------

		// 各Passでcbufferが変わらないようにここに定義する
        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		TEXTURE2D( _MainTex ) ;
		SAMPLER( sampler_MainTex ) ;

		CBUFFER_START( UnityPerMaterial )
		float4 _MainTex_ST ;
		float3 _AmbientLight ;
		CBUFFER_END

		ENDHLSL

		//-------------------------------------------------------------------------------------------

		Pass
		{
			Name "ForwardLit"
			Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			// Universal Pipeline shadow keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			struct appdata
			{
				float4 vertex	: POSITION ;
				float3 normal	: NORMAL ;
				float4 color	: COLOR ;
				float2 uv		: TEXCOORD0 ;
			} ;

			struct v2f
			{
				float4 vertex	: SV_POSITION ;
				float3 normal	: NORMAL ;
				float4 color	: COLOR ;
				float2 uv		: TEXCOORD0 ;
				float fogFactor	: TEXCOORD1 ;
				float3 posWS	: TEXCOORD2 ;
			} ;

			v2f vert( appdata v )
			{
				v2f o ;

				o.vertex	= TransformObjectToHClip( v.vertex.xyz ) ;
				o.normal	= TransformObjectToWorldNormal( v.normal ) ;
				o.color		= v.color ;
				o.uv		= TRANSFORM_TEX( v.uv, _MainTex ) ;
				o.fogFactor	= ComputeFogFactor( o.vertex.z ) ;
				o.posWS		= TransformObjectToWorld( v.vertex.xyz ) ;

				return o ;
			}

			float4 frag( v2f i ) : SV_Target
			{
				// sample the texture
				float4 color = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, i.uv ) ;

				//-------------

//				float4 shadowCoord = TransformWorldToShadowCoord( i.posWS ) ;

				// ライト情報を取得
//				Light mainLight = GetMainLight( shadowCoord ) ;

//				half shadow = mainLight.shadowAttenuation ;

//				Light addLight0 = GetAdditionalLight( 0, i.posWS ) ;

//				shadow *= addLight0.shadowAttenuation ;

//				color.rgb *= shadow ;

				//-----------------------
				// シンプルなシェード

				// ライト情報を取得
				Light mainLight = GetMainLight() ;

				// ピクセルの法線とライトの方向の内積を計算する
				float t = dot( i.normal, mainLight.direction ) ;

				// 内積の値を0以上の値にする
				t = max( 0, t ) ;

				// 拡散反射光を計算する
				float3 diffuseLight = mainLight.color * t ;

				// 拡散反射光を反映
				color.rgb *= ( diffuseLight + _AmbientLight ) ;

				//-------------

				color.rgba *= i.color ;

				// apply fog
				color.rgb = MixFog( color.rgb, i.fogFactor ) ;

				return color ;
			}
			ENDHLSL
		}
/*
		Pass
		{
			Tags { "LightMode"="ShadowCaster" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile_instancing
            
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			// ShadowsCasterPass.hlsl に定義されているグローバルな変数
			float3 _LightDirection ;
            
			struct appdata
			{
				float4 vertex : POSITION ;
				float3 normal : NORMAL ;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			} ;

			struct v2f
			{
				float4 pos : SV_POSITION;
			} ;

			v2f vert( appdata v )
			{
				UNITY_SETUP_INSTANCE_ID( v ) ;

				v2f o ;

				// ShadowsCasterPass.hlsl の GetShadowPositionHClip() を参考に
				float3 positionWS	= TransformObjectToWorld( v.vertex.xyz ) ;
				float3 normalWS		= TransformObjectToWorldNormal( v.normal ) ;
				float4 positionCS	= TransformWorldToHClip( ApplyShadowBias( positionWS, normalWS, _LightDirection ) ) ;

#if UNITY_REVERSED_Z
				positionCS.z = min( positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE ) ;
#else
				positionCS.z = max( positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE ) ;
#endif
				o.pos = positionCS ;

				return o ;
			}

			float4 frag( v2f i ) : SV_Target
			{
				return 0.0 ;
			}

			ENDHLSL
		}
*/
	}
}
