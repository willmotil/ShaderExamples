
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ShaderExamples
{
    public class Game1_TestingCanidateEffects : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        MouseState ms;

        Texture2D texture;
        Effect effect;

        //const int MAXSAMPLES = 60;
        //int numberOfSamples = 8;

        float time = 0.0f;
        Vector2 center = new Vector2(.5f, .5f);
        Vector3 shockParams = new Vector3(10.0f, 0.8f, 0.1f);

        bool shockwaveClicks = false;

        //float2 center; // Mouse position
        //float time; // effect elapsed time
        //float3 shockParams; // 10.0, 0.8, 0.1

        public Game1_TestingCanidateEffects()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Testing Effect.";
            IsMouseVisible = true;

            Test();
        }

        string msgMisc = "";
        public void Test()
        {
            int a = 12;
            int b = 4;
            int tOR = a | b;
            int tAnd = a & b;
            int tXor = a ^ b;
            int tOnesCompliment = ~a;
            int tleft = a << b;
            int tright = a >> b;

            msgMisc =
                     $" a = \n  {ToBinaryString(a)}  \n  b = \n  {ToBinaryString(b)}    \n  : " +
                    $"\n  a {a} |  b {b} = OR \n  { ToBinaryString(tOR) }    if either bit is on then we get a 1" +
                    $"\n  a {a} &  b {b} = And \n  {ToBinaryString(tAnd)}    if both bits are on we get a 1" +
                    $"\n  a {a} ^  b {b} = Xor \n  {ToBinaryString(tXor)}    if either bit is on we get a 1" +
                    $"\n  ~ a {a} = OnesCompliment \n  {ToBinaryString(tOnesCompliment)}    if a bit is on we get a 0 if off we get a 1  ~ reverses the bits" +
                    $"\n  a {a} <<  b {b} = tleft \n  {ToBinaryString(tleft)}    Increasing values" +
                    $"\n  a {a} >>  b {b} = tright \n  {ToBinaryString(tright)}    Decreasing values" +
                    $"\n  "
                    ;

            msgMisc += $"\n"+ tOR.ToSpacedBinaryString();

            msgMisc += $"\n" + tOR.ToWellFormatedBinaryString();

            Console.WriteLine(msgMisc);
        }

        public static string ToBinaryString(int value)
        {
            string result = "";
            int j = 0;
            for(int bitIndexToTest = 0; bitIndexToTest < 32; bitIndexToTest ++)
            {
                if (IsIntBitOn(value, bitIndexToTest))
                    result += "1";
                else
                    result += "0";
                j++;
                if (j > 7)
                {
                    result += " ";
                    j = 0;
                }
            }
            return result;
        }

        public static bool IsIntBitOn(int inValue, int bitIndexToTest)
        {
            if ( (inValue & (1 << (bitIndexToTest))) > 0)
                return true;
            else
                return false;
        }



        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            effect = Content.Load<Effect>("TestingCanidateEffects");

            // Change Directory.
            Content.RootDirectory = @"Content/Images";

            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");  // cutePuppy  ,  MG_Logo_Med_exCanvs.png

            // Change Directory.
            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");

            // Change Directory back.
            Content.RootDirectory = @"Content";

            MgExtensions.DelayTime = 0.09f;
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float secondsScalar = 1.5f;
            time += (float)gameTime.ElapsedGameTime.TotalSeconds * secondsScalar;
            float maxTime = 10.0f;

            ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed && gameTime.IsUnDelayed())
            {
                shockwaveClicks = true;
                time = .0f;
            }
            if (shockwaveClicks)
            {
                time = time.EnsureClampInRange( 0, maxTime);
                if (time >= maxTime)
                    shockwaveClicks = false;
            }
            else
                time = 100.0f;

            center = (ms.Position.ToVector2() / GraphicsDevice.Viewport.Bounds.Size.ToVector2()); // - new Vector2(.5f,.5f) ;

            base.Update(gameTime);
        }

        //if (Keyboard.GetState().IsKeyDown(Keys.Up))
        //    time += .01f;
        //if (Keyboard.GetState().IsKeyDown(Keys.Down))
        //    time -= .01f;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            effect.CurrentTechnique = effect.Techniques["TestTechnique"];
            effect.Parameters["center"].SetValue(center);
            effect.Parameters["time"].SetValue(time);
            effect.Parameters["shockParams"].SetValue(shockParams);
            //effect.Parameters["textureSize"].SetValue(new Vector2(texture.Width, texture.Height));


            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, effect, null);
            spriteBatch.Draw(texture, GraphicsDevice.Viewport.Bounds, Color.Blue);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.DrawString(font, $"Controls: left click, arrow keys \n radialScalar: {time.ToString("##0.000")} \n numberOfSamples: {shockParams} \n textureBlurUvOrigin: {center.ToString()} ", new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(font, $"\n " + msgMisc, new Vector2(210, 110), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region helper functions


        #endregion
    }
}


