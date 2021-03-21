﻿
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_IndexedMesh : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        SpriteFont font3;
        Texture2D texture;
        Texture2D dotTexture;
        Effect effect;
        RenderTarget2D rtScene;
        MouseState mouse;

        //QuadModel quad = new QuadModel();
        PrimitiveIndexedMesh mesh;

        Matrix view;
        Matrix projection;

        float fov = 1.4f;
        Matrix cameraWorld = Matrix.Identity;
        Vector3 cameraWorldPosition = new Vector3(0, 0, -500f);
        Vector3 cameraForwardVector = Vector3.Forward;
        Vector3 cameraUpVector = Vector3.Down;

        Vector3 quadWorldPosition = Vector3.Zero;
        Vector3 quadUpVector = Vector3.Up;
        Vector3 quadForwardVector = Vector3.Forward;
        float quadRotation = 0;

        public Game1_IndexedMesh()
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
            UpdateProjection();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Content.RootDirectory = @"Content/Shaders3D";
            //effect = Content.Load<Effect>("SimpleDrawingWithMatriceEffect");

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");
            dotTexture = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Green);

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");
            font3 = Content.Load<SpriteFont>("MgFont3");

            InitialView();
            UpdateProjection();

            SimpleDrawingWithMatrixClassEffect.Load(Content);
            SimpleDrawingWithMatrixClassEffect.Technique = "TriangleDrawWithTransforms";
            SimpleDrawingWithMatrixClassEffect.SpriteTexture = texture;
            SimpleDrawingWithMatrixClassEffect.View = view;
            SimpleDrawingWithMatrixClassEffect.Projection = projection;

            //quad.CreateQuad(GraphicsDevice.Viewport.Bounds, 1f, false, true);

            PrimitiveIndexedMesh.showOutput = true;
            mesh = new PrimitiveIndexedMesh(4,4, 300f, true, false, true);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float speed = 1f;
            float speed2 = speed * .01f;

            // Use the arrow keys to alter the camera position.
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                cameraWorld.Translation += cameraWorld.Right * speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                cameraWorld.Translation += cameraWorld.Right * -speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                cameraWorld.Translation += cameraWorld.Up * speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                cameraWorld.Translation += cameraWorld.Up * -speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
                cameraWorld.Translation += cameraWorld.Forward * speed;
            if (Keyboard.GetState().IsKeyDown(Keys.E))
                cameraWorld.Translation += cameraWorld.Forward * -speed;

            // Use wasd to alter the lookat direction.
            var t = cameraWorld.Translation;
            cameraWorld.Translation = Vector3.Zero;
            // 
            if (Keyboard.GetState().IsKeyDown(Keys.D))
                cameraWorld *= Matrix.CreateFromAxisAngle(cameraWorld.Up, -speed2);
            if (Keyboard.GetState().IsKeyDown(Keys.A))
                cameraWorld *= Matrix.CreateFromAxisAngle(cameraWorld.Up, speed2);
            if (Keyboard.GetState().IsKeyDown(Keys.S))
                cameraWorld *= Matrix.CreateFromAxisAngle(cameraWorld.Right, -speed2);
            if (Keyboard.GetState().IsKeyDown(Keys.W))
                cameraWorld *= Matrix.CreateFromAxisAngle(cameraWorld.Right, speed2);
            cameraWorld.Translation = t;

            // Use the Z and C keys to rotate the camera.
            if (Keyboard.GetState().IsKeyDown(Keys.Z))
                quadRotation += speed * .01f;
            if (Keyboard.GetState().IsKeyDown(Keys.C))
                quadRotation -= speed * .01f;
            if (quadRotation > 6.28)
                quadRotation = 0;
            if (quadRotation < 0)
                quadRotation = 6.28f;
            quadUpVector = new Vector3(MathF.Sin(quadRotation), MathF.Cos(quadRotation), 0);

            // Set the view matrix.
            cameraWorld = Matrix.CreateWorld(cameraWorld.Translation, cameraWorld.Forward, cameraWorld.Up);
            view = Matrix.Invert(cameraWorld);

            // Reset the view projection matrix.
            if (Keys.F1.IsKeyPressedWithDelay(gameTime))
                InitialView();

            base.Update(gameTime);
        }

        public void InitialView()
        {
            cameraWorldPosition.Z = MgMathExtras.GetRequisitePerspectiveSpriteBatchAlignmentZdistance(GraphicsDevice, fov);
            cameraWorld = Matrix.CreateWorld(cameraWorldPosition, Vector3.Zero - cameraWorldPosition, cameraUpVector);
            view = Matrix.Invert(cameraWorld);
            if (SimpleDrawingWithMatrixClassEffect.effect != null)
                SimpleDrawingWithMatrixClassEffect.View = view;
        }
        public void UpdateProjection()
        {
            projection = Matrix.CreatePerspectiveFieldOfView(fov, GraphicsDevice.Viewport.AspectRatio, 1f, 10000f);
            if (SimpleDrawingWithMatrixClassEffect.effect != null)
                SimpleDrawingWithMatrixClassEffect.Projection = projection;
        }

        RasterizerState rasterizerState_CULLNONE_WIREFRAME = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        RasterizerState rasterizerState_CULLNONE_SOLID = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.Solid };

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Moccasin);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;

            SimpleDrawingWithMatrixClassEffect.View = view;
            SimpleDrawingWithMatrixClassEffect.Projection = projection;
            SimpleDrawingWithMatrixClassEffect.World = Matrix.Identity;

            // draw regularly
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            SimpleDrawingWithMatrixClassEffect.SpriteTexture = texture;
            mesh.DrawPrimitive(GraphicsDevice, SimpleDrawingWithMatrixClassEffect.effect);

            // draw the wireframe
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_WIREFRAME;
            SimpleDrawingWithMatrixClassEffect.SpriteTexture = dotTexture;
            mesh.DrawPrimitive(GraphicsDevice, SimpleDrawingWithMatrixClassEffect.effect);

            //quad.OrientWorld(quadWorldPosition, Vector3.Forward, quadUpVector);
            //quad.Draw(GraphicsDevice);

            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
            // Draw all the regular stuff
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            string msg =
                $" \n The camera exists as a world matrix that holds a position and orientation." +
                $" \n The keys WASD change the forward view direction (which is the major take away here). ZC allows for spin." +
                $" \n The Arrows move the camera translation as strafing motion. " +
                $" \n  " +
                $" \n In this example we draw a extremely simple index mesh and map a texture onto it." +
                $" \n We then draw the mesh with a rasterizerstate we have created to turn on wireframe with a dot texture." +
                $" \n " +
                $" \n " +
                $" \n { cameraWorld.DisplayMatrix("cameraWorld") }" +
                $" \n { view.DisplayMatrix("view") }" +
                $" \n { projection.DisplayMatrix("projection") }" +
                $" \n" +
                $" \n"
                ;
            spriteBatch.DrawString(font, msg, new Vector2(10, 10), Color.Blue);
            spriteBatch.End();
        }

        // Wrap up our effect.
        public class SimpleDrawingWithMatrixClassEffect
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
                World = Matrix.Identity;
                View = Matrix.Identity;
                Projection = Matrix.CreatePerspectiveFieldOfView(1, 1.33f, 1f, 10000f); // just something default;
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




        public class PrimitiveIndexedMesh
        {
            public static bool showOutput = false;

            public VertexPositionNormalTexture[] vertices;
            public int[] indices;

            public PrimitiveIndexedMesh()
            {
                CreatePrimitiveMesh(2, 2, 1f, false, true, true, 0);
            }

            public PrimitiveIndexedMesh(int subdivisionWidth, int subdividsionHeight, float scale, bool clockwise, bool invertNormals, bool directionalFaces)
            {
                CreatePrimitiveMesh(subdivisionWidth, subdividsionHeight, scale, clockwise, invertNormals, directionalFaces, 0);
            }

            public void CreatePrimitiveMesh(int subdivisionWidth, int subdividsionHeight, float scale, bool clockwise, bool invertNormals, bool directionalFaces, int faceIndex)
            {
                List<VertexPositionNormalTexture> cubesFaceMeshLists = new List<VertexPositionNormalTexture>();
                List<int> cubeFaceMeshIndexLists = new List<int>();

                if (subdivisionWidth < 2)
                    subdivisionWidth = 2;
                if (subdividsionHeight < 2)
                    subdividsionHeight = 2;

                float depth = 0;

                float left = -1f;
                float right = +1f;
                float top = -1f;
                float bottom = +1f;

                int vertCounter = 0;
                for (int y = 0; y < subdividsionHeight; y++)
                {
                    float stepV = (float)(y) / (float)(subdividsionHeight - 1);
                    for (int x = 0; x < subdivisionWidth; x++)
                    {
                        float stepU = (float)(x) / (float)(subdivisionWidth - 1);

                        float X = Interpolate(left, right, stepU);
                        float Y = Interpolate(top, bottom, stepV);

                        var p0 = new Vector3(X, Y, depth) * scale;
                        var uv0 = new Vector2(stepU, stepV);
                        var vert = GetVertice(p0, faceIndex, directionalFaces, depth, uv0);

                        if (showOutput)
                            System.Console.WriteLine("vert["+ vertCounter + "]: " + vert);

                        cubesFaceMeshLists.Add(vert);
                        vertCounter += 1;
                    }
                }

                for (int y = 0; y < subdividsionHeight - 1; y++)
                {
                    for (int x = 0; x < subdivisionWidth - 1; x++)
                    {
                        var stride = subdivisionWidth;
                        var faceVerticeOffset = stride * y + x;
                        var tl = faceVerticeOffset;
                        var bl = faceVerticeOffset + stride;
                        var br = faceVerticeOffset + stride + 1;
                        var tr = faceVerticeOffset + 1;

                        cubeFaceMeshIndexLists.Add(tl);
                        cubeFaceMeshIndexLists.Add(bl);
                        cubeFaceMeshIndexLists.Add(br);

                        cubeFaceMeshIndexLists.Add(br);
                        cubeFaceMeshIndexLists.Add(tr);
                        cubeFaceMeshIndexLists.Add(tl);

                        if (showOutput)
                        {
                            System.Console.WriteLine();
                            System.Console.WriteLine("t0  face" + faceIndex + " cubeFaceMeshIndexLists [" + tl + "] " + "  vert " + cubesFaceMeshLists[tl]);
                            System.Console.WriteLine("t0  face" + faceIndex + " cubeFaceMeshIndexLists [" + bl + "] " + "  vert " + cubesFaceMeshLists[bl]);
                            System.Console.WriteLine("t0  face" + faceIndex + " cubeFaceMeshIndexLists [" + br + "] " + "  vert " + cubesFaceMeshLists[br]);

                            System.Console.WriteLine();
                            System.Console.WriteLine("t1  face" + faceIndex + " cubeFaceMeshIndexLists [" + br + "] " + "  vert " + cubesFaceMeshLists[br]);
                            System.Console.WriteLine("t1  face" + faceIndex + " cubeFaceMeshIndexLists [" + tr + "] " + "  vert " + cubesFaceMeshLists[tr]);
                            System.Console.WriteLine("t1  face" + faceIndex + " cubeFaceMeshIndexLists [" + tl + "] " + "  vert " + cubesFaceMeshLists[tl]);
                        }
                    }
                }
                vertices = cubesFaceMeshLists.ToArray();
                indices = cubeFaceMeshIndexLists.ToArray();
            }

            private float Interpolate(float A, float B, float t)
            {
                return ((B - A) * t) + A;
            }


            // TODO we should generate smooth normals here if flat faces is false.
            private VertexPositionNormalTexture GetVertice(Vector3 v, int faceIndex, bool directionalFaces, float depth, Vector2 uv)
            {
                return new VertexPositionNormalTexture(v, new Vector3(0, 0, 1) , uv);
            }

            public void DrawPrimitive(GraphicsDevice gd, Effect effect)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3, VertexPositionNormalTexture.VertexDeclaration);
                }
            }
        }


        //public class QuadModel
        //{
        //    CustomVertexPositionNormalTexture[] vertices;
        //    int[] indices;
        //    public Matrix world = Matrix.Identity;

        //    public QuadModel()
        //    {
        //    }

        //    public void CreateQuad(Rectangle destination, float scaleAdjustment, bool flipWindingDirection, bool center)
        //    {
        //        vertices = new CustomVertexPositionNormalTexture[4];
        //        indices = new int[6];

        //        var normal = Vector3.Forward; //  this is just a dummy value for now.
        //        var scale = scaleAdjustment;

        //        var origin = Vector2.Zero;
        //        if (center)
        //            origin = destination.Center.ToVector2() * scale;

        //        var left = destination.Left * scale - origin.X;
        //        var right = destination.Right * scale - origin.X;
        //        var top = destination.Top * scale - origin.Y;
        //        var bottom = destination.Bottom * scale - origin.Y;
        //        vertices[0] = new CustomVertexPositionNormalTexture(new Vector3(left, top, 0), normal, new Vector2(0f, 0f)); // tl
        //        vertices[1] = new CustomVertexPositionNormalTexture(new Vector3(left, bottom, 0), normal, new Vector2(0f, 1f)); // bl
        //        vertices[2] = new CustomVertexPositionNormalTexture(new Vector3(right, bottom, 0), normal, new Vector2(1f, 1f)); // br
        //        vertices[3] = new CustomVertexPositionNormalTexture(new Vector3(right, top, 0), normal, new Vector2(1f, 0f)); // tr

        //        if (flipWindingDirection)
        //        {
        //            // triangle 1
        //            indices[0] = 0;
        //            indices[1] = 1;
        //            indices[2] = 2;
        //            // triangle 2
        //            indices[3] = 0;
        //            indices[4] = 2;
        //            indices[5] = 3;
        //        }
        //        else
        //        {
        //            // triangle 1
        //            indices[0] = 0;
        //            indices[1] = 2;
        //            indices[2] = 1;
        //            // triangle 2
        //            indices[3] = 0;
        //            indices[4] = 3;
        //            indices[5] = 2;
        //        }
        //    }

        //    public void OrientWorld(Vector3 position, Vector3 forwardDirection, Vector3 up)
        //    {
        //        world = Matrix.CreateWorld(position, forwardDirection, up);
        //    }

        //    public void Draw(GraphicsDevice graphicsDevice)
        //    {
        //        SimpleDrawingWithMatrixClassEffect.World = world;
        //        foreach (EffectPass pass in SimpleDrawingWithMatrixClassEffect.GetEffect.CurrentTechnique.Passes)
        //        {
        //            pass.Apply();

        //            int numberOfVertices = vertices.Length;
        //            int numberOfVerticesInaTriangle = 3;
        //            int numberOfTriangles = indices.Length / numberOfVerticesInaTriangle;
        //            int startingVerticeInArray = 0; // the reason for this offset is incase you put more then one mesh or grouping of vertices into the same array.
        //            int startingIndiceInArray = 0; // likewise for this they should line up together you have to keep track of that though.

        //            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, startingVerticeInArray, numberOfVertices, indices, startingIndiceInArray, numberOfTriangles, CustomVertexPositionNormalTexture.VertexDeclaration);
        //        }
        //    }

        //    /// <summary>
        //    /// This will be a replacement to the monogame version VertexPositionNormalTexture.
        //    /// Later will make versions that take more data and or do more stuff.
        //    /// </summary>
        //    public struct CustomVertexPositionNormalTexture : IVertexType
        //    {
        //        // class members
        //        public Vector3 Position;
        //        public Vector3 Normal;
        //        public Vector2 TextureCoordinate;

        //        // constructor
        //        public CustomVertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 uvcoordinates)
        //        {
        //            Position = position;
        //            Normal = normal;
        //            TextureCoordinate = uvcoordinates;
        //        }

        //        // static vertex declaration
        //        public static VertexDeclaration VertexDeclaration = new VertexDeclaration
        //        (
        //              //
        //              // Note these line up with the shader structs.
        //              // The offset is the starting byte aka the second element starts were the first element ends in bytes 4+4+4 = 12 a vector3 is 12 bytes
        //              //
        //              new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),                //    float4 Position : POSITION0;
        //              new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),                //  	float4 Normal : NORMAL0;
        //              new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0) //	float2 TextureCoordinates : TEXCOORD0;
        //        );
        //        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
        //    }
        //}
    }
}
