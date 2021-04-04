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
float AmbientStrength;

matrix World;
matrix View;
matrix Projection;

Texture2D TextureDiffuse;
sampler2D TextureSamplerDiffuse = sampler_state
{
	texture = (TextureDiffuse);
};

Texture2D TextureNormalMap;
sampler TextureSamplerNormalMap = sampler_state
{
	texture = (TextureNormalMap);
	AddressU = clamp;
	AddressV = clamp;
};

// structs
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float4 Tangent : NORMAL1;
	float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Normal : NORMAL0;
	float4 Tangent : NORMAL1;
	float2 TextureCoordinates : TEXCOORD0;
	float3 Position3D : TEXCOORD1;
};

//
float3 FunctionNormalMapGeneratedBiTangent(float3 normal, float3 tangent, float2 texCoords)
{
	// Normal Map
	float3 NormalMap = tex2D(TextureSamplerNormalMap, texCoords).rgb;
	NormalMap.g = 1.0f - NormalMap.g;  // flips the y. the program i used fliped the green,  bump mapping is when you don't do this i guess.
	NormalMap = NormalMap * 2.0f - 1.0f;
	float3 bitangent = cross(normal, tangent);

	float3x3 mat;
	mat[0] = bitangent; // set right
	mat[1] = tangent; // set up
	mat[2] = normal; // set forward

	return normalize(mul(NormalMap, mat)); // norm to ensure later scaling wont break it.
}

// 
VertexShaderOutput VS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	matrix vp = mul(View, Projection);
	matrix wvp = mul(World, vp);

	output.Position = mul(input.Position, wvp); // we transform the position.
	output.TextureCoordinates = input.TextureCoordinates;
	output.Normal = mul(input.Normal, World);
	output.Tangent = mul(input.Tangent, World);
	output.Position3D = mul(input.Position, World);

	return output;
}

// 
float4 PS(VertexShaderOutput input) : COLOR
{
	float4 col = tex2D(TextureSamplerDiffuse, input.TextureCoordinates);

	// simple diffuse.
	float3 N = FunctionNormalMapGeneratedBiTangent(input.Normal, input.Tangent, input.TextureCoordinates);
	float3 L = normalize(LightPosition - input.Position3D);
	float LdotN = saturate(dot(N, L));
	col.rgb = (col.rgb * LdotN * 0.90f) + (col.rgb * AmbientStrength);

	return col;
}


// the technique.
technique Lighting
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS();
	}
};


