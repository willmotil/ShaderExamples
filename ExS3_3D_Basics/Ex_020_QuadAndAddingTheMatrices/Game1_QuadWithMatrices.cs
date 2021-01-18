
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_QuadWithMatrices : Game
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


        Matrix view;
        Matrix projection;

        public Game1_QuadWithMatrices()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Quad with shader and Matrices ";
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
            projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 100f);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = @"Content/Shaders3D";
            effect = Content.Load<Effect>("SimpleDrawingWithMatriceEffect");


            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");


            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");
            font3 = Content.Load<SpriteFont>("MgFont3");


            projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 100f);

            CreateQuadAkaTwoTriangles(GraphicsDevice.Viewport.Bounds, false);
        }


        public void CreateQuadAkaTwoTriangles(Rectangle destination, bool flipWindingDirection)
        {
            vertices = new VertexPositionNormalTexture[4];
            indices = new int[6];

            var normal = Vector3.Forward; //  this is just a dummy value for now.

            var left = destination.Left;
            var right = destination.Right;
            var top = destination.Top;
            var bottom = destination.Bottom;
            vertices[0] = new VertexPositionNormalTexture(new Vector3(left, top, 0), normal, new Vector2(0f, 0f)); // tl
            vertices[1] = new VertexPositionNormalTexture(new Vector3(left, bottom, 0), normal, new Vector2(0f, 1f)); // bl
            vertices[2] = new VertexPositionNormalTexture(new Vector3(right, bottom, 0), normal, new Vector2(1f, 1f)); // br
            vertices[3] = new VertexPositionNormalTexture(new Vector3(right, top, 0), normal, new Vector2(1f, 0f)); // tr


            if (flipWindingDirection)
            {
                // triangle 1
                indices[0] = 0;
                indices[1] = 1;
                indices[2] = 2;
                // triangle 2
                indices[3] = 0;
                indices[4] = 2;
                indices[5] = 3;
            }
            else
            {
                // triangle 1
                indices[0] = 0;
                indices[1] = 2;
                indices[2] = 1;
                // triangle 2
                indices[3] = 0;
                indices[4] = 3;
                indices[5] = 2;
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
            GraphicsDevice.Clear(new Color(20,20,20,255));
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            //GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;  // counter clockwise is the default.

            DrawRectangleTriangles();


            DrawSpriteBatchStuff(gameTime);

            base.Draw(gameTime);
        }

        public void DrawRectangleTriangles()
        {
            var world = Matrix.Identity;
            view = Matrix.Identity;

            effect.CurrentTechnique = effect.Techniques["TriangleDrawWithTransforms"];
            effect.Parameters["SpriteTexture"].SetValue(texture);

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                int numberOfVertices = vertices.Length;
                int numberOfVerticesInaTriangle = 3;
                int numberOfTriangles = indices.Length / numberOfVerticesInaTriangle;
                int startingVerticeInArray = 0; // the reason for this offset is incase you put more then one mesh or grouping of vertices into the same array.
                int startingIndiceInArray = 0; // likewise for this they should line up together you have to keep track of that though.

                GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, startingVerticeInArray, numberOfVertices, indices, startingIndiceInArray, numberOfTriangles, VertexPositionNormalTexture.VertexDeclaration);
            }
        }

        public void DrawSpriteBatchStuff(GameTime gameTime)
        {
            // Draw all the regular stuff
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.Draw(texture, new Rectangle(25, 50, 200, 200), Color.White);

            string msg = 
                $" In this example we are drawing two triangles which forms a quad or sprite" +
                $" \n We pass the world view and projection matrices to the shader. " +
                $" \n These alter the vertices we pass in with our draw." +
                $" \n However identity matrices have no affect only our projection is." +
                $" \n " +
                $" \n The world and view is set to identity then passed to the shader." +
                $" \n Identity is a matrix with 1's running down diagnally from top left to bottom right" +
                $" \n The rest of the elements are zero which has no affect on vertices." +
                $"\n" +
                $" \n Our shader uses the Position Normal Textures via VertexShaderInput" +
                $" \n That is just like we use for our vertices here when we declare VertexPositionNormalTexture." +
                $" \n The VertexPositionNormalTexture is a vertex structure built into monogame. " +
                $" \n However... well be mimicing it in a bit because later on. " +
                $" \n Well make our own vertex declarations that can use even more data " +
                $" \n for more complex shaders that handle lighting normal maps and other things."
                ;
            spriteBatch.DrawString(font2, msg, new Vector2(10, 10), Color.Moccasin);
            spriteBatch.End();
        }
    }
}