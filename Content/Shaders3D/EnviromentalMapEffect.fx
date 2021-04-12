﻿// Reference
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

float SpecularSharpener(float specular, float scalar)
{
	return saturate(specular - scalar) * (1.0f / (1.0f - scalar));
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

//++++++++++++++++++++++++++++++++++++++++
// P I X E L  S H A D E R S
//++++++++++++++++++++++++++++++++++++++++

// float3 N2 = float3(N.x, -N.y, N.z);
float4 PS_PhongWithEnviromentalLight(VertexShaderOutput input) : COLOR
{
	float3 N = FunctionNormalMapGeneratedBiTangent(input.Normal, input.Tangent, input.TextureCoordinates);  	
	float4 col = tex2D(TextureSamplerDiffuse, input.TextureCoordinates);
	float3 P = input.Position3D;
	float3 C = CameraPosition;
	float3 V = normalize(C - P);
	float3 L = normalize(LightPosition - P);
	float3 H = HalfNormal(L, V);
	float NdotH = MaxDot(N, H);
	float NdotL = MaxDot(N, L);
	float NdotV = MaxDot(N, V);
	float3 R = 2.0f * NdotV * N - V;
	float Stheta = SpecularPhong(V, L, N, 80.0f); // specularTheta

	//float4 enviromentalTexCubeDifCol = TexEnvCubeLod(CubeMapEnviromentalSampler,N, 0); // diffuse env texel. 
	float4 enviromentalTexCubeSpecCol = TexEnvCubeLod(CubeMapEnviromentalSampler,R, 0); // specular env texel.

	float3 specularColor = col.rgb * LightColor * Stheta * SpecularStrength;
	float3 diffuseColor = col.rgb * NdotL * DiffuseStrength;
	float3 ambientColor = col.rgb * AmbientStrength;

    col.rgb = ambientColor + (diffuseColor + specularColor) * enviromentalTexCubeSpecCol;
	return col;
}


// DX the texture cube stores data inverted  float3(N.x, -N.y, N.z);
float4 PS_RenderSkybox(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	//float3 ntex = float3(N.x, -N.y, N.z);
	float4 col = TexEnvCubeLod(CubeMapEnviromentalSampler, N, 0);//texCUBElod(CubeMapSampler, float4 (ntex, 0));
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


// DX the texture cube stores data inverted  float3(N.x, -N.y, -N.z)
float4 PS_RenderCube(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	//float3 N2 = float3(N.x, -N.y, -N.z);
	float4 col = TexCubeLod(CubeMapSampler, N, 0); // texCUBElod(CubeMapSampler, float4 (N2, 0));
	//clip(col.a - .01f); // straight clip low alpha.

	float3 P = input.Position3D;
	float3 C = CameraPosition;
	float3 V = normalize(C - P);
	float3 L = normalize(LightPosition - P);
	float3 H = HalfNormal(L, V);
	float NdotH = MaxDot(N, H);
	float NdotL = MaxDot(N, L);
	float NdotV = MaxDot(N, V);
	float3 R = 2.0f * NdotV * N - V;

	float spec = SpecularBlinnPhong(V, L, N, 100.0f);
	float3 speccol = col.rgb * LightColor;
	col.rgb = saturate((speccol.rgb * spec * SpecularStrength) + (col.rgb * NdotL * DiffuseStrength) + (col.rgb * AmbientStrength));

	return col;
}

// DX now we are going to render the cube here but with the envirmental light of another cube.
// this is probably something you wouldn't ever need to do but just to show it can be done.
// float3(N.x, -N.y, N.z);  float3(N.x, -N.y, N.z)
float4 PS_RenderCubeWithEnviromentalLight(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	float4 col = TexCubeLod(CubeMapSampler, N, 0);
	//clip(col.a - 0.02f); // straight clip low alpha.

	float3 P = input.Position3D;
	float3 C = CameraPosition;
	float3 V = normalize(C - P);
	float3 L = normalize(LightPosition - P);
	float3 H = HalfNormal(L, V);
	float NdotH = MaxDot(N, H);
	float NdotL = MaxDot(N, L);
	float NdotV = MaxDot(N, V);
	float3 R = 2.0f * NdotV * N - V;
	//R.y = -R.y;
    //float4 enviromentalTexCubeCol = texCUBElod(CubeMapEnviromentalSampler, float4 (R, 0));
 
    //float4 enviromentalTexCubeDifCol = TexEnvCubeLod(CubeMapEnviromentalSampler,R, 0); // diffuse env texel. 
	float4 enviromentalTexCubeSpecCol = TexEnvCubeLod(CubeMapEnviromentalSampler, R, 0); // specular env texel.


	float specularTheta = SpecularPhong(V, L, N, 100.0f);

	float3 specularColor = col.rgb * LightColor * specularTheta * enviromentalTexCubeSpecCol * SpecularStrength;
	float3 diffuseColor = ((col.rgb * (1.0f - NdotL)) + (enviromentalTexCubeSpecCol * NdotL)) * NdotL * DiffuseStrength;
	float3 ambientColor = col.rgb * AmbientStrength;

	col.rgb = diffuseColor + specularColor + ambientColor;
	return col;
}


//++++++++++++++++++++++++++++++++++++++++
// T E C H N I Q U E S.
//++++++++++++++++++++++++++++++++++++++++

technique Render_PhongWithEnviromentalLight
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS_PhongWithEnviromentalLight();
	}
};

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

