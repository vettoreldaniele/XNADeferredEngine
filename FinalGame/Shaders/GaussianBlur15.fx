float4x4 World;
float4x4 View;
float4x4 Projection;

// Half of a pixel.
float2 halfPixel;

// Inverted screen width and height. 
float2 inverseScreenSize;

texture Texture;

sampler texSampler = sampler_state
 {
	 Texture = (Texture);
	 AddressU = CLAMP;
	 AddressV = CLAMP;
	 MagFilter = POINT;
	 MinFilter = POINT;
	 Mipfilter = POINT;
 };

#define SAMPLE_COUNT 15

float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];

struct VertexShaderInput
{
	float3 Position : POSITION0;
	float2 texCoords : TEXCOORD0;

	// TODO: add input channels such as texture
	// coordinates and vertex colors here.
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 texCoords : TEXCOORD0;

	// TODO: add vertex shader outputs such as colors and texture
	// coordinates here. These values will automatically be interpolated
	// over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = float4(input.Position,1);

	// Align pixels to texels.
	output.texCoords = input.texCoords.xy - halfPixel;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 c = 0;
	
	// Combine a number of weighted image filter taps.
	for (int i = 0; i < SAMPLE_COUNT; i++)
	{
		c += tex2D(texSampler, input.texCoords + SampleOffsets[i] ) * SampleWeights[i];
	}
	
	return c;
}

technique Technique1
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
