
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_Into3Dwvp : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        SpriteFont font3;
        Texture2D texture;
        Effect effect;
        RenderTarget2D rtScene;
        MouseState mouse;

        QuadModel quad = new QuadModel();
        QuadModel quad2 = new QuadModel();

        Matrix view;
        Matrix projection;

        float fov = 1.4f;
        Matrix cameraWorld = Matrix.Identity;
        Vector3 cameraWorldPosition = new Vector3(0,0, -500f);
        Vector3 cameraForwardVector = Vector3.Forward;
        Vector3 cameraUpVector = Vector3.Down;

        Vector3 quadWorldPosition = Vector3.Zero;
        Vector3 quadUpVector = Vector3.Up;
        Vector3 quadForwardVector = Vector3.Forward;
        float quadRotation = 0;

        bool useOrtho = false;
        bool useFov = true;

        public Game1_Into3Dwvp()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Mimic Spritebatch test first ";
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
            SetProjection();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Content.RootDirectory = @"Content/Shaders3D";
            //effect = Content.Load<Effect>("SimpleDrawingWithMatriceEffect");

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");
            font3 = Content.Load<SpriteFont>("MgFont3");


            //if (useOrtho)
            //{
            //    MgMathExtras.CreateOrthographicViewSpriteBatchAligned(GraphicsDevice, Vector3.Zero, false, out cameraWorld, out projection);
            //    //cameraWorld = Matrix.CreateWorld(cameraWorldPosition, cameraForwardVector, cameraUpVector);
            //    cameraWorld = Matrix.CreateWorld(cameraWorldPosition, Vector3.Backward, Vector3.Down);
            //}
            //else
            //{
            //    MgMathExtras.CreatePerspectiveViewSpriteBatchAligned(GraphicsDevice, Vector3.Zero, fov, 1f, 10000f, out cameraWorld, out projection);
            //    //cameraWorld = Matrix.CreateWorld(cameraWorldPosition, Vector3.Backward, Vector3.Down);
            //}

            SetProjection();

            SimpleDrawingWithMatrixClassEffect.Load(Content);
            SimpleDrawingWithMatrixClassEffect.Technique = "TriangleDrawWithTransforms";
            SimpleDrawingWithMatrixClassEffect.SpriteTexture = texture;
            SimpleDrawingWithMatrixClassEffect.View = view;
            SimpleDrawingWithMatrixClassEffect.Projection = projection;

            quad.CreateQuad(GraphicsDevice.Viewport.Bounds, false);
            quad2.CreateQuad(GraphicsDevice.Viewport.Bounds, false);
        }

        protected override void UnloadContent()
        {
        }

        public void SetProjection()
        {
            if (useOrtho)
            {
                MgMathExtras.CreateOrthographicViewSpriteBatchAligned(GraphicsDevice, Vector3.Zero, false, out cameraWorld, out projection);
            }
            else
            {
                MgMathExtras.CreatePerspectiveViewSpriteBatchAligned(GraphicsDevice, Vector3.Zero, fov, 1f, 10000f, out cameraWorld, out projection);
            }

            //if (useOrtho)
            //{
            //    //projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 1, 10000f);

            //    projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, -GraphicsDevice.Viewport.Height, 0, 1f, 10000f);
            //    //MgMathExtras.CreateOrthographicViewSpriteBatchAligned(GraphicsDevice, Vector3.Zero, false, out cameraWorld, out projection);
            //}
            //else
            //{
            //    if (useFov)
            //    {
            //        //MgMathExtras.CreatePerspectiveViewSpriteBatchAligned(GraphicsDevice, Vector3.Zero, fov, 1f, 10000f, out cameraWorld, out projection);
            //        projection = MgMathExtras.CreateInfinitePerspectiveFieldOfViewRHLH(fov, GraphicsDevice.Viewport.AspectRatio, 1f, 10000f, true);

            //        //projection = Matrix.CreatePerspectiveFieldOfView(fov, GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height, 1f, 10000f);
            //    }
            //    //else
            //    //{
            //    //    cameraWorld = Matrix.CreateWorld(cameraWorldPosition, Vector3.Zero - cameraWorldPosition, Vector3.Up);
            //    //    view = Matrix.Invert(cameraWorld);
            //    //    projection = Matrix.CreatePerspectiveOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 1f, 10000f);
            //    //    //projection = Matrix.CreatePerspective(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 1f, 10000f);
            //    //    //projection = Matrix.CreateScale(1, -1, 1) * Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 10000);
            //    //}
            //}

            if (SimpleDrawingWithMatrixClassEffect.effect != null)
            {
                SimpleDrawingWithMatrixClassEffect.View = view;
                SimpleDrawingWithMatrixClassEffect.Projection = projection;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float speed = .1f;
            float speed2 = speed * .1f;

            if (Keyboard.GetState().IsKeyDown(Keys.Z))
                quadRotation += speed * .01f;
            if (Keyboard.GetState().IsKeyDown(Keys.C))
                quadRotation -= speed * .01f;
            if (quadRotation > 6.28) 
                quadRotation = 0;
            if (quadRotation < 0) 
                quadRotation = 6.28f;
            quadUpVector = new Vector3(MathF.Sin(quadRotation), MathF.Cos(quadRotation), 0);

            var t = cameraWorld.Translation;
            cameraWorld.Translation = Vector3.Zero;
            // 
            if (Keyboard.GetState().IsKeyDown(Keys.D))
                cameraWorld *= Matrix.CreateFromAxisAngle(cameraWorld.Up, speed2);
            if (Keyboard.GetState().IsKeyDown(Keys.A))
                cameraWorld *= Matrix.CreateFromAxisAngle(cameraWorld.Up, -speed2);
            if (Keyboard.GetState().IsKeyDown(Keys.S))
                cameraWorld *= Matrix.CreateFromAxisAngle(cameraWorld.Right, speed2);
            if (Keyboard.GetState().IsKeyDown(Keys.W))
                cameraWorld *= Matrix.CreateFromAxisAngle(cameraWorld.Right, -speed2);
            cameraWorld.Translation = t;

            // use the arrow keys to alter the camera lookat position.
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                cameraWorld.Translation += cameraWorld.Right * speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                cameraWorld.Translation += cameraWorld.Right * -speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                cameraWorld.Translation += cameraWorld.Up * speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                cameraWorld.Translation += cameraWorld.Up * -speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
                cameraWorld.Translation += cameraWorld.Forward * speed;
            if (Keyboard.GetState().IsKeyDown(Keys.E))
                cameraWorld.Translation += cameraWorld.Forward * -speed;

            // Set the view matrix.
            cameraWorld = Matrix.CreateWorld(cameraWorld.Translation, cameraWorld.Forward, cameraWorld.Up);
            view = Matrix.Invert(cameraWorld);

            if (Keys.D1.IsKeyPressedWithDelay(gameTime))
            {
                useOrtho = !useOrtho;
                SetProjection();
            }

            if (Keys.D2.IsKeyPressedWithDelay(gameTime))
            {
                useFov = !useFov;
                SetProjection();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Moccasin);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            SimpleDrawingWithMatrixClassEffect.View = view;
            SimpleDrawingWithMatrixClassEffect.Projection = projection;

            quad.OrientWorld(new Vector3(100, 100, 0), Vector3.Forward, Vector3.Up);
            quad.Draw(GraphicsDevice);

            quad2.OrientWorld(quadWorldPosition, Vector3.Forward, quadUpVector);
            quad2.Draw(GraphicsDevice);

            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
            // Draw all the regular stuff
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            string msg =
                $" \n useOrtho {useOrtho}" +
                $" \n useFov {useFov}" +
                $" \n { cameraWorld.DisplayMatrix("cameraWorld") }" +
                $" \n { view.DisplayMatrix("view") }" +
                $" \n { projection.DisplayMatrix("projection") }" +
                $" \n" +
                $" \n" +
                $" \n" +
                $" \n" 
                ;
            spriteBatch.DrawString(font, msg, new Vector2(10, 10), Color.Blue);
            spriteBatch.End();
        }
    }

    public class QuadModel
    {
        CustomVertexPositionNormalTexture[] vertices;
        int[] indices;
        public Matrix world = Matrix.Identity;

        public QuadModel()
        {
        }

        public void CreateQuad(Rectangle destination, bool flipWindingDirection)
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

        public void OrientWorld(Vector3 position, Vector3 forwardDirection, Vector3 up)
        {
            world = Matrix.CreateWorld(position, forwardDirection, up);
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            SimpleDrawingWithMatrixClassEffect.World = world;
            foreach (EffectPass pass in SimpleDrawingWithMatrixClassEffect.GetEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                int numberOfVertices = vertices.Length;
                int numberOfVerticesInaTriangle = 3;
                int numberOfTriangles = indices.Length / numberOfVerticesInaTriangle;
                int startingVerticeInArray = 0; // the reason for this offset is incase you put more then one mesh or grouping of vertices into the same array.
                int startingIndiceInArray = 0; // likewise for this they should line up together you have to keep track of that though.

                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, startingVerticeInArray, numberOfVertices, indices, startingIndiceInArray, numberOfTriangles, CustomVertexPositionNormalTexture.VertexDeclaration);
            }
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

    // Wrap up our effect.
    public static class SimpleDrawingWithMatrixClassEffect
    {
        public static Effect effect;

        public static void InfoForCreateMethods()
        {
            Console.WriteLine($"\n effect.Name: \n   {effect.Name} ");
            Console.WriteLine($"\n effect.Parameters: \n ");
            var pparams = effect.Parameters;
            foreach (var p in pparams)
            {
                Console.WriteLine($"   p.Name: {p.Name}  ");
            }
            Console.WriteLine($"\n effect.Techniques: \n ");
            var tparams = effect.Techniques;
            foreach (var t in tparams)
            {
                Console.WriteLine($"   t.Name: {t.Name}  ");
            }
        }
        public static void Load(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            Content.RootDirectory = @"Content/Shaders3D";
            effect = Content.Load<Effect>("SimpleDrawingWithMatriceEffect");
            effect.CurrentTechnique = effect.Techniques["TriangleDrawWithTransforms"];
        }
        public static Effect GetEffect
        {
            get { return effect; }
        }
        public static string Technique
        {
            set { effect.CurrentTechnique = effect.Techniques[value]; }
        }
        public static Texture2D SpriteTexture//(Texture2D value)
        {
            set { effect.Parameters["SpriteTexture"].SetValue(value); }
        }
        public static Matrix World
        {
            set { effect.Parameters["World"].SetValue(value); }
        }
        public static Matrix View
        {
            set { effect.Parameters["View"].SetValue(value); }
        }
        public static Matrix Projection
        {
            set { effect.Parameters["Projection"].SetValue(value); }
        }
    }


}




