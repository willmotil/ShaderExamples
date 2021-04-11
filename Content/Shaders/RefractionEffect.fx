#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float refractionRange;
float percent;
float strength;
float2 scrolldir;

Texture2D SpriteTexture;
Texture2D DisplacementTexture;
SamplerState Sampler = sampler_state
{
    Texture = <DisplacementTexture>;
    AddressU = Wrap;
    AddressV = Wrap;
    //magfilter = linear;
    //minfilter = linear;
};


float4 RefractPS(float4 position : SV_Position, float4 color : COLOR0, float2 TextureCoordinates : TEXCOORD0) : COLOR0
{
    // sample the displacement maps color
    float4 offset = DisplacementTexture.Sample(Sampler, TextureCoordinates + scrolldir);
    float4 col = SpriteTexture.Sample(Sampler, TextureCoordinates + float2(offset.x - 0.5f, offset.y - 0.5f) * refractionRange) * color;
    float alpha = col.a;
    // well fade things out as before just for fun.
    float2 dif = abs(TextureCoordinates - float2(0.5f, 0.5f)) * 2.0f;
    float dis = ((percent)-length(dif) / 1.579f) * strength;
    col.a = saturate(dis) * alpha;

    return col;
}

technique RefractionSimple
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL RefractPS();
    }
};



//__________________________
//
//__________________________


float4 MultiBlendPS(float4 position : SV_Position, float4 color : COLOR0, float2 TextureCoordinates : TEXCOORD0) : COLOR0
{
    float4 colDis = DisplacementTexture.Sample(Sampler, TextureCoordinates + scrolldir);
    float4 col = SpriteTexture.Sample(Sampler, TextureCoordinates);
    col = col * color;
    float alpha = col.a;
    col = saturate(col * .6f + colDis * 0.4f);
    col.a = alpha;
    return col;
}

technique MultiBlendTexture
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MultiBlendPS();
    }
};


