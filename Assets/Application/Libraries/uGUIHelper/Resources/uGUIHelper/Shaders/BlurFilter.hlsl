#ifndef BEAST_UI__FILTER__BLUR__BOX_FILTERING__INCLUDED
#define BEAST_UI__FILTER__BLUR__BOX_FILTERING__INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

/**
 * \brief BoxフィルタリングのUV計算
 * \param originUV Original UV from Vertex attributes
 * \param delta Sampling up or down
 * \param textureTexelSize Texture Size (e.g. _MainTex_TexelSize)
 * \param uv0 output uv
 * \param uv1 output uv
 * \param uv2 output uv
 * \param uv3 output uv
 * \param uv4
 */
void BoxFilteringUV(
    const in float2 originUV,
    const in float delta,
    const in float4 textureTexelSize,

    out float2 uv0,
    out float4 uv1,
    out float4 uv2,
    out float4 uv3,
    out float4 uv4
)
{
    const float4 offset = textureTexelSize.xyxy * float2(-delta, delta).xxyy;

	uv0.xy = originUV;

    uv1.xy = originUV + offset.xy;
    uv1.zw = originUV + offset.zy;
    uv2.xy = originUV + offset.xw;
    uv2.zw = originUV + offset.zw;

    uv3.xy = originUV + float2(0.0f, offset.y);
    uv3.zw = originUV + float2(0.0f, offset.w);
    uv4.xy = originUV + float2(offset.x, 0.0f);
    uv4.zw = originUV + float2(offset.z, 0.0f);
}

/**
 * \brief Boxフィルタリングのテクスチャサンプリング
 * \param uv0 Texture UV1
 * \param uv1 Texture UV2
 * \param uv2
 * \param uv3
 * \param uv4
 * \param tex Texture name
 * \param smp Texture sampler
 * \return color
 */
half4 BoxFilteringSampling(
    const in float2 uv0,
    const in float4 uv1,
    const in float4 uv2,
    const in float4 uv3,
    const in float4 uv4,
    TEXTURE2D_PARAM(tex, smp)
)
{
    const half4 color0 = SAMPLE_TEXTURE2D(tex, smp, uv0.xy);

	const half4 color1 = SAMPLE_TEXTURE2D(tex, smp, uv1.xy);
    const half4 color2 = SAMPLE_TEXTURE2D(tex, smp, uv1.zw);
    const half4 color3 = SAMPLE_TEXTURE2D(tex, smp, uv2.xy);
    const half4 color4 = SAMPLE_TEXTURE2D(tex, smp, uv2.zw);

	const half4 color5 = SAMPLE_TEXTURE2D(tex, smp, uv3.xy);
	const half4 color6 = SAMPLE_TEXTURE2D(tex, smp, uv3.zw);
	const half4 color7 = SAMPLE_TEXTURE2D(tex, smp, uv4.xy);
	const half4 color8 = SAMPLE_TEXTURE2D(tex, smp, uv4.zw);

    // return (color0 + color1 + color2 + color3) * 0.25h;
    return color0 * 0.25h
		+ (color1 + color2 + color3 + color4) * 0.0625h
		+ (color5 + color6 + color7 + color8) * 0.125h ;
}


#endif
