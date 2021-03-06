﻿#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0 //_level_9_1
#define PS_SHADERMODEL ps_4_0 //_level_9_1
#endif

float percent;
float strength;
float displacementTime;
float2 displacementDirection;
float distortionRange;
float distortionRadius;
int numberOfSamples;

Texture2D SpriteTexture;
Texture2D DisplacementTexture;

SamplerState Sampler = sampler_state
{
    Texture = <DisplacementTexture>;
    AddressU = Wrap; AddressV = Wrap; //magfilter = linear;//minfilter = linear;
};


float4 RefractPS(float4 position : SV_Position, float4 color : COLOR0, float2 TextureCoordinates : TEXCOORD0) : COLOR0
{
    float2 sampleDirPullOffset = normalize(displacementDirection).xy;
    float2 mapUvTimeOffset = sampleDirPullOffset * displacementTime;
    float2 sampleDirOffset = DisplacementTexture.Sample(Sampler, TextureCoordinates + mapUvTimeOffset).xy - float2(.5f,.5f);
   
    float winc = 0.02f;
    float w = 0;
    float wi = 0;
    float wsum = 0;
    float2 sampleOffset = float2(0, 0);
    float4 col = float4(0,0,0,0);
    uint numSamples = abs(numberOfSamples);
    for (uint i = 1; i < numSamples; i += 1) 
    {
        w = winc * i;
        wi = winc * (numSamples - i);
        sampleOffset = TextureCoordinates + ((sampleDirOffset * distortionRadius + sampleDirPullOffset * distortionRange) * w);
        col += SpriteTexture.Sample(Sampler, sampleOffset) * color  * wi;
        wsum += w;
    }
    col = col / wsum;

    // well fade things out as before as a seperate step.
    float2 dif = abs(TextureCoordinates - float2(0.5f, 0.5f)) * 2.0f;
    float dis = saturate( ( percent - length(dif) / 1.579f) * strength ); // 1.579f
    col.a = col.a * dis;
    return col;
}


technique RefractionDirectional
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL 
            RefractPS();
    }
};


