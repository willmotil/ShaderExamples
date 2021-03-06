﻿
// https://www.geeks3d.com/20141201/how-to-rotate-a-vertex-by-a-quaternion-in-glsl/
// https://code.google.com/archive/p/kri/wikis/Quaternions.wiki
// http://eigen.tuxfamily.org/dox/classEigen_1_1AngleAxis.html
// 
// https://www.fatalerrors.org/a/physical-based-ambient-lighting-diffuse-irradiance.html
// http://www.cse.chalmers.se/edu/course/TDA362/tutorials/lab4.html
// https://github.com/JoeyDeVries/LearnOpenGL/commit/534b6ec8c9851919649801d9c15c656c464ffb60

// real time raytracing with links for explinations ect... https://morgan3d.github.io/articles/2019-04-01-ddgi/ https://morgan3d.github.io/articles/2019-04-01-ddgi/intro-to-gi.html  http://graphicscodex.com/ 14 chapters on pbr


// nameing this version 4 april 12 cause i got to many of this class file copied and modified all over.
// this one doesn't have the other bug with the Y flip back to equarectangular.
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif
#define PI 3.14159265359f
#define ToDegrees 57.295779513f;
#define ToRadians 0.0174532925f;

int FaceToMap;

Texture2D Texture; // primary texture.
sampler2D TextureSamplerDiffuse = sampler_state
{
    texture = <Texture>;
};

TextureCube CubeMap;
samplerCUBE CubeMapSampler = sampler_state
{
    texture = <CubeMap>;
    magfilter = Linear;
    minfilter = Linear;
    mipfilter = Linear;
    AddressU = clamp;
    AddressV = clamp;
};


//____________________________________
// structs
//____________________________________

struct HdrToCubeMapVertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexureCoordinate : TEXCOORD0;
};

struct HdrToCubeMapVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Position3D : TEXCOORD1;
    float2 TexureCoordinate : TEXCOORD0;
};

struct FaceStruct
{
    float3 PositionNormal;
    float3 FaceNormal;
    float3 FaceUp;
};


//____________________________________
// functions
//____________________________________

// I made this up to do this tranform because i couldn't find the code to do it anywere.
// Ok so what people in a couple examples are doing regularly is like a matrix view transform, im not sure that is actually any better 
// since i do have to calculate or extrude the normal from the piexel anyways but i should test it later down the road.

FaceStruct UvFaceToCubeMapVector(float2 pos, int faceIndex)
{
    FaceStruct output = (FaceStruct)0;
    float u = pos.x;
    float v = pos.y;
    switch (abs(faceIndex))
    {
    case 0: //FACE_RIGHT: CubeMapFace.PositiveX
        output.PositionNormal = float3(1.0f, v, -u);
        output.FaceNormal = float3(1.0f, 0, 0);
        output.FaceUp = float3(0, 1, 0);
        break;
    case 1: //FACE_LEFT: CubeMapFace.NegativeX
        output.PositionNormal = float3(-1.0f, v, u);
        output.FaceNormal = float3(-1.0f, 0, 0);
        output.FaceUp = float3(0, 1, 0);
        break;
    case 2: //FACE_TOP: CubeMapFace.PositiveY
        output.PositionNormal = float3(u, 1.0f, -v);
        output.FaceNormal = float3(0, 1.0f, 0);
        output.FaceUp = float3(0, 0, 1);
        break;
    case 3: //FACE_BOTTOM : CubeMapFace.NegativeY
        output.PositionNormal = float3(u, -1.0f, v);   // dir = float3(v, -1.0f, u);
        output.FaceNormal = float3(0, -1.0f, 0);
        output.FaceUp = float3(0, 0, -1);
        break;
    case 4: //FACE_BACK: CubeMapFace.PositiveZ
        output.PositionNormal = float3(u, v, 1.0f);
        output.FaceNormal = float3(0, 0, 1.0f);
        output.FaceUp = float3(0, 1, 0);
        break;
    case 5: // FACE_FORWARD: CubeMapFace.NegativeZ
        output.PositionNormal = float3(-u, v, -1.0f);
        output.FaceNormal = float3(0, 0, -1.0f);
        output.FaceUp = float3(0, 1, 0);
        break;

    default:
        output.PositionNormal = float3(-1.0f, v, u); // na
        output.FaceNormal = float3(-1.0f, 0, 0);
        output.FaceUp = float3(0, 1, 0);
        break;
    }
    //output.PositionNormal = new Vector3(output.PositionNormal.z, -output.PositionNormal.y, output.PositionNormal.x); // invert
    output.PositionNormal = normalize(output.PositionNormal);
    return output;
}

