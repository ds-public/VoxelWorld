Shader "MeshHelper/Default3D"
{
	Properties
	{
		[PerRendererData] _MainTex( "Font Texture", 2D ) = "white" {}

		_Color( "Tint", Color ) = ( 1, 1, 1, 1 )

		_StencilComp( "Stencil Comparison", Float ) = 8
		_Stencil( "Stencil ID", Float ) = 0
		_StencilOp( "Stencil Operation", Float ) = 0
		_StencilWriteMask( "Stencil Write Mask", Float ) = 255
		_StencilReadMask( "Stencil Read Mask", Float ) = 255

		_ColorMask( "Color Mask", Float ) = 15
	}
	
	SubShader
	{
		LOD 100

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
				
		Cull Back
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0

				#include "UnityCG.cginc"

				#pragma multi_compile_fog
		        #define USING_FOG ( defined( FOG_LINEAR ) || defined( FOG_EXP ) || defined( FOG_EXP2 ) )

				struct appdata
				{
					float4 vertex   : POSITION ;
					float4 normal   : NORMAL ;
					float2 texcoord : TEXCOORD0 ;
					float4 color    : COLOR0 ;
				} ;
	
				struct v2f
				{
					float4 vertex	: SV_POSITION ;
					half2 texcoord	: TEXCOORD0 ;
					fixed4 color	: COLOR0 ;
#if USING_FOG
					fixed fog		: TEXCOORD3 ;
#endif
				} ;
	
				sampler2D _MainTex ;
				float4 _MainTex_ST ;
				fixed4 _Color ;
				
				v2f vert( appdata i )
				{
					v2f o ;

					o.vertex = UnityObjectToClipPos( i.vertex ) ;
					o.color.a = i.color.a ;

					//--------------------------------------------------------
					// Lighting

					//--------------------------------
					// 最初のライトのみ有効

#if USING_DIRECTIONAL_LIGHT
					// Lighting(Directional Light or Point Light)
					half3 invertLightDirection = _WorldSpaceLightPos0.xyz ;

					// ワールド空間での法線の方向
					float3 normal = UnityObjectToWorldNormal( i.normal ) ;

					// 光の当たり具合(正面=+1・垂直=0・反対=-1)
					float luminance = dot( normal, invertLightDirection ) ;
#endif
					//--------------------------------
					// 常に正面からディレクショナルライトが当たっているような効果

					// Lighting(Directional Light or Point Light)
					float3 invertLightDirection = float3( 0, 0, 1 ) ;

					// ワールド空間での法線の方向
					float3 normal = UnityObjectToWorldNormal( i.normal ) ;

					// カメラの向いている方向(ワールド座標系)
					float3 cameraFoward = UNITY_MATRIX_V[ 2 ].xyz ;

					// 光の当たり具合(正面=+1・垂直=0・反対=-1)
					float luminance = dot( normal, cameraFoward ) ;

					//--------------------------------

//					luminance = luminance * 0.5f + 0.5f ;
					o.color.rgb = i.color.rgb * luminance ;

					//--------------------------------------------------------

					// Material Color
					o.color = o.color * _Color ;
					o.texcoord = TRANSFORM_TEX( i.texcoord, _MainTex ) ;
#if USING_FOG
					float3 eyePosition = UnityObjectToViewPos( o.vertex ) ;
					float fogCoord = length( eyePosition.xyz ) ;
					UNITY_CALC_FOG_FACTOR_RAW( fogCoord ) ;
					o.fog = saturate( unityFogFactor ) ;
#endif
					return o ;
				}
				
				fixed4 frag( v2f i ) : SV_Target
				{
					fixed4 color = tex2D( _MainTex, i.texcoord ) * i.color ;
#if USING_FOG
					color.rgb = lerp( unity_FogColor.rgb, color.rgb, i.fog ) ;
#endif

					clip( color.a - 0.01 ) ;
					return color ;
				}
			ENDCG
		}
	}
}
