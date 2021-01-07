
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderExamples
{
    public class Game1_GlowingEffect : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D texture;
        //Texture2D textureDisplacementTexture;
        Effect effect;


        float refractionRange = .09f;
        float percent = 0.6f;
        float strength = 8.0f;
        Vector2 scrollDirection = new Vector2(3f, 1f);
        Vector2 scroll = Vector2.Zero;

        Vector2 textureSize;

        float _elapsed = 0;
        float _elapsedCycle = 0;
        float _cycleRate = .1f;

        public Game1_GlowingEffect()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Glowing Effect.";
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
            effect = Content.Load<Effect>("GlowingEffect");

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("cutePuppy");   // MG_Logo_Med_exCanvs  Terran02 cutePuppy
            //textureDisplacementTexture = Content.Load<Texture2D>("RefactionTexture");

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");

            effect.CurrentTechnique = effect.Techniques["Glowing"];
            //effect.Parameters["DisplacementTexture"].SetValue(textureDisplacementTexture);
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
                percent += .02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                percent -= .02f;

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                strength += .1f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                strength -= .1f;

            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
                refractionRange += .002f;
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
                refractionRange -= .002f;

            percent = percent.Clamp(0f, 1f);
            strength = strength.Wrap(0f, 20f);
            refractionRange = refractionRange.Wrap( 0f, 2f);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            effect.Parameters["percent"].SetValue(percent);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, effect, null);

            effect.Parameters["TextureSize"].SetValue(texture.Bounds.Size.ToVector2());
            spriteBatch.Draw(texture, new Rectangle(0, 0, 300, 300), Color.Red);

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.DrawString(font, $"Controls plus or minus keys and arrow keys \n RefractionRange: {refractionRange.ToString("##0.000")} \n Fade Percent: {percent.ToString("##0.000")} \n Fade Strength: {strength.ToString("##0.000")}", new Vector2(10, 10), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}