Shader "Hidden/UI/BlurFilter"
{
	Properties
	{
	   [PerRendererData]  _MainTex( "Main Texture", 2D ) = "white" {}
	}
	SubShader
	{
		CGINCLUDE
		#pragma vertex vert
		#pragma fragment frag
			
		#include "UnityCG.cginc"

		sampler2D _MainTex ;
		float4 _MainTex_ST ;
		float4 _MainTex_TexelSize ;

		struct appdata
		{
			float4 vertex : POSITION ;
			float2 uv : TEXCOORD0;
		} ;

		struct v2f
		{
			float4 vertex : SV_POSITION ;
			float2 uv : TEXCOORD0 ;
		} ;

		// メインテクスチャからサンプリングしてRGBのみ返す
		half3 sampleMain( float2 uv )
		{
			return tex2D( _MainTex, uv ).rgb ;
		}

		// 対角線上の4点からサンプリングした色の平均値を返す
		half3 sampleBox ( float2 uv, float delta )
		{
			float4 offset = _MainTex_TexelSize.xyxy * float2( -delta, delta ).xxyy ;
			half3 sum = sampleMain( uv + offset.xy ) + sampleMain( uv + offset.zy ) + sampleMain( uv + offset.xw ) + sampleMain( uv + offset.zw ) ;
			return sum * 0.25 ;
		}

		// 頂点シェーダは各パスで共通
		v2f vert ( appdata v )
		{
			v2f o ;
			o.vertex = UnityObjectToClipPos( v.vertex ) ;
			o.uv = TRANSFORM_TEX( v.uv, _MainTex ) ;
			return o ;
		}

		ENDCG

		Cull Off
		ZTest Always
		ZWrite Off

		Tags { "RenderType"="Opaque" }

		// ダウンサンプリング用のパス
		Pass
		{
			CGPROGRAM

			// ダウンサンプリング時には1ピクセル分ずらした対角線上の4点からサンプリング
			fixed4 frag ( v2f i ) : SV_Target
			{
				half4 color = 1 ;
				color.rgb = sampleBox( i.uv, 1.0 ) ;
				return color ;
			}

			ENDCG
		}

		// アップサンプリング用のパス
		Pass
		{
			CGPROGRAM

			// アップサンプリング時には0.5ピクセル分ずらした対角線上の4点からサンプリング
			fixed4 frag ( v2f i ) : SV_Target
			{
				half4 color = 1 ;
				color.rgb = sampleBox( i.uv, 0.5 ) ;
				return color ;
			}

			ENDCG
		}
	}
}