float2 CubeMapVectorToUvFace(float3 v, out int faceIndex)
{
    float3 vAbs = abs(v);
    float ma;
    float2 uv;
    if (vAbs.z >= vAbs.x && vAbs.z >= vAbs.y)
    {
        faceIndex = v.z < 0.0 ? 5 : 4;  //FACE_FRONT : FACE_BACK;   // z major axis.  we designate negative z forward.
        ma = 0.5f / vAbs.z;
        uv = float2(v.z < 0.0f ? -v.x : v.x, -v.y);
    }
    else if (vAbs.y >= vAbs.x)
    {
        faceIndex = v.y < 0.0f ? 3 : 2;  //FACE_BOTTOM : FACE_TOP;  // y major axis.
        ma = 0.5f / vAbs.y;
        uv = float2(v.x, v.y < 0.0 ? -v.z : v.z);
    }
    else
    {
        faceIndex = v.x < 0.0 ? 1 : 0;  //FACE_LEFT : FACE_RIGHT; // x major axis.
        ma = 0.5f / vAbs.x;
        uv = float2(v.x < 0.0 ? v.z : -v.z, -v.y);
    }
    return uv * ma + float2(0.5f, 0.5f);
}

float2 CubeMapNormalTo2dEquaRectangularMapUvCoordinates(float3 normal)
{
    float2 uv = float2((float)atan2(-normal.z, normal.x), (float)asin(normal.y));
    float2 INVERT_ATAN = float2(0.1591f, 0.3183f);
    uv = uv * INVERT_ATAN + float2(0.5f, 0.5f);
    return uv;
}

float3 EquaRectangularMapUvCoordinatesTo3dCubeMapNormal(float2 uvCoords)
{
    float pi = 3.14159265358f;
    float3 v = float3(0.0f, 0.0f, 0.0f);
    float2 uv = uvCoords;
    uv *= float2(2.0f * pi, pi);
    float siny = sin(uv.y);
    v.x = -sin(uv.x) * siny;
    v.y = cos(uv.y);
    v.z = -cos(uv.x) * siny;
    // adjustment rotational.
    float hpi = PI / 2.0f;
    v = float3(cos(hpi) * v.x + sin(hpi) * v.z, -v.y, -sin(hpi) * v.x + cos(hpi) * v.z);
    return v;
}



// http://www.codinglabs.net/article_physically_based_rendering.aspx
// 
// https://en.wikipedia.org/wiki/Rendering_equation
//
// F  needs work.
//
float4 GetIrradiance(float2 pixelpos, int faceToMap)
{
    FaceStruct input = UvFaceToCubeMapVector(pixelpos, faceToMap);

    float3 normal = input.PositionNormal;
    float3 up = input.FaceUp;
    float3 right = normalize(cross(up, input.PositionNormal));
    up = cross(input.PositionNormal, right);

    // the following values are in degrees
    float numberOfSamplesHemisphere = 30.0; // we want the smallest amount with good quality
    float numberOfSamplesAround = 4.0; // same as above
    float hemisphereMaxAngle = 5.0f; // we really want 90

    float minimumAdjustment = 2.1f; // this is to help control the sampling geometry.
    float mipSampleLevel = 0; // this is the sample or mipmap level from the enviromental map we take the current pixel from.

    // calculated from the above for adjusting the loop geometry pattern.
    float hemisphereMaxAngleTheta = hemisphereMaxAngle * ToRadians; // computed
    float stepTheta = (hemisphereMaxAngle / numberOfSamplesHemisphere) * ToRadians - 0.05f;
    float stepPhi = (360.0f / numberOfSamplesAround) * ToRadians - 0.05f;

    float3 accumulatedColor = float3(0, 0, 0);
    float totalWeight = 0;
    float3 averagedColor = float3(0, 0, 0);
    float totalSampleCount = 0;

    // sample enviromental cubemap
    for (float phi = 0.01; phi < 6.283; phi += stepPhi) // z rot.
    {
        for (float theta = 0.01f; theta < hemisphereMaxAngleTheta; theta += stepTheta) // y 
        {
            // calculate the new vector around the normal to sample rotationally.
            float3 temp = cos(phi) * right + sin(phi) * up;
            float4 sampleVector = float4(cos(theta) * normal + sin(theta) * temp, mipSampleLevel);
            sampleVector.rgb = normalize(sampleVector.rgb);
            float3 sampledColor = texCUBElod(CubeMapSampler, sampleVector).rgb;

            // some possible weighting functions.
            float avg = (sampledColor.r + sampledColor.b + sampledColor.g) * 0.33333f;
            float NdotS = saturate(dot(normal, sampleVector.rgb));
            float phiMuliplier = 1.0f - (phi / (5.283f + 1.0f));

            // accumulate and weigh the geometrical sampled pattern ... here is the hard part.
            accumulatedColor += sampledColor;
            totalWeight++;
        }

    }
    //float4 final = float4(PI * accumulatedColor / totalWeight, 1.0f );
    float4 final = float4(accumulatedColor / totalWeight, 1.0f);

    //// overlaid visualization.
    //float3 directColor = texCUBElod(CubeMapSampler, float4(normal, 0)).rgb;
    //directColor.rgb = (directColor.r + directColor.g + directColor.b) / 3;
    //final.rgb = final.rgb * 0.90f + directColor.brg * 0.10f;

    return final;
}




