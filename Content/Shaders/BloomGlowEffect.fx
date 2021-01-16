

// TODO  wip   humm trying to follow this exactly like the classical way of doing it if there is such a thing, not my janky way.

// https://learnopengl.com/Advanced-Lighting/Bloom
// https://learnopengl.com/code_viewer_gh.php?code=src/5.advanced_lighting/7.bloom/7.blur.fs


#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0  
#define PS_SHADERMODEL ps_4_0 //_level_9_1
#endif


int numberOfSamplesPerDimension;
float4 threshold;
float thresholdTolerance;
float2 textureSize;
bool horizontal;
float weight[] = { 0.227027f, 0.1945946f, 0.1216216f, 0.054054f, 0.016216f };


Texture2D SpriteTexture;
Texture2D SecondaryTexture;
SamplerState SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
	//AddressU = Wrap; AddressV = Wrap; //magfilter = linear;//minfilter = linear;
};
SamplerState SecondaryTextureSampler = sampler_state
{
	Texture = <SecondaryTexture>;
	//AddressU = Wrap; AddressV = Wrap; //magfilter = linear;//minfilter = linear;
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
	//float4 Color1 : COLOR1;
};




float4 ExtractOverThreshold(float4 col) 
{
	float4 color = float4(col.rgb, 1.0f);
	// check whether fragment output is higher than threshold, if so output as brightness color
	float brightness = dot(color.rgb, threshold.rgb );  //float3(0.2126, 0.7152, 0.0722)
	if (brightness > thresholdTolerance)  // 1.0f
		color = float4(color.rgb, 1.0f);
	else
		color = float4(0.0f, 0.0f, 0.0f, 1.0f);
	return color;
}




PixelShaderDuelOutput ExtractBrightColorsPS(VertexShaderOutput input)
{
	PixelShaderDuelOutput output;
	float4 col = tex2D(SpriteTextureSampler, input.TextureCoordinates) * input.Color;
	output.Color0 = ExtractOverThreshold(col);
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




PixelShaderDuelOutput BloomPS(VertexShaderOutput input)
{
	PixelShaderDuelOutput output;
	float2 texCoords = input.TextureCoordinates;
	float4 color = tex2D(SpriteTextureSampler, texCoords);
	float sampledistance = 1.0f;
	// ...
	float2 texOffset = (1.0f / float2(textureSize.x, textureSize.y)) * sampledistance; // gets size of single texel
	float3 result = color.rgb * weight[0]; // current fragment's contribution
	if (horizontal)
	{
		for (int i = 1; i < 5; ++i)
		{
			result += tex2D(SpriteTextureSampler, texCoords + float2(texOffset.x * i, 0.0f)).rgb * weight[i];
			result += tex2D(SpriteTextureSampler, texCoords - float2(texOffset.x * i, 0.0f)).rgb * weight[i];
		}
	}
	else
	{
		for (int i = 1; i < 5; ++i)
		{
			result += tex2D(SpriteTextureSampler, texCoords + float2(0.0f, texOffset.y * i)).rgb * weight[i];
			result += tex2D(SpriteTextureSampler, texCoords - float2(0.0f, texOffset.y * i)).rgb * weight[i];
		}
	}
		color = float4(result, 1.0f);
	output.Color0 = color;
	return output;
}


technique Bloom
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL 
			BloomPS();
	}
};






PixelShaderDuelOutput CombineBloomPS(VertexShaderOutput input)
{
	PixelShaderDuelOutput output;
	float4 col = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	float4 col2 = tex2D(SecondaryTextureSampler, input.TextureCoordinates);
	//output.Color0 = max(col , col2);
	output.Color0 = col + col2;
	return output;
}

technique  CombineBloom
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL
			CombineBloomPS();
	}
};

//_________________________________________________________________________________________________
//_________________________________________________________________________________________________


//float IsOverThreshold(float4 col)
//{
//	float flagR = saturate(sign(col.r - threshold.r));
//	float flagG = saturate(sign(col.g - threshold.g));
//	float flagB = saturate(sign(col.b - threshold.b));
//	float flagA = saturate(sign(col.a - threshold.a));
//	float sum = (flagR + flagG + flagB) + flagA - 3.0f;  // this will make the requisites that all r g b be above the threshold value.
//	float flag = saturate(sign(sum));
//	return flag;
//}


