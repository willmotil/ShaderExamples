
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_BarFillColorReplace : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        Texture2D texture;
        Effect effect;
        //SpriteFont font;
        //SpriteFont font2;
        //SpriteFont font3;
        RenderTarget2D rtScene;

        float percentageOfFill = 0;
        //float heightBias = .02f;
        float blueReplaceThreshold = .6f;

        public Game1_BarFillColorReplace()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Bar fill and color replace ...  Controls: up or down arrows.";
            IsMouseVisible = true;
            Window.ClientSizeChanged += OnResize;
        }

        public void OnResize(object sender, EventArgs e)
        {
            rtScene = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
        }

        protected override void Initialize() { base.Initialize(); }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("BarBlueInside");

            Content.RootDirectory = @"Content/Shaders";
            effect = Content.Load<Effect>("BarFillColorReplaceEffect");

            Content.RootDirectory = @"Content/Fonts";

            Content.RootDirectory = @"Content";

            effect.CurrentTechnique = effect.Techniques["SpriteDrawing"];
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                percentageOfFill += .01f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                percentageOfFill -= .01f;
            if (percentageOfFill > 1)
                percentageOfFill = 0;
            if (percentageOfFill < 0)
                percentageOfFill = 1;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            effect.Parameters["percentageOfFill"].SetValue(percentageOfFill);
            effect.Parameters["offset"].SetValue(0.02f);
            effect.Parameters["blueReplaceThreshold"].SetValue(blueReplaceThreshold);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, effect, null);
            spriteBatch.Draw(texture, GraphicsDevice.Viewport.Bounds, Color.Red);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.Draw(texture, new Rectangle(0,0,100,100), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}