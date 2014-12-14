float4x4 World;
float4x4 View;
float4x4 Projection;

// Depth and normal map.
texture depthMap;
texture normalMap;

// Half of a pixel.
float2 halfPixel;

// Inverse screen size.
float2 screenInverse;

sampler depthSampler = sampler_state
{
	 Texture = (depthMap);
	 AddressU = CLAMP;
	 AddressV = CLAMP;
	 MagFilter = POINT;
	 MinFilter = POINT;
	 Mipfilter = Point;
};

sampler normalSampler = sampler_state
{
	 Texture = (normalMap);
	 AddressU = CLAMP;
	 AddressV = CLAMP;
	 MagFilter = POINT;
	 MinFilter = POINT;
	 Mipfilter = Point;
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
};


VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = float4(input.Position,1);

	// Align pixels to texels.
	output.texCoords = input.texCoords.xy - halfPixel;

	return output;
}



// Outputs edges only using a A 3x3 edge filter kernel
float OutlinesFunction3x3(float2 texcoords)
{
  float4 lum = float4(0.30, 0.59, 0.11, 1);
	
  // TOP ROW
  float s11 = dot(tex2D(depthSampler, texcoords + float2(-screenInverse.x, -screenInverse.y)), lum);	// LEFT
  float s12 = dot(tex2D(depthSampler, texcoords + float2(0, -screenInverse.y)), lum);				// MIDDLE
  float s13 = dot(tex2D(depthSampler, texcoords+ float2(screenInverse.x, -screenInverse.y)), lum);	// RIGHT

  // MIDDLE ROW
  float s21 = dot(tex2D(depthSampler, texcoords + float2(-screenInverse.x, 0)), lum);				// LEFT
  // Omit center
  float s23 = dot(tex2D(depthSampler, texcoords + float2(screenInverse.x, 0)), lum); 				// RIGHT

  // LAST ROW
  float s31 = dot(tex2D(depthSampler, texcoords + float2(-screenInverse.x, screenInverse.y)), lum);	// LEFT
  float s32 = dot(tex2D(depthSampler, texcoords + float2(0, screenInverse.y)), lum);				// MIDDLE
  float s33 = dot(tex2D(depthSampler, texcoords + float2(screenInverse.x, screenInverse.y)), lum);	// RIGHT

  // Filter.
  float t1 = s13 + s33 + (2 * s23) - s11 - (2 * s21) - s31;
  float t2 = s31 + (2 * s32) + s33 - s11 - (2 * s12) - s13;

  float col;

  if (((t1 * t1) + (t2 * t2)) > 0.2f) {
  col = 0;
  } else {
	col = 1;
  }

  return col;
}

float NormalOutlinesFunction3x3(float2 texcoords)
{
  float4 lum = float4(0.30, 0.59, 0.11, 1);
	
  // TOP ROW
  float3 s11 = dot(tex2D(normalSampler, texcoords + float2(-screenInverse.x, -screenInverse.y)), lum);	// LEFT
  float3 s12 = dot(tex2D(normalSampler, texcoords + float2(0, -screenInverse.y)), lum);				// MIDDLE
  float3 s13 = dot(tex2D(normalSampler, texcoords+ float2(screenInverse.x, -screenInverse.y)), lum);	// RIGHT

  // MIDDLE ROW
  float3 s21 = dot(tex2D(normalSampler, texcoords + float2(-screenInverse.x, 0)), lum);				// LEFT
  // Omit center
  float3 s23 = dot(tex2D(normalSampler, texcoords + float2(screenInverse.x, 0)), lum); 				// RIGHT

  // LAST ROW
  float3 s31 = dot(tex2D(normalSampler, texcoords + float2(-screenInverse.x, screenInverse.y)), lum);	// LEFT
  float3 s32 = dot(tex2D(normalSampler, texcoords + float2(0, screenInverse.y)), lum);				// MIDDLE
  float3 s33 = dot(tex2D(normalSampler, texcoords + float2(screenInverse.x, screenInverse.y)), lum);	// RIGHT

  // Filter.
  float3 t1 = s13 + s33 + (2 * s23) - s11 - (2 * s21) - s31;
  float3 t2 = s31 + (2 * s32) + s33 - s11 - (2 * s12) - s13;

  float col;

  if ((dot((t1 * t1) + (t2 * t2),0.33333f)) > 0.7f) {
  col = 0;
  } else {
	col = 1;
  }

  return col;
}

