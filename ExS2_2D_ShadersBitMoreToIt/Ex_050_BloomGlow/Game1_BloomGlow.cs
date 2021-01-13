
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_BloomGlow : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        Texture2D texture;
        Texture2D texture1;
        Texture2D texture2;
        Texture2D texture3;
        Texture2D textureDisplacementTexture;
        Effect effect;
        MouseState mouseState;
        RenderTarget2D offscreenRenderTarget0;
        RenderTarget2D offscreenRenderTarget1;
        RenderTargetBinding[] renderTargetBinding;

        bool _useBlur = true;
        const int MAXSAMPLES = 100;
        int _numberOfSamples = 10;
        Vector3 threshold = new Vector3(.35f, .50f, .50f);
        

        #region  timing stuff

        float elapsed = 0;
        float elapsedPercentageOfSecondsPerCycle = 0;
        float secondsPerCycle = .1f;
        //ColorCycler colorCycle;

        #endregion

        public Game1_BloomGlow()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Blur Effect.";
            IsMouseVisible = true;
        }
        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = @"Content/Shaders";
            effect = Content.Load<Effect>("BloomGlowEffect");

            // Change Directory.
            Content.RootDirectory = @"Content/Images";

            texture1 = Content.Load<Texture2D>("cutePuppy");
            texture2 = Content.Load<Texture2D>("BloomTestImage");
            texture3 = Content.Load<Texture2D>("MG_Logo_Modifyed");

            texture = texture2;

            // Change Directory.
            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");

            // Change Directory back.
            Content.RootDirectory = @"Content";

            effect.CurrentTechnique = effect.Techniques["BloomGlow"];
            effect.Parameters["numberOfSamplesPerDimension"].SetValue(_numberOfSamples);

            Vector2 size = GraphicsDevice.Viewport.Bounds.Size.ToVector2();
            Vector2 halfsize = size /2; //  / 2;
            //offscreenRenderTarget0 = new RenderTarget2D(GraphicsDevice, (int)size.X, (int)size.Y, false, SurfaceFormat.Color, DepthFormat.None);
            offscreenRenderTarget0 = new RenderTarget2D(GraphicsDevice, (int)halfsize.X, (int)halfsize.Y, false, SurfaceFormat.Color, DepthFormat.None);

            offscreenRenderTarget1 = new RenderTarget2D(GraphicsDevice, (int)size.X, (int)size.Y, false, SurfaceFormat.Color, DepthFormat.None);
            //offscreenRenderTarget1 = new RenderTarget2D(GraphicsDevice, (int)halfsize.X, (int)halfsize.Y, false, SurfaceFormat.Color, DepthFormat.None);

            //renderTargetBinding = new RenderTargetBinding[2];
            //renderTargetBinding[0] = offscreenRenderTarget0;
            //renderTargetBinding[1] = offscreenRenderTarget1;

        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            UpdateStandardStuff(gameTime);

            if (IsPressedWithDelay(Keys.F1, gameTime))
                _useBlur = !_useBlur;

            if (IsPressedWithDelay(Keys.Right, gameTime))
                _numberOfSamples++;

            if (IsPressedWithDelay(Keys.Left, gameTime))
                _numberOfSamples--;

            _numberOfSamples = _numberOfSamples.Clamp(0, MAXSAMPLES);

            base.Update(gameTime);
        }

        public void UpdateStandardStuff(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            mouseState = Mouse.GetState();
            elapsed = (float)(gameTime.ElapsedGameTime.TotalSeconds);
            elapsedPercentageOfSecondsPerCycle += elapsed * secondsPerCycle;
            if (elapsedPercentageOfSecondsPerCycle > 1.0f)
                elapsedPercentageOfSecondsPerCycle -= 1.0f;
        }



        //The Blur Algorithm  http://www.rastertek.com/dx11tut36.html
        //
        //1. Render the scene to rt.
        //
        //2. Down sample the rt to to half its size or less.   // maybe simpler to just call mipmaping.
        //
        //3. Perform a horizontal blur on the down sampled texture.
        //
        //4. Perform a vertical blur.
        //
        //5. Up sample the rt back to the original screen size.
        //
        //6. Render that texture to the screen.

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            effect.Parameters["textureSize"].SetValue(new Vector2(texture.Width, texture.Height));
            effect.Parameters["numberOfSamplesPerDimension"].SetValue(_numberOfSamples);
            effect.Parameters["threshold"].SetValue(threshold);


            // render the textures to a scene.

            GraphicsDevice.SetRenderTarget(offscreenRenderTarget0);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1, 0);

            // 1.  and
            // 2. in rt1
            effect.CurrentTechnique = effect.Techniques["ExtractGlowColors"]; // ExtractGlowColors  BloomGlow.

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, null, effect, null);
            spriteBatch.Draw(texture, offscreenRenderTarget0.Bounds, Color.White);
            spriteBatch.End();

            // render targets to backbuffer.

            GraphicsDevice.SetRenderTargets(null);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(offscreenRenderTarget0, GraphicsDevice.Viewport.Bounds, Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.Draw(texture, new Rectangle(0, 0, 150, 150), Color.White);
            spriteBatch.DrawString(font, $"Controls plus or minus keys and arrow keys  \n _numberOfSamples: {_numberOfSamples.ToString("##0.000")} ", new Vector2(10, 10), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        //protected override void Draw(GameTime gameTime)
        //{
        //    GraphicsDevice.Clear(Color.CornflowerBlue);

        //    effect.Parameters["textureSize"].SetValue(new Vector2(texture.Width, texture.Height));
        //    effect.Parameters["numberOfSamplesPerDimension"].SetValue(_numberOfSamples);
        //    effect.Parameters["threshold"].SetValue(threshold);

        //    // render the textures to a scene.

        //    //GraphicsDevice.SetRenderTargets(renderTargetBinding);

        //    GraphicsDevice.SetRenderTarget(offscreenRenderTarget0);
        //    GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1, 0);

        //    // 1.  and
        //    // 2. in rt1
        //    effect.CurrentTechnique = effect.Techniques["ExtractGlowColors"]; // ExtractGlowColors  BloomGlow.

        //    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, null, effect, null);
        //    spriteBatch.Draw(texture, offscreenRenderTarget0.Bounds, Color.White);
        //    //spriteBatch.Draw(texture2, new Rectangle(320, 0, 300, 300), Color.White);
        //    spriteBatch.End();


        //    GraphicsDevice.SetRenderTargets(null);
        //    GraphicsDevice.Clear(Color.CornflowerBlue);

        //    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, null);
        //    //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, effect, null);
        //    //spriteBatch.Draw(texture, GraphicsDevice.Viewport.Bounds, Color.White);

        //    spriteBatch.Draw(offscreenRenderTarget0, GraphicsDevice.Viewport.Bounds, Color.White);
        //    //spriteBatch.Draw(offscreenRenderTarget1, GraphicsDevice.Viewport.Bounds, Color.White);

        //    //spriteBatch.Draw(offscreenRenderTarget0, new Rectangle(0, 305, 300, 290), Color.White);
        //    //spriteBatch.Draw(offscreenRenderTarget1, new Rectangle(300, 305, 300, 290), Color.White);

        //    spriteBatch.End();

        //    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
        //    spriteBatch.Draw(texture, new Rectangle(0, 0, 150, 150), Color.White);
        //    spriteBatch.DrawString(font, $"Controls plus or minus keys and arrow keys  \n _numberOfSamples: {_numberOfSamples.ToString("##0.000")} ", new Vector2(10, 10), Color.White);
        //    spriteBatch.End();

        //    base.Draw(gameTime);
        //}


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