//____________________________________
// shaders and technique SphericalToCubeMap
// Copy 2d spherical hdr to enviromental cubemap
//____________________________________

HdrToCubeMapVertexShaderOutput HdrToEnvCubeMapVS(in HdrToCubeMapVertexShaderInput input)
{
    HdrToCubeMapVertexShaderOutput output = (HdrToCubeMapVertexShaderOutput)0;
    output.Position = input.Position;
    output.Position3D = input.Position;
    return output;
}

float4 SphericalToCubeMapPS(HdrToCubeMapVertexShaderOutput input) : COLOR
{
    
    //FaceStruct face = UvFaceToCubeMapVectorAlt(input.Position3D, FaceToMap);
    FaceStruct face = UvFaceToCubeMapVector(input.Position3D, FaceToMap);
    float3 v = face.PositionNormal;
    float2 uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);
    //uv = float2(uv.x, uv.y);
    uv = float2(uv.x, 1.0f - uv.y);  // raw dx transform ok in hind site this shortcut hack in was a bad idea.
    float4 color = float4(tex2D(TextureSamplerDiffuse, uv).rgb, 1.0f);
    return color;
}

technique SphericalToCubeMap
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL
            HdrToEnvCubeMapVS();
        PixelShader = compile PS_SHADERMODEL
            SphericalToCubeMapPS();
    }
};


//____________________________________
// shaders and technique CubeMapToSpherical
// Copy enviromental cubemap to 2d spherical
//____________________________________

float4 CubeMapToSphericalPS(HdrToCubeMapVertexShaderOutput input) : COLOR
{
    float2 uv = (input.Position3D.xy + 1.0f) / 2.0f;
    float3 n = EquaRectangularMapUvCoordinatesTo3dCubeMapNormal(uv);
    return texCUBElod(CubeMapSampler, float4(n, 0.0f));
}

technique CubeMapToSpherical
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL
            HdrToEnvCubeMapVS();
        PixelShader = compile PS_SHADERMODEL
            CubeMapToSphericalPS();
    }
};

//____________________________________
// shaders and technique CubeMapToTexture
// Copy enviromental cubemap to 2d Texture
//____________________________________

float4 CubeMapToTexturePS(HdrToCubeMapVertexShaderOutput input) : COLOR
{
    FaceStruct face = UvFaceToCubeMapVector(input.Position3D, FaceToMap);
    float3 n = face.PositionNormal;
    return texCUBElod(CubeMapSampler, float4(n, 0.0f));
}

technique CubeMapToTexture
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL
            HdrToEnvCubeMapVS();
        PixelShader = compile PS_SHADERMODEL
            CubeMapToTexturePS();
    }
};

//____________________________________
// shaders and technique TextureFacesToSpherical
// Copy enviromental Faces to 2d spherical
//____________________________________

float4 TextureFacesToSphericalPS(HdrToCubeMapVertexShaderOutput input) : COLOR
{
    // We are looping the spherical texture we will need to do this 6 times and we must know the Face of which the position we are on represents..
    float2 uv = (input.Position3D.xy + 1.0f) / 2.0f;
    float3 n = EquaRectangularMapUvCoordinatesTo3dCubeMapNormal(uv);
    int sphericalFace;
    float2 sample_uv = CubeMapVectorToUvFace(n, sphericalFace);
    // When we call this function we will pass each face texture in sequence and the face it represents, we must compare if the faces match.
    // Each face we send in might not even have any eligible pixels to be sampled and may need to be cliped if its face doesn't match the current render pixels face if it does the derived sample uv is valid other wise we clip.
    float4 color = float4(0, 0, 0, 0);
    if (FaceToMap == sphericalFace)
    {
        color = float4(tex2D(TextureSamplerDiffuse, sample_uv).rgb, 1.0f);
    }
    else
        clip(-1);
    return color;
    //float2 uv = (input.Position3D.xy + 1.0f) / 2.0f;
}

technique TextureFacesToSpherical
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL
            HdrToEnvCubeMapVS();
        PixelShader = compile PS_SHADERMODEL
            TextureFacesToSphericalPS();
    }
};


//____________________________________
// shaders and technique TextureFacesToSpherical
// Copy enviromental Faces to 2d spherical
//____________________________________

