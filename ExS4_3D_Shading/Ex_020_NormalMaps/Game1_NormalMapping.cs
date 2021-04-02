
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_NormalMapping : Game
    {
        bool rotateLight = true;
        bool displayWireframe = true;
        bool displayNormals = true;


        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        SpriteFont font3;
        Texture2D texture; 
        Texture2D textureNormalMap;
        Texture2D dotTexture;
        Texture2D dotTexture2;
        Texture2D dotTexture3;
        RenderTarget2D rtScene;

        PrimitiveIndexedMesh mesh;
        PrimitiveNormals visualNormals = new PrimitiveNormals();
        PrimitiveNormals visualLightNormal = new PrimitiveNormals();


        float[] heightMap = new float[]
        {
            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f, 0.2f, 0.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.2f, 0.5f, 0.2f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.2f, 0.5f, 0.7f, 0.5f, 0.2f, 0.0f, 0.0f,
            0.0f, 0.2f, 0.5f, 0.7f, 0.9f, 0.7f, 0.5f, 0.2f, 0.0f,
            0.0f, 0.0f, 0.2f, 0.5f, 0.7f, 0.5f, 0.2f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.2f, 0.5f, 0.2f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f, 0.2f, 0.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f
        };

        Matrix view;
        Matrix projection;

        float fov = 1.4f;
        Matrix cameraWorld = Matrix.Identity;
        Vector3 cameraWorldPosition = new Vector3(0, 0, 500f);
        Vector3 cameraForwardVector = Vector3.Forward;
        Vector3 cameraUpVector = Vector3.Down;

        Vector3 quadWorldPosition = Vector3.Zero;
        Vector3 quadUpVector = Vector3.Up;
        Vector3 quadForwardVector = Vector3.Forward;
        float quadRotation = 0;

        Vector3 lightPosition = new Vector3(0, 0, 500f);
        Matrix lightTransform = Matrix.Identity;
        float lightRotationRadians = 0f;
        

        public Game1_NormalMapping()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Normal Mapping with Diffuse lighting ";
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
            texture = Content.Load<Texture2D>("walltomap");
            textureNormalMap = Content.Load<Texture2D>("wallnormmap");

            dotTexture = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Red);
            dotTexture2 = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Green);
            dotTexture3 = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.White);

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");
            font3 = Content.Load<SpriteFont>("MgFont3");

            InitialView();
            UpdateProjection();

            DiffuseLightEffectClass.Load(Content);
            DiffuseLightEffectClass.TextureDiffuse = dotTexture3;
            DiffuseLightEffectClass.View = view;
            DiffuseLightEffectClass.Projection = projection;


            PrimitiveIndexedMesh.showOutput = false;
            PrimitiveIndexedMesh.AveragingOption = PrimitiveIndexedMesh.AVERAGING_OPTION_USE_RED;

            mesh = new PrimitiveIndexedMesh(4,4, new Vector3(300f, 250, 0f ), true);
            //mesh = new PrimitiveIndexedMesh(heightMap, 9, new Vector3( 300f, 250, 70f ), true);
            //mesh = new PrimitiveIndexedMesh(texture, new Vector3(300f, 250, 5f), true);

            CreateVisualMeshNormals(mesh, dotTexture2, 0.60f, 3.0f);
            CreateVisualLightNormal(dotTexture2, 20, 300);
        }

        public void CreateVisualMeshNormals(PrimitiveIndexedMesh mesh, Texture2D texture, float thickness, float scale)
        {
            VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; i++)
                tmp[i] = new VertexPositionNormalTexture() { Position = mesh.vertices[i].Position, Normal = mesh.vertices[i].Normal, TextureCoordinate = mesh.vertices[i].TextureCoordinate };
            visualNormals.CreateVisualNormalsForPrimitiveMesh(tmp, mesh.indices, texture, thickness, scale);
        }

        public void CreateVisualLightNormal(Texture2D texture, float thickness, float scale)
        {
            VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[1];
            tmp[0] = new VertexPositionNormalTexture() { Position = new Vector3(0, 0, 0), Normal = new Vector3(0, 0, 1), TextureCoordinate = new Vector2(0, 0) };
            int[] tmpindices = new int[1];
            tmpindices[0] = 0;
            visualLightNormal.CreateVisualNormalsForPrimitiveMesh(tmp, tmpindices, texture, thickness, scale);
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
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                cameraWorld.Translation += cameraWorld.Up * +speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                cameraWorld.Translation += cameraWorld.Up * -speed;
            if (Keyboard.GetState().IsKeyDown(Keys.E))
                cameraWorld.Translation += cameraWorld.Forward * speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
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

            if (rotateLight)
            {
                lightRotationRadians += .005f;
                if (lightRotationRadians > 6.12)
                    lightRotationRadians = 0;
                lightTransform = Matrix.CreateRotationY(lightRotationRadians);
                lightPosition = Vector3.Transform(new Vector3(0, 0, 500), lightTransform);
            }

            base.Update(gameTime);
        }

        public void InitialView()
        {
            cameraWorldPosition.Z = MgMathExtras.GetRequisitePerspectiveSpriteBatchAlignmentZdistance(GraphicsDevice, fov);
            cameraWorld = Matrix.CreateWorld(cameraWorldPosition, Vector3.Zero - cameraWorldPosition, cameraUpVector);
            // well make it specific.
            cameraWorld = new Matrix
            (
                1f, 0f, 0f, 0f,
                0f, -0.141f, -0.990f, 0f,
                0f, 0.990f, -0.141f, 0f,
                153f, 253f, -20f, 1.0f
            );
            view = Matrix.Invert(cameraWorld);
            if (DiffuseLightEffectClass.effect != null)
                DiffuseLightEffectClass.View = view;
        }
        public void UpdateProjection()
        {
            projection = Matrix.CreatePerspectiveFieldOfView(fov, GraphicsDevice.Viewport.AspectRatio, 1f, 10000f);
            if (DiffuseLightEffectClass.effect != null)
                DiffuseLightEffectClass.Projection = projection;
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

            DiffuseLightEffectClass.View = view;
            DiffuseLightEffectClass.Projection = projection;
            DiffuseLightEffectClass.World = Matrix.Identity;
            DiffuseLightEffectClass.LightPosition = lightPosition;

            DrawMesh(dotTexture3);

            if(displayWireframe)
                DrawWireFrameMesh();

            //if(displayNormals)
            //    DrawNormalsForMesh();

            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        public void DrawMesh(Texture2D texture)
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            DiffuseLightEffectClass.TextureDiffuse = texture;
            DiffuseLightEffectClass.TextureNormalMap = textureNormalMap;
            mesh.DrawPrimitive(GraphicsDevice, DiffuseLightEffectClass.effect);
        }

        public void DrawWireFrameMesh()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_WIREFRAME;
            DiffuseLightEffectClass.TextureDiffuse = dotTexture;
            mesh.DrawPrimitive(GraphicsDevice, DiffuseLightEffectClass.effect);
        }

        public void DrawNormalsForMesh()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            DiffuseLightEffectClass.TextureDiffuse = visualNormals.texture;
            visualNormals.Draw(GraphicsDevice, DiffuseLightEffectClass.effect);

            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            DiffuseLightEffectClass.World = lightTransform;
            DiffuseLightEffectClass.TextureDiffuse = dotTexture;
            visualLightNormal.Draw(GraphicsDevice, DiffuseLightEffectClass.effect);
        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
            // Draw all the regular stuff
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            string msg =
                $" \n The camera exists as a world matrix that holds a position and orientation." +
                $" \n The keys WASD change the forward view direction (which is the major take away here). ZC allows for spin." +
                $" \n The Arrows move the camera translation as strafing motion. " +
                $" \n The F2 key will turn a wireframe on or off. F3 will show normals." +
                $" \n  " +
                $" \n In this example we make a shader that creates a diffuse light." +
                $" \n The light rotates around the mesh illuminating faces depending on the triangle normals." +
                $" \n We also create a class that allows us to visualize normals per vertice and for the light." +
                $" \n Well place the light into rotation so we can see how the diffuse shader works." +
                $" \n Simple diffuse lighting is achieved via a dot product on the light and normals aka NdotL ." +
                $" \n this can be found in the shader" +
                $" \n  " +
                $" \n { cameraWorld.DisplayMatrix("cameraWorld") }" +
                $" \n { view.DisplayMatrix("view") }" +
                $" \n { projection.DisplayMatrix("projection") }" +
                $" \n" +
                $" \n {lightPosition.VectorToString("LightPosition")}" +
                $" \n"
                ;
            spriteBatch.DrawString(font, msg, new Vector2(10, 10), Color.Blue);
            spriteBatch.End();
        }

        // Wrap up our effect.
        public class DiffuseLightEffectClass
        {
            public static Effect effect;

            public static void Load(Microsoft.Xna.Framework.Content.ContentManager Content)
            {
                Content.RootDirectory = @"Content/Shaders3D";
                effect = Content.Load<Effect>("NormalMapEffect");
                effect.CurrentTechnique = effect.Techniques["NormalMapDiffuseLighting"];
                World = Matrix.Identity;
                View = Matrix.Identity;
                Projection = Matrix.CreatePerspectiveFieldOfView(1, 1.33f, 1f, 10000f); // just something default;
                LightPosition = new Vector3(0, 0, 10000);
            }
            public static Effect GetEffect
            {
                get { return effect; }
            }
            public static string Technique
            {
                set { effect.CurrentTechnique = effect.Techniques[value]; }
            }
            public static Texture2D TextureDiffuse
            {
                set { effect.Parameters["TextureDiffuse"].SetValue(value); }
            }
            public static Texture2D TextureNormalMap
            {
                set { effect.Parameters["TextureNormalMap"].SetValue(value); }
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

            public static Vector3 LightPosition
            {
                set { effect.Parameters["LightPosition"].SetValue(value); }
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

            public VertexPositionNormalTextureTangentWeights[] vertices;
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
                List<VertexPositionNormalTextureTangentWeights> cubesFaceMeshVertexLists = new List<VertexPositionNormalTextureTangentWeights>();
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

                        cubesFaceMeshVertexLists.Add(GetInitialVertice(p0, uv0));
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
                        CalcululateNormalsAndTangentsAddToVertices(k + 0, k + 1, k + 2, k + 3, ref cubesFaceMeshVertexLists, ref cubeFaceMeshIndexLists);

                        if (showOutput)
                            ConsoleOutput(0, tl, tr, bl, br, cubesFaceMeshVertexLists);
                        k += 6;
                    }
                }

                // vector addition normals and tangents normalized.
                for (int i = 0; i < cubesFaceMeshVertexLists.Count; i++)
                {
                    var v = cubesFaceMeshVertexLists[i];
                    if (negateNormalDirection)
                        v.Normal = -(Vector3.Normalize(v.Normal));
                    else
                        v.Normal = Vector3.Normalize(v.Normal);

                    v.Tangent = -(Vector3.Normalize(v.Tangent));
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

            public void CalcululateNormalsAndTangentsAddToVertices( int tl, int tr, int bl, int br, ref List<VertexPositionNormalTextureTangentWeights> cubesFaceMeshVertexLists,ref List<int> cubeFaceMeshIndexLists)
            {
                //t0
                var v0 = cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[tl]];
                var v1 = cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[bl]];
                var v2 = cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[br]];
                var t0 = Vector3.Normalize(v1.Position - v0.Position);
                var n0 = Vector3.Cross(v1.Position - v0.Position, v2.Position - v0.Position);
                //t1
                var v3 = cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[br]];
                var v4 = cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[tr]];
                var v5 = cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[tl]];
                var t1 = Vector3.Normalize(v4.Position - v3.Position);
                var n1 = Vector3.Cross(v4.Position - v3.Position, v5.Position - v3.Position);
                // t0
                v0.Normal += n0;
                v1.Normal += n0;
                v2.Normal += n0;
                // left vertices.
                v0.Tangent += t0;
                v1.Tangent += t0;
                // t1
                v3.Normal += n1;
                v4.Normal += n1;
                v5.Normal += n1;
                // right vertices.
                v3.Tangent += t1;
                v4.Tangent += t1;
                // t0
                cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[tl]] = v0;
                cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[bl]] = v1;
                cubesFaceMeshVertexLists[cubeFaceMeshIndexLists[br]] = v2;
                // t1
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

            private VertexPositionNormalTextureTangentWeights GetInitialVertice(Vector3 v, Vector2 uv)
            {
                return new VertexPositionNormalTextureTangentWeights(v, defaultNormal, uv, Vector3.UnitX, new Color(1, 0, 0, 0) , new Color(1, 0, 0, 0) );
            }

            private byte GetAvgHeightFromFloatAsByte(float v)
            {
                return (byte)(v * 255f);
            }

            private float GetAvgHeightFromColorAsUnitLengthValue(Color c, int option)
            {
                float result = 0;
                float alphamult = (c.A / 255f);
                switch (option)
                {
                    case AVERAGING_OPTION_USE_NONALPHACONSISTANTLY :
                        result =  c.A > 0 ? 1f : 0f;
                        break;
                    case AVERAGING_OPTION_USE_HIGHEST:
                        result = c.R;
                        result = c.G > result ? c.G : result;
                        result = c.B > result ? c.B : result;
                        result = (c.R / 255f) * alphamult;
                        break;
                    case AVERAGING_OPTION_USE_AVERAGE:
                        result = (( ((c.R + c.G + c.B) / 3f) / 255f) * alphamult);
                        break;
                    case AVERAGING_OPTION_USE_RED:
                        result = (c.R / 255f);
                        break;
                    default:
                        result = alphamult;
                        break;
                }
                return result;
            }

            public void ConsoleOutput(int faceIndex, int tl, int tr, int bl, int br, List<VertexPositionNormalTextureTangentWeights> cubesFaceMeshLists)
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
                    gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3, VertexPositionNormalTextureTangentWeights.VertexDeclaration);
                }
            }
        }

        /// <summary>
        /// Vertex Definition.
        /// </summary>
        public struct VertexPositionNormalTextureTangentWeights : IVertexType
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TextureCoordinate;
            public Vector3 Tangent;
            public Color BlendIndices;
            public Color BlendWeights;

            public VertexPositionNormalTextureTangentWeights(Vector3 position, Vector3 normal, Vector2 texcoord, Vector3 tangent, Color blendindices, Color blendweights)
            {
                Position = position; TextureCoordinate = texcoord; Normal = normal; Tangent = tangent; BlendIndices = blendindices; BlendWeights = blendweights;
            }

            public static VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                  new VertexElement(VertexElementByteOffset.PositionStartOffset(), VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                  new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                  new VertexElement(VertexElementByteOffset.OffsetVector2(), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                  new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 1),
                  new VertexElement(VertexElementByteOffset.OffsetColor(), VertexElementFormat.Byte4, VertexElementUsage.BlendIndices, 0),
                  new VertexElement(VertexElementByteOffset.OffsetColor(), VertexElementFormat.Byte4, VertexElementUsage.BlendWeight, 0)
            );
            VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
        }

        /// <summary>
        /// This is a helper struct for tallying byte offsets
        /// </summary>
        public struct VertexElementByteOffset
        {
            public static int currentByteSize = 0;
            //[STAThread]
            public static int PositionStartOffset() { currentByteSize = 0; var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
            public static int Offset(int n) { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
            public static int Offset(float n) { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
            public static int Offset(Vector2 n) { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
            public static int Offset(Color n) { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
            public static int Offset(Vector3 n) { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
            public static int Offset(Vector4 n) { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }

            public static int OffsetInt() { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
            public static int OffsetFloat() { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
            public static int OffsetColor() { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
            public static int OffsetVector2() { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
            public static int OffsetVector3() { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
            public static int OffsetVector4() { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }
        }




        public class PrimitiveNormals
        {
            public VertexPositionNormalTexture[] vertices;
            public int[] indices;
            public Texture2D texture;

            public PrimitiveNormals(){ }

            public void CreateVisualNormalsForPrimitiveMesh(VertexPositionNormalTexture[] inVertices, int[] inIndices, Texture2D t, float thickness, float scale)
            {
                texture = t;
                int len = inVertices.Length;

                // for each vertice in the model we will make a quad composed of 4 vertices and 6 indices.
                VertexPositionNormalTexture[] nverts = new VertexPositionNormalTexture[len * 4];
                int[] nindices = new int[len * 6];

                for (int j = 0; j < len; j++)
                {
                    int v = j * 4;
                    int i = j * 6;
                    //
                    nverts[v + 0].Position = new Vector3(0f, 0f, 0f) + inVertices[j].Position;
                    nverts[v + 1].Position = new Vector3(0f, -.2f * thickness, 0f) + inVertices[j].Position;
                    nverts[v + 2].Position = new Vector3(0f, 0f, 0f) + inVertices[j].Position + inVertices[j].Normal * scale;
                    nverts[v + 3].Position = new Vector3(0f, -.2f * thickness, 0f) + inVertices[j].Position + inVertices[j].Normal * scale;
                    //
                    nverts[v + 0].TextureCoordinate = new Vector2(0f, 0f);
                    nverts[v + 1].TextureCoordinate = new Vector2(0f, .33f);
                    nverts[v + 2].TextureCoordinate = new Vector2(1f, .0f);
                    nverts[v + 3].TextureCoordinate = new Vector2(1f, .33f);
                    //
                    nverts[v + 0].Normal = inVertices[j].Normal;
                    nverts[v + 1].Normal = inVertices[j].Normal;
                    nverts[v + 2].Normal = inVertices[j].Normal;
                    nverts[v + 3].Normal = inVertices[j].Normal;

                    // indices
                    nindices[i + 0] = 0 + v;
                    nindices[i + 1] = 1 + v;
                    nindices[i + 2] = 2 + v;
                    nindices[i + 3] = 2 + v;
                    nindices[i + 4] = 1 + v;
                    nindices[i + 5] = 3 + v;
                }
                this.vertices = nverts;
                this.indices = nindices;
            }

            public void Draw(GraphicsDevice gd, Effect effect)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionNormalTexture.VertexDeclaration);
                }
            }
        }

    }
}
