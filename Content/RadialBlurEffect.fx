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
float2 textureBlurUvOrigin;
float radialScalar;

//
//sampler2D SpriteTextureSampler : register(s0)
//{
//	Texture = (Texture);
//};

Texture2D SpriteTexture;
SamplerState SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
	AddressU = Clamp;
	AddressV = Clamp;
	magfilter = Point;
	minfilter = Point;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	//float halfnumberOfSamplesPerDimension = numberOfSamplesPerDimension / 2.0f;
	float2 texelCoef = 1.0f / float2(textureSize.x, textureSize.y);
	float2 adjustedSampleCoords = normalize(textureBlurUvOrigin - input.TextureCoordinates) * radialScalar;
	for (float u = 0.0f; u < numberOfSamplesPerDimension; u += 1.0f)
	{
		for (float v = 0.0f; v < numberOfSamplesPerDimension; v += 1.0f)
		{
			float2 uv = adjustedSampleCoords * length( float2(u, v) * texelCoef * numberOfSamplesPerDimension );
			color += tex2D(SpriteTextureSampler, input.TextureCoordinates + uv);
		}
	}
	color /= numberOfSamplesPerDimension * numberOfSamplesPerDimension;
	return color * input.Color;
}

technique RadialBlur
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

