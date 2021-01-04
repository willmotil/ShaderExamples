// TestingEffect.fx
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0     //_level_9_1
#define PS_SHADERMODEL ps_4_0     //_level_9_1
#endif

matrix World;
matrix View;
matrix Projection;

Texture2D SpriteTexture;

SamplerState SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
    //AddressU = Wrap; AddressV = Wrap; //magfilter = linear;//minfilter = linear;
};

//_______________________________________________________________
// techniques 
// Quad Draw  Position Normal Texture
//_______________________________________________________________
struct VsInputQuad
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};
struct VsOutputQuad
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
    float3 Position3D : TEXCOORD1;
    float3 Normal3D : TEXCOORD2;
};
struct PsOutputQuad
{
    float4 Color : COLOR0;
};


// ____________________________
VsOutputQuad VertexShaderQuadDraw(VsInputQuad input)
{
    VsOutputQuad output;
    output.Position3D = mul(input.Position, World);
    output.Normal3D = mul(input.Normal, World);
    float4x4 wvp = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, wvp);
    output.Normal = mul(input.Normal, wvp);
    output.TextureCoordinate = input.TextureCoordinate;
    return output;
}
PsOutputQuad PixelShaderQuadDraw(VsOutputQuad input)
{
    PsOutputQuad output;
    output.Color = tex2D(SpriteTextureSampler, input.TextureCoordinate);
    output.Color.rg = 1.0f;
    output.Color.a = 1.0f;
    return output;
}

technique TriangleDraw
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL 
            VertexShaderQuadDraw();
        PixelShader = compile PS_SHADERMODEL 
            PixelShaderQuadDraw();
    }
}


