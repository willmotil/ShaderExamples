

//  Not yet implemented  ... aka todo sorry ...  put this on hold a minute .

//The Blur Algorithm  http://www.rastertek.com/dx11tut36.html
//1. Render the scene to rt.
//2. Down sample the rt to to half its size or less.   // maybe simpler to just call mipmaping.
//3. Perform a horizontal blur on the down sampled texture.
//4. Perform a vertical blur.
//5. Up sample the rt back to the original screen size.
//6. Render that texture to the screen.

// this is the learn open gl bllom. 
// https://learnopengl.com/Advanced-Lighting/Bloom


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
        //SpriteFont font2;
        Texture2D texture;
        Texture2D texture1;
        Texture2D texture2;
        Texture2D texture3;
        //Texture2D textureDisplacementTexture;
        Effect effect;
        MouseState mouseState;

        RenderTarget2D rtScene;
        RenderTarget2D rtExtracted;
        RenderTarget2D rtBloom;
        RenderTarget2D rtResult;

        // unused to simplify
        RenderTarget2D rtReduced0;
        RenderTarget2D rtReduced1;

        //RenderTargetBinding[] renderTargetBinding;

        bool useWeightBalance = true;
        bool useHorizontal = true;
        const int MAXSAMPLES = 100;
        int numberOfSamples = 6;
        Vector4 threshold = new Vector4(0.2126f, 0.7152f, 0.0722f, 0.89f);   // (.59f, .59f, .59f, .89f);
        float thresholdTolerance = .75f;
        float[] weights = new float[] { 0.227027f, 0.1945946f, 0.1216216f, 0.054054f, 0.016216f };
        

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
            Window.Title = " ex Gaussian Bloom (RenderTarget ping pong) effect.";
            IsMouseVisible = true;
        }
        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            UpdateStandardStuff(gameTime);

            if (IsPressedWithDelay(Keys.Space, gameTime))
                useWeightBalance = !useWeightBalance;

            if (IsPressedWithDelay(Keys.Right, gameTime))
                numberOfSamples++;

            if (IsPressedWithDelay(Keys.Left, gameTime))
                numberOfSamples--;

            numberOfSamples = numberOfSamples.Clamp(0, MAXSAMPLES);


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

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.SetRenderTargets(rtScene);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.Draw(texture1, new Rectangle(0, 0, rtScene.Width, 250), Color.White);
            spriteBatch.Draw(texture2, new Rectangle(0, 250, rtScene.Width, 250), Color.White);
            spriteBatch.End();

            effect.Parameters["numberOfSamplesPerDimension"].SetValue(numberOfSamples);
            effect.Parameters["threshold"].SetValue(threshold);
            effect.Parameters["thresholdTolerance"].SetValue(thresholdTolerance);
            effect.Parameters["horizontal"].SetValue(useHorizontal);
            effect.Parameters["weight"].SetValue(weights);
            effect.Parameters["textureSize"].SetValue(new Vector2(rtScene.Width, rtScene.Height));
            effect.Parameters["SecondaryTexture"].SetValue(rtScene);

            ExtractColors(rtScene, rtExtracted);

            effect.Parameters["horizontal"].SetValue(false);
            Bloom(rtExtracted, rtBloom);
            effect.Parameters["horizontal"].SetValue(true);
            Bloom(rtBloom, rtResult);
            effect.Parameters["horizontal"].SetValue(false);
            Bloom(rtResult, rtBloom);
            effect.Parameters["horizontal"].SetValue(true);
            Bloom(rtBloom, rtResult);
            effect.Parameters["horizontal"].SetValue(false);
            Bloom(rtResult, rtBloom);
            effect.Parameters["horizontal"].SetValue(true);
            Bloom(rtBloom, rtResult);
            effect.Parameters["horizontal"].SetValue(false);
            Bloom(rtResult, rtBloom);
            effect.Parameters["horizontal"].SetValue(true);
            Bloom(rtBloom, rtResult);

            Combine(rtScene, rtBloom, rtResult);


            // back buffer it now.
            GraphicsDevice.SetRenderTargets(null);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.Draw(rtScene, new Rectangle(0, 0, 250, 250), Color.White);
            spriteBatch.Draw(rtExtracted, new Rectangle(0, 255, 250, 250), Color.White);
            spriteBatch.Draw(rtBloom, new Rectangle(0, 510, 250, 250), Color.White);
            spriteBatch.Draw(rtResult, new Rectangle(250, 0, GraphicsDevice.Viewport.Bounds.Width - 250, GraphicsDevice.Viewport.Bounds.Height), Color.White);
            spriteBatch.DrawString(font, $"Controls plus or minus keys and arrow keys  \n _numberOfSamples: {numberOfSamples.ToString("##0.000")} _useWeightBalance {useWeightBalance}" , new Vector2(10, 10), Color.Moccasin);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void ExtractColors(Texture2D a, RenderTarget2D b)
        {
            GraphicsDevice.SetRenderTarget(b);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1, 0);

            effect.CurrentTechnique = effect.Techniques["ExtractGlowColors"]; // ExtractGlowColors  BloomGlow.

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, effect, null);
            spriteBatch.Draw(a, b.Bounds, Color.White);
            spriteBatch.End();
        }

        public void UpOrDownSample(Texture2D a, RenderTarget2D b)
        {
            GraphicsDevice.SetRenderTarget(b);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1, 0);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, null, null, null);
            spriteBatch.Draw(a, b.Bounds, Color.White);
            spriteBatch.End();
        }

        public void Bloom(Texture2D a, RenderTarget2D b)
        {
            GraphicsDevice.SetRenderTarget(b);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1, 0);

            effect.CurrentTechnique = effect.Techniques["Bloom"]; // ExtractGlowColors  BloomGlow.
            effect.Parameters["textureSize"].SetValue(new Vector2(a.Width, a.Height));

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, null, null, effect, null); // BlendState.NonPremultiplied
            spriteBatch.Draw(a, b.Bounds, Color.White);
            spriteBatch.End();
        }

        public void Combine(Texture2D a, RenderTarget2D b, RenderTarget2D c)
        {
            GraphicsDevice.SetRenderTarget(c);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1, 0);

            effect.CurrentTechnique = effect.Techniques["CombineBloom"]; // ExtractGlowColors  BloomGlow.
            effect.Parameters["SecondaryTexture"].SetValue(b);
            //effect.Parameters["textureSize"].SetValue(new Vector2(a.Width, a.Height));

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, effect, null);
            spriteBatch.Draw(a, b.Bounds, Color.White);
            spriteBatch.End();
        }

        public void ToBackBuffer(Texture2D a)
        {
            GraphicsDevice.SetRenderTargets(null);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, null, null, null);
            //spriteBatch.Draw(a, GraphicsDevice.Viewport.Bounds, Color.White);
            spriteBatch.Draw(a, new Rectangle(250, 0, GraphicsDevice.Viewport.Bounds.Width - 250, GraphicsDevice.Viewport.Bounds.Height), Color.White);
            spriteBatch.End();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = @"Content/Shaders";
            effect = Content.Load<Effect>("BloomGlowEffect");

            // Change Directory.
            Content.RootDirectory = @"Content/Images";

            texture1 = Content.Load<Texture2D>("puppy03"); // cutePuppy puppy03
            texture2 = Content.Load<Texture2D>("BloomTestImage");
            texture3 = Content.Load<Texture2D>("MG_Logo_Modifyed");

            texture = texture2;

            // Change Directory.
            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");

            // Change Directory back.
            Content.RootDirectory = @"Content";

            effect.CurrentTechnique = effect.Techniques["BloomGlow"];
            effect.Parameters["numberOfSamplesPerDimension"].SetValue(numberOfSamples);

            Vector2 size = GraphicsDevice.Viewport.Bounds.Size.ToVector2();
            Vector2 reducedsize = size / 2;             //  / 2;

            rtReduced0 = new RenderTarget2D(GraphicsDevice, (int)reducedsize.X, (int)reducedsize.Y, false, SurfaceFormat.Color, DepthFormat.None);
            rtReduced1 = new RenderTarget2D(GraphicsDevice, (int)reducedsize.X, (int)reducedsize.Y, false, SurfaceFormat.Color, DepthFormat.None);

            rtScene = new RenderTarget2D(GraphicsDevice, (int)size.X, (int)size.Y, false, SurfaceFormat.Color, DepthFormat.None);
            rtExtracted = new RenderTarget2D(GraphicsDevice, (int)size.X, (int)size.Y, false, SurfaceFormat.Color, DepthFormat.None);
            rtBloom = new RenderTarget2D(GraphicsDevice, (int)size.X, (int)size.Y, false, SurfaceFormat.Color, DepthFormat.None);
            rtResult = new RenderTarget2D(GraphicsDevice, (int)size.X, (int)size.Y, false, SurfaceFormat.Color, DepthFormat.None);

            //renderTargetBinding = new RenderTargetBinding[2];
            //renderTargetBinding[0] = offscreenRenderTarget0;
            //renderTargetBinding[1] = offscreenRenderTarget1;
        }

        protected override void UnloadContent()
        {
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
