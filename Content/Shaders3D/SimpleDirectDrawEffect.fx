//______________________________________________________________________________________
// The shader level.  we are using Dx 
//______________________________________________________________________________________

#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0 //_level_9_1
	#define PS_SHADERMODEL ps_4_0 //_level_9_1
#endif

//______________________________________________________________________________________
// inputs from game 1
//______________________________________________________________________________________




texture2D SpriteTexture; // the texture parameter input from game1


//______________________________________________________________________________________
// the sampler that we associate with the sprite texture
//______________________________________________________________________________________

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = (SpriteTexture);
};



//______________________________________________________________________________________
// Structures we will be using the vertices input must match the vertex structure definition in game1
//______________________________________________________________________________________

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 TextureCoordinates : TEXCOORD0;
};



//______________________________________________________________________________________
// Our own vertex shader.
//______________________________________________________________________________________

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.Position = input.Position;
	output.TextureCoordinates = input.TextureCoordinates;
	return output;
}


//______________________________________________________________________________________
// Our familiar pixel shader.
//______________________________________________________________________________________

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 col = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	return col;
}


//______________________________________________________________________________________
// The technique.
//______________________________________________________________________________________

technique SimpleTriangleDraw
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL 
			MainVS();
		PixelShader = compile PS_SHADERMODEL 
			MainPS();
	}
};