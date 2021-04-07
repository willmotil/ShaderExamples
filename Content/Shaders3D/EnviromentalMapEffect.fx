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

float AmbientStrength;
float DiffuseStrength;
float SpecularStrength;

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

Texture2D TextureNormalMap;
sampler TextureSamplerNormalMap = sampler_state
{
	texture = (TextureNormalMap);
	AddressU = clamp;
	AddressV = clamp;
};

TextureCube CubeMapDiffuse;
samplerCUBE CubeMapSampler = sampler_state
{
	texture = <CubeMapDiffuse>;
	AddressU = clamp;
	AddressV = clamp;
	//magfilter = Linear;//minfilter = Linear;//mipfilter = Linear;
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

float3 GammaToLinear(float3 gammaColor)
{
	return pow(gammaColor, 2.2f);
}

// to viewer can be   pos   or  v   aka  cam - pixel
float SpecularPhong(float3 toViewer, float3 toLight, float3 normal, float shininess)
{
	float3 viewDir = normalize(-toViewer);
	float3 reflectDir = reflect(toLight, normal);
	float b = max(dot(reflectDir, viewDir), 0.0f);
	return pow(b, shininess / 4.0f); // note that the exponent is different here
}

float SpecularBlinnPhong(float3 toViewer, float3 toLight, float3 normal, float shininess)
{
	toViewer = normalize(toViewer);
	float3 halfnorm = normalize(toLight + toViewer);
	float cosb = max(dot(normal, halfnorm), 0.0f);
	return pow(cosb, shininess);
}

float SpecularSharpener(float specular, float scalar)
{
	return saturate(specular - scalar) * (1.0f / (1.0f - scalar));
}

float SpecularCurveFit(float3 V, float3 L, float3 N, float sharpness)
{
	float3 h = normalize(L + V);
	float ndoth = max(dot(N, h), 0.0f);

	float a = sharpness / (sharpness + 0.1f); // infinite sliding limit.
	float ndotl = saturate(dot(L, N));
	float r = (dot(V, reflect(-L, N)) + ndoth * 0.07f) / 1.07f;  // *ndotl;
	float result = saturate(r - a) * ( 1.0f / ( 1.0f - a) );

	return result;
}


float Falloff(float distance, float lightRadius)
{
	return pow(saturate(1.0f - pow(distance / lightRadius, 4)), 2) / (distance * distance + 1);
}

// Determines inflection position on the opposite side of a plane defined by (point and a normal) \|/
float3 InflectionPositionFromPlane(float3 anyPositionOnPlaneP, float3 theSurfaceNormalN, float3 theCameraPostionC)
{
	float camToPlaneDist = dot(theSurfaceNormalN, theCameraPostionC - anyPositionOnPlaneP);
	return theCameraPostionC - theSurfaceNormalN * camToPlaneDist * 2;
}

float ReflectionTheta(float3 L, float3 N, float3 V)
{
	return dot(V, reflect(-L, N));
}

float3 FunctionNormalMapGeneratedBiTangent(float3 normal, float3 tangent, float2 texCoords)
{
	// Normal Map
	float3 NormalMap = tex2D(TextureSamplerNormalMap, texCoords).rgb;
	NormalMap.g = 1.0f - NormalMap.g;  // flips the y. the program i used fliped the green,  bump mapping is when you don't do this i guess.
	NormalMap = NormalMap * 2.0f - 1.0f;
	float3 bitangent = normalize(cross(normal, tangent));

	float3x3 mat;
	mat[0] = bitangent; // set right
	mat[1] = tangent; // set up
	mat[2] = normal; // set forward

	return normalize(mul(NormalMap, mat)); // norm to ensure later scaling wont break it.
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


VertexShaderOutput RenderCubeMapVS(in VertexShaderInput input)
{
	VertexShaderOutput output;
	float4x4 vp = mul(View, Projection);
	float4 pos = mul(input.Position, World);
	float4 norm = mul(input.Normal, World);
	output.Position = mul(pos, vp);
	output.Position3D = pos.xyz;
	output.Normal = norm.xyz;
	output.TextureCoordinates = input.TextureCoordinates;
	return output;
}

//++++++++++++++++++++++++++++++++++++++++
// P I X E L  S H A D E R S
//++++++++++++++++++++++++++++++++++++++++

float4 PS_Phong(VertexShaderOutput input) : COLOR
{
	float4 col = tex2D(TextureSamplerDiffuse, input.TextureCoordinates);
	float3 N = FunctionNormalMapGeneratedBiTangent(input.Normal, input.Tangent, input.TextureCoordinates);
	float3 P = input.Position3D;
	float3 C = CameraPosition;
	float3 V = normalize(C - P);
	float NdotV = MaxDot(N, V);
	float3 R = 2.0f * NdotV * N - V;
	float3 L = normalize(LightPosition - P);
	float3 H = HalfNormal(L, V);
	float NdotH = MaxDot(N, H);
	float NdotL = MaxDot(N, L);

	float spec= SpecularPhong(V, L, N, 100.0f);
	float3 speccol = col.rgb * LightColor;
	col.rgb =  (speccol.rgb * spec * SpecularStrength) +(col.rgb * NdotL * NdotL * DiffuseStrength) + (col.rgb * AmbientStrength);
	//col.rgb = (speccol.rgb * spec * (SpecularStrength + DiffuseStrength));
	return col;
}

float4 PS_BlinnPhong(VertexShaderOutput input) : COLOR
{
	float4 col = tex2D(TextureSamplerDiffuse, input.TextureCoordinates);
	float3 N = FunctionNormalMapGeneratedBiTangent(input.Normal, input.Tangent, input.TextureCoordinates);
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
	col.rgb = (speccol.rgb * spec * SpecularStrength) + (col.rgb * NdotL * NdotL * DiffuseStrength) + (col.rgb * AmbientStrength);
	//col.rgb = (speccol.rgb * spec * (SpecularStrength + DiffuseStrength));
	return col;
}

float4 RenderCubeMapPS(VertexShaderOutput input) : COLOR
{
	//float4 baseColor = tex2D(TextureSamplerDiffuse, input.TextureCoordinate); 
	////clip(baseColor.a - .01f); // just straight clip super low alpha.
	//float3 P = input.Position3D;
	//float3 N = normalize(input.Normal3D.xyz);
	//float3 V = normalize(CameraPosition - input.Position3D);
	//float NdotV = max(0.0, dot(N, V));
	//float3 R = 2.0 * NdotV * N - V;

	//float4 envMapColor = texCUBElod(CubeMapSampler, float4(R, testValue1));
	//return float4(envMapColor.rgb, 1.0f);

	float3 N = normalize(input.Normal.xyz);
	float4 envMapColor = texCUBElod(CubeMapSampler, float4(N, 0));
	//clip(envMapColor.a - .01f); // just straight clip super low alpha.
	return float4(envMapColor.rgb, 1.0f);
}

//++++++++++++++++++++++++++++++++++++++++
// T E C H N I Q U E S.
//++++++++++++++++++++++++++++++++++++++++

technique Lighting_Phong
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS_Phong();
	}
};

technique Lighting_Blinn
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS_BlinnPhong();
	}
};

technique PhongCubeMap
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL
			RenderCubeMapVS();
		PixelShader = compile PS_SHADERMODEL
			RenderCubeMapPS();
	}
};


