//-----------------------------------------------------------------------------
// This effect represents a deferred directional light.
//-----------------------------------------------------------------------------

// Paramaters for a directional light.
float3 lightDirection;
float3 lightColor;

// Position of the camera (the player).
float3 eyePosition; 

// World matrix for camera (Inverse of view).
float4x4 camWorld;

// Frustum corners.
float3 frustumCorners[4];

// The inverse of View * Projection (used to compute world position of pixel).
float4x4 InverseViewProjection; 

// Albedo texture
texture colorMap; 

// Normal texture
texture normalMap;

// Depth texture
texture depthMap;

// Cast shadows?
bool CastShadows;

// Half of a pixel.
float2 halfPixel;

sampler colorSampler = sampler_state
 {
	 Texture = (colorMap);
	 AddressU = CLAMP;
	 AddressV = CLAMP;
	 MagFilter = LINEAR;
	 MinFilter = LINEAR;
	 Mipfilter = LINEAR;
 };
 sampler depthSampler = sampler_state
 {
	 Texture = (depthMap);
	 AddressU = CLAMP;
	 AddressV = CLAMP;
	 MagFilter = POINT;
	 MinFilter = POINT;
	 Mipfilter = POINT;
 };
 sampler normalSampler = sampler_state
 {
	 Texture = (normalMap);
	 AddressU = CLAMP;
	 AddressV = CLAMP;
	 MagFilter = POINT;
	 MinFilter = POINT;
	 Mipfilter = POINT;
 };


// TODO: add effect parameters here.

struct VertexShaderInput
{
	float3 Position : POSITION0;
	float3 texCoords : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 texCoords : TEXCOORD0;
	float4 frustRay : TEXCOORD1;

};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = float4(input.Position,1);

	// Align pixels to texels.
	output.texCoords = input.texCoords.xy - halfPixel;

	output.frustRay = mul(frustumCorners[input.texCoords.z],camWorld);

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	 if (tex2D(colorSampler, input.texCoords).a == 0.0f && tex2D(normalSampler, input.texCoords).a == 0.0f)
	 {
		float3 backgroundColor = tex2D(colorSampler,input.texCoords).rgb;
		return float4(backgroundColor,0.0f);
	 }

	// Get normal data from the normalMap.
	float4 normalData = tex2D(normalSampler,input.texCoords);

	// Transform normal back into [-1,1] range.

	float3 normal = 2.0f * normalData.xyz - 1.0f;

	// Get depth from the depthMap.
	float depth = tex2D(depthSampler,input.texCoords).r;

	// Compute screen-space position.
	// We have the depth in the depthMap, and position on the screen in the [0,1][0,1] range, which comes from the texture coordinates. 
	// We will move this into screen coordinates, which are in [-1,1][1,-1] range, and then using the InverseViewProjection matrix, we can get them back into world coordinates. 

	float4 position;
	/* position.x = input.texCoords.x * 2.0f - 1.0f;
	position.y = -(input.texCoords.y * 2.0f - 1.0f);
	position.z = depth;
	position.w = 1.0f;

	// Transform to world space.
	position = mul(position, InverseViewProjection);
	position /= position.w; */


	position = float4(eyePosition + depth * input.frustRay,1);


	// Surface-to-light vector.
	float3 lightVector = -normalize(lightDirection);

	// Compute diffuse light.
	float NormalDotLight = max(0, dot(normal, lightVector));
	float3 diffuseLight = NormalDotLight * lightColor.rgb;

	// Reflection vector.
	float3 reflectionVector = normalize(reflect(-lightVector, normal));

	// Eye-to-surface vector.
	float3 directionToCamera = normalize(eyePosition - position);

	// Get specular intensity and power.
	 float specularIntensity = tex2D(colorSampler, input.texCoords).a;
	 float specularPower = tex2D(normalSampler, input.texCoords).a * 255;

	// Compute specular light (as the dot product between the light reflection vector and the eye vector).
	float specularLight = specularIntensity * pow( saturate(dot(reflectionVector, directionToCamera)), specularPower);

	// Output the two lights.
	// Diffuse in the rgb channel, specular in alpha channel.
	return float4(diffuseLight.rgb, specularLight);
}

technique DirectionalLight
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
