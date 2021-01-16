

#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0  
#define PS_SHADERMODEL ps_4_0 //_level_9_1
#endif


int numberOfSamplesPerDimension;
float concentration;
float2 textureSize;


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

struct PixelShaderOutput
{
	float4 Color0 : COLOR0;
};


PixelShaderOutput ShadowTextIllusionPS(VertexShaderOutput input)
{
	PixelShaderOutput output;
	float2 texCoords = input.TextureCoordinates;
	float sampledistance = 1.0f * concentration;
	float2 texOffset = (1.0f / float2(textureSize.x, textureSize.y)) * sampledistance; // gets size of single texel
	float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
	for (int i = 1; i < numberOfSamplesPerDimension; ++i)
	{
		float4 temp = tex2D(SpriteTextureSampler, texCoords + float2(texOffset.x * i, -texOffset.y * i));
		if (temp.a > 0.01f) 
		{
			float seeThru = (1.0f - (i / numberOfSamplesPerDimension)) * 0.15f * temp.a;
			col += float4(0.0f, 0.0f, 0.0f, seeThru); // or passed in shadow color.rgb
		}
			
	}
	float4 cur = tex2D(SpriteTextureSampler, texCoords)* input.Color;
	if (cur.a > 0.01f)
		col += cur;
	output.Color0 = col;
	return output;
}

technique  ShadowText
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL
			ShadowTextIllusionPS();
	}
};

//_________________________________________________________________________________________________
//_________________________________________________________________________________________________