// render texture2d regular.
float4 Face2DToFaceCopyPS(HdrToCubeMapVertexShaderOutput input) : COLOR
{
    float2 uv = (input.Position3D.xy + 1.0f) / 2.0f;
    // forced hack due to the line in SphericalToCubeMapPS uv = float2(uv.x, 1.0f - uv.y);
    // this should be removable on the condition i remove that and adjust everything to then be in proper alignment.
    uv = float2(uv.x, 1.0f - uv.y);
    return float4(tex2D(TextureSamplerDiffuse, uv).rgb, 1.0f);
}

technique TextureFacesToCubeFaces
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL
            HdrToEnvCubeMapVS();
        PixelShader = compile PS_SHADERMODEL
            Face2DToFaceCopyPS();
    }
};

//____________________________________
// shaders and technique CubemapToDiffuseIlluminationCubeMap
// Generate diffuse illumination map from enviroment cubemap.
//____________________________________

HdrToCubeMapVertexShaderOutput HdrToDiffuseIlluminationCubeMapVS(in HdrToCubeMapVertexShaderInput input)
{
    HdrToCubeMapVertexShaderOutput output = (HdrToCubeMapVertexShaderOutput)0;
    output.Position = input.Position;
    output.Position3D = input.Position;
    return output;
}

float4 CubemapToDiffuseIlluminationCubeMapPS(HdrToCubeMapVertexShaderOutput input) : COLOR
{
    return GetIrradiance(input.Position3D, FaceToMap);
}

technique CubemapToDiffuseIlluminationCubeMap
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL
            HdrToDiffuseIlluminationCubeMapVS();
        PixelShader = compile PS_SHADERMODEL
            CubemapToDiffuseIlluminationCubeMapPS();
    }
};



// another guys solution to the irradiance map that i keep having problems with.

//float4 ColorPixelShader(PixelInputType input) : SV_TARGET
//{
//    float3 normal = normalize(input.position.xyz);
//
//    float3 irradiance = float3(0.0f, 0.0f, 0.0f);
//
//    float3 up = g_upVectorVal;
//    float3 right = cross(up, normal);
//    up = cross(normal, right);
//
//    float sampleDelta = 0.025f;
//    float nrSamples = 0.0f;
//    for (float phi = 0.0f; phi < 2.0 * PI; phi += sampleDelta)
//    {
//        for (float theta = 0.0f; theta < 0.5 * PI; theta += sampleDelta)
//        {
//            float3 tangentSample = float3(sin(theta) * cos(phi), sin(theta) * sin(phi), cos(theta));
//            float3 sampleVec = (tangentSample.x * right) + (tangentSample.y * up) + (tangentSample.z * normal);
//
//            irradiance += shaderTexture.Sample(SampleType, sampleVec).rgb * cos(theta) * sin(theta);
//            nrSamples++;
//        }
//    }
//    irradiance = PI * irradiance * (1.0f / nrSamples);
//    return float4(irradiance, 1.0f);
//}



//
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//



