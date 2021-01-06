

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderExamples
{
    public class Game1_RefractionDirectional : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        MouseState mouseState;

        Texture2D texture;
        Texture2D textureDisplacementTexture;
        Effect effect;
        RenderTarget2D offscreenRenderTarget;

        string msgInfo = "";


        #region  controls the shader refraction.

        float distortionRange = .03f;
        float distortionRadius = .08f;
        float numberOfSamples = 7;
        // edge fade and inner fade percentages.
        float fadePercent = 1.2f;
        float fadeStrength = 15.0f;
        // refraction time and direction of refraction.
        float displacementTime = 0;
        Vector2 displacementDirection = new Vector2(3f, 1f);

        #endregion

        #region  timing stuff

        float elapsed = 0;
        float elapsedPercentageOfSecondsPerCycle = 0;
        float secondsPerCycle = .1f;
        ColorCycler colorCycle;

        #endregion


        public Game1_RefractionDirectional()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Refraction via Texel Displacement.";
            IsMouseVisible = true;
        }
        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = @"Content";
            effect = Content.Load<Effect>("RefractionDirectionalEffect");

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");
            textureDisplacementTexture = Content.Load<Texture2D>("RefactionTexture");

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");

            effect.CurrentTechnique = effect.Techniques["RefractionDirectional"];
            effect.Parameters["DisplacementTexture"].SetValue(textureDisplacementTexture);

            offscreenRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
        }

        protected override void UnloadContent()
        {
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

        protected override void Update(GameTime gameTime)
        {
            UpdateStandardStuff(gameTime);

            displacementTime = displacementTime +  elapsed * secondsPerCycle;
                
            displacementDirection = - (mouseState.Position.ToVector2().VirtualScreenCoords(GraphicsDevice) - new Vector2(.5f,.5f) );  // we use our own extension method here then by center offset it.

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                distortionRange += .002f; 
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                distortionRange -= .005f; 

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                distortionRadius += .002f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                distortionRadius -= .002f; 

            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
                fadePercent += .002f;
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
                fadePercent -= .002f;

            if (Keyboard.GetState().IsKeyDown(Keys.OemCloseBrackets))
                fadeStrength += .1f;
            if (Keyboard.GetState().IsKeyDown(Keys.OemOpenBrackets))
                fadeStrength -= .1f;

            // use a extension method in MgExt to slow these presses down we dont want them to fire fast.

            if (Keys.O.IsKeyPressedWithDelay(gameTime))
                numberOfSamples += 1f;
            if (Keys.P.IsKeyPressedWithDelay(gameTime))
                numberOfSamples -= 1f;

            // these are also extensions that wrap around or clamp values to the limits.

            fadePercent = fadePercent.Clamp(0f, 1f);
            fadeStrength = fadeStrength.Clamp(0f, 30f);
            distortionRange = distortionRange.Clamp(0f, 5f);
            distortionRadius = distortionRadius.Clamp(0f, 1f);
            numberOfSamples = numberOfSamples.Wrap(1, 20);

            msgInfo = $"Controls mouse, plus or minus keys brackets + - {"{ }" } and arrow keys \n distortionRange: {distortionRange.ToString("##0.000")} \n distortionRadius: {distortionRadius.ToString("##0.000")} \n numberOfSamples: {numberOfSamples.ToString("##0.000")} \n Fade Percent: {fadePercent.ToString("##0.000")} \n Fade Strength: {fadeStrength.ToString("##0.000")}  \n displacementDirection: {displacementDirection} \n displacementTime: {displacementTime.ToString("##0.000")}";

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            SetShaderParameters();

            var clearingColor = Color.CornflowerBlue;
            GraphicsDevice.SetRenderTarget(offscreenRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target, clearingColor, 1, 0);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.Draw(texture, GraphicsDevice.Viewport.Bounds, Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null,null, null, null, null, null); 
            spriteBatch.DrawString(font, msgInfo, new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(font2, $"MonoGame RenderTargets and Shaders ", new Vector2(30, 430), colorCycle.GetColor(gameTime, 20f));
            spriteBatch.DrawString(font2, $"It's AOS time", new Vector2(230, 230), colorCycle.GetColor(gameTime, 20f), 0, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 0);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(clearingColor);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, effect, null);
            spriteBatch.Draw(offscreenRenderTarget, GraphicsDevice.Viewport.Bounds, Color.White); 
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.Draw(texture, new Rectangle(0, 300, 200, 200), Color.White);
            spriteBatch.DrawString(font, msgInfo, new Vector2(10, 10), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void SetShaderParameters()
        {
            effect.Parameters["distortionRange"].SetValue(distortionRange);
            effect.Parameters["distortionRadius"].SetValue(distortionRadius);
            effect.Parameters["numberOfSamples"].SetValue((int)numberOfSamples);
            effect.Parameters["percent"].SetValue(fadePercent);
            effect.Parameters["strength"].SetValue(fadeStrength);
            effect.Parameters["displacementTime"].SetValue(displacementTime);
            effect.Parameters["displacementDirection"].SetValue(displacementDirection);
            effect.Parameters["DisplacementTexture"].SetValue(textureDisplacementTexture);
        }

        public void SetCommonlyUsedStates()
        {
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        }

        public struct ColorCycler
        {
            private float timerA;
            private float timerB;
            private float timerG;
            private float timerR;

            public Color GetColor(GameTime gameTime, float cycleDurationInSeconds)
            {
                var elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds / cycleDurationInSeconds;
                timerR += elapsed + elapsed *  .74f;
                timerG += elapsed + elapsed *  .36f;
                timerB += elapsed + elapsed *  .12f;
                timerA += elapsed;
                return new Color(GetColElement(Wrap(timerR)), GetColElement(Wrap(timerG)), GetColElement(Wrap(timerB)), GetColElement(Wrap(timerA)));
            }
            float Wrap(float t)
            {
                if (t > 1f)
                    t -= 1f;
                return t;
            }
            int GetColElement(float t)
            {
                var res = (float)Math.Sin(t * 6.28218f);
                if (res < 0)
                    res = -res;
                return (byte)(255f * res);
            }
        }

    }
}


/*
protected override void Draw(GameTime gameTime)
{
    SetShaderParameters();

    var clearingColor = Color.CornflowerBlue;


    // Tell the graphics device that we want to draw onto the offscreen rendertarget.
    // Clear the RT buffer to the clearing color.

    GraphicsDevice.SetRenderTarget(offscreenRenderTarget);
    GraphicsDevice.Clear(ClearOptions.Target, clearingColor, 1, 0);

    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null); 
    spriteBatch.Draw(texture, GraphicsDevice.Viewport.Bounds, Color.White);
    spriteBatch.End();

    spriteBatch.Begin(SpriteSortMode.Immediate, null,null, null, null, null, null); 
    spriteBatch.DrawString(font, msgInfo, new Vector2(10, 10), Color.White);
    int cyc = (int)(255f * Math.Sin(_elapsedCycle * 3.1459));
    int icyc = 255 - cyc;
    var color = new Color(cyc, cyc, icyc, icyc); 
    spriteBatch.DrawString(font2, $"MonoGame RenderTargets and Shaders ", new Vector2(30, 430), color );
    spriteBatch.End();



    // Tell the graphics device that we intend to draw to the actual backbuffer.
    // Tell the graphics device to clear the backbuffer to the clearing color.

    GraphicsDevice.SetRenderTarget(null);
    GraphicsDevice.Clear(clearingColor);

    // We now draw the offscreen buffer to the back buffer aka to the presentation buffer for rendering to the screen.

    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, effect, null);
    spriteBatch.Draw(offscreenRenderTarget, GraphicsDevice.Viewport.Bounds, Color.White); 
    spriteBatch.End();

    //
    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
    spriteBatch.Draw(texture, new Rectangle(0, 300, 200, 200), Color.White);
    spriteBatch.DrawString(font, msgInfo, new Vector2(10, 10), Color.White);
    spriteBatch.End();

    base.Draw(gameTime);
}

 */
