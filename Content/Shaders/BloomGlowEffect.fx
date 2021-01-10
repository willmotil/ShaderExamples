#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0  
#define PS_SHADERMODEL ps_4_0 //_level_9_1
#endif

int numberOfSamplesPerDimension;
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

struct PixelShaderOutput
{
	float4 Color : COLOR0;
	float4 Color2 : COLOR1;
};

//float4 MainPS(VertexShaderOutput input) : COLOR0 : COLOR1
PixelShaderOutput MainPS(VertexShaderOutput input)
{
	PixelShaderOutput output;
	float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	float halfnumberOfSamplesPerDimension = numberOfSamplesPerDimension / 2.0f;
	float2 texelCoef = 1.0f / float2(textureSize.x, textureSize.y);
	for (float u = 0.0f; u < numberOfSamplesPerDimension; u += 1.0f)
	{
		for (float v = 0.0f; v < numberOfSamplesPerDimension; v += 1.0f)
		{
			float su = (u - halfnumberOfSamplesPerDimension) * texelCoef.x;
			float sv = (v - halfnumberOfSamplesPerDimension) * texelCoef.y;
			color += tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(su, sv));
		}
	}
	color /= numberOfSamplesPerDimension * numberOfSamplesPerDimension;
	output.Color = color * input.Color;
	output.Color2 = color * 0.5f;
	return output;
}

technique BloomGlow
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

float4 RegularPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	// Color here is the input color from spriteBatch.Draw(, ,, Color.White , , , );  white doesn't change anything.
	return color * input.Color;
}

technique Basic
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL RegularPS();
	}
};
