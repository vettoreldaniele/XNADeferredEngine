//-----------------------------------------------------------------------------
// This effect combines the results of the g buffer and the light accumulation render target to create the final image.
//-----------------------------------------------------------------------------

// Effect parameters.
texture colorMap;
texture edgeMap;
texture lightMap;
texture normalMap;

 sampler colorSampler = sampler_state
 {
	 Texture = (colorMap);
	 AddressU = CLAMP;
	 AddressV = CLAMP;
	 MagFilter = LINEAR;
	 MinFilter = LINEAR;
	 Mipfilter = LINEAR;
 };

 
 sampler normalSampler = sampler_state
 {
	 Texture = (normalMap);
	 AddressU = CLAMP;
	 AddressV = CLAMP;
	 MagFilter = LINEAR;
	 MinFilter = LINEAR;
	 Mipfilter = LINEAR;
 };

 sampler edgeSampler = sampler_state
 {
	 Texture = (edgeMap);
	 AddressU = CLAMP;
	 AddressV = CLAMP;
	 MagFilter = POINT;
	 MinFilter = POINT;
	 Mipfilter = POINT;
 };


 sampler lightSampler = sampler_state
 {
	 Texture = (lightMap);
	 AddressU = CLAMP;
	 AddressV = CLAMP;
	 MagFilter = LINEAR;
	 MinFilter = LINEAR;
	 Mipfilter = LINEAR;
 };

 // Half of a pixel for aligning pixels to texels.
 float2 halfPixel;

 // Toon shading enabled ?
 bool ToonShading;


struct VertexShaderInput
{
	float3 Position : POSITION0;
	float2 texCoords : TEXCOORD0;
};

struct VertexShaderOutput
{
	 float4 Position : POSITION0;
	 float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = float4(input.Position,1);

	// Align pixels to texels.
	output.TexCoord = input.texCoords - halfPixel;

	return output;
}

// Settings controlling the Toon lighting technique.
float ToonThresholds[3] = { 0.8, 0.4, 0.2};
float ToonBrightnessLevels[4] = { 0.9, 0.7, 0.5, 0.3 };

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Get the diffuse color from the color map.
	float3 diffuseColor = tex2D(colorSampler, input.TexCoord).rgb;

	if (tex2D(colorSampler, input.TexCoord).a == 0.0f && tex2D(normalSampler, input.TexCoord).a == 0.0f)
	 {
		clip(-1);
	 }

	// Get the light from the light map (diffuse in rgb, specular in alpha).
	float4 light = tex2D(lightSampler,input.TexCoord);
	float3 diffuseLight = light.rgb;
	float specularLight = light.a;

	float edge = 1.0f;

	// Toon shading (banded light).
	if (ToonShading)
	{
		float ndl = light.rgb;

		float lightA;

		if (ndl > ToonThresholds[0])
			lightA = ToonBrightnessLevels[0];
		else if (ndl > ToonThresholds[1])
			lightA = ToonBrightnessLevels[1];
		else if (ndl > ToonThresholds[2])
			lightA = ToonBrightnessLevels[2];
		else
			lightA = ToonBrightnessLevels[3];

		edge = tex2D(edgeSampler,input.TexCoord).r;
		diffuseColor *= lightA;
	}

	// Return final color as DiffuseColor * DiffuseLight + SpecularLight.
	return float4(edge * (diffuseColor * (diffuseLight + specularLight)),1);
}

technique Combine
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
