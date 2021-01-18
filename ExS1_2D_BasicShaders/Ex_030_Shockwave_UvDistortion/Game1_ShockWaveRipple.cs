
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_ShockWaveRipple : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D texture;
        Effect effect;

        MouseState ms;

        float time = 0.0f;
        Vector2 center = new Vector2(.5f, .5f);
        Vector3 shockParams = new Vector3(10.0f, 0.8f, 0.1f);
        float waveSpeed = 1.1f;

        bool shockwaveClicks = false;

        public Game1_ShockWaveRipple()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Shockwave Ripple Effect.";
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
            effect = Content.Load<Effect>("ShockwaveRippleEffect");

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

            float secondsScalar =  waveSpeed;
            time += (float)gameTime.ElapsedGameTime.TotalSeconds * secondsScalar;
            float maxTime = 10.0f;

            ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed  && IsClickedWithDelay(ms, gameTime) )
            {
                shockwaveClicks = true;
                time = 0f;
                delay = .5f;
                center = (ms.Position.ToVector2() / GraphicsDevice.Viewport.Bounds.Size.ToVector2()); // - new Vector2(.5f,.5f) ;
            }
            if(shockwaveClicks )
            {
                time = time.Clamp( 0, maxTime);
                if(time >= maxTime)
                    shockwaveClicks = false;
            }
            else
                time = 100.0f;

            if (Keys.Q.IsKeyDown())
                shockParams.X += .02f;
            if (Keys.A.IsKeyDown())
                shockParams.X -= .02f;
            if (Keys.W.IsKeyDown())
                shockParams.Y += .02f;
            if (Keys.S.IsKeyDown())
                shockParams.Y -= .02f;
            if (Keys.E.IsKeyDown())
                shockParams.Z += .02f;
            if (Keys.D.IsKeyDown())
                shockParams.Z -= .02f;
            if (Keys.R.IsKeyDown())
                waveSpeed += .02f;
            if (Keys.F.IsKeyDown())
                waveSpeed -= .02f;

            base.Update(gameTime);
        }

        //if (Keyboard.GetState().IsKeyDown(Keys.Up))
        //    time += .01f;
        //if (Keyboard.GetState().IsKeyDown(Keys.Down))
        //    time -= .01f;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            effect.CurrentTechnique = effect.Techniques["Shockwave"];
            
            effect.Parameters["currentMouse"].SetValue(ms.Position.ToVector2() / GraphicsDevice.Viewport.Bounds.Size.ToVector2());
            effect.Parameters["center"].SetValue(center);
            effect.Parameters["time"].SetValue(time);
            effect.Parameters["shockParams"].SetValue(shockParams);
            //effect.Parameters["textureSize"].SetValue(new Vector2(texture.Width, texture.Height));

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, effect, null);
            spriteBatch.Draw(texture, GraphicsDevice.Viewport.Bounds, Color.Blue);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.DrawString(font, $"Controls left click .. arrow keys \n radialScalar: {time.ToString("##0.000")} \n numberOfSamples: {shockParams} \n textureBlurUvOrigin: {center.ToString()} ", new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(font, $"Keys \n (Q A) shockParams.X: {shockParams.X.ToString("##0.000")} \n (W S) shockParams.Y: {shockParams.Y} \n (E D) shockParams.Z: {shockParams.Z.ToString()} \n (R F) waveSpeed: {waveSpeed.ToString()} ", new Vector2(10, 100), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region helper functions

        public bool IsClickedWithDelay(MouseState m, GameTime gameTime)
        {
            delay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if ((m.LeftButton == ButtonState.Pressed || m.RightButton == ButtonState.Pressed))
            {
                if (IsUnDelayed(gameTime))
                {
                    delay = .45f;
                    return true;
                }
                else
                {
                    delay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    return false;
                }
            }
            else
                delay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            return false;
        }

        public bool IsPressedWithDelay(Keys key, GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(key))
            {
                if (IsUnDelayed(gameTime))
                {
                    delay = .25f;
                    return true;
                }
                else
                {
                    delay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    return false;
                }
            }
            else
                delay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            return false;
        }

        float delay = 0f;
        private bool IsUnDelayed(GameTime gametime)
        {
            if (delay < 0)
                return true;
            else
                return false;
        }

        #endregion
    }
}