//public class Primitive2dQuadBuffer
//{
//    // ccw winding.

//    List<VertexPositionNormalTexture> verticeList = new List<VertexPositionNormalTexture>();
//    VertexPositionNormalTexture[] vertices;
//    public void AddVertexRectangleToBuffer(GraphicsDevice gd, Rectangle r, float depth)
//    {
//        var normal = Vector3.Normalize(new Vector3(0, 0, depth));
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Top, depth) , normal, new Vector2(0f, 0f))); ;  // p1
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Bottom, depth) , normal, new Vector2(0f, 1f))); // p0
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Bottom, depth) , normal, new Vector2(1f, 1f)));// p3

//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Bottom, depth) , normal, new Vector2(1f, 1f)));// p3
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Top, depth) , normal, new Vector2(1f, 0f)));// p2
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Top, depth) , normal, new Vector2(0f, 0f))); // p1

//        vertices = verticeList.ToArray();
//    }

//    public void AlterVertexRectanglePositionInBuffer(GraphicsDevice gd, int index, Rectangle r, float depth)
//    {
//        // Triangle 1
//        vertices[index + 0].Position = new Vector3(r.Left, r.Top, depth);  // p1
//        vertices[index + 1].Position = new Vector3(r.Left, r.Bottom, depth); // p0
//        vertices[index + 2].Position = new Vector3(r.Right, r.Bottom, depth); // p3
//        // Triangle 2
//        vertices[index + 3].Position = new Vector3(r.Right, r.Bottom, depth);// p3
//        vertices[index + 4].Position = new Vector3(r.Right, r.Top, depth); // p2
//        vertices[index + 5].Position = new Vector3(r.Left, r.Top, depth); // p1
//    }

