
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
        //Effect effect;
        //MouseState mouse;
        RenderTarget2D rtScene;
        PrototypePrimitiveIndexedMesh mesh;
        CameraAndKeyboardControls cam = new CameraAndKeyboardControls();


        public Game1_IndexedMesh()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Indexed Mesh ";
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
            cam.UpdateProjection(GraphicsDevice);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");
            dotTexture = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Green);

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");
            font3 = Content.Load<SpriteFont>("MgFont3");

            cam.InitialView(GraphicsDevice, new Vector3(89.500f, +157.642f, -381.373f), -new Vector3(0.000f, +0.165f, -0.986f), Vector3.Down);
            cam.UpdateProjection(GraphicsDevice);

            SimpleDrawingWithMatrixClassEffect.Load(Content);
            SimpleDrawingWithMatrixClassEffect.Technique = "TriangleDrawWithTransforms";
            SimpleDrawingWithMatrixClassEffect.SpriteTexture = texture;
            SimpleDrawingWithMatrixClassEffect.View = cam.view;
            SimpleDrawingWithMatrixClassEffect.Projection = cam.projection;

            PrototypePrimitiveIndexedMesh.showOutput = true;
            mesh = new PrototypePrimitiveIndexedMesh(4,4, 300f, true, false, true);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            cam.Update(gameTime);

            base.Update(gameTime);
        }



        RasterizerState rasterizerState_CULLNONE_WIREFRAME = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        RasterizerState rasterizerState_CULLNONE_SOLID = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.Solid };

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Moccasin);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;

            SimpleDrawingWithMatrixClassEffect.View = cam.view;
            SimpleDrawingWithMatrixClassEffect.Projection = cam.projection;
            SimpleDrawingWithMatrixClassEffect.World = Matrix.Identity;

            // draw regularly
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            SimpleDrawingWithMatrixClassEffect.SpriteTexture = texture;
            mesh.DrawPrimitive(GraphicsDevice, SimpleDrawingWithMatrixClassEffect.effect);

            // draw the wireframe
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_WIREFRAME;
            SimpleDrawingWithMatrixClassEffect.SpriteTexture = dotTexture;
            mesh.DrawPrimitive(GraphicsDevice, SimpleDrawingWithMatrixClassEffect.effect);

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
                $" \n We also put some of our update code camera related stuff into its own camera class." +
                $" \n This will now reside in the ExamplSupportClasses folder were most of our stuff we reuse will go. " +
                $" \n We'll start to do this a bit more in the next example as we will add quite a bit as prep to 3d shading." +
                $" \n " +
                $" \n { cam.cameraWorld.ToWellFormatedString("cameraWorld") }" +
                $" \n { cam.view.ToWellFormatedString("view") }" +
                $" \n { cam.projection.ToWellFormatedString("projection") }" +
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

        /// <summary>
        /// This is a simple indexed mesh it demonstrates the changes going from a quad to a grid of vertices and indices ... aka a mesh.
        /// </summary>
        public class PrototypePrimitiveIndexedMesh
        {
            public static bool showOutput = false;

            public VertexPositionNormalTexture[] vertices;
            public int[] indices;

            public PrototypePrimitiveIndexedMesh()
            {
                CreatePrimitiveMesh(2, 2, 1f, false, true, true, 0);
            }

            public PrototypePrimitiveIndexedMesh(int subdivisionWidth, int subdividsionHeight, float scale, bool clockwise, bool invertNormals, bool directionalFaces)
            {
                CreatePrimitiveMesh(subdivisionWidth, subdividsionHeight, scale, clockwise, invertNormals, directionalFaces, 0);
            }

            public void CreatePrimitiveMesh(int subdivisionWidth, int subdividsionHeight, float scale, bool clockwise, bool invertNormals, bool directionalFaces, int faceIndex)
            {
                List<VertexPositionNormalTexture> meshList = new List<VertexPositionNormalTexture>();
                List<int> indexList = new List<int>();

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

                        meshList.Add(vert);
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

                        indexList.Add(tl);
                        indexList.Add(bl);
                        indexList.Add(br);

                        indexList.Add(br);
                        indexList.Add(tr);
                        indexList.Add(tl);

                        if (showOutput)
                        {
                            System.Console.WriteLine();
                            System.Console.WriteLine("t0  face" + faceIndex + " cubeFaceMeshIndexLists [" + tl + "] " + "  vert " + meshList[tl]);
                            System.Console.WriteLine("t0  face" + faceIndex + " cubeFaceMeshIndexLists [" + bl + "] " + "  vert " + meshList[bl]);
                            System.Console.WriteLine("t0  face" + faceIndex + " cubeFaceMeshIndexLists [" + br + "] " + "  vert " + meshList[br]);

                            System.Console.WriteLine();
                            System.Console.WriteLine("t1  face" + faceIndex + " cubeFaceMeshIndexLists [" + br + "] " + "  vert " + meshList[br]);
                            System.Console.WriteLine("t1  face" + faceIndex + " cubeFaceMeshIndexLists [" + tr + "] " + "  vert " + meshList[tr]);
                            System.Console.WriteLine("t1  face" + faceIndex + " cubeFaceMeshIndexLists [" + tl + "] " + "  vert " + meshList[tl]);
                        }
                    }
                }
                vertices = meshList.ToArray();
                indices = indexList.ToArray();
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

    }
}
