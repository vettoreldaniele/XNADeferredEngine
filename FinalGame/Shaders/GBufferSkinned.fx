
// Max number of bones. (for VS/PS 2.0).
// If you change this, update SkinnedModelProcessor.cs to match.
#define MaxBones 59

float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 Bones[MaxBones];

// The main texture applied to the model.
texture Texture;

// Normal and specular maps.
texture SpecularMap;
texture NormalMap;

// Far plane of the camera.
float farPlane;

sampler texSampler = sampler_state
{
	Texture = (Texture);
	
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Point;
	
	AddressU = Wrap;
	AddressV = Wrap;
};

 sampler specularSampler = sampler_state
 {
	 Texture = (SpecularMap);
	 MagFilter = LINEAR;
	 MinFilter = LINEAR;
	 Mipfilter = LINEAR;
	 AddressU = Wrap;
	 AddressV = Wrap;
 };

 sampler normalSampler = sampler_state
 {
	 Texture = (NormalMap);
	 MagFilter = LINEAR;
	 MinFilter = LINEAR;
	 Mipfilter = LINEAR;
	 AddressU = Wrap;
	 AddressV = Wrap;
 };

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float3 Binormal : BINORMAL0;
	float3 Tangent : TANGENT0;
	float2 TexCoord : TEXCOORD0;
	float4 BoneIndices : BLENDINDICES0;
	float4 BoneWeights : BLENDWEIGHT0;

	// TODO: add input channels such as texture
	// coordinates and vertex colors here.
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float Depth : TEXCOORD1;
	float3x3 tangentToWorld : TEXCOORD2;
	// TODO: add vertex shader outputs such as colors and texture
	// coordinates here. These values will automatically be interpolated
	// over the triangle, and provided as input to your pixel shader.
};

struct PixelShaderOutput
{
	float4 Diffuse : COLOR0;
	float4 Normals : COLOR1;
	float4 DepthLight : COLOR2;
};

float4x4 CreateSkinTransform(float4x4 bones[MaxBones],                       
							 float4   boneWeights, 
							 float4   boneIndices)
{
	float4x4 skinTransform = 0;
	
	skinTransform += Bones[boneIndices.x] * boneWeights.x;
	skinTransform += Bones[boneIndices.y] * boneWeights.y;
	skinTransform += Bones[boneIndices.z] * boneWeights.z;
	skinTransform += Bones[boneIndices.w] * boneWeights.w; 

	return skinTransform;

}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	// Create skin matrix.
	float4x4 skinTransform = CreateSkinTransform(Bones, input.BoneWeights, input.BoneIndices);

	// Skin position.
	float4 worldPosition = mul(input.Position, skinTransform);

	float4 viewPosition = mul(worldPosition, View);

	output.Position = mul(viewPosition, Projection);
	
	output.TexCoord = input.TexCoord;

	// Output the linear z depth in view space.
	output.Depth.x = viewPosition.z;

	// Calculate tangent space to world space matrix using the world space tangent,
	// binormal, and normal as basis vectors.
	output.tangentToWorld[0] = mul(input.Tangent, skinTransform);
	output.tangentToWorld[1] = mul(input.Binormal, skinTransform);
	output.tangentToWorld[2] = mul(input.Normal, skinTransform);

	return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
   PixelShaderOutput output;

   float4 specularAttributes = tex2D(specularSampler, input.TexCoord);
   
   // Diffuse color and specular intensity.
   output.Diffuse.rgb = tex2D(texSampler,input.TexCoord).rgb;
   output.Diffuse.a = specularAttributes.r; // Specular intensity.

   // Normals.
   float3 normalFromMap = tex2D(normalSampler, input.TexCoord);

   // Transform to [-1,1].
   normalFromMap = 2.0f * normalFromMap - 1.0f;

   // Transform into world space.
   normalFromMap = mul(normalFromMap, input.tangentToWorld);

   // Normalize the result.
   normalFromMap = normalize(normalFromMap);

   // Output the normal, in [0,1] space.
   output.Normals.rgb = 0.5f * (normalFromMap + 1.0f);
//	output.Normals.rgb = 0.5f * (normalize(input.tangentToWorld[2]) + 1.0f);
   output.Normals.a = specularAttributes.a; // Specular power.
   
   // Depth.

   // Negate and divide by distance to far-clip plane, (so that depth is in range [0,1]).
   // This is for right-handed coordinate system.
   float depth = -input.Depth.x/farPlane;

   output.DepthLight.r = depth;

   float slit = 1.0f;
  
   output.DepthLight.g = slit; // 1.0f if the pixel will be lighted in the lightning pass, 0.0f otherwise.
   output.DepthLight.ba = 0.0f;

   return output;
}

technique DrawSkinnedGBuffer
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
