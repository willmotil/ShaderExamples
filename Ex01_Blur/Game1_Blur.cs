
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_Blur : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D texture;
        Effect effect;

        bool _useBlur = true;
        const int MAXSAMPLES = 20;
        int _numberOfSamples = 10;

        public Game1_Blur()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }
        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            effect = Content.Load<Effect>("GausianBlurEffect");

            // Change Directory.
            Content.RootDirectory = @"Content/Images";

            texture = Content.Load<Texture2D>("cutePuppy");

            // Change Directory back.
            Content.RootDirectory = @"Content";

            effect.CurrentTechnique = effect.Techniques["Blur"];
            effect.Parameters["numberOfSamplesPerDimension"].SetValue(_numberOfSamples);
            effect.Parameters["pixelResolutionX"].SetValue(1f / texture.Width);
            effect.Parameters["pixelResolutionY"].SetValue(1f / texture.Height);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (IsPressedWithDelay(Keys.F1, gameTime))
                _useBlur = ! _useBlur;

            if (IsPressedWithDelay(Keys.Right, gameTime))
                _numberOfSamples++;

            if (IsPressedWithDelay(Keys.Left, gameTime))
                _numberOfSamples--;

            if (_numberOfSamples < 1)
                _numberOfSamples = MAXSAMPLES - 1;
            if (_numberOfSamples >= MAXSAMPLES)
                _numberOfSamples = 1;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            if (_useBlur)
                effect.CurrentTechnique = effect.Techniques["Blur"];
            else
                effect.CurrentTechnique = effect.Techniques["Basic"];

            effect.Parameters["numberOfSamplesPerDimension"].SetValue(_numberOfSamples);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, effect, null);
            spriteBatch.Draw(texture, new Rectangle(0, 0, 300, 300), Color.Red);
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
