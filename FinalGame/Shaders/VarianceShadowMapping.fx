
#define MaxBones 59

float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 Bones[MaxBones];

float maxLightDistance;

// TODO: add effect parameters here.

struct VertexShaderInput
{
	float4 Position : POSITION0;

	// TODO: add input channels such as texture
	// coordinates and vertex colors here.
};

struct VertexShaderInputSkinned
{
	float4 Position : POSITION0;
	float4 BoneIndices : BLENDINDICES0;
	float4 BoneWeights : BLENDWEIGHT0;

	// TODO: add input channels such as texture
	// coordinates and vertex colors here.
};


struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 viewPosition : TEXCOORD0;

	// TODO: add vertex shader outputs such as colors and texture
	// coordinates here. These values will automatically be interpolated
	// over the triangle, and provided as input to your pixel shader.
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

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	output.viewPosition = viewPosition;
	// TODO: add your vertex shader code here.

	return output;
}

VertexShaderOutput VertexShaderFunctionSkinned(VertexShaderInputSkinned input)
{
	VertexShaderOutput output;

	// Create skin matrix.
	float4x4 skinTransform = CreateSkinTransform(Bones, input.BoneWeights, input.BoneIndices);

	float4 worldPosition = mul(input.Position, skinTransform);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	output.viewPosition = viewPosition;
	// TODO: add your vertex shader code here.

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float distToLight = length(input.viewPosition) / maxLightDistance;

	float2 Moments;  

	// First moment is the depth itself.  
	Moments.x = distToLight;

	// Compute partial derivatives of depth.  
	float dx = ddx(distToLight);  
	float dy = ddy(distToLight);  

	// Compute second moment over the pixel extents.  
	Moments.y = distToLight*distToLight + 0.25*(dx*dx + dy*dy);  

	return float4(Moments.x, Moments.y, 0, 1);
}

technique ShadowMapDepth
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}

technique ShadowMapDepthSkinned
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderFunctionSkinned();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
