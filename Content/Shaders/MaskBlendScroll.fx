#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0  
#define PS_SHADERMODEL ps_4_0
#endif

float CycleTime;

texture2D SpriteStencilTexture;

sampler2D SpriteTextureSampler : register(s0)
{
	Texture = (Texture);
	magfilter = POINT; minfilter = POINT; mipfilter = POINT;
};

sampler2D SpriteStencilTextureSampler = sampler_state
{
	Texture = (SpriteStencilTexture);
};


struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};


//_________________________________________________
// PixelShaders PS
//_________________________________________________

float4 MaskAndBlendPS(VertexShaderOutput input) : COLOR
{
    float2 texCoordOffset = float2(CycleTime, 0.0f);
	float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates + texCoordOffset);
	float4 stencilColor = tex2D(SpriteStencilTextureSampler, input.TextureCoordinates);

	color.a *= stencilColor.a;
	if (stencilColor.a < 0.01f)
		color.rgba = 0.0f;

	return color *= input.Color;
}

float4 MaskAndOverlayPS(VertexShaderOutput input) : COLOR
{
	float2 texCoordOffset = float2(CycleTime,0.0f);
	float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates + texCoordOffset);
	float4 stencilColor = tex2D(SpriteStencilTextureSampler, input.TextureCoordinates);

	color.rgba *= stencilColor.rgba;

	if (stencilColor.a < 0.01f)
		color.rgba = 0.0f;

	// Color here is the input color from spriteBatch.Draw(, , , Color.White , , , );  white doesn't change anything.
	return color *= input.Color;
}

float4 BasicPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	// Color here is the input color from spriteBatch.Draw(, ,, Color.White , , , );  white doesn't change anything.
	return color * input.Color;
}

//_________________________________________________
// Techniques
//_________________________________________________

technique MaskAndBlend
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL 
			MaskAndBlendPS();
	}
};

technique MaskAndOverlay
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL 
			MaskAndOverlayPS();
	}
};


technique Basic
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL 
			BasicPS();
	}
};