
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_ImprovedIndexedMesh : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        SpriteFont font3;
        Texture2D texture;
        Texture2D dotTexture;
        Texture2D dotTexture2;
        //Effect effect;
        RenderTarget2D rtScene;
        //MouseState mouse;

        PrimitiveIndexedMesh mesh;
        PrimitiveNormalArrows visualNormals = new PrimitiveNormalArrows();

        float[] heightMap = new float[]
        {
            0.0f, 0.1f, 0.2f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.8f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.5f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f
        };

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

        bool displayWireframe = false;
        bool displayNormals = false;

        public Game1_ImprovedIndexedMesh()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Improved Indexed Mesh ";
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

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs"); // MG_Logo_Med_exCanvs  blue_atmosphere
            dotTexture = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Red);
            dotTexture2 = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Green);

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");
            font3 = Content.Load<SpriteFont>("MgFont3");

            InitialView();
            UpdateProjection();

            ImprovedIndexMeshEffectClass.Load(Content);
            ImprovedIndexMeshEffectClass.SpriteTexture = texture;
            ImprovedIndexMeshEffectClass.View = view;
            ImprovedIndexMeshEffectClass.Projection = projection;

            PrimitiveIndexedMesh.showOutput = true;

            //mesh = new PrimitiveIndexedMesh(4,4, new Vector3(300f, 250, 0f ), true);

            //mesh = new PrimitiveIndexedMesh(heightMap, 6, new Vector3( 300f, 250, 10f ), true);
            //visualNormals.CreateVisualNormalsForPrimitiveMesh(mesh.vertices, mesh.indices, dotTexture2, 0.75f, 2.5f);

            mesh = new PrimitiveIndexedMesh(texture, new Vector3(300f, 250, 5f), true);
            visualNormals.CreateVisualNormalsForPrimitiveMesh(mesh.vertices, mesh.indices, dotTexture2, 0.25f, 1.5f);
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
            //cameraWorld.Up = quadUpVector;

            // Set the view matrix.
            cameraWorld = Matrix.CreateWorld(cameraWorld.Translation, cameraWorld.Forward, cameraWorld.Up);
            view = Matrix.Invert(cameraWorld);

            // Reset the view projection matrix.
            if (Keys.F1.IsKeyPressedWithDelay(gameTime))
                InitialView();

            if (Keys.F2.IsKeyPressedWithDelay(gameTime))
                displayWireframe = !displayWireframe;
            if (Keys.F3.IsKeyPressedWithDelay(gameTime))
                displayNormals = !displayNormals;
            

            base.Update(gameTime);
        }

        public void InitialView()
        {
            cameraWorldPosition.Z = MgMathExtras.GetRequisitePerspectiveSpriteBatchAlignmentZdistance(GraphicsDevice, fov);
            cameraWorld = Matrix.CreateWorld(cameraWorldPosition, Vector3.Zero - cameraWorldPosition, cameraUpVector);
            view = Matrix.Invert(cameraWorld);
            if (ImprovedIndexMeshEffectClass.effect != null)
                ImprovedIndexMeshEffectClass.View = view;
        }
        public void UpdateProjection()
        {
            projection = Matrix.CreatePerspectiveFieldOfView(fov, GraphicsDevice.Viewport.AspectRatio, 1f, 10000f);
            if (ImprovedIndexMeshEffectClass.effect != null)
                ImprovedIndexMeshEffectClass.Projection = projection;
        }

        RasterizerState rasterizerState_CULLNONE_WIREFRAME = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        RasterizerState rasterizerState_CULLNONE_SOLID = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.Solid };

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Moccasin);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;

            ImprovedIndexMeshEffectClass.View = view;
            ImprovedIndexMeshEffectClass.Projection = projection;
            ImprovedIndexMeshEffectClass.World = Matrix.Identity;
            ImprovedIndexMeshEffectClass.SpriteTexture = texture;

            DrawMesh();

            if(displayWireframe)
                DrawWireFrameMesh();

            if(displayNormals)
            DrawNormalsForMesh();

            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        public void DrawMesh()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            ImprovedIndexMeshEffectClass.SpriteTexture = texture;
            mesh.DrawPrimitive(GraphicsDevice, ImprovedIndexMeshEffectClass.effect);
        }

        public void DrawWireFrameMesh()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_WIREFRAME;
            ImprovedIndexMeshEffectClass.SpriteTexture = dotTexture;
            mesh.DrawPrimitive(GraphicsDevice, ImprovedIndexMeshEffectClass.effect);
        }

        public void DrawNormalsForMesh()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            ImprovedIndexMeshEffectClass.SpriteTexture = visualNormals.texture;
            visualNormals.Draw(GraphicsDevice, ImprovedIndexMeshEffectClass.effect);
        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
            // Draw all the regular stuff
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            string msg =
                $" \n The camera exists as a world matrix that holds a position and orientation." +
                $" \n The keys WASD change the forward view direction (which is the major take away here). ZC allows for spin." +
                $" \n The Arrows move the camera translation as strafing motion. " +
                $" \n The F2 key will turn a wireframe on or off." +
                $" \n  " +
                $" \n In this example we improve the previous mesh we allow the mesh to take a height map." +
                $" \n The map can be in the form of a array or of a texture we also create normals for the mesh." +
                $" \n This is created on the cpu at runtime." +
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
        public class ImprovedIndexMeshEffectClass
        {
            public static Effect effect;

            public static void Load(Microsoft.Xna.Framework.Content.ContentManager Content)
            {
                Content.RootDirectory = @"Content/Shaders3D";
                effect = Content.Load<Effect>("ImprovedIndexMeshEffect");
                effect.CurrentTechnique = effect.Techniques["IndexedMeshDraw"];
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
        }



        //_____________________________________________________
        //_____________________________________________________
        //

        public class PrimitiveIndexedMesh
        {
            public static bool showOutput = false;
            public static int AveragingOption { get; set; } = AVERAGING_OPTION_USE_NONALPHACONSISTANTLY;

            public const int AVERAGING_OPTION_USE_NONALPHACONSISTANTLY = 3;
            public const int AVERAGING_OPTION_USE_HIGHEST = 2;
            public const int AVERAGING_OPTION_USE_AVERAGE = 1;
            public const int AVERAGING_OPTION_USE_RED = 0;

            public VertexPositionNormalTexture[] vertices;
            public int[] indices;

            Color[] heightColorArray;
            Vector3 defaultNormal = new Vector3(0, 0, 1);

            public PrimitiveIndexedMesh()
            {
                int w = 2;
                int h = 2;
                heightColorArray = new Color[w * h];
                for (int i = 0; i < w * h; i++)
                {
                    heightColorArray[i].R = 0;
                    heightColorArray[i].A = 0;
                }
                CreatePrimitiveMesh(heightColorArray, 2, Vector3.Zero, false);
                heightColorArray = new Color[0];
            }
            public PrimitiveIndexedMesh(int subdivisionWidth, int subdividsionHeight, Vector3 scale, bool negateNormalDirection)
            {
                heightColorArray = new Color[subdivisionWidth * subdividsionHeight];
                for (int i = 0; i < subdivisionWidth * subdividsionHeight; i++)
                {
                    heightColorArray[i].R = 0;
                    heightColorArray[i].A = 0;
                }
                CreatePrimitiveMesh(heightColorArray, subdivisionWidth, scale, negateNormalDirection);
                heightColorArray = new Color[0];
            }

            public PrimitiveIndexedMesh(float[] heightArray, int strideWidth)
            {
                heightColorArray = new Color[heightArray.Length];
                for (int i = 0; i < heightArray.Length; i++)
                {
                    heightColorArray[i].R = GetAvgHeightFromFloatAsByte(heightArray[i]);
                    heightColorArray[i].A = GetAvgHeightFromFloatAsByte(heightArray[i]);
                }
                CreatePrimitiveMesh(heightColorArray, strideWidth, Vector3.Zero, false);
                heightColorArray = new Color[0];
            }

            public PrimitiveIndexedMesh(float[] heightArray, int strideWidth, Vector3 scale, bool negateNormalDirection)
            {
                heightColorArray = new Color[heightArray.Length];
                for (int i = 0; i < heightArray.Length; i++)
                {
                    heightColorArray[i].R = GetAvgHeightFromFloatAsByte(heightArray[i]);
                    heightColorArray[i].A = GetAvgHeightFromFloatAsByte(heightArray[i]);
                }
                CreatePrimitiveMesh(heightColorArray, strideWidth, scale, negateNormalDirection);
                heightColorArray = new Color[0];
            }

            public PrimitiveIndexedMesh(Texture2D heightTexture, Vector3 scale, bool negateNormalDirection)
            {
                Color[] heightColorArray = new Color[heightTexture.Width * heightTexture.Height];
                heightTexture.GetData<Color>(heightColorArray);
                CreatePrimitiveMesh(heightColorArray, heightTexture.Width, scale, negateNormalDirection);
                heightColorArray = new Color[0];
            }

            public void CreatePrimitiveMesh(Color[] heighColorArray, int strideWidth, Vector3 scale, bool negateNormalDirection)
            {
                List<VertexPositionNormalTexture> cubesFaceMeshVertexLists = new List<VertexPositionNormalTexture>();
                List<int> cubeFaceMeshIndexLists = new List<int>();

                int subdivisionWidth = strideWidth;
                int subdividsionHeight = (int)(heighColorArray.Length / strideWidth);

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

                        float val = GetAvgHeightFromColorAsUnitLengthValue( heighColorArray[GetIndex(x, y, strideWidth) ] , AveragingOption);
                        float hval = -val;

                        float X = Interpolate(left, right, stepU);
                        float Y = Interpolate(top, bottom, stepV);

                        var p0 = new Vector3(stepU, stepV, hval) * scale;
                        var uv0 = new Vector2(stepU, stepV);

                        cubesFaceMeshVertexLists.Add(GetVertice(p0, uv0));
                        vertCounter += 1;
                    }
                }

                int k = 0;
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

                        AddQuadIndexes(tl, tr, bl, br, ref cubeFaceMeshIndexLists);
                        CalcululateNormalAddToVertices(k + 0, k + 1, k + 2, k + 3, ref cubesFaceMeshVertexLists, ref cubeFaceMeshIndexLists);

                        if (showOutput)
                            ConsoleOutput(0, tl, tr, bl, br, cubesFaceMeshVertexLists);
                        k += 6;
                    }
                }

                // vector addition normals normalize.
                for (int i = 0; i < cubesFaceMeshVertexLists.Count; i++)
                {
                    var v = cubesFaceMeshVertexLists[i];
                    if(negateNormalDirection)
                        v.Normal = -(Vector3.Normalize(v.Normal));
                    else
                        v.Normal = Vector3.Normalize(v.Normal);
                    cubesFaceMeshVertexLists[i] = v;
                }

                vertices = cubesFaceMeshVertexLists.ToArray();
                indices = cubeFaceMeshIndexLists.ToArray();
            }

            public void AddQuadIndexes( int tl, int tr, int bl, int br, ref List<int> cubeFaceMeshIndexLists)
            {
                cubeFaceMeshIndexLists.Add(tl);
                cubeFaceMeshIndexLists.Add(bl);
                cubeFaceMeshIndexLists.Add(br);

                cubeFaceMeshIndexLists.Add(br);
                cubeFaceMeshIndexLists.Add(tr);
                cubeFaceMeshIndexLists.Add(tl);
            }

            public void CalcululateNormalAddToVertices( int tl, int tr, int bl, int br, ref List<VertexPositionNormalTexture> cubesFaceMeshVertexLists,ref List<int> cubeFaceMeshIndexLists)
            {
                //t0
                var v0 = cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[tl]];
                var v1 = cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[bl]];
                var v2 = cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[br]];
                var n0 = Vector3.Cross(v1.Position - v0.Position, v2.Position - v0.Position);
                v0.Normal += n0;
                v1.Normal += n0;
                v2.Normal += n0;
                cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[tl]] = v0;
                cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[bl]] = v1;
                cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[br]] = v2;
                //t1
                var v3 = cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[br]];
                var v4 = cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[tr]];
                var v5 = cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[tl]];
                var n1 = Vector3.Cross(v4.Position - v3.Position, v5.Position - v3.Position);
                v3.Normal += n1;
                v4.Normal += n1;
                v5.Normal += n1;
                cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[br]] = v3;
                cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[tr]] = v4;
                cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[tl]] = v5;
            }

            public int GetIndex(int x, int y, int stride)
            {
                return x + y * stride;
            }

            public void GetIndexXy(int Index, int stride, out int x, out int y)
            {
                y = (int)(Index / stride);
                x = Index - (y * stride);
            }

            private float Interpolate(float A, float B, float t)
            {
                return ((B - A) * t) + A;
            }

            private VertexPositionNormalTexture GetVertice(Vector3 v, Vector2 uv)
            {
                return new VertexPositionNormalTexture(v, defaultNormal, uv);
            }

            private byte GetAvgHeightFromFloatAsByte(float v)
            {
                return (byte)(v * 255f);
            }

            private float GetAvgHeightFromColorAsUnitLengthValue(Color c, int option)
            {
            float result = 0;
                switch (option)
                {
                    case AVERAGING_OPTION_USE_NONALPHACONSISTANTLY :
                        result =  c.A > 0 ? 1f : 0f;
                        break;
                    case AVERAGING_OPTION_USE_HIGHEST:
                        result = c.R;
                        result = c.G > result ? c.G : result;
                        result = c.B > result ? c.B : result;
                        result = (c.R / 255f) * (c.A / 255f);
                        break;
                    case AVERAGING_OPTION_USE_AVERAGE:
                        result = (((c.R + c.G + c.B) / 3f) * (c.A / 255f));
                        break;
                    case AVERAGING_OPTION_USE_RED:
                        result = (c.R / 255f);
                        break;
                    default:
                        result = (c.R / 255f);
                        break;
                }
                return result;
            }

            //void CreateTangents(VertexPositionNormalTextureTangents[] vertices, int surfacePointWidth)
            //{
            //    for (int i = 0; i < vertices.Length; i++)
            //    {
            //        int y = i / surfacePointWidth;
            //        int x = i - (y * surfacePointWidth);
            //        int up = (y - 1) * surfacePointWidth + x;
            //        int down = (y + 1) * surfacePointWidth + x;
            //        Vector3 tangent = new Vector3();
            //        if (down >= vertices.Length)
            //        {
            //            tangent = vertices[up].Position - vertices[i].Position;
            //            tangent.Normalize();
            //            vertices[i].Tangent = tangent;
            //        }
            //        else
            //        {
            //            tangent = vertices[i].Position - vertices[down].Position;
            //            tangent.Normalize();
            //            vertices[i].Tangent = tangent;
            //        }
            //    }
            //}


            public void ConsoleOutput(int faceIndex, int tl, int tr, int bl, int br, List<VertexPositionNormalTexture> cubesFaceMeshLists)
            {
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
