// References
// 
//
// struct semantics
//     float2 TexureCoordinate : TEXCOORD0; //float4 Color : COLOR0; //float3 Normal : NORMAL0; //float3 Tangent : NORMAL1; //float4 BoneIds : BLENDINDICES; //float4 BoneWeights : BLENDWEIGHT;
//
// sampler options
//     Magfilter = LINEAR; //Minfilter = LINEAR; //Mipfilter = LINEAR; //AddressU = mirror; //AddressV = mirror;

//++++++++++++++++++++++++++++++++++++++++
// D E F I N E S
//++++++++++++++++++++++++++++++++++++++++

#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0 //_level_9_1
#define PS_SHADERMODEL ps_4_0 //_level_9_1
#endif
#define PI 3.14159265359f
#define RECIPROCAL_PI 0.31830988618f
#define EPSILON 1e-6



//++++++++++++++++++++++++++++++++++++++++
// S H A D E R  G L O B A L S
//++++++++++++++++++++++++++++++++++++++++

float3 CameraPosition;
float3 LightPosition;
float3 LightColor;

matrix World;
matrix View;
matrix Projection;



//++++++++++++++++++++++++++++++++++++++++
// T E X T U R E S  A N D  S A M P L E R S
//++++++++++++++++++++++++++++++++++++++++

Texture2D TextureDiffuse;
sampler2D TextureSamplerDiffuse = sampler_state
{
	texture = (TextureDiffuse);
};

TextureCube TextureCubeDiffuse;
samplerCUBE CubeMapSampler = sampler_state
{
	texture = <TextureCubeDiffuse>;
	AddressU = clamp;
	AddressV = clamp;
};

TextureCube TextureCubeEnviromental;
samplerCUBE CubeMapEnviromentalSampler = sampler_state
{
	texture = <TextureCubeEnviromental>;
	AddressU = clamp;
	AddressV = clamp;
};



//++++++++++++++++++++++++++++++++++++++++
// S T R U C T S
//++++++++++++++++++++++++++++++++++++++++

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float3 Tangent : NORMAL1;
	float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL0;
	float3 Tangent : NORMAL1;
	float2 TextureCoordinates : TEXCOORD0;
	float3 Position3D : TEXCOORD1;
};

struct VsInputCalcSceneDepth
{
	float4 Position : POSITION0;
};
struct VsOutputCalcSceneDepth
{
	float4 Position     : SV_Position;
	float4 Position3D    : TEXCOORD0;
};



//++++++++++++++++++++++++++++++++++++++++
// F U N C T I O N S
//++++++++++++++++++++++++++++++++++++++++

float4 TexEnvCubeLod(samplerCUBE samp, float3 normal, float miplevel) {
	normal.y = -normal.y;
	return texCUBElod(samp , float4 (normal, miplevel));
}
float4 TexCubeLod(samplerCUBE samp, float3 normal, float miplevel) {
	normal.yz = -normal.yz;
	return texCUBElod(samp, float4 (normal, miplevel));
}



//++++++++++++++++++++++++++++++++++++++++
// V E R T E X  S H A D E R S
//++++++++++++++++++++++++++++++++++++++++

VertexShaderOutput VS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	matrix vp = mul(View, Projection);
	matrix wvp = mul(World, vp);

	output.TextureCoordinates = input.TextureCoordinates;
	output.Position3D = mul(input.Position, World);
	output.Normal = mul(input.Normal, World);
	output.Tangent = mul(input.Tangent, World);
	output.Position = mul(input.Position, wvp); // we transform the position.

	return output;
}

// Shader.
VsOutputCalcSceneDepth VS_CreateDepthMapVertexShader(VsInputCalcSceneDepth input)//(float4 inPos : POSITION)
{
	VsOutputCalcSceneDepth output;
	output.Position3D = mul(input.Position, World);
	float4x4 vp = mul(View, Projection);
	output.Position = mul(output.Position3D, vp);

	return output;
}



//++++++++++++++++++++++++++++++++++++++++
// P I X E L  S H A D E R S
//++++++++++++++++++++++++++++++++++++++++

float4 PS_RenderBasicScene(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	float4 col = tex2D(TextureSamplerDiffuse, input.TextureCoordinates);

	return col;
}

float4 PS_CreateDepthMapPixelShader(VsOutputCalcSceneDepth input) : COLOR
{
	return length(LightPosition - input.Position3D);
}

// DX the texture cube stores data inverted  float3(N.x, -N.y, -N.z)
float4 PS_RenderVisualizationDepthCube(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	//float4 col = texCUBElod(CubeMapSampler, float4 (N, 0));
	float4 col = TexCubeLod(CubeMapSampler, N, 0);
	//float4 col = TexEnvCubeLod(CubeMapSampler, N, 0);
	col.a = 1.0f;
	col.rgb = saturate(col.r / float3(10000.0f, 1000.0f, 100.0f));
	col.b = 1.0f - col.r;

	return col;
}

float4 PS_RenderBasicCubeMap(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	//float4 col = texCUBElod(CubeMapSampler, float4(N, 0));
	float4 col = TexCubeLod(CubeMapSampler, N, 0); 

	col.rgb += float3(0.05f, 0.05f, 0.05f); // little bit of ambient.

	return col;
}

float4 PS_RenderBasicUnalteredRenderTargetCubeMap(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	float4 col = texCUBElod(CubeMapSampler, float4(N, 0));
	//float4 col = TexCubeLod(CubeMapSampler, N, 0);

	col.rgb += float3(0.05f, 0.05f, 0.05f); // little bit of ambient.

	return col;
}

float4 PS_RenderBasicSkyCubeMap(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	//float4 col = texCUBElod(CubeMapSampler, float4(N, 0));
	float4 col = TexEnvCubeLod(CubeMapSampler, N, 0);

	col.rgb += float3(0.05f, 0.05f, 0.05f); // little bit of ambient.

	return col;
}

//++++++++++++++++++++++++++++++++++++++++
// T E C H N I Q U E S.
//++++++++++++++++++++++++++++++++++++++++

technique Render_BasicScene
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS_RenderBasicScene();
	}
}

technique Render_LightDepth
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL 
			VS_CreateDepthMapVertexShader();
		PixelShader = compile PS_SHADERMODEL 
			PS_CreateDepthMapPixelShader();
	}
}

technique Render_VisualizationDepthCube
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS_RenderVisualizationDepthCube();
	}
};

technique Render_BasicCubeMapScene
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS_RenderBasicCubeMap();
	}
}

technique Render_BasicSkyCubeMapScene
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS_RenderBasicSkyCubeMap();
	}
}

technique Render_BasicUnalteredRenderTargetCubeMap
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS_RenderBasicUnalteredRenderTargetCubeMap();
	}
}


