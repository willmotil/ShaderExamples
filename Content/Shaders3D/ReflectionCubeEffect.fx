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

// (L , V)
float3 HalfNormal(float3 L_PixelToLight, float3 V_PixelToCamera)
{
	return normalize( L_PixelToLight + V_PixelToCamera );
}

float3 Average(float3 col)
{
	return (col.x + col.y + col.z) / 3.0f;
}

float ReflectionTheta(float3 L, float3 N, float3 V)
{
	return dot(V, reflect(-L, N));
}

// N, -L
float3 Reflection(float3 n, float3 i)
{
	return ((n.x * i.x + n.y * i.y + n.z * i.z) * 2.0f) * n - i;
}

float IsFrontFaceToLight(float3 L, float3 N)
{
	return sign(saturate(dot(L, N)));
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

// to viewer can be   pos   or  v   aka  cam - pixel
float SpecularPhong(float3 toViewer, float3 toLight, float3 normal, float shininess)
{
	float3 viewDir = normalize(-toViewer);
	float3 reflectDir = reflect(toLight, normal);
	float b = max(dot(reflectDir, viewDir), 0.0f);
	float backfaceRemoval = saturate(sign(dot(toLight, normal))); // should ndot v the dot result too to ensure its both light to surface backface and normal to camera color removed.
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

float3 FunctionNormalMapGeneratedBiTangent(float3 normal, float3 tangent, float2 texCoords)
{
	// Normal Map
	float3 NormalMap = tex2D(TextureSamplerNormalMap, texCoords).rgb;
	NormalMap.g = NormalMap.g;
	NormalMap = NormalMap * 2.0f - 1.0f;
	float3 bitangent = normalize(cross(normal, tangent));

	float3x3 mat;
	mat[0] = bitangent; // set right
	mat[1] = tangent; // set up
	mat[2] = normal; // set forward

	return normalize(mul(NormalMap, mat)); // norm to ensure later scaling wont break it.
}

float3 FunctionBumpMapGeneratedBiTangent(float3 normal, float3 tangent, float2 texCoords)
{
	// Bump Map
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



//struct CommonOutput
//{
//	float3 N;
//	float3 P;
//	float3 C;
//	float3 L;
//	float3 V;
//	float3 H;
//	float NdotL;
//	float NdotH;
//	float NdotV;
//	float3 R;
//	float3 R2;
//};
//
//CommonOutput CalculateVariables(float3 Pos, float3 Norm)
//{
//	CommonOutput output = (CommonOutput)0;
//	output.N = Norm;
//	output.P = Pos;
//	output.C = CameraPosition;
//	output.V = normalize(CameraPosition - Pos);
//	output.L = normalize(LightPosition - Pos);
//	output.H = normalize(LightPosition + output.V);
//	output.NdotL = MaxDot(Norm, LightPosition);
//	output.NdotH = MaxDot(Norm, output.H);
//	output.NdotV = MaxDot(Norm, output.V);
//	output.R = 2.0f * output.NdotV * Norm - output.V;
//	return output;
//}


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
VsOutputCalcSceneDepth CreateDepthMapVertexShader(VsInputCalcSceneDepth input)//(float4 inPos : POSITION)
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

// float3 N2 = float3(N.x, -N.y, N.z);
float4 PS_PhongWithNormalMapEnviromentalMap(VertexShaderOutput input) : COLOR
{
	//float3 N = FunctionNormalMapGeneratedBiTangent(input.Normal, input.Tangent, input.TextureCoordinates);  	
	float3 N = FunctionBumpMapGeneratedBiTangent(input.Normal, input.Tangent, input.TextureCoordinates);
	float4 col = tex2D(TextureSamplerDiffuse, input.TextureCoordinates) * float4(LightColor, 1); // blending light color here isn't really close to correct however this isn't a pbr shader.
	float3 P = input.Position3D;
	float3 C = CameraPosition;
	float3 V = normalize(C - P);
	float3 L = normalize(LightPosition - P);
	float3 H = normalize(L + V);   // H = HalfNormal(L, V);
	float NdotL = MaxDot(N, L);    // this is a check to see if a pixel is facing the light it has no information on shadowed occlusions.
	float NdotH = MaxDot(N, H);   // this is a aproximation to compare the reflection angle from the light on the pixel to the camera or eye vector.
	float NdotV = MaxDot(N, V);   // this essentially is a check to see if the pixel's is facing the camera.
	float Stheta = SpecularPhong(V, L, N, 80.0f); // specularTheta or the amount of specular intensity.
	float3 R = 2.0f * NdotV * N - V;  // this is the reflection vector to the viewer's eye.  
	//float3 R = Reflection(N, V);

	float4 envSpecularReflectiveCol = TexEnvCubeLod(CubeMapEnviromentalSampler,R, 0); 

	float3 specularColor = col.rgb * envSpecularReflectiveCol * Stheta * SpecularStrength;
	float3 diffuseColor = col.rgb * NdotL * DiffuseStrength;
	float3 ambientColor = col.rgb * AmbientStrength;
	col.rgb = (diffuseColor + specularColor + ambientColor) * envSpecularReflectiveCol;  // just hack this in for the moment to see it.

	return col;
}

// float3 N2 = float3(N.x, -N.y, N.z);
float4 PS_PhongWithEnviromentalMap(VertexShaderOutput input) : COLOR
{
	float3 N = input.Normal;
	float4 col = tex2D(TextureSamplerDiffuse, input.TextureCoordinates) * float4(LightColor, 1); // blending light color here isn't really close to correct however this isn't a pbr shader.
	float3 P = input.Position3D;
	float3 C = CameraPosition;
	float3 V = normalize(C - P);
	float3 L = normalize(LightPosition - P);
	float3 H = normalize(L + V);   // H = HalfNormal(L, V);
	float NdotL = MaxDot(N, L);    // this is a check to see if a pixel is facing the light it has no information on shadowed occlusions.
	float NdotH = MaxDot(N, H);   // this is a aproximation to compare the reflection angle from the light on the pixel to the camera or eye vector.
	float NdotV = MaxDot(N, V);   // this essentially is a check to see if the pixel's is facing the camera.
	float3 R = 2.0f * NdotV * N - V;  // this is the reflection vector to the viewer's eye.  
	float Stheta = SpecularPhong(V, L, N, 80.0f); // specularTheta or the amount of specular intensity.

	//float3 InflectionOrReflectionVector = InflectionPositionFromPlane(P, N, C);
	//float3 cubeDir = normalize(input.Position3D - InflectionOrReflectionVector);
	//float4 envSpecularReflectiveCol = texCUBElod(CubeMapEnviromentalSampler, float4 (cubeDir, 0));

	//float4 envSpecularReflectiveCol = TexEnvCubeLod(CubeMapEnviromentalSampler,N, 0); 
	float4 envSpecularReflectiveCol = texCUBElod(CubeMapEnviromentalSampler, float4 (R, 0));

	float3 specularColor = col.rgb * envSpecularReflectiveCol * Stheta * SpecularStrength;
	float3 diffuseColor = col.rgb * NdotL * DiffuseStrength;
	float3 ambientColor = col.rgb * AmbientStrength;
	col.rgb = (diffuseColor + specularColor + ambientColor) * envSpecularReflectiveCol;  // just hack this in for the moment to see it.

	return col;
}

float4 PS_PhongWithNormalMap(VertexShaderOutput input) : COLOR
{
	//float3 N = FunctionNormalMapGeneratedBiTangent(input.Normal, input.Tangent, input.TextureCoordinates);  	
	float3 N = FunctionBumpMapGeneratedBiTangent(input.Normal, input.Tangent, input.TextureCoordinates);
	float4 col = tex2D(TextureSamplerDiffuse, input.TextureCoordinates) * float4(LightColor, 1); // blending light color here isn't really close to correct however this isn't a pbr shader.
	float3 P = input.Position3D;
	float3 C = CameraPosition;
	float3 V = normalize(C - P);
	float3 L = normalize(LightPosition - P);
	float3 H = normalize(L + V);   // H = HalfNormal(L, V);
	float NdotL = MaxDot(N, L);    // this is a check to see if a pixel is facing the light it has no information on shadowed occlusions.
	float NdotH = MaxDot(N, H);   // this is a aproximation to compare the reflection angle from the light on the pixel to the camera or eye vector.
	float NdotV = MaxDot(N, V);   // this essentially is a check to see if the pixel's is facing the camera.
	float3 R = 2.0f * NdotV * N - V;  // this is the reflection vector to the viewer's eye.  
	//float3 R = dot(V, reflect(-L, N));
	float Stheta = SpecularPhong(V, L, N, 80.0f); // specularTheta or the amount of specular intensity.

	float3 specularColor = col.rgb * Stheta * SpecularStrength;
	float3 diffuseColor = col.rgb * NdotL * DiffuseStrength;
	float3 ambientColor = col.rgb * AmbientStrength;
	col.rgb = (diffuseColor + specularColor + ambientColor);
	return col;
}


// DX the texture cube stores data inverted  float3(N.x, -N.y, N.z);
float4 PS_RenderSkybox(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	//float3 ntex = float3(N.x, -N.y, N.z);
	float4 col = TexEnvCubeLod(CubeMapSampler, N, 0);//texCUBElod(CubeMapSampler, float4 (ntex, 0));
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


//
// DX the texture cube stores data inverted  float3(N.x, -N.y, -N.z)
float4 PS_RenderDepthCube(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	float4 col = TexCubeLod(CubeMapSampler, N, 0); // texCUBElod(CubeMapSampler, float4 (N2, 0));
	col.a = 1.0f;
	col.rgb = col.rgb / 10000.0f;

	return col;
}

float4 CreateDepthMapPixelShader(VsOutputCalcSceneDepth input) : COLOR
{
	return length(LightPosition - input.Position3D);
}



//++++++++++++++++++++++++++++++++++++++++
// T E C H N I Q U E S.
//++++++++++++++++++++++++++++++++++++++++

technique Render_PhongWithNormMapEnviromentalLight
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS_PhongWithNormalMapEnviromentalMap();
	}
};

technique Render_PhongWithNormMap
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS_PhongWithNormalMap();
	}
};

