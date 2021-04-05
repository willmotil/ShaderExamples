//__________________________________________________________
// the shader defines
//__________________________________________________________


#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0 //_level_9_1
	#define PS_SHADERMODEL ps_4_0 //_level_9_1
#endif

float3 LightPosition;

matrix World;
matrix View;
matrix Projection;

texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
	Texture = (SpriteTexture);
};

// structs
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL0;
	float2 TextureCoordinates : TEXCOORD0;
	float4 Position3D : TEXCOORD1;
};


//   using the matrices in our vertice shader this time.

VertexShaderOutput VS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	matrix vp = mul(View, Projection);
	matrix wvp = mul(World, vp);

	output.Position = mul(input.Position, wvp); // we transform the position.
	output.TextureCoordinates = input.TextureCoordinates;
	output.Normal = mul(input.Normal, World);
	output.Position3D = mul(input.Position, World);

	return output;
}


// our familiar pixel shader.
float4 PS(VertexShaderOutput input) : COLOR
{
	float4 col = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	float3 P = input.Position3D;
	float3 L = normalize(LightPosition - P);
	float3 N = input.Normal;

	// simple diffuse.
	float NdotL = max(0.0f, dot( N, L) );

	//	col.rgb = NdotL;
	col.rgb = (col.rgb * NdotL) + (col.rgb * 0.1f);

	return col;
}


// the technique.
technique DiffuseLighting
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS();
	}
};