//  this ones not yet fixed.
//
//// https://www.geeks3d.com/20141201/how-to-rotate-a-vertex-by-a-quaternion-in-glsl/
//// https://code.google.com/archive/p/kri/wikis/Quaternions.wiki
//// http://eigen.tuxfamily.org/dox/classEigen_1_1AngleAxis.html
//
//
//#if OPENGL
//#define SV_POSITION POSITION
//#define VS_SHADERMODEL vs_3_0
//#define PS_SHADERMODEL ps_3_0
//#else
//#define VS_SHADERMODEL vs_4_0
//#define PS_SHADERMODEL ps_4_0
//#endif
//#define PI 3.14159265359f
//#define ToDegrees 57.295779513f;
//#define ToRadians 0.0174532925f;
//
//int FaceToMap;
//
//Texture2D Texture; // primary texture.
//sampler2D TextureSamplerDiffuse = sampler_state
//{
//    texture = <Texture>;
//};
//
//TextureCube CubeMap;
//samplerCUBE CubeMapSampler = sampler_state
//{
//    texture = <CubeMap>;
//    magfilter = Linear;
//    minfilter = Linear;
//    mipfilter = Linear;
//    AddressU = clamp;
//    AddressV = clamp;
//};
//
//
////____________________________________
//// structs
////____________________________________
//
//struct HdrToCubeMapVertexShaderInput
//{
//    float4 Position : POSITION0;
//    float2 TexureCoordinate : TEXCOORD0;
//};
//
//struct HdrToCubeMapVertexShaderOutput
//{
//    float4 Position : SV_POSITION;
//    float3 Position3D : TEXCOORD1;
//    float2 TexureCoordinate : TEXCOORD0;
//};
//
//struct FaceStruct
//{
//    float3 PositionNormal;
//    float3 FaceNormal;
//    float3 FaceUp;
//    float2 SphericalUv;
//    float2 Uv;
//};
//
//
////____________________________________
//// functions
////____________________________________
//
//float3   rotatePointAboutYaxis(float3 p, float q)
//{
//    //z' = z*cos s - x*sin s
//    //x' = z*sin s + x*cos s
//    //y' = y
//    return float3((p.z * cos(q) - p.x * sin(q)), p.y, (p.z * sin(q) + p.x * cos(q)));
//}
//float3   rotatePointAboutZaxis(float3 p, float q)
//{
//    //x' = x*cos s - y*sin s
//    //y' = x*sin s + y*cos s 
//    //z' = z
//    return float3((p.x * cos(q) - p.y * sin(q)),(p.x * sin(q) + p.y * cos(q)), p.z);
//}
//
//float4x4 CreateFromAxisAngle(float3 axis, float angle)
//{
//    float x = axis.x;
//    float y = axis.y;
//    float z = axis.z;
//    float sinTheta = sin(angle);
//    float cosTheta = cos(angle);
//    float numXX = x * x;
//    float numYY = y * y;
//    float numZZ = z * z;
//    float numXY = x * y;
//    float numXZ = x * z;
//    float numYZ = y * z;
//    float4x4 result = float4x4
//    (
//        numXX + (cosTheta * (1.0f - numXX)),
//        (numXY - (cosTheta * numXY)) + (sinTheta * z),
//        (numXZ - (cosTheta * numXZ)) - (sinTheta * y),
//        0.0f,
//        (numXY - (cosTheta * numXY)) - (sinTheta * z),
//        numYY + (cosTheta * (1.0f - numYY)),
//        (numYZ - (cosTheta * numYZ)) + (sinTheta * x),
//        0.0f,
//        (numXZ - (cosTheta * numXZ)) + (sinTheta * y),
//        (numYZ - (cosTheta * numYZ)) - (sinTheta * x),
//        numZZ + (cosTheta * (1.0f - numZZ)),
//        0.0f,
//        0.0f,
//        0.0f,
//        0.0f,
//        1.0f
//    );
//    return result;
//}
//
//// I made this up to do this tranform because i couldn't find the code to do it anywere.
//FaceStruct PosUvFaceToNormal(float2 pos, int faceIndex)
//{
//    FaceStruct output = (FaceStruct)0;
//    float u = pos.x;
//    float v = pos.y; // -pos.y changes top and bottom via negation and flips y this will also affect uv for top and bottom
//    switch (abs(faceIndex))
//    {
//    case 1: //FACE_LEFT: CubeMapFace.NegativeX
//        output.PositionNormal = float3(-1.0f, v, u);
//        output.FaceNormal = float3(-1.0f, 0, 0);
//        output.FaceUp = float3(0, 1, 0);
//        break;
//    case 5: // FACE_FORWARD: CubeMapFace.NegativeZ
//        output.PositionNormal = float3(-u, v, -1.0f);
//        output.FaceNormal = float3(0, 0, -1.0f);
//        output.FaceUp = float3(0, 1, 0);
//        break;
//    case 0: //FACE_RIGHT: CubeMapFace.PositiveX
//        output.PositionNormal = float3(1.0f, v, -u);
//        output.FaceNormal = float3(1.0f, 0, 0);
//        output.FaceUp = float3(0, 1, 0);
//        break;
//    case 4: //FACE_BACK: CubeMapFace.PositiveZ
//        output.PositionNormal = float3(u, v, 1.0f);
//        output.FaceNormal = float3(0, 0, 1.0f);
//        output.FaceUp = float3(0, 1, 0);
//        break;
//
//    case 2: //FACE_TOP: CubeMapFace.PositiveY  2 
//        output.PositionNormal = float3(u, 1.0f, -v);
//        output.FaceNormal = float3(0, 1.0f, 0);
//        output.FaceUp = float3(0, 0, 1);
//        break;
//    case 3: //FACE_BOTTOM : CubeMapFace.NegativeY  3
//        output.PositionNormal = float3(u, -1.0f, v);   // dir = float3(v, -1.0f, u);
//        output.FaceNormal = float3(0, -1.0f, 0);
//        output.FaceUp = float3(0, 0, -1);
//        break;
//
//    default:
//        output.PositionNormal = float3(-1.0f, v, u); // na
//        output.FaceNormal = float3(-1.0f, 0, 0);
//        output.FaceUp = float3(0, 1, 0);
//        break;
//    }
//    output.Uv = (pos.xy + 1.0f) / 2.0f;
//    //output.PositionNormal = new Vector3(output.PositionNormal.z, -output.PositionNormal.y, output.PositionNormal.x); // invert
//    output.PositionNormal = normalize(output.PositionNormal);
//    return output;
//}
//
//// used by TextureFacesToSpherical
////
//float2 NormalToUvFace(float3 v, out int faceIndex)
//{
//    float3 vAbs = abs(v);
//    float ma;
//    float2 uv;
//    if (vAbs.z >= vAbs.x && vAbs.z >= vAbs.y)
//    {
//        faceIndex = v.z < 0.0 ? 5 : 4; // Right , left _  //FACE_FRONT : FACE_BACK;   // z major axis.  we designate negative z forward.
//        ma = 0.5f / vAbs.z;
//        uv = float2(v.z < 0.0f ? -v.x : v.x, -v.y);
//    }
//    else if (vAbs.y >= vAbs.x)
//    {
//        faceIndex = v.y < 0.0f ? 3 : 2; // bot , top  //FACE_BOTTOM : FACE_TOP;  // y major axis.
//        ma = 0.5f / vAbs.y;
//        uv = float2(v.x, v.y < 0.0 ? -v.z : v.z);
//    }
//    else
//    {
//        faceIndex = v.x < 0.0 ? 1 : 0; // back , front  //FACE_LEFT : FACE_RIGHT; // x major axis.
//        ma = 0.5f / vAbs.x;
//        uv = float2(v.x < 0.0 ? v.z : -v.z, -v.y);
//    }
//    return uv * ma + float2(0.5f, 0.5f);
//}
//
//// used by SphericalToCubeMapPS
////
//float2 NormalTo2dSphericalUvCoordinates(float3 normal)
//{
//    float2 uv = float2((float)atan2(-normal.z, normal.x), (float)asin(normal.y));
//    float2 INVERT_ATAN = float2(0.1591f, 0.3183f);
//    uv = uv * INVERT_ATAN + float2(0.5f, 0.5f);
//    return uv;
//}
//
//// used by CubeMapToSpherical  TextureFacesToSpherical
//float3 SphericalUvCoordinatesToNormal(float2 uvCoords)
//{
//    float pi = 3.14159265358f;
//    float3 v = float3(0.0f, 0.0f, 0.0f);
//    float2 uv = uvCoords;
//    uv *= float2(2.0f * pi, pi);
//    float siny = sin(uv.y);
//    v.x = -sin(uv.x) * siny;
//    v.y = cos(uv.y);
//    v.z = -cos(uv.x) * siny;
//    // adjustment rotational.
//    float hpi = PI / 2.0f;
//    v = float3(cos(hpi) * v.x + sin(hpi) * v.z, v.y, -sin(hpi) * v.x + cos(hpi) * v.z);
//    return v;
//}
//
//float4 GetSphericalIrradiance(float2 pixelpos)
//{
//    float3 irradiance = float3(0.0f, 0.0f, 0.0f);
//    float sampleDelta = 0.025f;
//    float nrSamples = 0.0f;
//
//    float2 uv = (pixelpos.xy + 1.0f) / 2.0f;
//    float3 normal = SphericalUvCoordinatesToNormal(uv);
//    float2 nuv = NormalTo2dSphericalUvCoordinates(normal);
//    irradiance += tex2D(TextureSamplerDiffuse, nuv).rgb;
//
//    return float4(irradiance, 1.0f);
//}
//
//
//// http://www.codinglabs.net/article_physically_based_rendering.aspx
////
//// F  needs work.  this is really hard to get right.  The more i fuxs with this the more i think just use mip maps but..... texcubelod mip call is busted on gl or was it dx arrg.
////
//float4 GetCubeIrradiance(float2 pixelpos, int faceToMap)
//{
//    FaceStruct input = PosUvFaceToNormal(pixelpos, faceToMap);
//
//    float3 normal = normalize(input.PositionNormal);
//    float3 up = float3(0, -1, 0);
//    float3 right = normalize(cross(up, normal));
//    up = cross(normal, right);
//
//    // the following values are in degrees.
//    float numberOfSamplesHemisphere = 10.0f; // we want the smallest amount with good quality
//    float numberOfSamplesAround = 4.0f; // same as above
//    float hemisphereMaxAngle = 45.0f; // we really want at least 90 but yikes.  , 10 for point testing.
//
//    float minimumAdjustment = 2.1f; // this is to help control the sampling geometry.
//    float mipSampleLevel = 0; // this is the sample or mipmap level from the enviromental map we take the current pixel from.
//
//    // calculated from the above for adjusting the loop geometry pattern.
//    float hemisphereMaxAngleTheta = hemisphereMaxAngle * ToRadians; // computed
//    float stepTheta = (hemisphereMaxAngleTheta / numberOfSamplesHemisphere);// *ToRadians; // -0.05f;
//    float stepPhi = (360.0f / numberOfSamplesAround) * ToRadians; // -0.05f;
//
//    float3 accumulatedColor = float3(0, 0, 0);
//    float totalWeight = 0;
//    float3 averagedColor = float3(0, 0, 0);
//    float totalSampleCount = 0;
//    
//    // sample enviromental cubemap
//        for (float theta = 0.01f; theta < hemisphereMaxAngleTheta; theta += stepTheta) // y rot
//        {
//            stepPhi = 7.0f;
//            //float3 temp = normalize(rotatePointAboutYaxis(normal, theta));
//            for (float phi = 0.01f; phi < 6.283f; phi += stepPhi) // z rot.
//            {
//                // OK To do i believe that this is actually a Y axis rotation rotating to the left and right which duhhhh is why im actually having all these problems its actually side to side rotation not a up down one  wtf.
//                // i don't even know.
//
//                 // calculate the new vector around the normal to sample rotationally.
//                 float3 temp = cos(phi) * right + sin(phi) * up;
//                 float4 sampleVector = float4(cos(theta) * normal + sin(theta) * temp, mipSampleLevel);
//                 sampleVector.rgb = normalize(sampleVector.rgb);
//                 float3 sampledColor = texCUBElod(CubeMapSampler, sampleVector).rgb;
//
//                 float NdotS = saturate(dot(normal, sampleVector.rgb));
//                 float phiMuliplier = 1.0f - (phi / (5.283f + 1.0f));
//
//                accumulatedColor += sampledColor * NdotS;
//                totalWeight += NdotS;
//        }
//    }
//    float4 final = float4(accumulatedColor / totalWeight, 1.0f);
//    return final;
//}
//
////____________________________________
//// shaders and technique SphericalToCubeMap
//// Copy 2d spherical hdr to enviromental cubemap
////____________________________________
//
//HdrToCubeMapVertexShaderOutput HdrToSphericalVS(in HdrToCubeMapVertexShaderInput input)
//{
//    HdrToCubeMapVertexShaderOutput output = (HdrToCubeMapVertexShaderOutput)0;
//    output.Position = input.Position;
//    output.Position3D = input.Position;
//    return output;
//}
//float4 SphericalToSphericalPS(HdrToCubeMapVertexShaderOutput input) : COLOR
//{
//    float4 color = GetSphericalIrradiance(input.Position3D);
//    return color;
//}
//technique SphericalToIlluminationSpherical
//{
//    pass P0
//    {
//        VertexShader = compile VS_SHADERMODEL
//            HdrToSphericalVS();
//        PixelShader = compile PS_SHADERMODEL
//            SphericalToSphericalPS();
//    }
//};
//
////____________________________________
//// shaders and technique SphericalToCubeMap
//// Copy 2d spherical hdr to enviromental cubemap
////____________________________________
//
//HdrToCubeMapVertexShaderOutput HdrToEnvCubeMapVS(in HdrToCubeMapVertexShaderInput input)
//{
//    HdrToCubeMapVertexShaderOutput output = (HdrToCubeMapVertexShaderOutput)0;
//    output.Position = input.Position;
//    output.Position3D = input.Position;
//    return output;
//}
//float4 SphericalToCubeMapPS(HdrToCubeMapVertexShaderOutput input) : COLOR
//{
//    FaceStruct face = PosUvFaceToNormal(input.Position3D, FaceToMap);
//    float3 v = face.PositionNormal;
//    float2 uv = NormalTo2dSphericalUvCoordinates(v);
//    uv = float2(uv.x, 1.0f - uv.y);  // raw dx transform ok in hind site this shortcut hack in was a bad idea.
//    //float2 texcoords = float2(uv.x, uv.y);  // i will have to perform this fix later on.
//    float4 color = float4(tex2D(TextureSamplerDiffuse, uv).rgb, 1.0f);
//    return color;
//}
//technique SphericalToCubeMap
//{
//    pass P0
//    {
//        VertexShader = compile VS_SHADERMODEL
//            HdrToEnvCubeMapVS();
//        PixelShader = compile PS_SHADERMODEL
//            SphericalToCubeMapPS();
//    }
//};
//
//
////____________________________________
//// shaders and technique CubemapToDiffuseIlluminationCubeMap
//// Generate diffuse illumination map from enviroment cubemap.
////____________________________________
//
//HdrToCubeMapVertexShaderOutput HdrToDiffuseIlluminationCubeMapVS(in HdrToCubeMapVertexShaderInput input)
//{
//    HdrToCubeMapVertexShaderOutput output = (HdrToCubeMapVertexShaderOutput)0;
//    output.Position = input.Position;
//    output.Position3D = input.Position;
//    return output;
//}
//float4 CubemapToDiffuseIlluminationCubeMapPS(HdrToCubeMapVertexShaderOutput input) : COLOR
//{
//    return GetCubeIrradiance(input.Position3D, FaceToMap);
//}
//technique CubemapToDiffuseIlluminationCubeMap
//{
//    pass P0
//    {
//        VertexShader = compile VS_SHADERMODEL
//            HdrToDiffuseIlluminationCubeMapVS();
//        PixelShader = compile PS_SHADERMODEL
//            CubemapToDiffuseIlluminationCubeMapPS();
//    }
//};
//
//
////____________________________________
//// shaders and technique CubeMapToTexture
//// Copy enviromental cubemap to 2d Texture face array
////____________________________________
//
//float4 CubeMapToTexturePS(HdrToCubeMapVertexShaderOutput input) : COLOR
//{
//    //float2 pos = float2(input.Position3D.x, - input.Position3D.y);
//    FaceStruct face = PosUvFaceToNormal(input.Position3D, FaceToMap);
//    float3 n = face.PositionNormal;
//    return texCUBElod(CubeMapSampler, float4(n, 0.0f));
//}
//technique CubeMapToTexture
//{
//    pass P0
//    {
//        VertexShader = compile VS_SHADERMODEL
//            HdrToEnvCubeMapVS();
//        PixelShader = compile PS_SHADERMODEL
//            CubeMapToTexturePS();
//    }
//};
//
//
////____________________________________
//// shaders and technique TextureFacesToCubeFaces
//// Copy enviromental individural Faces to cube map
////____________________________________
//
//// render texture2d regular.
//float4 Faces2DToCubeFacesPS(HdrToCubeMapVertexShaderOutput input) : COLOR
//{
//    float2 uv = (input.Position3D.xy + 1.0f) / 2.0f;
//    // forced hack due to the line in SphericalToCubeMapPS uv = float2(uv.x, 1.0f - uv.y);
//    // this should be removable on the condition i remove that and adjust everything to then be in proper alignment.
//    uv = float2(uv.x, 1.0f - uv.y); 
//    return float4(tex2D(TextureSamplerDiffuse, uv).rgb, 1.0f);
//}
//technique TextureFacesToCubeFaces
//{
//    pass P0
//    {
//        VertexShader = compile VS_SHADERMODEL
//            HdrToEnvCubeMapVS();
//        PixelShader = compile PS_SHADERMODEL
//            Faces2DToCubeFacesPS();
//    }
//};
//
//
////____________________________________
//// shaders and technique CubeMapToSpherical
//// Copy enviromental cubemap to 2d spherical
////____________________________________
//
//float4 CubeMapToSphericalPS(HdrToCubeMapVertexShaderOutput input) : COLOR
//{
//    float2 uv = (input.Position3D.xy + 1.0f) / 2.0f;
//    //uv = float2(uv.x, 1.0f - uv.y);
//    float3 n = SphericalUvCoordinatesToNormal(uv);
//    return texCUBElod(CubeMapSampler, float4(n, 0.0f) );
//}
//technique CubeMapToSpherical
//{
//    pass P0
//    {
//        VertexShader = compile VS_SHADERMODEL
//            HdrToEnvCubeMapVS();
//        PixelShader = compile PS_SHADERMODEL
//            CubeMapToSphericalPS();
//    }
//};
//
////____________________________________             Needs fixed i think
//// shaders and technique TextureFacesToSpherical
//// Copy enviromental Faces to 2d spherical
////
////____________________________________
//
//float4 TextureFacesToSphericalPS(HdrToCubeMapVertexShaderOutput input) : COLOR
//{
//    // We are looping the spherical texture we will need to do this 6 times and we must know the Face of which the position we are on represents..
//    float2 uv = (input.Position3D.xy + 1.0f) / 2.0f;
//    //uv = float2(uv.x, 1.0f - uv.y);
//    float3 n = SphericalUvCoordinatesToNormal(uv);
//    int sphericalFace;
//    float2 sample_uv = NormalToUvFace(n, sphericalFace);
//    // When we call this function we will pass each face texture in sequence and the face it represents, we must compare if the faces match.
//    // Each face we send in might not even have any eligible pixels to be sampled and may need to be cliped if its face doesn't match the current render pixels face if it does the derived sample uv is valid other wise we clip.
//    float4 color = float4(0, 0, 0, 0);
//    if (FaceToMap == sphericalFace)
//    {
//        color = float4(tex2D(TextureSamplerDiffuse, sample_uv).rgb, 1.0f);
//    }
//    else
//        clip(-1);
//    return color;
//}
//technique TextureFacesToSpherical
//{
//    pass P0
//    {
//        VertexShader = compile VS_SHADERMODEL
//            HdrToEnvCubeMapVS();
//        PixelShader = compile PS_SHADERMODEL
//            TextureFacesToSphericalPS();
//    }
//};
//
//// http://www.codinglabs.net/article_physically_based_rendering.aspx