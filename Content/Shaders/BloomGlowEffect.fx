

//The Blur Algorithm
//
//1. Render the scene to texture.
//
//2. Down sample the texture to to half its size or less.
//
//3. Perform a horizontal blur on the down sampled texture.
//
//4. Perform a vertical blur.
//
//5. Up sample the texture back to the original screen size.
//
//6. Render that texture to the screen.



#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0  
#define PS_SHADERMODEL ps_4_0 //_level_9_1
#endif


int numberOfSamplesPerDimension;
float3 threshold;
float2 textureSize;

sampler2D SpriteTextureSampler : register(s0)
{
	Texture = (Texture);
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

struct PixelShaderDuelOutput
{
	float4 Color0 : COLOR0;
	float4 Color1 : COLOR1;
};







PixelShaderDuelOutput ExtractBrightColorsPS(VertexShaderOutput input)
{
	PixelShaderDuelOutput output;
	float4 originalColor = tex2D(SpriteTextureSampler, input.TextureCoordinates) * input.Color;
	float4 color = originalColor;

	float flag = saturate(sign((color.x + color.y + color.z) / 3.0f - threshold));

	output.Color0 = color * flag;
	output.Color1 = color;
	return output;
}

technique ExtractGlowColors
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL
			ExtractBrightColorsPS();
	}
};






PixelShaderDuelOutput BloomGlowPS(VertexShaderOutput input)
{
	PixelShaderDuelOutput output;
	float4 originalColor = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	float4 color = originalColor;
	float2 hnod = numberOfSamplesPerDimension / 2.0f;
	float sampledistance = 1.0f;
	float2 texelCoef = (1.0f / float2(textureSize.x, textureSize.y)) * sampledistance;
	for (float v = -hnod.y; v < hnod.y; v += 1.0f)
	{
		//float su = u * texelCoef.x;
		float sv = v * texelCoef.y;
		color += tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(0, sv));
	}
	for (float u = -hnod.x; u < hnod.x; u += 1.0f)
	{
		float su = u * texelCoef.x;
		//float sv = v * texelCoef.y;
		color += tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(su, 0));
	}
	color = color / (numberOfSamplesPerDimension * numberOfSamplesPerDimension) * input.Color;
	//output.Color = originalColor;
	output.Color0 = color;

	//float flag = saturate(sign((color.x + color.y + color.z) / 3.0f - threshold));

	float flagR = saturate( sign( color.r - threshold.r ) );
	float flagG = saturate( sign( color.g - threshold.g ) );
	float flagB = saturate( sign( color.b - threshold.b ) );
	float flag = saturate( sign( flagR + flagG + flagB ) );

	color.a = flag;
	output.Color1 = color * flag;
	return output;
}

technique BloomGlow
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL 
			BloomGlowPS();
	}
};
















//PixelShaderDuelOutput BloomGlowPS(VertexShaderOutput input)
//{
//	PixelShaderDuelOutput output;
//	float4 originalColor = tex2D(SpriteTextureSampler, input.TextureCoordinates);
//	float4 color = originalColor;
//	float2 hnod = numberOfSamplesPerDimension / 2.0f;
//	float sampledistance = 1.0f;
//	float2 texelCoef = (1.0f / float2(textureSize.x, textureSize.y)) * sampledistance;
//	for (float u = -hnod.x; u < hnod.x; u += 1.0f)
//	{
//		for (float v = -hnod.y; v < hnod.y; v += 1.0f)
//		{
//			float su = u * texelCoef.x;
//			float sv = v * texelCoef.y;
//			color += tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(su, sv));
//		}
//	}
//	color = color /  (numberOfSamplesPerDimension * numberOfSamplesPerDimension) * input.Color;
//	//output.Color = originalColor;
//	output.Color0 = color;
//
//	float flag = saturate(sign((color.x + color.y + color.z) / 3.0f - threshold));
//
//	//float a = color.a;
//	//float flag =  color * saturate( sign( color - threshold ) );  // (color.x + color.y + color. z) / 3.0f
//	//output.Color1.a = a;
//    //float flag =  color * dot(color.rgb, float3(0.2126f, 0.7152f, 0.0722f));
//
//	color.a = flag;
//    output.Color1 = color * flag; 
//	return output;
//}





//PixelShaderDuelOutput BloomGlowPS(VertexShaderOutput input)
//{
//	PixelShaderDuelOutput output;
//	float4 originalColor = tex2D(SpriteTextureSampler, input.TextureCoordinates);
//	float4 color = originalColor;
//	float2 hnod = numberOfSamplesPerDimension / 2.0f;
//	float sampledistance = 1.0f;
//	float2 texelCoef = (1.0f / float2(textureSize.x, textureSize.y)) * sampledistance;
//	for (float u = -hnod.x; u < hnod.x; u += 1.0f)
//	{
//		for (float v = -hnod.y; v < hnod.y; v += 1.0f)
//		{
//			float su = u * texelCoef.x;
//			float sv = v * texelCoef.y;
//			color += tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(su, sv));
//		}
//	}
//	color = color / (numberOfSamplesPerDimension * numberOfSamplesPerDimension) * input.Color;
//	//output.Color = originalColor;
//	output.Color0 = color;
//
//	//float flag = saturate(sign((color.x + color.y + color.z) / 3.0f - threshold));
//
//	float flagR = saturate(sign(color.r - threshold.r));
//	float flagG = saturate(sign(color.g - threshold.g));
//	float flagB = saturate(sign(color.b - threshold.b));
//	float flag = saturate(sign(flagR + flagG + flagB));
//
//	color.a = flag;
//	output.Color1 = color * flag;
//	return output;
//}