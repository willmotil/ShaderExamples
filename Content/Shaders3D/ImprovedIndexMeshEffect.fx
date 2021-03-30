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
	float4 Normal : NORMAL0;
	float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Normal : NORMAL0;
	float2 TextureCoordinates : TEXCOORD0;
};


//   using the matrices in our vertice shader this time.

VertexShaderOutput TriangleDrawWithTransformsVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	matrix vp = mul(View, Projection);
	matrix wvp = mul(World, vp);

	output.Position = mul(input.Position, wvp); // we transform the position.
	output.Normal = mul(input.Normal, wvp);
	output.TextureCoordinates = input.TextureCoordinates;

	return output;
}


// our familiar pixel shader.
float4 TriangleDrawWithTransformsPS(VertexShaderOutput input) : COLOR
{
	float4 col = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	//col.g += 0.5f;
	return col;
}


// the technique.
technique IndexedMeshDraw
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL
			TriangleDrawWithTransformsVS();
		PixelShader = compile PS_SHADERMODEL
			TriangleDrawWithTransformsPS();
	}
};


//______________________________________________________________________________

// GraphicsDevice.Textures[0] = myTexture2d;  Alternately we could assign the texture manually to the graphics device in game1 that sets it to it to register T0
//sampler2D SpriteTextureSampler : register(s0)
//{
//	Texture = (Texture);  //magfilter = POINT; minfilter = POINT; mipfilter = POINT;
//};