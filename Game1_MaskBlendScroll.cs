using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ShaderExamples
{
    public class Game1_MaskBlendScroll : Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Effect effect;
        Texture2D loadedBackgroundTexture01;
        Texture2D loadedForgroundTexture02;
        Texture2D loadedStencilTexture03;
        Texture2D generatedAlphaStencilTexture;
        Texture2D backGroundTexture;
        Texture2D foreGroundTexture;
        Texture2D stenciledTexture;

        bool _useBlend = false;
        float _elapsed = 0.0f;
        float _elapsedCycle = 0.0f;
        float _cycleRate = 1f / 12.0f;
        float _elapsedCycle2 = 0.0f;
        float _cycleRate2 = 1f / 10.0f;

        public Game1_MaskBlendScroll()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 600;
        }
        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = @"Content/Images";

            loadedBackgroundTexture01 = Content.Load<Texture2D>("Terran02");
            loadedForgroundTexture02 = Content.Load<Texture2D>("clouds-heavy");
            loadedStencilTexture03 = Content.Load<Texture2D>("planet_stencil"); 
            generatedAlphaStencilTexture = MgTextureGenerator.GenerateAlphaStencilCircle(GraphicsDevice, Color.White);

            backGroundTexture = loadedBackgroundTexture01;
            foreGroundTexture = loadedForgroundTexture02;
            stenciledTexture = generatedAlphaStencilTexture;

            Content.RootDirectory = @"Content";

            effect = Content.Load<Effect>("MaskBlendScroll");
            effect.CurrentTechnique = effect.Techniques["MaskAndBlend"];
            effect.Parameters["SpriteStencilTexture"].SetValue(stenciledTexture);
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

            _elapsedCycle2 += _elapsed * _cycleRate2;
            if (_elapsedCycle2 > 1.0f)
                _elapsedCycle2 -= 1.0f;

            if (IsPressedWithDelay(Keys.F1, gameTime))
                _useBlend = !_useBlend;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            effect.CurrentTechnique = effect.Techniques["Basic"];

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, effect, null);
            spriteBatch.Draw(backGroundTexture, new Rectangle(0, 0, 300, 300), Color.White);
            spriteBatch.Draw(foreGroundTexture, new Rectangle(0, 320, 300, 300), Color.White);
            spriteBatch.Draw(loadedStencilTexture03, new Rectangle(310, 320, 300, 300), Color.White);
            spriteBatch.Draw(generatedAlphaStencilTexture, new Rectangle(620, 320, 300, 300), Color.White);
            spriteBatch.End();

            if (_useBlend)
                effect.CurrentTechnique = effect.Techniques["MaskAndBlend"];
            else
                effect.CurrentTechnique = effect.Techniques["MaskAndOverlay"];

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, effect, null);

            effect.Parameters["SpriteStencilTexture"].SetValue(loadedStencilTexture03);
            effect.Parameters["CycleTime"].SetValue(_elapsedCycle);
            spriteBatch.Draw(backGroundTexture, new Rectangle(310, 0, 300, 300), Color.White);

            effect.Parameters["SpriteStencilTexture"].SetValue(generatedAlphaStencilTexture);
            effect.Parameters["CycleTime"].SetValue(_elapsedCycle2);
            spriteBatch.Draw(foreGroundTexture, new Rectangle(310, 0, 300, 300), Color.White);

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