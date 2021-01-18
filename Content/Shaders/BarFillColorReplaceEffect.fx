
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float offset;
float percentageOfFill;
float blueReplaceThreshold;

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state { Texture = <SpriteTexture>; };
struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR
{
	float2 coords = input.TextureCoordinates;
	float4 col = tex2D(SpriteTextureSampler, coords);
	float boolmult = saturate(sign(blueReplaceThreshold - col.b));
	return  (1.0f - saturate(sign((1.0f - coords.y) - percentageOfFill + offset))) * (1.0f - boolmult) * input.Color + boolmult * col;
}
technique SpriteDrawing
{
	pass 
	{ 
		PixelShader = compile PS_SHADERMODEL 
		PixelShaderFunction(); 
	}
};