

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
        MouseState ms;

        Texture2D texture;
        Texture2D textureDisplacementTexture;
        Effect effect;

        // controls the shader refraction
        float distortionRange = .03f;
        float distortionRadius = .06f;
        // edge fade and inner fade percentages.
        float fadePercent = 0.8f;
        float fadeStrength = 7.0f;
        // refraction time and direction of refraction.
        float displacementTime = 0;
        Vector2 displacementDirection = new Vector2(3f, 1f);

        // timing
        float _elapsed = 0;
        float _elapsedCycle = 0;
        float _cycleRate = .1f;

        RenderTarget2D rt;

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

            rt = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _elapsed = (float)(gameTime.ElapsedGameTime.TotalSeconds);

            _elapsedCycle += _elapsed * _cycleRate;
            if (_elapsedCycle > 1.0f)
                _elapsedCycle -= 1.0f;

            displacementTime = displacementTime +  _elapsed * _cycleRate;

            ms = Mouse.GetState();
            //if (ms.LeftButton == ButtonState.Pressed && gameTime.IsUnDelayed())
                
            displacementDirection = - (ms.Position.ToVector2().VirtualScreenCoords(GraphicsDevice) - new Vector2(.5f,.5f) );  // we use our own extension method here then by center offset it.

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


            fadePercent = fadePercent.EnsureClampInRange(0f, 1f);
            fadeStrength = fadeStrength.EnsureClampInRange(0f, 20f);
            distortionRange = distortionRange.EnsureClampInRange(0f, 5f);
            distortionRadius = distortionRadius.EnsureClampInRange(0f, 1f);

            base.Update(gameTime);
        }

        float EnsureInRange(float n, float max)
        {
            if (n > max)
                n = 0.0f;
            if (n < 0f)
                n = max;
            return n;
        }

        public void SetShaderParameters()
        {
            effect.Parameters["displacementTime"].SetValue(displacementTime);
            effect.Parameters["displacementDirection"].SetValue(displacementDirection);
            effect.Parameters["distortionRange"].SetValue(distortionRange);
            effect.Parameters["distortionRadius"].SetValue(distortionRadius);
            effect.Parameters["percent"].SetValue(fadePercent);
            effect.Parameters["strength"].SetValue(fadeStrength);
            effect.Parameters["DisplacementTexture"].SetValue(textureDisplacementTexture);
        }

        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1,0);

            // we tell the graphics device that we want to draw onto the offscreen rendertarget.
            GraphicsDevice.SetRenderTarget(rt);

            SetShaderParameters();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, null);
            spriteBatch.Draw(texture, GraphicsDevice.Viewport.Bounds, Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, null);
            spriteBatch.DrawString(font, $"Controls mouse, plus or minus keys brackets + - {"{ }" } and arrow keys \n distortionRange: {distortionRange.ToString("##0.000")} \n distortionRadius: {distortionRadius.ToString("##0.000")} \n Fade Percent: {fadePercent.ToString("##0.000")} \n Fade Strength: {fadeStrength.ToString("##0.000")}  \n displacementDirection: {displacementDirection} \n displacementTime: {displacementTime.ToString("##0.000")}", new Vector2(10, 10), Color.White);
            int cyc = (int)(255f * Math.Sin(_elapsedCycle * 3.1459));
            int icyc = 255 - cyc;
            var color = new Color(cyc, cyc, icyc, icyc); 
            spriteBatch.DrawString(font2, $"MonoGame RenderTargets and Shaders ", new Vector2(30, 430), color );
            spriteBatch.End();

            // we now tell the graphics device that we intend to draw to the actual backbuffer that is presented to the gpu as the monitors frame data to output.
            GraphicsDevice.SetRenderTarget(null);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, effect, null);
            spriteBatch.Draw(rt, GraphicsDevice.Viewport.Bounds, Color.White); //  <<<<< here we pass the render target itself to spritebatch for the texture to use.
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}