/*

    //float w1 = 0.100;
    //float w2 = 0.080;
    //float w3 = 0.060;
    //float w4 = 0.040;
    //float w5 = 0.020;
    //float wsum = w1 + w2 + w3 + w4;

    //float2 sampleOffset1 = coords + ((sampleDirOffset * dsr + sampleDirPullOffset * distortionRange) * w1 );
    //float2 sampleOffset2 = coords + ((sampleDirOffset * dsr + sampleDirPullOffset * distortionRange) * w2 );
    //float2 sampleOffset3 = coords + ((sampleDirOffset * dsr + sampleDirPullOffset * distortionRange) * w3 );
    //float2 sampleOffset4 = coords + ((sampleDirOffset * dsr + sampleDirPullOffset * distortionRange) * w4 );
    //float2 sampleOffset5 = coords + ((sampleDirOffset * dsr + sampleDirPullOffset * distortionRange) * w5 );

    //float4 col1 = SpriteTexture.Sample(Sampler, sampleOffset1) * color;
    //float4 col2 = SpriteTexture.Sample(Sampler, sampleOffset2) * color;
    //float4 col3 = SpriteTexture.Sample(Sampler, sampleOffset3) * color;
    //float4 col4 = SpriteTexture.Sample(Sampler, sampleOffset4) * color;
    //float4 col5 = SpriteTexture.Sample(Sampler, sampleOffset5) * color;
    //float4 col = (col1 * w5 + col2 * w4 + col3 * w3 + col4 * w2 + col5 * w1) / wsum;


float4 RefractPS(float4 position : SV_Position, float4 color : COLOR0, float2 TextureCoordinates : TEXCOORD0) : COLOR0
{
    float2 coords = TextureCoordinates;
    float2 sampleDirPullOffset = normalize(displacementDirection).xy;
    float2 mapUvTimeOffset = sampleDirPullOffset * displacementTime;
    float2 sampleDirOffset = DisplacementTexture.Sample(Sampler, coords + mapUvTimeOffset).xy - float2(.5,.5f);

    float dsr = distortionRadius;
    float isr = 1.0f - dsr;

    float w1 = 0.100;
    float w2 = 0.080;
    float w3 = 0.060;
    float w4 = 0.040;
    float w5 = 0.020;
    float wsum = w1 + w2 + w3 + w4;

    float2 sampleOffset1 = coords + ((sampleDirOffset * dsr + sampleDirPullOffset * distortionRange) * w1 );
    float2 sampleOffset2 = coords + ((sampleDirOffset * dsr + sampleDirPullOffset * distortionRange) * w2 );
    float2 sampleOffset3 = coords + ((sampleDirOffset * dsr + sampleDirPullOffset * distortionRange) * w3 );
    float2 sampleOffset4 = coords + ((sampleDirOffset * dsr + sampleDirPullOffset * distortionRange) * w4 );
    float2 sampleOffset5 = coords + ((sampleDirOffset * dsr + sampleDirPullOffset * distortionRange) * w5 );

    float4 col1 = SpriteTexture.Sample(Sampler, sampleOffset1) * color;
    float4 col2 = SpriteTexture.Sample(Sampler, sampleOffset2) * color;
    float4 col3 = SpriteTexture.Sample(Sampler, sampleOffset3) * color;
    float4 col4 = SpriteTexture.Sample(Sampler, sampleOffset4) * color;
    float4 col5 = SpriteTexture.Sample(Sampler, sampleOffset5) * color;
    float4 col = (col1 * w5 + col2 * w4 + col3 * w3 + col4 * w2 + col5 * w1) / wsum;

    // well fade things out as before just for fun.
    float2 dif = abs(TextureCoordinates - float2(0.5f, 0.5f)) * 2.0f;
    float dis = saturate( ( percent - length(dif) / 1.579f) * strength ); // 1.579f
    col.a = col.a * dis;
    return col;
}

float4 RefractPS(float4 position : SV_Position, float4 color : COLOR0, float2 TextureCoordinates : TEXCOORD0) : COLOR0
{

    float2 coords = TextureCoordinates;
    float2 sampleDirPullOffset = normalize(displacementDirection).xy;// mapNormDir;
    float2 mapUvTimeOffset = sampleDirPullOffset * displacementTime;
    float2 sampleDirOffset = DisplacementTexture.Sample(Sampler, coords + mapUvTimeOffset).xy - float2(.5,.5f);


    float dsr = distortionRadius;
    float isr = 1.0f - dsr;

    float w1 = .5;
    float w2 = .3125;
    float w3 = .125;
    float w4 = 0.0625;
    float wsum = w1 + w2 + w3 + w4;

    float2 sampleOffset1 = coords + ((sampleDirOffset * dsr + sampleDirPullOffset * distortionRange) * w1 );
    float2 sampleOffset2 = coords + ((sampleDirOffset * dsr + sampleDirPullOffset * distortionRange) * w2 );
    float2 sampleOffset3 = coords + ((sampleDirOffset * dsr + sampleDirPullOffset * distortionRange) * w3 );
    float2 sampleOffset4 = coords + ((sampleDirOffset * dsr + sampleDirPullOffset * distortionRange) * w4 );

    float4 col1 = SpriteTexture.Sample(Sampler, sampleOffset1) * color;
    float4 col2 = SpriteTexture.Sample(Sampler, sampleOffset2) * color;
    float4 col3 = SpriteTexture.Sample(Sampler, sampleOffset3) * color;
    float4 col4 = SpriteTexture.Sample(Sampler, sampleOffset4) * color;
    float4 col = (col1 * w4 + col2 * w3 + col3 * w2 + col4 * w1 ) / wsum;

    // well fade things out as before just for fun.
    float2 dif = abs(TextureCoordinates - float2(0.5f, 0.5f)) * 2.0f;
    float dis = ((percent)-length(dif) / 1.579f) * strength;
    float da = saturate(dis);
    col.a = col.a * da + da * 0.001f;

    return col;
}

*/