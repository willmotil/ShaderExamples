using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_TriangleToGpuClipSpace : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        SpriteFont font3;
        Texture2D texture;
        Effect effect;
        RenderTarget2D rtScene;


        VertexPositionNormalTexture[] vertices;
        int[] indices;


        public Game1_TriangleToGpuClipSpace()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Triangle Quad and Vertex structures  SimpleDirectDrawEffect.fx";
            IsMouseVisible = true;
            Window.ClientSizeChanged += OnResize;
        }
        protected override void Initialize()
        {
            base.Initialize();
        }

        public void OnResize(object sender, EventArgs e)
        {
            rtScene = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = @"Content/Shaders3D";
            effect = Content.Load<Effect>("SimpleDirectDrawEffect");


            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");


            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");
            font3 = Content.Load<SpriteFont>("MgFont3");

            rtScene = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);


            CreateDirectlyBlitableTriangle(false);
        }

        public void CreateDirectlyBlitableTriangle( bool windForClockwiseCulling)
        {
            vertices = new VertexPositionNormalTexture[3];
            indices = new int[3];

            var normal = Vector3.Forward; //  this is just a dummy value for now.

            vertices[0] = new VertexPositionNormalTexture(new Vector3( 0,   1, 0), normal, new Vector2(.5f, 0f));  
            vertices[1] = new VertexPositionNormalTexture(new Vector3(-1,  -1, 0), normal, new Vector2( 0f, 1f));
            vertices[2] = new VertexPositionNormalTexture(new Vector3( 1,  -1, 0), normal, new Vector2( 1f, 1f));

            if (windForClockwiseCulling)
            {
                indices[0] = 0;
                indices[1] = 1;
                indices[2] = 2;
            }
            else
            {
                indices[0] = 0;
                indices[1] = 2;
                indices[2] = 1;
            }
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Moccasin);

            DrawTriangleDirectlyToGpu(effect);

            base.Draw(gameTime);
        }

        public void DrawTriangleDirectlyToGpu(Effect effect)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            effect.CurrentTechnique = effect.Techniques["SimpleTriangleDraw"];
            effect.Parameters["SpriteTexture"].SetValue(texture);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    int numberOfTriangles = indices.Length / 3;
                    GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 3, indices, 0, numberOfTriangles);
                }
        }
    }
}
