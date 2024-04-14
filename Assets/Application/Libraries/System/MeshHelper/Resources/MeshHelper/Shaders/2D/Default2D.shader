Shader "MeshHelper/Default2D"
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
		
		Cull Off
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				#include "UnityUI.cginc"

				struct appdata
				{
					float4 vertex : POSITION ;
					float2 texcoord : TEXCOORD0 ;
					float4 color : COLOR ;
				} ;
	
				struct v2f
				{
					float4 vertex : SV_POSITION ;
					half2 texcoord : TEXCOORD0 ;
					fixed4 color : COLOR ;
				} ;
	
				sampler2D _MainTex ;
				float4 _MainTex_ST ;
				fixed4 _Color ;
				
				v2f vert( appdata i )
				{
					v2f o ;
					o.vertex = UnityObjectToClipPos( i.vertex ) ;
					o.texcoord = TRANSFORM_TEX( i.texcoord, _MainTex ) ;
					o.color = i.color * _Color ;

#ifdef UNITY_HALF_TEXEL_OFFSET
					o.vertex.xy += ( _ScreenParams.zw - 1.0 ) * float2( -1, 1 ) ;
#endif
					return o;
				}
				
				fixed4 frag( v2f i ) : SV_Target
				{
					fixed4 color = tex2D( _MainTex, i.texcoord ) * i.color ;
					clip( color.a - 0.01 ) ;
					return color ;
				}
			ENDCG
		}
	}
}
