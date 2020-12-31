#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0  
#define PS_SHADERMODEL ps_4_0 //_level_9_1
#endif

//int numberOfSamplesPerDimension;
//float2 textureSize;
//float2 textureBlurUvOrigin;
//float radialScalar;

//uniform sampler2D sceneTex; // 0
float2 center; // Mouse position
float time; // effect elapsed time
float3 shockParams; // 10.0, 0.8, 0.1


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
		float2 uv = input.TextureCoordinates.xy;
		float2 texCoord = uv;
		float dis = distance(uv, center);
		float perturbation = 0.0f;
		if ((dis <= (time + shockParams.z)) && (dis >= (time - shockParams.z)))
		{
			float diff = (dis - time);
			float powDiff = 1.0 - pow( abs(diff * shockParams.x), shockParams.y);
			float diffTime = diff * powDiff;
			float2 diffUV = normalize(uv - center);
			texCoord = uv + (diffUV * diffTime);
			perturbation = diffTime;
		}
		float4 col = tex2D(SpriteTextureSampler, texCoord);
		col.rgb = col.rgb + input.Color.rgb * 2.0f * perturbation;
		return col;
}

technique Shockwave
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

//
//float4 SphereMovePS(VertexShaderOutput input) : COLOR
//{
//  float2 tc = input.TextureCoordinates.xy;
//  float2 p = -1.0 + 2.0 * tc;
//  float r = dot(p,p);
//  float4 col = tex2D(SpriteTextureSampler ,tc);
//  if (r <= 1.0f)
//  {
//  float f = (1.0 - sqrt(1.0 - r)) / (r);
//  float2 uv;
//  uv.x = p.x * f + time;
//  uv.y = p.y * f + time;
//  col = float4(tex2D(SpriteTextureSampler ,uv).xyz, 1.0f);
//  //col.rgb = col.rgb + input.Color.rgb * 1.0f * f;
//  }
//  return col;
//}

//float4 AlgorithmicRefractionWavePS(VertexShaderOutput input) : COLOR
//{
//	float2 tc = input.TextureCoordinates.xy;
//	float2 p = -1.0f + 2.0f * tc;
//	float len = length(p);
//	float2 uv = tc + (p / len) * cos(len * 12.0f - time * 4.0f) * 0.03f;
//	float4 col = tex2D(SpriteTextureSampler, uv);
//	return col;
//}
