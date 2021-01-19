
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_ManipulatingTheMatrices : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        SpriteFont font3;
        Texture2D texture;
        Effect effect;
        RenderTarget2D rtScene;

        CustomVertexPositionNormalTexture[] vertices;
        int[] indices;


        Matrix view;
        Matrix projection;

        Vector3 cameraWorldPosition = Vector3.Zero;
        Vector3 quadWorldPosition = Vector3.Zero;
        Vector3 quadUpVector = Vector3.Up;
        float quadRotation = 0;


        public Game1_ManipulatingTheMatrices()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Manipulating the Matrices ";
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
            vertices = new CustomVertexPositionNormalTexture[4];
            indices = new int[6];

            var normal = Vector3.Forward; //  this is just a dummy value for now.

            var left = destination.Left;
            var right = destination.Right;
            var top = destination.Top;
            var bottom = destination.Bottom;
            vertices[0] = new CustomVertexPositionNormalTexture(new Vector3(left, top, 0), normal, new Vector2(0f, 0f)); // tl
            vertices[1] = new CustomVertexPositionNormalTexture(new Vector3(left, bottom, 0), normal, new Vector2(0f, 1f)); // bl
            vertices[2] = new CustomVertexPositionNormalTexture(new Vector3(right, bottom, 0), normal, new Vector2(1f, 1f)); // br
            vertices[3] = new CustomVertexPositionNormalTexture(new Vector3(right, top, 0), normal, new Vector2(1f, 0f)); // tr


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

            float speed = 1f;

            // use the wasd to alter the quads position.
            if (Keyboard.GetState().IsKeyDown(Keys.D))
                quadWorldPosition.X += speed;
            if (Keyboard.GetState().IsKeyDown(Keys.A))
                quadWorldPosition.X -= speed;
            if (Keyboard.GetState().IsKeyDown(Keys.S))
                quadWorldPosition.Y += speed;
            if (Keyboard.GetState().IsKeyDown(Keys.W))
                quadWorldPosition.Y -= speed;

            if (Keyboard.GetState().IsKeyDown(Keys.Z))
                quadRotation += speed * .01f;
            if (Keyboard.GetState().IsKeyDown(Keys.C))
                quadRotation -= speed * .01f;

            if (quadRotation > 6.28)
                quadRotation = 0;
            if (quadRotation < 0)
                quadRotation = 6.28f;


            quadUpVector = new Vector3(MathF.Sin(quadRotation), MathF.Cos(quadRotation), 0);


            // use the arrow keys to alter the camera lookat position
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                cameraWorldPosition.X += speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                cameraWorldPosition.X -= speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                cameraWorldPosition.Y += speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                cameraWorldPosition.Y -= speed;

            // Set the view matrix.

            view = Matrix.CreateLookAt(cameraWorldPosition, Vector3.Forward + cameraWorldPosition, Vector3.Up);  // identity.


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Moccasin);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            //GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;  // counter clockwise is the default.



            DrawRectangleTriangles(new Vector3(100, 100, 0), Vector3.Forward, Vector3.Up);

            DrawRectangleTriangles(quadWorldPosition, Vector3.Forward, quadUpVector);



            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        public void DrawRectangleTriangles(Vector3 position, Vector3 forwardDirection, Vector3 up)
        {
            effect.CurrentTechnique = effect.Techniques["TriangleDrawWithTransforms"];
            effect.Parameters["SpriteTexture"].SetValue(texture);

            var world = Matrix.CreateWorld(position, forwardDirection, up);

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

                GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, startingVerticeInArray, numberOfVertices, indices, startingIndiceInArray, numberOfTriangles, CustomVertexPositionNormalTexture.VertexDeclaration);
            }
        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
            // Draw all the regular stuff
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            string msg =
                $" We use the WASD keys to move our quad this is different from spritebatch which creates a quad with a rectangle and places it." +
                $" \n So we will draw another quad at a specific position that wont change." +
                $" \n" +
                $" \n Further well use the ARROW keys to change the cameras position. {cameraWorldPosition}" +
                $" \n The projection matrix is only set up once typically and may change if the user resizes the window." +
                $" \n The view is typically updated one time per frame its based on the camera orientation in the world." +
                $" \n The world matrix applies to (or belongs to) each quad or each model or game object at the time it is drawn. " +
                $" \n Typically a game object or model keeps its own world matrix. " +
                $" \n Here the position is only used and the world matrix is re-made." +
                $" \n " +
                $" \n Now so far these transformations have been restricted to be illustrative." +
                $" \n From here on well start encapsulating these primitives and their matrices and use them fully."
                ;
            spriteBatch.DrawString(font, msg, new Vector2(10, 10), Color.Blue);
            spriteBatch.End();
        }



        /// <summary>
        /// This will be a replacement to the monogame version VertexPositionNormalTexture.
        /// Later will make versions that take more data and or do more stuff.
        /// </summary>
        public struct CustomVertexPositionNormalTexture : IVertexType
        {
            // class members
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TextureCoordinate;

            // constructor
            public CustomVertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 uvcoordinates)
            {
                Position = position;
                Normal = normal;
                TextureCoordinate = uvcoordinates;
            }

            // static vertex declaration
            public static VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                  //
                  // Note these line up with the shader structs.
                  // The offset is the starting byte aka the second element starts were the first element ends in bytes 4+4+4 = 12 a vector3 is 12 bytes
                  //
                  new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),                //    float4 Position : POSITION0;
                  new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),                //  	float4 Normal : NORMAL0;
                  new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0) //	float2 TextureCoordinates : TEXCOORD0;
            );
            VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
        }

    }
}