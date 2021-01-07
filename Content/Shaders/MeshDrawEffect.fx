// TestingEffect.fx
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0     //_level_9_1
#define PS_SHADERMODEL ps_4_0     //_level_9_1
#endif


matrix Projection;
matrix View;
matrix World;

Texture2D SpriteTexture;
SamplerState SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>; //AddressU = Wrap; AddressV = Wrap; //magfilter = linear;//minfilter = linear;
};

struct VsInputPTNT
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
    float3 Tangent : NORMAL1;
};
struct VsOutputPTNT
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
    float3 Position3D : TEXCOORD1;
    float3 Normal3D : TEXCOORD2;
    float3 Tangent3D : TEXCOORD3;
};

//___________________________________________

VsOutputPTNT VertexShaderPTNT(VsInputPTNT input)
{
    VsOutputPTNT output;
    float4x4 wvp = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, wvp);
    output.Normal = mul(input.Normal, wvp);
    output.Position3D = mul(input.Position, World);
    output.Normal3D = mul(input.Normal, World);
    output.Tangent3D = mul(input.Tangent, World);
    output.TextureCoordinate = input.TextureCoordinate;
    return output;
}
float4 PixelShaderPTNT(VsOutputPTNT input) : COLOR0
{
    //float4 col = SpriteTexture.Sample(SpriteTextureSampler, input.TextureCoordinate);
    float4 col = tex2D(SpriteTextureSampler, input.TextureCoordinate);
    return col;
}
technique NonIndexedMeshDraw
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL
            VertexShaderPTNT();
        PixelShader = compile PS_SHADERMODEL
            PixelShaderPTNT();
    }
}

// Position texture normal

struct VsInputPNT
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
    float3 Normal : NORMAL0;
};
struct VsOutputPNT
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;
    float3 Normal : NORMAL0;
    float3 Normal3D : TEXCOORD1;
};

VsOutputPNT VertexShaderPNT(VsInputPNT input)
{
    VsOutputPNT output;
    float4x4 wvp = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, wvp);
    output.Normal = mul(input.Normal, wvp);
    output.Normal3D = mul(input.Normal, World);
    output.TextureCoordinate = input.TextureCoordinate;
    return output;
}
float4 PixelShaderPNT(VsOutputPNT input) : COLOR0
{
    float4 col = SpriteTexture.Sample(SpriteTextureSampler, input.TextureCoordinate);
    //float4 col = tex2D(SpriteTextureSampler, input.TextureCoordinate) * input.Color;
    float result = dot(input.Normal3D, float3(0, 0.707f, 0.707f));
    col = col * 0.3f + col * result * 0.7f; // quick cheasy lighting.
    return col;
}
technique TriangleDrawPNT
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL
            VertexShaderPNT();
        PixelShader = compile PS_SHADERMODEL
            PixelShaderPNT();
    }
}


// Position texture color .

struct VsInputPCT
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};
struct VsOutputPCT
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};

VsOutputPCT VertexShaderPCT(VsInputPCT input)
{
    VsOutputPCT output;
    float4x4 wvp = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, wvp);
    output.Color = input.Color;
    output.TextureCoordinate = input.TextureCoordinate;
    return output;
}
float4 PixelShaderPCT(VsOutputPCT input) : COLOR0
{
    float4 col = SpriteTexture.Sample(SpriteTextureSampler, input.TextureCoordinate) * input.Color;
    //float4 col = tex2D(SpriteTextureSampler, input.TextureCoordinate) * input.Color;
    return col;
}
technique TriangleDrawPCT
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL
            VertexShaderPCT();
        PixelShader = compile PS_SHADERMODEL
            PixelShaderPCT();
    }
}


// Position and Texture.

struct VsInputPT
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};
struct VsOutputPT
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;
};

VsOutputPT VertexShaderPT(VsInputPT input)
{
    VsOutputPT output;
    float4x4 wvp = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, wvp);
    output.TextureCoordinate = input.TextureCoordinate;
    return output;
}
float4 PixelShaderPT(VsOutputPT input) : COLOR0
{
    float4 col = SpriteTexture.Sample(SpriteTextureSampler, input.TextureCoordinate);
    //float4 col = tex2D(SpriteTextureSampler, input.TextureCoordinate);
    col.a = 1.0f;
    return col;
}
technique TriangleDrawPT
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL 
            VertexShaderPT();
        PixelShader = compile PS_SHADERMODEL 
            PixelShaderPT();
    }
}

