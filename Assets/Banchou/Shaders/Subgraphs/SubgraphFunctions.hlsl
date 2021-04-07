#ifndef SUBGRAPHS_INCLUDED
#define SUBGRAPHS_INCLUDED

/**
 * Snaps a given vector to its approximated value at the center of the nearest texel
 */
void TexelSnap_float(
    float4 Value,
    float2 UV,
    float2 TexelSize,
    out float4 Snapped
) {
    float2 originalUV = UV;
    float2 centerUV = floor(originalUV * TexelSize) / TexelSize;
    float2 dUV = centerUV - originalUV;

    // Get the change in UVs across fragments
    float2 ddxUV = ddx(originalUV);
    float2 ddyUV = ddy(originalUV);

    // Invert the matrix created by the ddx/y of the UVs
    float2x2 ddMatrix = float2x2(ddyUV.y, -ddyUV.x, -ddxUV.y, ddxUV.x);
    ddMatrix = ddMatrix * (1.0f / determinant(ddMatrix));

    // Convert the UV delta to a fragment space delta
    float2 components = mul(ddMatrix, dUV);

    Snapped = Value + clamp(ddx(Value) * components.x + ddy(Value) * components.y, -1, 1);
}

/**
 * Applies a dither pattern to a texture space
 */
void TextureSpaceDither_float(
    float In,
    float2 UV,
    float2 TexelSize,
    out float Out
) {
    float2 uv = UV * TexelSize;
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

void AdditionalLightsDithered_float(
    float3 SpecColor,
    float Smoothness,
    float3 WorldPosition,
    float3 WorldNormal,
    float3 WorldView,
    float DitherValue,
    float2 UV,
    float2 TexelSize,
    out float3 Diffuse,
    out float3 Specular
) {
    float3 diffuseColor = 0;
    float3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    int pixelLightCount = GetAdditionalLightsCount();

    float dither;
    TextureSpaceDither_float(DitherValue, UV, TexelSize, dither);
    dither = max(0, dither);

    for (int i = 0; i < pixelLightCount; ++i) {
        Light light = GetAdditionalLight(i, WorldPosition, half4(1,1,1,1));
        float nDotL = max(0, dot(WorldNormal, light.direction));
        float ditheredNDotL = nDotL - ((1 - nDotL) * dither);

        float3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);

        diffuseColor += attenuatedLightColor * ditheredNDotL;
        specularColor += LightingSpecular(attenuatedLightColor, light.direction, WorldNormal, WorldView, float4(SpecColor, 0), Smoothness);
    }
#endif

    Diffuse = diffuseColor;
    Specular = specularColor;
}

#endif