float OtherDetection(float2 texcoords)
{
 // Look up four values from the normal/depth texture, offset along the
		// four diagonals from the pixel we are currently shading.
		float2 edgeOffset = 1 / float2(1024,768);
		
		float4 n1 = float4(tex2D(normalSampler, texcoords + float2(-1, -1) * edgeOffset).rgb, tex2D(depthSampler, texcoords + float2(-1, -1) * edgeOffset).r);
		float4 n2 = float4(tex2D(normalSampler, texcoords + float2( 1,  1) * edgeOffset).rgb, tex2D(depthSampler, texcoords + float2( 1,  1) * edgeOffset).r);
		float4 n3 = float4(tex2D(normalSampler, texcoords + float2(-1,  1) * edgeOffset).rgb, tex2D(depthSampler, texcoords + float2(-1,  1) * edgeOffset).r);
		float4 n4 = float4(tex2D(normalSampler, texcoords + float2( 1, -1) * edgeOffset).rgb, tex2D(depthSampler, texcoords + float2( 1, -1) * edgeOffset).r);

		// Work out how much the normal and depth values are changing.
		float4 diagonalDelta = abs(n1 - n2) + abs(n3 - n4);

		float normalDelta = dot(diagonalDelta.xyz, 1);
		float depthDelta = diagonalDelta.w;
		
		// Filter out very small changes, in order to produce nice clean results.
		normalDelta = saturate((normalDelta - 0.5f) * 1);
		depthDelta = saturate((depthDelta - 0.1f) * 10);

		// Does this pixel lie on an edge?
		float edgeAmount = saturate(normalDelta + depthDelta) * 1.5f;
		
		return (1.0f - edgeAmount);
}

float4 Sobel(VertexShaderOutput input) : COLOR0
{
/*
	// Depth
	float s00 = tex2D(depthSampler,input.texCoords + float2(-screenInverse.x, -screenInverse.y));

	float s01 = tex2D(depthSampler,input.texCoords + float2(0, -screenInverse.y));

	float s02 = tex2D(depthSampler,input.texCoords + float2(screenInverse.x, -screenInverse.y));
	
	float s10 = tex2D(depthSampler,input.texCoords + float2(-screenInverse.x, 0));

	float s12 = tex2D(depthSampler,input.texCoords + float2(screenInverse.x, 0));

	float s20 = tex2D(depthSampler,input.texCoords + float2(-screenInverse.x, screenInverse.y));
	
	float s21 = tex2D(depthSampler,input.texCoords + float2(0, screenInverse.y));
	
	float s22 = tex2D(depthSampler,input.texCoords + float2(screenInverse.x, screenInverse.y));
	
	// sobel filter in x and y directions
	float sobelX = s00 + 2 * s10 + s20 - s02 - 2 * s12 - s22;
	float sobelY = s00 + 2 * s01 + s02 - s20 - 2 * s21 - s22;

	// find edge using a threshold
	float thresholdZ = 0.0015f;
	float edgeSqr = sobelX * sobelX + sobelY * sobelY;
	float resultDepth = 1.0f - (edgeSqr > (thresholdZ * thresholdZ)); 

	// Normal

	float3 n00 = tex2D(normalSampler,input.texCoords + float2(-screenInverse.x, -screenInverse.y));

	float3 n01 = tex2D(normalSampler,input.texCoords + float2(0, -screenInverse.y));

	float3 n02 = tex2D(normalSampler,input.texCoords + float2(screenInverse.x, -screenInverse.y));
	
	float3 n10 = tex2D(normalSampler,input.texCoords + float2(-screenInverse.x, 0));

	float3 n12 = tex2D(normalSampler,input.texCoords + float2(screenInverse.x, 0));

	float3 n20 = tex2D(normalSampler,input.texCoords + float2(-screenInverse.x, screenInverse.y));
	
	float3 n21 = tex2D(normalSampler,input.texCoords + float2(0, screenInverse.y));
	
	float3 n22 = tex2D(normalSampler,input.texCoords + float2(screenInverse.x, screenInverse.y));

	// sobel filter in x and y directions
	float3 nsobelX = n00 + 2 * n10 + n20 - n02 - 2 * n12 - n22;
	float3 nsobelY = n00 + 2 * n01 + n02 - n20 - 2 * n21 - n22;

	// find edge using a threshold
	float thresholdNormal = 0.80f;
	float3 edgeSqr3 = nsobelX * nsobelX + nsobelY * nsobelY;
	edgeSqr = dot(edgeSqr3,0.33333f);

	float resultNormal = 1.0f - (edgeSqr > (thresholdNormal * thresholdNormal)); */

	float resultDepth = OutlinesFunction3x3(input.texCoords);
	float resultNormal = NormalOutlinesFunction3x3(input.texCoords);

	// return float4(resultDepth, resultNormal, 0, 0);
	//  return float4( resultNormal, 0, 0, 0);

	 float depthValue = tex2D(depthSampler, input.texCoords).r;
	// float depthValue = 0.05f;

	// Do not draw edges far in the scene.
	if ( depthValue < 0.5f)
	{
		return float4(min(resultNormal, resultDepth), 0, 0, 0);
	} else return float4(1, 0, 0, 0);
}





technique SobelTechnique
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 Sobel();
	}
}
