
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderExamples
{
    public class Game1_ScrollEffect_TextureWrapping : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D texture;
        Effect effect;

        float percent = 0.6f;
        float strength = 8.0f;
        Vector2 scrollDirection = new Vector2(3f, 1f);
        Vector2 scroll = Vector2.Zero;

        float _elapsed = 0;
        float _elapsedCycle = 0;
        float _cycleRate = .1f;

        public Game1_ScrollEffect_TextureWrapping()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = "TextureWrapping ex.  Scroll Effect.fx";
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
            effect = Content.Load<Effect>("ScrollEffect");
            effect.CurrentTechnique = effect.Techniques["ScrollFadingEdges"];

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs"); 

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
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

            scroll = scroll + Vector2.Normalize(scrollDirection) * _elapsed * _cycleRate;

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                percent += .002f;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                percent -= .002f;

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                strength += .1f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                strength -= .1f;

            percent = EnsureInRange(percent, 1f);
            strength = EnsureInRange(strength, 20f);

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

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            effect.Parameters["percent"].SetValue(percent);
            effect.Parameters["strength"].SetValue(strength);
            effect.Parameters["scrolldir"].SetValue(scroll);
            

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, effect, null);
            spriteBatch.Draw(texture, new Rectangle(0, 0, 300, 300), Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.DrawString(font, $" press arrow keys to alter image  \n Percent: {percent.ToString("##0.000")} \n Strength: {strength.ToString("##0.000")}", new Vector2(10, 10), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
