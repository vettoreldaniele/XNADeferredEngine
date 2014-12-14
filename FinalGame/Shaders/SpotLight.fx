//-----------------------------------------------------------------------------
// This effect represents a deferred spot light, that is created by using a cone volume translated to the position of the light, rotated to its direction and scaled accordingly.
// Spot light decay rate is applied.
//-----------------------------------------------------------------------------


float4x4 World;
float4x4 View;
float4x4 Projection;

// Light parameters.
float3 lightPos;
float3 lightDirection;
float cosLightAngle;
float spotDecayExponent;
float maxDistance;
float lightIntensity;
float3 color;

float4x4 lightViewProj;

// Specular power and intensity.
float specularIntensity = 0.8f;
float specularPower = 15.0f;

// Camera position and distance to far clip.
float3 eyePosition;
float farClip;

// Inverse matrices.
float4x4 inverseViewProj;
float4x4 inverseView;

// Albedo texture.
texture colorMap; 

// Normal texture.
texture normalMap;

// Depth texture.
texture depthMap;

// Shadow map.
texture shadowMap;

// Cookie attenuation texture.
texture cookieTexture;

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
 sampler cookieSampler = sampler_state
 {
	 Texture = (cookieTexture);
	 AddressU = CLAMP;
	 AddressV = CLAMP;
	 MagFilter = LINEAR;
	 MinFilter = LINEAR;
	 Mipfilter = LINEAR;
 };
 sampler shadowSampler = sampler_state
 {
	 Texture = (shadowMap);
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

//Manually Linear Sample
float4 manualSample(sampler Sampler, float2 UV, float2 textureSize)
{
	float2 texelpos = textureSize * UV; 
	float2 lerps = frac(texelpos); 
	float texelSize = 1.0 / textureSize;                 
 
	float4 sourcevals[4]; 
	sourcevals[0] = tex2D(Sampler, UV); 
	sourcevals[1] = tex2D(Sampler, UV + float2(texelSize, 0)); 
	sourcevals[2] = tex2D(Sampler, UV + float2(0, texelSize)); 
	sourcevals[3] = tex2D(Sampler, UV + float2(texelSize, texelSize));   
		 
	float4 interpolated = lerp(lerp(sourcevals[0], sourcevals[1], lerps.x), lerp(sourcevals[2], sourcevals[3], lerps.x ), lerps.y); 

	return interpolated;
}

 /*
float4 linearSample(float2 texCoords)
{
	float textureSize = 1024; 
 
	float2 texelpos = textureSize * texCoords; 
	float2 lerps = frac( texelpos ); 
	float texelSize = 1.0 / textureSize;                 
 
	float4 sourcevals[4]; 
	sourcevals[0] = tex2D(shadowSampler, texCoords); 
	sourcevals[1] = tex2D(shadowSampler, texCoords + float2(texelSize, 0)); 
	sourcevals[2] = tex2D(shadowSampler, texCoords + float2(0, texelSize)); 
	sourcevals[3] = tex2D(shadowSampler, texCoords + float2(texelSize, texelSize));   
         
	float4 interpolated = lerp( lerp( sourcevals[0], sourcevals[1], lerps.x ), 
                          lerp( sourcevals[2], sourcevals[3], lerps.x ), 
                          lerps.y ); 

} */

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
		return float4(backgroundColor, 0.0f);
	 }

	 // Align texels to pixels.
	 texCoord -= halfPixel;

	 // Get normal data from the normalMap.
	 float4 normalData = tex2D(normalSampler,texCoord);

	 // Tranform normal back into [-1,1] range.
	 float3 normal = 2.0f * normalData.xyz - 1.0f;

	 // Read depth
	 float depthVal = tex2D(depthSampler,texCoord).r;

	 // float depthVal = manualSample(depthSampler, texCoord, float2(1024,768)).x;

	 // Calculate the frustum ray using the view-space position.
	 // farCip is the distance to the camera's far clipping plane.
	 // Negating the Z component is only necessary for right-handed coordinates.
	 float3 vFrustumRayVS = input.ViewPosition.xyz * (farClip/-input.ViewPosition.z);
	 float3 viewPosition  = depthVal * vFrustumRayVS;
	 float4 position = float4(viewPosition,1.0f);
	 position = mul(position, inverseView);

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
	 
	 //Calculate Homogenous Position with respect to light.
	 float4 lightScreenPos = mul(position, lightViewProj);
	 lightScreenPos /= lightScreenPos.w;

	 // Calculate projected UV from light point of view.
	 float2 LUV = 0.5f * (float2(lightScreenPos.x, -lightScreenPos.y) + 1);	


	 // Surface-to-light vector.
	 float3 lightVector = lightPos.xyz - position.xyz;

	 // Get attenuation based on an attenuation cookie.
	 float radialAttenuation = tex2D(cookieSampler, LUV).r;

	 // Calculate height Attenuation
	// float heightAttenuation = 1.0f - saturate(length(lightVector) - (maxDistance / 2));
	 float heightAttenuation = saturate(1.0f - length(lightVector)/maxDistance);

	 float Attenuation = min(radialAttenuation, heightAttenuation);

	// float Attenuation = radialAttenuation;

	 // Normalize light vector.
	 lightVector = normalize(lightVector); 

	 // Cosine of the angle between the spot light direction and the light vector.	 
	 float SpotDotLight = dot(-lightVector, lightDirection);

	 float4 Shading = float4(0,0,0,0);

	 if (SpotDotLight > cosLightAngle)
	 {
		//float spotIntensity = pow(abs(SpotDotLight), spotDecayExponent);
		//float spotIntensity =  pow(SpotDotLight,10); 

		// Compute diffuse light.
	 float NormalDotLight = max(0,(dot(normal,lightVector)));
	 float3 diffuseLight = NormalDotLight * color.rgb;

	 // Reflection vector.
	 float3 reflectionVector = normalize(reflect(-lightVector, normal));

	 // Eye-to-surface vector.
	 float3 directionToCamera = normalize(eyePosition - position.xyz);

	 // Compute specular light.
	 float specularLight = specularIntensity * pow( saturate(dot(reflectionVector, directionToCamera)), specularPower);

	//  Take into account attenuation and light intensity.
	 Shading = ( Attenuation *  lightIntensity) * float4(diffuseLight.rgb,specularLight); 

	 if (CastShadows)
	  {

		float g_MinVariance = 0.0001f;

	//	float2 Moments = tex2D(shadowSampler, LUV).xy ;
		float2 Moments = manualSample(shadowSampler,LUV,1024).xy;
		
		float t = length(position - lightPos) / maxDistance ;

		// One-tailed inequality valid if t > Moments.x  
		float p = (t <= Moments.x);  

		// Compute variance.  
		float Variance = Moments.y - (Moments.x * Moments.x);  
		Variance = max(Variance, g_MinVariance);  

		// Compute probabilistic upper bound.  
		float d = t - Moments.x;
		float p_max = Variance / (Variance + d*d);
		   
		float shadow = max(p, p_max);
		 
		Shading *= shadow;
	  }
	 }

	return Shading;
}


technique SpotLight
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
