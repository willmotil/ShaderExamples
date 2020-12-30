#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

//
float percent;
float2 TextureSize;


Texture2D SpriteTexture;
Texture2D DisplacementTexture;
SamplerState Sampler = sampler_state
{
    Texture = <SpriteTexture>;
    AddressU = Clamp;
    AddressV = Clamp;
    magfilter = Point;
    minfilter = Point;
};


//// function highlight golden ratio steped.
//float4 FuncHighlight(float4 col, float2 texCoord)
//{
//    // try to brighten on blue.
//    float2 temp = texCoord;
//    float dist = 0.003f;
//    float gldr = 1.6f;
//    temp += float2(dist * gldr, 0.00f);
//    float4 col01 = tex2D(Sampler, temp);
//    temp = texCoord;
//    temp += float2(0.00f, dist * gldr * 2.0f);
//    float4 col03 = tex2D(Sampler, temp);
//    temp = texCoord;
//    temp += float2(-dist * gldr * 3.0f, 0.00f);
//    float4 col02 = tex2D(Sampler, temp);
//    temp = texCoord;
//    temp += float2(0.00f, -dist * gldr * 4.0f);
//    float4 col04 = tex2D(Sampler, temp);
//
//    float4 tempCol = ((col01 + col02 + col03 + col04) - col) / 2.0f;
//    //tempCol.b = col.b;
//
//    //col.rgb = saturate(col.rgb * tempCol.rgb);
//    col.rgb = saturate( tempCol.rgb);
//
//    return col;
//}


float4 FuncBoxBlur(float2 texCoord, float halfRange)
{
    int range = halfRange;
    float2 inc = float2(1.0f / TextureSize.x, 1.0f / TextureSize.y);
    float2 texelRange = float2(range * inc.x, range * inc.y);
    float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
    float total = 0.0f;
    for (int y = -range; y < range; y++)
    {
        for (int x = -range; x < range; x++)
        {
            col += tex2D(Sampler, (texCoord.xy +  float2( x * inc.x, y * inc.y ) ));
            total += 1.0f;
        }
    }
    col = col / total;
    //col = col / (range * 4);      //  //(range *4); // total;
    return col;
}

float4 FuncBoxGlow(float2 texCoord, float halfRange)
{
    int range = halfRange;
    float2 inc = float2(1.0f / TextureSize.x, 1.0f / TextureSize.y);
    float2 texelRange = float2(range * inc.x, range * inc.y);
    float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
    float total = 0.0f;
    for (int y = -range; y < range; y++)
    {
        for (int x = -range; x < range; x++)
        {
            col += tex2D(Sampler, (texCoord.xy + float2(x * inc.x, y * inc.y)));
            total += 1.0f;
        }
    }
    //col = col / total;
    col = col / (range * 4); 
    return col;
}


float4 FuncBoxGlowV2(float2 texCoord, float halfRange,float percent, float4 colorTest, float4 glowColor)
{
    //float4(0.0f, 0.0f, 0.0f, 0.0f);
    int range = halfRange;
    float2 inc = float2(1.0f / TextureSize.x, 1.0f / TextureSize.y);
    float2 texelRange = float2(range * inc.x, range * inc.y);
    float4 col = tex2D(Sampler, (texCoord.xy)) *2.0f;
    float total = 1.0f * 2.0f;
    for (int y = -range; y < range; y++)
    {
        for (int x = -range; x < range; x++)
        {
            float4 t = tex2D(Sampler, (texCoord.xy + float2(x * inc.x, y * inc.y)));
            float dis = length(t.rgb - colorTest.rgb) / 1.73f; // square root of 3.0 is 1.73
            float inv = 1.0f - dis;
            inv = pow(inv , 10.0f) * percent;
            col += ( inv * glowColor * (float4(1.0f, 1.0f, 1.0f, 1.0f)));
            total += 1.0f  * inv;
        }
    }
    col = col / total;
    return col;
}


//
//
float4 GlowingPS(float4 position : SV_Position, float4 color : COLOR0, float2 TextureCoordinates : TEXCOORD0) : COLOR0
{
    //float4 blueish = float4(0.5f, 0.5f, 0.99f, 1.0f);
    float4 green = float4(0.0f, 1.0f, 0.0f, 1.0f);
    float4 white = float4(1.0f, 1.0f, 1.0f, 1.0f);
    float4 red = float4(1.0f, 0.0f, 0.0f, 1.0f);

    float4 col = SpriteTexture.Sample(Sampler, TextureCoordinates);
    col = FuncBoxGlowV2(TextureCoordinates, percent *40.0f + 1.0f, percent, color, white); // +1.0f

    return col;
}

technique Glowing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL GlowingPS();
    }
};


