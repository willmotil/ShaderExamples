#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0  
#define PS_SHADERMODEL ps_4_0
#endif

texture2D SpriteMultiTexture;
texture2D SpriteStencilTexture;

sampler2D SpriteTextureSampler : register(s0)
{
	Texture = (Texture);
	magfilter = POINT; minfilter = POINT; mipfilter = POINT;
};

sampler2D SpriteMultiTextureSampler = sampler_state
{
	Texture = (SpriteMultiTexture);
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

float4 MaskAndOverlayPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	float4 overlayColor = tex2D(SpriteMultiTextureSampler, input.TextureCoordinates);
	float4 stencilColor = tex2D(SpriteStencilTextureSampler, input.TextureCoordinates);

	float alpha = overlayColor.a;
	float invalpha = 1.0f - overlayColor.a;

    color.rgb = color.rgb * invalpha + overlayColor.rgb * alpha;

	if (stencilColor.a < 0.01f)
		color.rgba = 0.0f;

	// Color here is the input color from spriteBatch.Draw(, ,, Color.White , , , );  white doesn't change anything.
	return color *= input.Color;
}

float4 MaskAndBlendPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	float4 textureBlendColor = tex2D(SpriteMultiTextureSampler, input.TextureCoordinates);
	float4 stencilColor = tex2D(SpriteStencilTextureSampler, input.TextureCoordinates);

	color.rgb *= textureBlendColor.rgb;

	if (stencilColor.a < 0.01f)
		color.rgba = 0.0f;

	return color *= input.Color;
}

float4 RegularPS(VertexShaderOutput input) : COLOR
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
		PixelShader = compile PS_SHADERMODEL MaskAndBlendPS();
	}
};

technique MaskAndOverlay
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MaskAndOverlayPS();
	}
};


technique Basic
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL RegularPS();
	}
};