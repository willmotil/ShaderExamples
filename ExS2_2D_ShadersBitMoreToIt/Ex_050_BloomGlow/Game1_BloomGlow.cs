

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
        int _numberOfSamples = 20;
        float threshold = .5f;

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

            offscreenRenderTarget0 = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
            offscreenRenderTarget1 = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);

            renderTargetBinding = new RenderTargetBinding[2];
            renderTargetBinding[0] = offscreenRenderTarget0;
            renderTargetBinding[1] = offscreenRenderTarget1;

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

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            effect.CurrentTechnique = effect.Techniques["BloomGlow"];
            effect.Parameters["textureSize"].SetValue(new Vector2(texture.Width, texture.Height));
            effect.Parameters["numberOfSamplesPerDimension"].SetValue(_numberOfSamples);
            effect.Parameters["threshold"].SetValue(threshold);


            //GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            GraphicsDevice.SetRenderTargets(renderTargetBinding);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1, 0);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, effect, null);
            spriteBatch.Draw(texture, new Rectangle(0, 0, 300, 300), Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRenderTargets(null);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, effect, null);
            //spriteBatch.Draw(texture, GraphicsDevice.Viewport.Bounds, Color.White);
            spriteBatch.Draw(offscreenRenderTarget0, GraphicsDevice.Viewport.Bounds, Color.White);
            spriteBatch.Draw(offscreenRenderTarget1, GraphicsDevice.Viewport.Bounds, Color.White);

            spriteBatch.Draw(offscreenRenderTarget0, new Rectangle(0, 300, 300, 300), Color.White);
            spriteBatch.Draw(offscreenRenderTarget1, new Rectangle(300, 300, 300, 300), Color.White);

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.DrawString(font, $"Controls plus or minus keys and arrow keys  \n _numberOfSamples: {_numberOfSamples.ToString("##0.000")} ", new Vector2(10, 10), Color.White);
            spriteBatch.End();

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
