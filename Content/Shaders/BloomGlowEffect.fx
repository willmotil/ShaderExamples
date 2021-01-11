#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0  
#define PS_SHADERMODEL ps_4_0 //_level_9_1
#endif

int numberOfSamplesPerDimension;
float threshold;
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

PixelShaderOutput MainPS(VertexShaderOutput input)
{
	PixelShaderOutput output;
	float4 originalColor = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	float4 color = originalColor;
	float2 hnod = numberOfSamplesPerDimension / 2.0f;
	float sampledistance = 1.0f;
	float2 texelCoef = (1.0f / float2(textureSize.x, textureSize.y)) * sampledistance;
	for (float u = -hnod.x; u < hnod.x; u += 1.0f)
	{
		for (float v = -hnod.y; v < hnod.y; v += 1.0f)
		{
			float su = u * texelCoef.x;
			float sv = v * texelCoef.y;
			color += tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(su, sv));
		}
	}
	color = color /  (numberOfSamplesPerDimension * numberOfSamplesPerDimension) * input.Color;
	//output.Color = originalColor;
	output.Color = color;

	float flag = saturate(sign((color.x + color.y + color.z) / 3.0f - threshold));

	//float a = color.a;
	//float flag =  color * saturate( sign( color - threshold ) );  // (color.x + color.y + color. z) / 3.0f
	//output.Color2.a = a;
    //float flag =  color * dot(color.rgb, float3(0.2126f, 0.7152f, 0.0722f));

	color.a = flag;
    output.Color2 = color * flag; 
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
	float4 c = tex2D(SpriteTextureSampler, input.TextureCoordinates) * input.Color;

	//float R = saturate(c.r - threshold);
	//float G = saturate(c.g - threshold);
	//float B = saturate(c.b - threshold);

	//float flag = saturate(sign((c.x + c.y + c.z) / 3.0f - threshold));


	return c;
}

technique ExtractGlowColors
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL RegularPS();
	}
};