//_______________________________________________________________
// techniques 
// Quad Draw
//_______________________________________________________________

//struct VsInputPTNT
//{
//    float4 Position : POSITION0;
//    float3 Normal : NORMAL0;
//    float2 TextureCoordinate : TEXCOORD0;
//    float3 Tangent : NORMAL1;
//};
//
//struct VsInputPCNT
//{
//    float4 Position : POSITION0;
//    float4 Color : COLOR0;
//    float3 Normal : NORMAL0;
//    float2 TextureCoordinate : TEXCOORD0;
//};
//struct VsInputPCT
//{
//    float4 Position : POSITION0;
//    float3 Color : COLOR0;
//    float2 TextureCoordinate : TEXCOORD0;
//};
//struct VsInputPNT
//{
//    float4 Position : POSITION0;
//    float3 Normal : NORMAL0;
//    float2 TextureCoordinate : TEXCOORD0;
//};
//struct VsInputPT
//{
//    float4 Position : POSITION0;
//    float2 TextureCoordinate : TEXCOORD0;
//};
//struct VsOutputQuad
//{
//    float4 Position : SV_POSITION;
//    float3 Normal : NORMAL0;
//    float4 Color : COLOR0;
//    float2 TextureCoordinate : TEXCOORD0;
//    float3 Position3D : TEXCOORD1;
//    float3 Normal3D : TEXCOORD2;
//    float3 Tangent3D : TEXCOORD3;
//};

//struct PsOutputQuad
//{
//    float4 Color : COLOR0;
//};

//
////// ____________________________
////VsOutputQuad VertexShaderPCNTT(VsInputPCNT input)
////{
////    VsOutputQuad output;
////    output.Position3D = mul(input.Position, World);
////    output.Normal3D = mul(input.Normal, World);
////    float4x4 wvp = mul(World, mul(View, Projection));
////    output.Position = mul(input.Position, wvp);
////    output.Normal = mul(input.Normal, wvp);
////    output.Color = input.Color;
////    output.TextureCoordinate = input.TextureCoordinate;
////    return output;
////}
//
////VsOutputQuad VertexShaderPCT(VsInputPCT input)
////{
////    VsOutputQuad output;
////    output.Position3D = mul(input.Position, World);
////    output.Normal3D = mul(input.Normal, World);
////    float4x4 wvp = mul(World, mul(View, Projection));
////    output.Position = mul(input.Position, wvp);
////    output.Normal = mul(input.Normal, wvp);
////    output.Color = input.Color;
////    output.TextureCoordinate = input.TextureCoordinate;
////    return output;
////}
////VsOutputQuad VertexShaderPNT(VsInputPNT input)
////{
////    VsOutputQuad output;
////    output.Position3D = mul(input.Position, World);
////    output.Normal3D = mul(input.Normal, World);
////    float4x4 wvp = mul(World, mul(View, Projection));
////    output.Position = mul(input.Position, wvp);
////    output.Normal = mul(input.Normal, wvp);
////    output.Color = float4(1,1,1, 1);
////    output.TextureCoordinate = input.TextureCoordinate;
////    return output;
////}
//VsOutputQuad VertexShaderPT(VsInputPT input)
//{
//    VsOutputQuad output;
//    output.Position3D = mul(input.Position, World);
//    float4x4 wvp = mul(World, mul(View, Projection));
//    output.Color = float4(1,1,1, 1);
//    output.Position = mul(input.Position, wvp);
//    output.TextureCoordinate = input.TextureCoordinate;
//    return output;
//}
//PsOutputQuad PixelShaderQuadDraw(VsOutputQuad input)
//{
//    PsOutputQuad output;
//    output.Color = tex2D(SpriteTextureSampler, input.TextureCoordinate) * input.Color;
//    output.Color.a = 1.0f;
//    return output;
//}
//
//technique TriangleDraw
//{
//    pass
//    {
//        VertexShader = compile VS_SHADERMODEL 
//            VertexShaderPT();
//        PixelShader = compile PS_SHADERMODEL 
//            PixelShaderQuadDraw();
//    }
//}