technique Render_PhongWithEnviromentalMap
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS_PhongWithEnviromentalMap();
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

technique Render_VisualizationDepthCube
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL
			VS();
		PixelShader = compile PS_SHADERMODEL
			PS_RenderDepthCube();
	}
};


technique Render_LightDepth
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL 
			CreateDepthMapVertexShader();
		PixelShader = compile PS_SHADERMODEL 
			CreateDepthMapPixelShader();
	}
}

//_________________



//

////_______________________________________________________________
//// technique
//// Render shadow depth
////_______________________________________________________________/
//struct VsInputCalcSceneDepth
//{
//	float4 Position : POSITION0;
//};
//struct VsOutputCalcSceneDepth
//{
//	float4 Position     : SV_Position;
//	float4 Position3D    : TEXCOORD0;
//};
//// Shader.
//VsOutputCalcSceneDepth CreateDepthMapVertexShader(VsInputCalcSceneDepth input)//(float4 inPos : POSITION)
//{
//	VsOutputCalcSceneDepth output;
//	output.Position3D = mul(input.Position, World);
//	float4x4 vp = mul(View, Projection);
//	output.Position = mul(output.Position3D, vp);
//
//	return output;
//}
//float4 CreateDepthMapPixelShader(VsOutputCalcSceneDepth input) : COLOR
//{
//	//float4 depth = EncodeFloatRGBA(length(WorldLightPosition - input.Position3D));
//	//return depth; //distance(WorldLightPosition, input.Position3D);
//	//return Square(WorldLightPosition - input.Position3D); // might even be a little worse
//	return length(LightPosition - input.Position3D);
//}
//
//technique RenderShadowDepth
//{
//	pass Pass0
//	{
//		VertexShader = compile VS_SHADERMODEL CreateDepthMapVertexShader();
//		PixelShader = compile PS_SHADERMODEL CreateDepthMapPixelShader();
//	}
//}
//
////_______________________________________________________________
//// technique 
//// ReflectionWaterShader.  
////
//// TextureCubeSampler InflectionOrReflectionVector ReflectiveTextureContribution, TransparencyOverride
////_______________________________________________________________
//// ToDo ....  
////__________________________________________________________
//
//struct VsInReflectionWater
//{
//	float4 Position : POSITION0;
//	float3 Normal : NORMAL0;
//	float2 TexureCoordinateA : TEXCOORD0;
//};
//struct VsOutReflectionWater
//{
//	float4 Position : SV_Position;
//	float2 TexureCoordinateA : TEXCOORD0;
//	float4 Position3D    : TEXCOORD1;
//	float3 Normal : TEXCOORD2;
//	//float3 thisToPixel : TEXCOORD3;
//};
//// shaders
//VsOutReflectionWater VsReflectionWater(VsInReflectionWater input)
//{
//	VsOutReflectionWater output;
//	output.Position3D = mul(input.Position, World);
//	//output.thisToPixel = output.Position3D - World._m30_m31_m32;
//	float4x4 vp = mul(View, Projection);
//	output.Position = mul(output.Position3D, vp);
//	output.TexureCoordinateA = input.TexureCoordinateA;
//	output.Normal = normalize(mul(input.Normal, World)); // Reminder, if we want to rotate a scaled world normal we need to renormalize it.
//	return output;
//}
////// this one is more complicated it has to have regular attributes while drawn for reflection and different ones when drawn.
//float4 PsReflectionWater(VsOutReflectionWater input) : Color
//{
//	float3 cubeDir = normalize(input.Position3D - InflectionOrReflectionVector);
//	float4 reflectionCubeColor = texCUBE(TextureCubeSampler, cubeDir);
//	float4 texelColor = tex2D(TextureSamplerA, input.TexureCoordinateA);
//	//float4 resultColor = (0.9f * reflectionCubeColor) + (texelColor * 0.1f);
//	//return float4(resultColor.xyz, 1.0f);
//	float4 resultColor = ((1.0f - ReflectiveTextureContribution) * reflectionCubeColor) + (texelColor * ReflectiveTextureContribution);
//	return float4(resultColor.xyz, (1.0f - TransparencyOverride));
//}
//technique ReflectionWaterShader
//{
//	pass
//	{
//		VertexShader = compile VS_SHADERMODEL 
//			VsReflectionWater();
//		PixelShader = compile PS_SHADERMODEL 
//			PsReflectionWater();
//	}
//}
//
////_______
//
////_______________________________________________________________
//// technique
//// Tranparency.
////
////
////_______________________________________________________________
//struct VsInTranparency
//{
//	float4 Position : POSITION0;
//	float2 TexureCoordinateA : TEXCOORD0;
//};
//struct VsOutTranparency
//{
//	float4 Position : SV_Position;
//	float2 TexureCoordinateA : TEXCOORD0;
//	//float2 Depth : TEXCOORD1;
//};
//
//// shaders
//VsOutTranparency VsTranparency(VsInTranparency input)
//{
//	VsOutTranparency output;
//	float4 wpos = mul(input.Position, World);
//	float4x4 vp = mul(View, Projection);
//	output.Position = mul(wpos, vp);
//	output.TexureCoordinateA = input.TexureCoordinateA;
//	//output.Depth.x = output.Position.z;
//	//output.Depth.y = output.Position.w;
//	return output;
//}
//float4 PsTranparency(VsOutTranparency input) : COLOR
//{
//	float4 texelcolor = tex2D(TextureSamplerA, input.TexureCoordinateA);
//	// AlphaDiscardThreshold)   // .02f if (output.Color.a < alphaTestValue)
//	if (texelcolor.a < 0.0f)
//	{
//		clip(-1);
//	}
//	return texelcolor;
//}
//technique Transparency
//{
//	pass
//	{
//		VertexShader = compile VS_SHADERMODEL VsTranparency();
//		PixelShader = compile PS_SHADERMODEL PsTranparency();
//	}
//}
//
////_______________________________________________________________
//// technique
//// LightShadowShader 
////
//// This uses a normal from the vertex structure it uses a depth map for shadows and makes light.
////
//// WorldLightPosition
////_______________________________________________________________
//struct VsInLightShadowNormal
//{
//	float4 Position : POSITION0;
//	float3 Normal : NORMAL0;
//	float2 TexureCoordinateA : TEXCOORD0;
//};
//struct VsOutLightShadowNormal
//{
//	float4 Position : SV_Position;
//	float2 TexureCoordinateA : TEXCOORD0;
//	float4 Position3D    : TEXCOORD1;
//	float3 Normal : TEXCOORD2;
//};
//// shaders
//VsOutLightShadowNormal VsLightShadowNormal(VsInLightShadowNormal input)
//{
//	VsOutLightShadowNormal output;
//	output.Position3D = mul(input.Position, World);
//	float4x4 vp = mul(View, Projection);
//	output.Position = mul(output.Position3D, vp);
//	output.TexureCoordinateA = input.TexureCoordinateA;
//	output.Normal = normalize(mul(input.Normal, World)); // Reminder, if we want to rotate a scaled world normal we need to renormalize it.
//	return output;
//}
//
//float4 PsLightShadowNormal(VsOutLightShadowNormal input) : Color
//{
//	float3 temp = WorldLightPosition - input.Position3D;
//	float distancePixelToLight = length(temp);
//	//
//	float3 surfaceToCamera = normalize(CameraPosition - input.Position3D);
//	float3 surfaceToLight = temp / distancePixelToLight; // cheapen the normalize. normalize(pixelToLight);
//	float3 lightToSurface = -surfaceToLight;
//	//
//	float shadowDepth = texCUBE(TextureDepthSampler, float4(lightToSurface, 0)).x;
//	float4 TexelColor = tex2D(TextureSamplerA, input.TexureCoordinateA) * (1.0f - LightVsTexelRatio) + (LightColor * LightVsTexelRatio); // LightVsTexelRatio == .5 is normal
//																																		 // shadow 
//	float lightFalloff = (1.0f - saturate(distancePixelToLight / (IlluminationRange + 0.001f)));
//	float LightDistanceIntensity = saturate(sign((shadowDepth + .2f) - distancePixelToLight)) * lightFalloff; // if else replacement.
//	// lighting
//	float3 normal = input.Normal;
//	float diffuse = saturate((dot(lightToSurface, -normal) + DiffuseCresting) * (1.0f / (1.0f + DiffuseCresting))); // I've added over or underdraw to the diffuse.
//	float3 reflectionTheta = dot(surfaceToCamera,  -reflect(surfaceToLight, normal));
//	float specular = saturate(reflectionTheta - SpecularSharpness) * (1.0f / (1.0f - SpecularSharpness)); // this is for sharpness
//	// finalize it.
//	float3 additiveAmbient = AmbientStrength;
//	float3 additiveDiffuse = diffuse * DiffuseStrength * LightDistanceIntensity;
//	float3 additiveSpecular = specular * SpecularStrength * LightDistanceIntensity;
//	float3 FinalColor = TexelColor * (additiveAmbient + additiveDiffuse + additiveSpecular);  //  *float4(1.0f, 0.0f, 0.0f, 0.0f);
//	return float4(FinalColor, 1.0f);
//}
//
//technique LightShadowShader
//{
//	pass
//	{
//		VertexShader = compile VS_SHADERMODEL VsLightShadowNormal();
//		PixelShader = compile PS_SHADERMODEL PsLightShadowNormal();
//	}
//}
//
//
//
////___
//
//
////_______________________________________________________________
//// technique 
//// DepthVisualization
////_______________________________________________________________
//// shaders
//struct VsInDepthVisualization
//{
//	float4 Position : POSITION0;
//	float3 Normal : NORMAL0;
//	float2 TexureCoordinateA : TEXCOORD0;
//};
//struct VsOutDepthVisualization
//{
//	float4 Position : SV_Position;
//	float3 Dir3D    : TEXCOORD1;
//	float2 TexureCoordinateA : TEXCOORD0;
//};
//
//VsOutDepthVisualization VsDepthVisualization(VsInDepthVisualization input)
//{
//	VsOutDepthVisualization output;
//	float4 worldPos = float4(WorldLightPosition + input.Position, 1);
//	float4x4 vp = mul(View, Projection);
//	output.Position = mul(worldPos, vp);
//	output.Dir3D = input.Position;
//	output.TexureCoordinateA = input.TexureCoordinateA;
//	return output;
//}
//
//// regular
//float4 PsDepthVisualization(VsOutDepthVisualization input) : COLOR
//{
//	float4 texelcolor = tex2D(TextureSamplerA, input.TexureCoordinateA);
//	float shadowDepth = DecodeFloatRGB(texCUBE(TextureDepthSampler, float4(input.Dir3D, 0)).xyz);
//
//	float3 c = EncodeFloatRGB(shadowDepth);
//	float4 shadowVisualColoring = shadowDepth * 0.01 * float4(c.z,c.y,c.x , 1.0f);
//	return  saturate(shadowVisualColoring);
//}
//
//technique DepthVisualization
//{
//	pass
//	{
//		VertexShader = compile VS_SHADERMODEL VsDepthVisualization();
//		PixelShader = compile PS_SHADERMODEL PsDepthVisualization();
//	}
//}









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