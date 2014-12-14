//-----------------------------------------------------------------------------
// SkinnedModel.fx
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


// Maximum number of bone matrices we can render using shader 2.0 in a single pass.
// If you change this, update SkinnedModelProcessor.cs to match.
#define MaxBones 59


float4x4 Bones[MaxBones];

float3 Light1Direction = normalize(float3(1, 1, -2));
float3 Light1Color = float3(0.9, 0.8, 0.7);

float3 Light2Direction = normalize(float3(-1, -1, 1));
float3 Light2Color = float3(0.1, 0.3, 0.8);

float3 AmbientColor = 0.2;

float4x4 World;
float4x4 View;
float4x4 Projection;

// The light direction is shared between the Lambert and Toon lighting techniques.
float3 LightDirection = normalize(float3(1, 1, 1));

// Settings controlling the Lambert lighting technique.
float3 DiffuseLight = 0.5;
float3 AmbientLight = 0.5;

// Settings controlling the Toon lighting technique.
float ToonThresholds[2] = { 0.8, 0.4 };
float ToonBrightnessLevels[3] = { 1.3, 0.9, 0.5 };

// Is texturing enabled?
bool TextureEnabled = true;


// The main texture applied to the object, and a sampler for reading it.
texture Texture;

sampler Sampler = sampler_state
{
    Texture = (Texture);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
     AddressU = Wrap;
     AddressV = Wrap;
};


// Vertex shader input structure.
struct VS_INPUT
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float4 BoneIndices : BLENDINDICES0;
    float4 BoneWeights : BLENDWEIGHT0;
};


// Vertex shader output structure.
struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float LightAmount : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
};


// Vertex shader program.
VS_OUTPUT VertexShaderFunction(VS_INPUT input)
{
    VS_OUTPUT output;
    
    // Blend between the weighted bone matrices.
    float4x4 skinTransform = 0;
    
    skinTransform += Bones[input.BoneIndices.x] * input.BoneWeights.x;
    skinTransform += Bones[input.BoneIndices.y] * input.BoneWeights.y;
    skinTransform += Bones[input.BoneIndices.z] * input.BoneWeights.z;
    skinTransform += Bones[input.BoneIndices.w] * input.BoneWeights.w;
    
    // Skin the vertex position.
    float4 position = mul(input.Position, skinTransform);
    
    output.Position = mul(mul(position, View), Projection);

    // Skin the vertex normal, then compute lighting.
    
    float3 worldNormal = mul(input.Normal, World);

    output.LightAmount = dot(worldNormal, LightDirection);

    output.TexCoord = input.TexCoord;
    
    return output;
}


// Pixel shader input structure.
struct PS_INPUT
{
    float LightAmount : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
};


// Pixel shader program.
float4 PixelShaderFunction(PS_INPUT input) : COLOR0
{
    float4 color = TextureEnabled ? tex2D(Sampler, input.TexCoord) : 0;

    float light;

    if (input.LightAmount > ToonThresholds[0])
        light = ToonBrightnessLevels[0];
    else if (input.LightAmount > ToonThresholds[1])
        light = ToonBrightnessLevels[1];
    else
        light = ToonBrightnessLevels[2];
                
    color.rgb *= light;

    return color;
}

// Output structure for the vertex shader that renders normal and depth information.
struct NormalDepthVertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};


// Alternative vertex shader outputs normal and depth values, which are then
// used as an input for the edge detection filter in PostprocessEffect.fx.
NormalDepthVertexShaderOutput NormalDepthVertexShader(VS_INPUT input)
{
    NormalDepthVertexShaderOutput output;
    
    // Blend between the weighted bone matrices.
    float4x4 skinTransform = 0;
    
    skinTransform += Bones[input.BoneIndices.x] * input.BoneWeights.x;
    skinTransform += Bones[input.BoneIndices.y] * input.BoneWeights.y;
    skinTransform += Bones[input.BoneIndices.z] * input.BoneWeights.z;
    skinTransform += Bones[input.BoneIndices.w] * input.BoneWeights.w;
    
    // Skin the vertex position.
    float4 position = mul(input.Position, skinTransform);
    
    output.Position = mul(mul(position, View), Projection);
    
    float3 worldNormal = normalize(mul(input.Normal, skinTransform));

    // The output color holds the normal, scaled to fit into a 0 to 1 range.
    output.Color.rgb = (worldNormal + 1) / 2;

    // The output alpha holds the depth, scaled to fit into a 0 to 1 range.
    output.Color.a = output.Position.z / output.Position.w;
    
    return output;    
}


// Simple pixel shader for rendering the normal and depth information.
float4 NormalDepthPixelShader(float4 color : COLOR0) : COLOR0
{
    return color;
}


technique SkinnedModelTechnique
{
    pass SkinnedModelPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

technique Toon
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}



// Technique draws the object as normal and depth values.
technique NormalDepth
{
    pass P0
    {
        VertexShader = compile vs_2_0 NormalDepthVertexShader();
        PixelShader = compile ps_2_0 NormalDepthPixelShader();
    }
}


