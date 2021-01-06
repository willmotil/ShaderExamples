
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_RadialBlur : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D texture;
        Effect effect;

        MouseState ms;

        const int MAXSAMPLES = 60;

        int numberOfSamples = 8;
        float radialScalar = 0.0f;
        Vector2 textureBlurUvOrigin = new Vector2(.5f, .5f);

        public Game1_RadialBlur()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Radial Blur Effect.";
            IsMouseVisible = true;
        }
        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            effect = Content.Load<Effect>("RadialBlurEffect");

            // Change Directory.
            Content.RootDirectory = @"Content/Images";

            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");  // cutePuppy  ,  MG_Logo_Med_exCanvs.png

            // Change Directory.
            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");

            // Change Directory back.
            Content.RootDirectory = @"Content";
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                radialScalar += .01f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                radialScalar -= .01f;

            // We start using extension methods here to make stuff easier these functions are defined in MgExt class.
            // While overdoing them isn't recommended there are times and especally per project were they are helpful.
            //
            if (Keys.Right.IsKeyPressedWithDelay(gameTime))
                numberOfSamples++;
            if (Keys.Left.IsKeyPressedWithDelay(gameTime))
                numberOfSamples--;

            radialScalar = radialScalar.Clamp( -4f, 4f);
            numberOfSamples = numberOfSamples.Clamp( 0, MAXSAMPLES);

            ms = Mouse.GetState();
            textureBlurUvOrigin = (ms.Position.ToVector2() / GraphicsDevice.Viewport.Bounds.Size.ToVector2()); // - new Vector2(.5f,.5f) ;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            effect.CurrentTechnique = effect.Techniques["RadialBlur"];
            effect.Parameters["textureBlurUvOrigin"].SetValue(textureBlurUvOrigin);
            effect.Parameters["radialScalar"].SetValue(radialScalar);
            effect.Parameters["textureSize"].SetValue(new Vector2(texture.Width, texture.Height));
            effect.Parameters["numberOfSamplesPerDimension"].SetValue(numberOfSamples);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, effect, null);
            spriteBatch.Draw(texture, new Rectangle(0, 0, 300, 300), Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.DrawString(font, $"Controls arrow keys \n radialScalar: {radialScalar.ToString("##0.000")} \n numberOfSamples: {numberOfSamples.ToString("##0.000")} \n textureBlurUvOrigin: {textureBlurUvOrigin.ToString()} ", new Vector2(10, 10), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region helper functions


        #endregion
    }
}

