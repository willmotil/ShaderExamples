#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float percent;
float strength;
float2 scrolldir;

sampler2D TextureSampler : register(s0)
{
    Texture = (Texture);
    magfilter = POINT; 
    minfilter = POINT; 
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};

float4 MainPS(float4 position : SV_Position, float4 color : COLOR0, float2 TextureCoordinates : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(TextureSampler, TextureCoordinates + scrolldir);

    float alpha = col.a;

    col *= color;

    float2 dif = abs(TextureCoordinates - float2(0.5f, 0.5f)) * 2.0f;

    float dis = ((percent)-length(dif) / 1.579f) * strength;

    col.a = saturate(dis) * alpha;

    return col;
}

technique ScrollFadingEdges
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};