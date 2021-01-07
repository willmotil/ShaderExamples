#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

//float percent;

sampler2D TextureSampler : register(s0)
{
	Texture = (Texture);
};


float4 GammaPS(float4 position : SV_Position, float4 color : COLOR0, float2 TextureCoordinates : TEXCOORD0) : COLOR0
{
	float4 originalcol = tex2D(TextureSampler, TextureCoordinates) * color;
	float alpha = originalcol.a;
	float gamma = 2.2f;
	float falloff = 1.0f - saturate(TextureCoordinates.y - 0.5f) * 2.0f;

	// falloff as is;  seen in.                                                               // segment 1 / of 5;
	float4 col = originalcol * falloff;

	if (TextureCoordinates.x > 0.20f && TextureCoordinates.x < 0.40f) // segment 2 of 5;
		col =  ((originalcol * (1.0f / gamma)) * falloff) ;

	if (TextureCoordinates.x > 0.40f && TextureCoordinates.x < 0.60f) // segment 3 of 5;
		col = pow( ((originalcol * gamma) * falloff) , gamma);

	if (TextureCoordinates.x > 0.60f && TextureCoordinates.x < 0.80f) // segment 4 of 5;
		col = (originalcol * (1.0f / gamma)) * falloff * gamma;

	if (TextureCoordinates.x > 0.80f)                                                // segment 5 of 5;
		col = (originalcol * (1.0f / gamma)) * falloff; //* gamma;


	if (TextureCoordinates.y > 0.85f)                                                // white created strip.
		col = float4(1.0f, 1.0f, 1.0f, 1.0f) * (1.0f / gamma) * TextureCoordinates.x;
	if (TextureCoordinates.y > 0.90f)                                                // white linear created strip gamma'ed.
		col = float4(1.0f, 1.0f, 1.0f, 1.0f) * TextureCoordinates.x;      // * gamma * TextureCoordinates.x * (1.0f / gamma);
	if (TextureCoordinates.y > 0.95f)                                                // white linear created strip gammaed then linearized.
		col = float4(1.0f, 1.0f, 1.0f, 1.0f) * gamma * TextureCoordinates.x ;

	col.a = alpha;
	return col;
}

technique Gamma
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL 
			GammaPS();
	}
};