//    public void DrawQuadBuffer(GraphicsDevice device, Effect effect)
//    {
//        if (vertices != null)
//        {
//            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
//            {
//                pass.Apply();
//                int numberOfTriangles = vertices.Length / 3;
//                device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, numberOfTriangles);
//            }
//        }
//    }
//    public void DrawQuadRangeInBuffer(GraphicsDevice device, Effect effect, int startQuad, int quadDrawLength)
//    {
//        int startVertice = startQuad * 2 * 3;
//        int numberOfTriangles = quadDrawLength * 2;
//        if (vertices != null)
//        {
//            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
//            {
//                pass.Apply();
//                device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, startVertice, numberOfTriangles);
//            }
//        }
//    }
//}

//public class Primitive2dIndexedQuadBuffer
//{
//    // ccw winding.

//    List<VertexPositionNormalTexture> verticeList = new List<VertexPositionNormalTexture>();
//    List<int> indiceList = new List<int>();
//    VertexPositionNormalTexture[] vertices;
//    int[] indices;
//    public void AddVertexRectangleToBuffer(GraphicsDevice gd, Rectangle r, float depth)
//    {
//        int currentCount = verticeList.Count;
//        var normal = Vector3.Normalize(new Vector3(0, 0, depth));
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Top, depth), normal, new Vector2(0f, 0f)));  // p1
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Bottom, depth), normal, new Vector2(0f, 1f))); // p0
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Bottom, depth), normal, new Vector2(1f, 1f)));// p3
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Top, depth), normal, new Vector2(1f, 0f)));// p4
//        vertices = verticeList.ToArray();

