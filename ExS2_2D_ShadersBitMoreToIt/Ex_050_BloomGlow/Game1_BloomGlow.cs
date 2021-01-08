

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
        Texture2D textureDisplacementTexture;
        Effect effect;
        MouseState mouseState;
        RenderTarget2D offscreenRenderTarget0;
        RenderTarget2D offscreenRenderTarget1;

        bool _useBlur = true;
        const int MAXSAMPLES = 40;
        int _numberOfSamples = 20;

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

            texture = Content.Load<Texture2D>("cutePuppy");

            // Change Directory.
            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");

            // Change Directory back.
            Content.RootDirectory = @"Content";

            effect.CurrentTechnique = effect.Techniques["BloomGlow"];
            effect.Parameters["numberOfSamplesPerDimension"].SetValue(_numberOfSamples);

            offscreenRenderTarget0 = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
            offscreenRenderTarget1 = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);

            RenderTargetBinding[] renderTargetBinding = new RenderTargetBinding[2];
            renderTargetBinding[0] = offscreenRenderTarget0;
            renderTargetBinding[1] = offscreenRenderTarget1;

            GraphicsDevice.SetRenderTargets(renderTargetBinding);
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

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, effect, null);
            spriteBatch.Draw(texture, new Rectangle(0, 0, 300, 300), Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.DrawString(font, $"Controls plus or minus keys and arrow keys  \n _numberOfSamples: {_numberOfSamples.ToString("##0.000")} ", new Vector2(10, 10), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        //protected override void Draw(GameTime gameTime)
        //{
        //    SetShaderParameters();

        //    var clearingColor = Color.CornflowerBlue;
        //    GraphicsDevice.SetRenderTarget(offscreenRenderTarget);
        //    GraphicsDevice.Clear(ClearOptions.Target, clearingColor, 1, 0);

        //    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
        //    spriteBatch.Draw(texture, GraphicsDevice.Viewport.Bounds, Color.White);
        //    spriteBatch.End();

        //    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
        //    spriteBatch.DrawString(font, msgInfo, new Vector2(10, 10), Color.White);
        //    spriteBatch.DrawString(font2, $"MonoGame RenderTargets and Shaders ", new Vector2(30, 430), colorCycle.GetColor(gameTime, 20f));
        //    spriteBatch.DrawString(font2, $"It's AOS time", new Vector2(230, 230), colorCycle.GetColor(gameTime, 20f), 0, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 0);
        //    spriteBatch.End();

        //    GraphicsDevice.SetRenderTarget(null);
        //    GraphicsDevice.Clear(clearingColor);

        //    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, effect, null);
        //    spriteBatch.Draw(offscreenRenderTarget, GraphicsDevice.Viewport.Bounds, Color.White);
        //    spriteBatch.End();

        //    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
        //    spriteBatch.Draw(texture, new Rectangle(0, 300, 200, 200), Color.White);
        //    spriteBatch.DrawString(font, msgInfo, new Vector2(10, 10), Color.White);
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
