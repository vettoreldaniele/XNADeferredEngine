//-----------------------------------------------------------------------------
// This effect represents a deferred point light, that is created by using a sphere volume scaled to the radius of the light.
// Linear attenuation is performed.
//-----------------------------------------------------------------------------

float4x4 World;
float4x4 View;
float4x4 Projection;

// Light parameters.
float3 lightPos;
float radius;
float lightIntensity = 1.0f;
float3 color;

// Camera position and distance to far clip.
float3 eyePosition;
float farClip;

// Inverse matrices.
float4x4 inverseViewProj;
float4x4 inverseView;

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

struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 ScreenPosition : TEXCOORD0;
	float4 ViewPosition : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	// We make a copy of the position because the Position variable is required by the GPU pipeline and it is not available in the pixel shader.
	// Also pass the view position needed for the calculation of the frustum ray.
	output.ScreenPosition = output.Position;
	output.ViewPosition = viewPosition;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	 // Obtain screen position.
	 input.ScreenPosition.xy /= input.ScreenPosition.w;

	 // Obtain textureCoordinates corresponding to the current pixel.
	 // The screen coordinates are in [-1,1]*[1,-1]
	 // The texture coordinates need to be in [0,1]*[0,1]

	 float2 texCoord = 0.5f * (float2(input.ScreenPosition.x,-input.ScreenPosition.y) + 1);

	 if (tex2D(colorSampler, texCoord).a == 0.0f && tex2D(normalSampler, texCoord).a == 0.0f)
	 {
		float3 backgroundColor = tex2D(colorSampler, texCoord).rgb;
		return float4(backgroundColor,0.0f);
	 }

	 // Align texels to pixels.
	 texCoord -= halfPixel;

	 // Get normal data from the normalMap.
	 float4 normalData = tex2D(normalSampler,texCoord);

	 // Tranform normal back into [-1,1] range.
	 float3 normal = 2.0f * normalData.xyz - 1.0f;

	 // Get specular intensity and power.
	 float specularIntensity = tex2D(colorSampler, texCoord).a;
	 float specularPower = normalData.a * 255;

	 // Read depth
	 float depthVal = tex2D(depthSampler,texCoord).r;

	 // Calculate the frustum ray using the view-space position.
	 // farCip is the distance to the camera's far clipping plane.
	 // Negating the Z component is only necessary for right-handed coordinates.
	 float3 vFrustumRayVS = input.ViewPosition.xyz * (farClip/-input.ViewPosition.z);
	 float3 viewPosition  = depthVal * vFrustumRayVS;
	 float4 position = float4(viewPosition,1.0f);
	 position = mul(position,inverseView);

	 /*

	 // Compute screen-space position.
	 float4 position;
	 position.xy = input.ScreenPosition.xy;
	 position.z = depthVal;
	 position.w = 1.0f;

	 // Transform to world space.
	 position = mul(position, inverseViewProj);
	 position /= position.w;

	 */
	 
	 // Surface-to-light vector.
	 float3 lightVector = lightPos - position;

	 // Compute attenuation based on distance (linear attenuation).
	 float attenuation = saturate(1.0f - length(lightVector)/radius); 

	 // Normalize light vector.
	 lightVector = normalize(lightVector); 

	 // Compute diffuse light.
	 float NormalDotLight = max(0,dot(normal,lightVector));
	 float3 diffuseLight = NormalDotLight * color.rgb;

	 // Reflection vector.
	 float3 reflectionVector = normalize(reflect(-lightVector, normal));

	 // Eye-to-surface vector.
	 float3 directionToCamera = normalize(eyePosition - position);

	 // Compute specular light.
	 float specularLight = specularIntensity * pow( saturate(dot(reflectionVector, directionToCamera)), specularPower);

	 // Take into account attenuation and light intensity.
	 return attenuation * lightIntensity * float4(diffuseLight.rgb,specularLight);
}

technique PointLight
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
