
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderExamples
{
    public class Game1_GammaCorrection : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        Texture2D texture;
        Texture2D texture2;
        Texture2D texture3;
        Effect effect;

        List<string> timedMsg = new List<string>();
        float elapsedTotal = 0;
        float msgDuration = 200;
        int index = 0;
        bool respectMsgTimer = true;

        float percent = 1.0f;

        public Game1_GammaCorrection()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 800;
        }
        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = @"Content/Shaders";
            effect = Content.Load<Effect>("GammaTestEffect");
            effect.CurrentTechnique = effect.Techniques["Gamma"];

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");
            texture2 = Content.Load<Texture2D>("Tanzania");
            texture3 = Content.Load<Texture2D>("gamma_correction_brightness");

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont3");

            CreateMsgs();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            elapsedTotal += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (elapsedTotal > msgDuration)
                elapsedTotal = 0;

            if (IsPressedWithDelay(Keys.Space, gameTime) || Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                percent -= .005f;
                if (percent <= 0)
                    percent = 1.0f;
            }
            //effect.Parameters["percent"].SetValue(percent);

            if(Keys.Space.IsKeyPressedWithDelay(gameTime))
            {
                if (index > timedMsg.Count - 2)
                    index = 0;
                index ++;
                respectMsgTimer = false;
            }
            if (Keys.Back.IsKeyPressedWithDelay(gameTime))
            {
                if (index < 1)
                    index = timedMsg.Count - 1;
                index--;
                respectMsgTimer = false;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Moccasin);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, effect, null);
            spriteBatch.Draw(texture2, new Rectangle(300, 0, 290, 290), Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.DrawString(font, $" press space or back   \n Percent: {percent.ToString("##0.000")}", new Vector2(10, 10), Color.Black);
            spriteBatch.Draw(texture2, new Rectangle(0, 0, 290, 290), Color.White);
            spriteBatch.Draw(texture3, new Rectangle(0, 300, 590, 70), Color.White);
            if (respectMsgTimer)
                spriteBatch.DrawString(font2, timedMsg[(int)((timedMsg.Count - 1) * (elapsedTotal / msgDuration))], new Vector2(10, 400), Color.Black, 0, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 0);
            else
                spriteBatch.DrawString(font2, timedMsg[index], new Vector2(10, 400), Color.Black, 0, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 0);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public bool IsPressedWithDelay(Keys key, GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(key) && IsUnDelayed(gameTime))
                return true;
            else
                return false;
        }

        float delay = 0f;
        bool IsUnDelayed(GameTime gametime)
        {
            if (delay < 0)
            {
                delay = .25f;
                return true;
            }
            else
            {
                delay -= (float)gametime.ElapsedGameTime.TotalSeconds;
                return false;
            }
        }

        public void CreateMsgs()
        {
            timedMsg.Add(" Gamma Correction");
            timedMsg.Add(" Press space or back to page thru msgs.");
            timedMsg.Add(" Consider our following shader as seen above. From half way down the image this shader begins to dim as is goes downwards.");
            timedMsg.Add(" The color space that it is in matters to how the results are interpolated and returned..");
            timedMsg.Add(" When we generate images or colors on the shader they are in linear space.");
            timedMsg.Add(
            @"
float4 GammaPS(float4 position : SV_Position, float4 color : COLOR0, float2 TextureCoordinates : TEXCOORD0) : COLOR0
{
float4 col = tex2D(TextureSampler, TextureCoordinates) * color;
float alpha = col.a;
float gamma = 2.2f;
float falloff = 1.0f - saturate(TextureCoordinates.y - 0.5f) * 2.0f;

            ");
            timedMsg.Add(
            @"// falloff as is;  seen in.                                                               // segment 1 / of 5;
     float4 col = originalcol * falloff;

if (TextureCoordinates.x > 0.20f && TextureCoordinates.x < 0.40f) // segment 2 of 5;
     col =  ((originalcol * (1.0f / gamma)) * falloff) ;

if (TextureCoordinates.x > 0.40f && TextureCoordinates.x < 0.60f) // segment 3 of 5;
     col = pow( ((originalcol * gamma) * falloff) , gamma);

if (TextureCoordinates.x > 0.60f && TextureCoordinates.x < 0.80f) // segment 4 of 5;
     col = (originalcol * (1.0f / gamma)) * falloff * gamma;

if (TextureCoordinates.x > 0.80f)                                                // segment 5 of 5;
     col = (originalcol * (1.0f / gamma)) * falloff; //* gamma;
            ");
            timedMsg.Add(
@"// white created strips.
if (TextureCoordinates.y > 0.85f)                                                
     col = float4(1.0f, 1.0f, 1.0f, 1.0f) * (1.0f / gamma) * TextureCoordinates.x;
if (TextureCoordinates.y > 0.90f)                                                
     col = float4(1.0f, 1.0f, 1.0f, 1.0f) * TextureCoordinates.x;      
if (TextureCoordinates.y > 0.95f)                                                
     col = float4(1.0f, 1.0f, 1.0f, 1.0f) * gamma * TextureCoordinates.x ;

     col.a = alpha;
     return col;
}
            ");

            timedMsg.Add("Quoted From https://learnopengl.com/Advanced-Lighting/Gamma-Correction ");
            timedMsg.Add(" In the old days of digital imaging most monitors were cathode-ray tube (CRT) monitors. " );
            timedMsg.Add("These monitors had the physical property that twice the input voltage did not result in twice the amount of brightness." );
            timedMsg.Add(" \n Doubling the input voltage resulted in a brightness equal to an exponential relationship of roughly \n " +
                "2.2\n known as the gamma of a monitor. " );
            timedMsg.Add("This happens to (coincidently) also closely match how human beings measure brightness" +
                "\n  as brightness is also displayed with a similar (inverse) power relationship. " +
                "\n");
            timedMsg.Add("To better understand what this all means take a look at the scaled image ");
            timedMsg.Add(" ");
            timedMsg.Add(" one has the gamma applied the other doe not image:");
            timedMsg.Add(" ");
            timedMsg.Add("The top line looks like the correct brightness scale to the human eye, " );
            timedMsg.Add(" doubling the brightness(from 0.1 to 0.2 for example) does indeed look like it's twice as bright with nice consistent differences.  ");
            timedMsg.Add(" However "); 
            timedMsg.Add(" when we're talking about the physical brightness of light e.g. amount of photons leaving a light source. " +
                "\n the bottom scale actually displays the correct brightness. ");
            timedMsg.Add(" Because the human eyes prefer to see brightness colors according to the top scale, " );
            timedMsg.Add(" monitors (still today) use a power relationship for displaying output colors. ");
            timedMsg.Add(" so that the original physical brightness colors are mapped to the non-linear brightness colors in the top scale.");
            timedMsg.Add(" we have assumed we were working in linear space, " +
                "\n but we've actually been working in the monitor's output space.");
            timedMsg.Add(" So all colors and lighting variables we configured weren't physically correct, but merely looked (sort of) right on our monitor");
            timedMsg.Add(" ");
            timedMsg.Add(" Because colors are configured based on the display's output ");
            timedMsg.Add(" All intermediate (lighting) calculations in linear-space are ... physically incorrect. ");
            timedMsg.Add(" ");
            timedMsg.Add(" The idea of gamma correction is to apply the inverse of the monitor's gamma." +
                " \n to the final output color  ... before displaying to the monitor.");
            timedMsg.Add("  We multiply each of the linear output colors by this inverse gamma curve (making them brighter) " +
                "\n and as soon as the colors are displayed on the monitor,  " );
            timedMsg.Add("the monitor's gamma curve is applied and the resulting colors become linear. ");
            timedMsg.Add("We effectively brighten the intermediate colors so that as soon as the monitor darkens them, ");
            timedMsg.Add(" it balances all out.");
            timedMsg.Add(" A gamma value of 2.2 is a default gamma value that roughly estimates the average gamma of most displays. " +
                "\n The color space as a result of this gamma of 2.2 is called the sRGB color space");
            timedMsg.Add("Because monitors display colors with gamma applied, whenever you draw, edit, or paint a picture on your computer " +
                "\n you are picking colors based on what you see on the monitor. ");
            timedMsg.Add(" This effectively means all the pictures you create or edit are not in linear space, but in sRGB space " +
                "\n e.g. doubling a dark-red color on your screen based on perceived brightness, does not equal double the red component. ");
            timedMsg.Add(" ");

            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");


        }
    }
}
