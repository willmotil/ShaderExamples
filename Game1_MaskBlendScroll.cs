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
        Texture2D texture;
        Texture2D shadingMultiTexture;
        Texture2D stenciledTexture;
        Texture2D generatedTexture;

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
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
        }
        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            generatedTexture = GenerateCircle(GraphicsDevice, Color.White);

            Content.RootDirectory = @"Content/Images";

            texture = Content.Load<Texture2D>("Terran02");
            shadingMultiTexture = Content.Load<Texture2D>("clouds-heavy");
            //stenciledTexture = Content.Load<Texture2D>("GeneratedSphere");  //"planet_stencil"

            stenciledTexture = generatedTexture;

            Content.RootDirectory = @"Content";

            effect = Content.Load<Effect>("MaskBlendScroll");
            effect.CurrentTechnique = effect.Techniques["MaskAndBlend"];
            effect.Parameters["SpriteMultiTexture"].SetValue(shadingMultiTexture);
            effect.Parameters["SpriteStencilTexture"].SetValue(stenciledTexture);
        }

        public static Texture2D GenerateCircle(GraphicsDevice device, Color color)
        {
            Color[] data = new Color[100 * 100];
            var center = new Vector2(50, 50);
            var a = new Vector2(0, 1.00f);
            var b = new Vector2(80, 0.90f);
            var c = new Vector2(90, 0.00f);
            for (int x =0; x < 100; x++)
            {
                for (int y = 0; y < 100; y++)
                {
                    var p = new Vector2(x, y);
                    var dist = Vector2.Distance(center, p);
                    var curvepoint = GetPointAtTimeOn2ndDegreePolynominalCurve(a, b, c, dist);

                    data[x + y * 100] = new Color(curvepoint.Y* 255, curvepoint.Y * 255, curvepoint.Y * 255, curvepoint.Y * 255);
                }
            }
            Texture2D tex = new Texture2D(device, 100, 100);
            tex.SetData<Color>(data);
            return tex;
        }

        public static Vector2 GetPointAtTimeOn2ndDegreePolynominalCurve(Vector2 A, Vector2 B, Vector2 C, float t)
        {
            float i = 1.0f - t;
            float plotX = 0; 
            float plotY = 0;
            plotX = (float)( A.X * 1 * (i * i) + B.X * 2 * (i * t) + C.X * 1 * (t * t) );
            plotY = (float)( A.Y * 1 * (i * i) + B.Y * 2 * (i * t) + C.Y * 1 * (t * t) );
            return new Vector2(plotX, plotY);
        }
        public float Power(float baseVal, float exponentVal)
        {
            float result = 0;
            for (float exponent = exponentVal; exponent > 0; exponent--)
            {
                result = result * baseVal;
            }
            return result;
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
            GraphicsDevice.Clear(Color.CornflowerBlue);


            effect.CurrentTechnique = effect.Techniques["Basic"];

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, effect, null);
            spriteBatch.Draw(texture, new Rectangle(0, 0, 300, 300), Color.White);
            spriteBatch.Draw(shadingMultiTexture, new Rectangle(0, 300, 300, 300), Color.White);
            spriteBatch.Draw(stenciledTexture, new Rectangle(300, 300, 300, 300), Color.White);
            spriteBatch.End();

            if (_useBlend)
                effect.CurrentTechnique = effect.Techniques["MaskAndBlend"];
            else
                effect.CurrentTechnique = effect.Techniques["MaskAndOverlay"];

            effect.Parameters["CycleTime"].SetValue(_elapsedCycle);
            effect.Parameters["CycleTime2"].SetValue(_elapsedCycle2);

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