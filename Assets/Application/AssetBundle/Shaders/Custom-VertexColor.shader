// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified VertexLit shader. Differences from regular VertexLit one:
// - no per-material color
// - no specular
// - no emission

Shader "Custom/VertexColor"
{
	Properties
	{
		_MaterialColor ("Material Color", Color) = (1, 1, 1, 1)
		_MainTexture ("Main Texture", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM
				#pragma vertex vertexFunction
				#pragma fragment fragmentFunction
				#pragma target 2.0

				#include "UnityCG.cginc"

				#pragma multi_compile_fog
		        #define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))

				struct appdata
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float2 uv : TEXCOORD0;
					fixed4 color : COLOR0;
				};

				struct v2f
				{
					float4 position : SV_POSITION;
					float2 uv : TEXCOORD0;
					fixed4 color : COLOR0;
#if USING_FOG
					fixed fog : TEXCOORD3;
#endif
				};

				float4 _MaterialColor;
				sampler2D _MainTexture;

				v2f vertexFunction (appdata IN)
				{
					v2f OUT;

					OUT.position = UnityObjectToClipPos(IN.vertex);
					OUT.uv = IN.uv;

					float4 invLightDir = mul(UNITY_MATRIX_M, WorldSpaceLightDir(IN.vertex));
//					float luminance = dot(IN.normal, normalize(invLightDir));
//					luminance = luminance * 0.25f + 0.75f;
//					OUT.color = IN.color * luminance;
					

					OUT.color = IN.color ;


				#if USING_FOG
					float3 eyePos = UnityObjectToViewPos(IN.vertex);
					float fogCoord = length(eyePos.xyz);
					UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
					OUT.fog = saturate(unityFogFactor);
				#endif
					
					return OUT;
				}

				fixed4 fragmentFunction (v2f IN) : SV_TARGET
				{
					fixed4 color;

					color = tex2D(_MainTexture, IN.uv) * IN.color;

            #if USING_FOG
                    color.rgb = lerp(unity_FogColor.rgb, color.rgb, IN.fog);
            #endif

					return color ;
				}
			ENDCG
		}
	}
}