//        // triangle 0 indices.
//        indiceList.Add(currentCount + 0);
//        indiceList.Add(currentCount + 1);
//        indiceList.Add(currentCount + 3);
//        // triangle 1 indices.
//        indiceList.Add(currentCount + 1);
//        indiceList.Add(currentCount + 2);
//        indiceList.Add(currentCount + 3);
//        indices = indiceList.ToArray();
//    }

//    public void AlterVertexRectanglePositionInBuffer(GraphicsDevice gd, int index, Rectangle r, float depth)
//    {
//        vertices[index + 0].Position = new Vector3(r.Left, r.Top, depth);  // p1
//        vertices[index + 1].Position = new Vector3(r.Left, r.Bottom, depth); // p0
//        vertices[index + 2].Position = new Vector3(r.Right, r.Bottom, depth); // p3
//        vertices[index + 4].Position = new Vector3(r.Right, r.Top, depth); // p2
//    }

//    public void DrawQuadBuffer(GraphicsDevice device, Effect effect)
//    {
//        if (vertices != null)
//        {
//            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
//            {
//                pass.Apply();
//                int numberOfTriangles = vertices.Length / 3;
//                device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, numberOfTriangles);
//            }
//        }
//    }
//    public void DrawQuadRangeInBuffer(GraphicsDevice device, Effect effect, int startQuad, int quadDrawLength)
//    {
//        int startVertice = startQuad * 2 * 3;
//        int numberOfTriangles = quadDrawLength * 2;
//        if (vertices != null)
//        {
//            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
//            {
//                pass.Apply();
//                device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, startVertice, numberOfTriangles);
//            }
//        }
//    }
//}



/*
        public void DrawTriangleDirectlyToGpu(Effect currenteffect)
        {
            //GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;  // counter clockwise is the default.
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            currenteffect.CurrentTechnique = currenteffect.Techniques["TriangleDrawWithTransforms"];
            currenteffect.Parameters["SpriteTexture"].SetValue(texture);

            var identity = new Matrix
            (
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );

            //var world = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);  // identity.
            //var view = Matrix.CreateLookAt(new Vector3(0, 0, 0), Vector3.Forward, Vector3.Up);  // identity.

            var world = identity;
            var view = identity;
            var projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 100f);

            currenteffect.Parameters["World"].SetValue(world);
            currenteffect.Parameters["View"].SetValue(view);
            currenteffect.Parameters["Projection"].SetValue(projection);

            foreach (EffectPass pass in currenteffect.CurrentTechnique.Passes)
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

        public void DrawSpriteBatches(GameTime gameTime)
        {
            // Draw all the regular stuff
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.Draw(texture, new Rectangle(25, 50, 200, 200), Color.White);
            spriteBatch.DrawString(font, $" Because we are in Dx matrix identity for world is a bit off so we change M22 to be negative typically. \n However this time i have directly lined up the vertex positions and texture coordinates so they can directly be sent to the shader..", new Vector2(10, 10), Color.Blue);
            spriteBatch.End();
        }
 */
