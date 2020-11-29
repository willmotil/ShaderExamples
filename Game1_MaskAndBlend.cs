using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_MaskAndBlend : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Effect effect;
        Texture2D texture;
        Texture2D shadingMultiTexture;
        Texture2D stenciledTexture;

        bool _useBlend = false;

        public Game1_MaskAndBlend()
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

            // "Terran02" "GeneratedSphere" "planet_stencil" "clouds-heavy" "cutePuppy"
            texture = Content.Load<Texture2D>("Terran02");
            shadingMultiTexture = Content.Load<Texture2D>("clouds-heavy"); 
            stenciledTexture = Content.Load<Texture2D>("planet_stencil");

            effect = Content.Load<Effect>("MaskAndBlendTextures");
            effect.CurrentTechnique = effect.Techniques["MaskAndBlend"];
            effect.Parameters["SpriteMultiTexture"].SetValue(shadingMultiTexture);
            effect.Parameters["SpriteStencilTexture"].SetValue(stenciledTexture);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (IsPressedWithDelay(Keys.F1, gameTime))
                _useBlend = !_useBlend;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            effect.CurrentTechnique = effect.Techniques["Basic"];

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, effect, null);
            spriteBatch.Draw(texture, new Rectangle(0, 0, 300, 300), Color.White);
            spriteBatch.End();

            if (_useBlend)
                effect.CurrentTechnique = effect.Techniques["MaskAndBlend"];
            else
                effect.CurrentTechnique = effect.Techniques["MaskAndOverlay"];

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, effect, null);
            spriteBatch.Draw(texture, new Rectangle(300, 0, 300, 300), Color.White);
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