
// ToDo ...


using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_TriangleToQuadsVertexStructures : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        SpriteFont font3;
        Texture2D texture;
        Effect effect;
        RenderTarget2D rtScene;

        //const int MAXSAMPLES = 10;
        //int numberOfSamples = 5;
        //float concentration = 1.0f;

        public Game1_TriangleToQuadsVertexStructures()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Triangle Quad and Vertex structures ";
            IsMouseVisible = true;
            Window.ClientSizeChanged += OnResize;
        }
        protected override void Initialize()
        {
            base.Initialize();
        }

        public void OnResize(object sender, EventArgs e)
        {
            rtScene = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = @"Content/Shaders";
            effect = Content.Load<Effect>("TextShadowEffect");

            // Change Directory.
            Content.RootDirectory = @"Content/Images";

            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");

            // Change Directory.
            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");
            font3 = Content.Load<SpriteFont>("MgFont3");

            // Change Directory back.
            Content.RootDirectory = @"Content";

            rtScene = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);

            effect.CurrentTechnique = effect.Techniques["ShadowText"];
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            //if (IsPressedWithDelay(Keys.Right, gameTime))
            //    numberOfSamples++;
            //if (IsPressedWithDelay(Keys.Left, gameTime))
            //    numberOfSamples--;
            //numberOfSamples = numberOfSamples.Clamp(0, MAXSAMPLES);

            //if (IsPressedWithDelay(Keys.Up, gameTime))
            //    concentration += .1f;
            //if (IsPressedWithDelay(Keys.Down, gameTime))
            //    concentration -= .1f;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Moccasin);

            //// Draw to the rendertarget the things we intend to shadow.

            //GraphicsDevice.SetRenderTarget(rtScene);
            //GraphicsDevice.Clear(Color.Transparent);

            //spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, null);
            //spriteBatch.DrawString(font, $"Controls plus or minus keys and arrow keys  \n _numberOfSamples: {numberOfSamples.ToString("##0.000")} \n concentration {concentration} ", new Vector2(10, 10), Color.Black);
            //spriteBatch.DrawString(font3, $"Hello Monogame   ", new Vector2(100, 100), Color.White);
            //spriteBatch.End();


            //effect.CurrentTechnique = effect.Techniques["ShadowText"];
            //effect.Parameters["textureSize"].SetValue(new Vector2(rtScene.Width, rtScene.Height));
            //effect.Parameters["numberOfSamplesPerDimension"].SetValue(numberOfSamples);
            //effect.Parameters["concentration"].SetValue(concentration);



            //// Set the backbuffer as the target and clear it to the color we want.

            //GraphicsDevice.SetRenderTarget(null);
            //GraphicsDevice.Clear(Color.CornflowerBlue);



            //// Draw all the regular stuff without the shadow

            //spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            //spriteBatch.Draw(texture, new Rectangle(25, 50, 200, 200), Color.White);
            //spriteBatch.End();



            //// Draw the shadow text and sprites.

            //spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.LinearClamp, null, null, effect, null);
            //spriteBatch.Draw(rtScene, GraphicsDevice.Viewport.Bounds, Color.White);
            //spriteBatch.End();


            base.Draw(gameTime);
        }



        #region helper functions

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

        #endregion
    }
}
