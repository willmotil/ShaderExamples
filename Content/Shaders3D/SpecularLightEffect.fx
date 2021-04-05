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
float AmbientStrength;

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

float SpecularBlinnPhong(float3 pos, float3 lightDir, float shininess, float3 normal)
{
	float3 viewDir = normalize(-pos);
	float3 halfDir = normalize(lightDir + viewDir);
	float specAngle = max(dot(halfDir, normal), 0.0f);
	return pow(specAngle, shininess);
}

float SpecularPhong(float3 pos, float3 lightDir, float shininess, float3 normal)
{
	float3 viewDir = normalize(-pos);
	float3 reflectDir = reflect(-lightDir, normal);
	float specAngle = max(dot(reflectDir, viewDir), 0.0f);
	return pow(specAngle, shininess / 4.0f); // note that the exponent is different here
}

// My own old lighting geometry function. Slide values between 0 and 1 up or down depending on sharpness in a curved rate.
// for specular you put it low you get a small spot, high you get a big spot, there can also be falloff if the value for sharpness is not 0 or 1.
float SpecularCurveFit(float NdotH, float sharpness)
{
	float n = NdotH;
	float t = sharpness;
	float i = 1.0f - t;
	float b = ( (n - 1.0f) + n * 3.0f) * .5f;
	return (i * i) + b * 2.0f * (i * t) + (t * t);
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

float4 PS(VertexShaderOutput input) : COLOR
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

	// simple diffuse.

	// specular.
	float sbp = SpecularBlinnPhong( P, L, 50.0f , N);
	float sp= SpecularPhong(P, L, 50.0f, N);
	float sm = SpecularCurveFit(NdotH, 0.5f);

	// combine.
	//col.rgb = (col.rgb * AmbientStrength) +(col.rgb * NdotL * (1.0f - AmbientStrength));
	//col.rgb = (col.rgb * AmbientStrength) + (col.rgb * specular * 0.90f) +(col.rgb * NdotL * 0.10f);
	col.rgb = (col.rgb * 0.1f);
	col.b += NdotL * NdotL * 0.5f;
	col.r += sbp * 0.5f;
	//col.r += sp * 0.5f;
	//col.r += sm * 0.5f;
	return col;
}


//++++++++++++++++++++++++++++++++++++++++
// T E C H N I Q U E S.
//++++++++++++++++++++++++++++++++++++++++

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