technique Render_CubeWithEnviromentalLight
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS_RenderCubeWithEnviromentalLight();
	}
};





//
//float4 PS_BlinnPhong(VertexShaderOutput input) : COLOR
//{
//	float4 col = tex2D(TextureSamplerDiffuse, input.TextureCoordinates);
//	float3 N = FunctionNormalMapGeneratedBiTangent(input.Normal, input.Tangent, input.TextureCoordinates);
//	float3 P = input.Position3D;
//	float3 C = CameraPosition;
//	float3 V = normalize(C - P);
//	float NdotV = MaxDot(N, V);
//	float3 R = 2.0f * NdotV * N - V;
//	float3 L = normalize(LightPosition - P);
//	float3 H = HalfNormal(L, V);
//	float NdotH = MaxDot(N, H);
//	float NdotL = MaxDot(N, L);
//
//	float spec = SpecularBlinnPhong(V, L, N, 100.0f);
//	float3 speccol = col.rgb * LightColor;
//	col.rgb = (speccol.rgb * spec * SpecularStrength) + (col.rgb * NdotL * NdotL * DiffuseStrength) + (col.rgb * AmbientStrength);
//	return col;
//}
//
//technique Lighting_Blinn
//{
//	pass P0
//	{
//		VertexShader = compile VS_SHADERMODEL
//			VS();
//		PixelShader = compile PS_SHADERMODEL
//			PS_BlinnPhong();
//	}
//};


// We don't typically normal map a skybox but we might want to for a cube and who knows maybe for a skybox.
// Typically this is simple but with the mg bug ill have to double check and make sure works right first.
//
//// DX the texture cube stores data inverted most of this is handled in the class file so we just get one shader
//float4 PS_CubeSkyboxWithNormalMap(VertexShaderOutput input) : COLOR
//{
//	float3 N = FunctionNormalMapGeneratedBiTangent(input.Normal, input.Tangent, input.TextureCoordinates);
//	float4 col = texCUBElod(CubeMapSampler, float4 (N, 0));
//	clip(col.a - .01f); // straight clip low alpha.
//	float3 P = input.Position3D;
//	float3 C = CameraPosition;
//	float3 V = normalize(C - P);
//	float NdotV = MaxDot(N, V);
//	float3 R = 2.0f * NdotV * N - V;
//	float3 L = normalize(LightPosition - P);
//	float3 H = HalfNormal(L, V);
//	float NdotH = MaxDot(N, H);
//	float NdotL = MaxDot(N, L);
//
//	float spec = SpecularBlinnPhong(V, L, N, 100.0f);
//	float3 speccol = col.rgb * LightColor;
//	col.rgb = (speccol.rgb * spec * SpecularStrength) + (col.rgb * NdotL * NdotL * DiffuseStrength) + (col.rgb * AmbientStrength);
//	return col;
//}

//technique Render_CubeSkyboxWithNormalMap
//{
//	pass P0
//	{
//		VertexShader = compile VS_SHADERMODEL
//			VS();
//		PixelShader = compile PS_SHADERMODEL
//			PS_CubeSkyboxWithNormalMap();
//	}
//};