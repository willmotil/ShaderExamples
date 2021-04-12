// dx

// Reference
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

float3 NormalCoordAdjustment;

float AmbientStrength;
float DiffuseStrength;
float SpecularStrength;

matrix World;
matrix View;
matrix Projection;

//++++++++++++++++++++++++++++++++++++++++
// T E X T U R E S  A N D  S A M P L E R S
//++++++++++++++++++++++++++++++++++++++++

TextureCube TextureCubeDiffuse;
samplerCUBE CubeMapSampler = sampler_state
{
	texture = <TextureCubeDiffuse>;
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


//++++++++++++++++++++++++++++++++++++++++
// F U N C T I O N S
//++++++++++++++++++++++++++++++++++++++++

float3 GammaToLinear(float3 gammaColor)
{
	return pow(gammaColor, 2.2f);
}
float3 LinearToGamma(float3 gammaColor)
{
	return pow(gammaColor, .454f);
}

float MaxDot(float3 a, float3 b)
{
	return max(0.0f, dot(a, b));
}

// L , V
float3 HalfNormal(float3 pixelToLight, float3 pixelToCamera)
{
	return normalize(pixelToLight + pixelToCamera);
}

float IsFrontFaceToLight(float3 L, float3 N)
{
	return sign(saturate(dot(L, N)));
}

// to viewer can be   pos   or  v   aka  cam - pixel
float SpecularPhong(float3 toViewer, float3 toLight, float3 normal, float shininess)
{
	float3 viewDir = normalize(-toViewer);
	float3 reflectDir = reflect(toLight, normal);
	float b = max(dot(reflectDir, viewDir), 0.0f);
	float backfaceRemoval = saturate( sign( dot(toLight, normal) ) );
	return pow(b, shininess / 4.0f) * backfaceRemoval; // note that the exponent is different here
}

float SpecularBlinnPhong(float3 toViewer, float3 toLight, float3 normal, float shininess)
{
	toViewer = normalize(toViewer);
	float3 halfnorm = normalize(toLight + toViewer);
	float cosb = max(dot(normal, halfnorm), 0.0f);
	float backfaceRemoval = saturate(sign(dot(toLight, normal)));
	return pow(cosb, shininess) * backfaceRemoval;
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

//++++++++++++++++++++++++++++++++++++++++
// P I X E L  S H A D E R S
//++++++++++++++++++++++++++++++++++++++++

// DX the texture cube stores data inverted this is a real pain in the ass.
float4 PS_RenderCube(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	float3 ntex = N * NormalCoordAdjustment;  // norm float3(N.x, -N.y, -N.z);
	float4 col = texCUBElod(CubeMapSampler, float4 (ntex, 0));
	//clip(col.a - .01f); // straight clip low alpha.

	float3 P = input.Position3D;
	float3 C = CameraPosition;
	float3 V = normalize(C - P);
	float NdotV = MaxDot(N, V);
	float3 R = 2.0f * NdotV * N - V;
	float3 L = normalize(LightPosition - P);
	float3 H = HalfNormal(L, V);
	float NdotH = MaxDot(N, H);
	float NdotL = MaxDot(N, L);

	float spec = SpecularBlinnPhong(V, L, N, 100.0f);
	float3 speccol = col.rgb * LightColor;
	col.rgb = saturate( (speccol.rgb * spec * SpecularStrength) + (col.rgb * NdotL * DiffuseStrength) + (col.rgb * AmbientStrength) );

	return col;
}

// DX the texture cube stores data inverted this is a real pain in the ass.
float4 PS_RenderSkybox(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	float3 ntex = N * NormalCoordAdjustment; // float3(N.x, -N.y, N.z);
	float4 col = texCUBElod(CubeMapSampler, float4 (ntex, 0));
	//clip(col.a - .01f); // straight clip low alpha.

	col.rgb = saturate(col.rgb * AmbientStrength);

	return col;
}

//++++++++++++++++++++++++++++++++++++++++
// T E C H N I Q U E S.
//++++++++++++++++++++++++++++++++++++++++

technique Render_Skybox
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS_RenderSkybox();
	}
};

technique Render_Cube
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS_RenderCube();
	}
};

