
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
        bool displayMesh = true;
        bool displayWireframe = true;
        bool displayNormals = true;


        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        SpriteFont font3;
        Texture2D texture; 
        Texture2D textureNormalMap;
        Texture2D dotTextureRed;
        Texture2D dotTextureBlue;
        Texture2D dotTextureGreen;
        Texture2D dotTextureWhite;
        RenderTarget2D rtScene;

        PrimitiveIndexedMesh mesh;
        PrimitiveNormals visualNormals = new PrimitiveNormals();
        PrimitiveNormals visualTangents = new PrimitiveNormals();
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

        float fov = 1.0f; // 1.4
        Matrix cameraWorld = Matrix.Identity;
        Vector3 cameraWorldPosition = new Vector3(0, 0, 500f);
        Vector3 cameraForwardVector = Vector3.Forward;
        Vector3 cameraUpVector = Vector3.Down;

        Vector3 quadWorldPosition = Vector3.Zero;
        Vector3 quadUpVector = Vector3.Up;
        Vector3 quadForwardVector = Vector3.Forward;
        float quadRotation = 0;

        Vector3 lightPosition = new Vector3(0, 0, 5000f);
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
            //textureNormalMap = Content.Load<Texture2D>("RefactionTexture"); // with the opposite encoding

            dotTextureRed = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Red);
            dotTextureGreen = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Green);
            dotTextureBlue = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Blue);
            dotTextureWhite = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.White);

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");
            font3 = Content.Load<SpriteFont>("MgFont3");

            InitialView();
            UpdateProjection();

            DiffuseLightEffectClass.Load(Content);
            DiffuseLightEffectClass.TextureDiffuse = dotTextureWhite;
            DiffuseLightEffectClass.View = view;
            DiffuseLightEffectClass.Projection = projection;


            PrimitiveIndexedMesh.ShowOutput = false;
            PrimitiveIndexedMesh.AveragingOption = PrimitiveIndexedMesh.AVERAGING_OPTION_USE_HIGHEST; //PrimitiveIndexedMesh.AVERAGING_OPTION_USE_RED;

            mesh = new PrimitiveIndexedMesh(5,5, new Vector3(300f, 250, 0f ), false);
            //mesh = new PrimitiveIndexedMesh(heightMap, 9, new Vector3( 300f, 250, 70f ), false);
            //mesh = new PrimitiveIndexedMesh(texture, new Vector3(300f, 250, 5f), false);

            CreateVisualMeshNormals(mesh, dotTextureGreen, .1f, 10.0f);  // .6  3
            CreateVisualMeshTangents(mesh, dotTextureBlue, .1f, 10.0f);
            CreateVisualLightNormal(dotTextureWhite, 1, 300);
        }

        public void CreateVisualMeshNormals(PrimitiveIndexedMesh mesh, Texture2D texture, float thickness, float scale)
        {
            VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; i++)
                tmp[i] = new VertexPositionNormalTexture() { Position = mesh.vertices[i].Position, Normal = mesh.vertices[i].Normal, TextureCoordinate = mesh.vertices[i].TextureCoordinate };
            visualNormals.CreateVisualNormalsForPrimitiveMesh(tmp, mesh.indices, texture, thickness, scale);
            visualNormals.SetUpBasicEffect(GraphicsDevice, dotTextureGreen, view, projection);
        }

        public void CreateVisualMeshTangents(PrimitiveIndexedMesh mesh, Texture2D texture, float thickness, float scale)
        {
            VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; i++)
                tmp[i] = new VertexPositionNormalTexture() { Position = mesh.vertices[i].Position, Normal = mesh.vertices[i].Tangent, TextureCoordinate = mesh.vertices[i].TextureCoordinate };
            visualTangents.CreateVisualNormalsForPrimitiveMesh(tmp, mesh.indices, texture, thickness, scale);
            visualTangents.SetUpBasicEffect(GraphicsDevice, dotTextureBlue, view, projection);
        }

        public void CreateVisualLightNormal(Texture2D texture, float thickness, float scale)
        {
            VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[1];
            tmp[0] = new VertexPositionNormalTexture() { Position = new Vector3(0, 0, 0), Normal = new Vector3(0, 0, 1), TextureCoordinate = new Vector2(0, 0) };
            int[] tmpindices = new int[1];
            tmpindices[0] = 0;
            visualLightNormal.CreateVisualNormalsForPrimitiveMesh(tmp, tmpindices, texture, thickness, scale);
            visualLightNormal.SetUpBasicEffect(GraphicsDevice, dotTextureWhite, view, projection);
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
                cameraWorld.Translation += cameraWorld.Right * -speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                cameraWorld.Translation += cameraWorld.Right * +speed;
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
            if (Keys.F4.IsKeyPressedWithDelay(gameTime))
                displayMesh = !displayMesh;

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
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;

            DiffuseLightEffectClass.View = view;
            DiffuseLightEffectClass.Projection = projection;
            DiffuseLightEffectClass.World = Matrix.Identity;
            DiffuseLightEffectClass.LightPosition = lightPosition;

            if (displayMesh)
                DrawMesh(texture);

            if (displayNormals)
            {
                DrawNormalsForMesh();
                //DrawTangentsForMesh();
                DrawLightLine();
            }

            if (displayWireframe)
                DrawWireFrameMesh();


            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        public void DrawMesh(Texture2D texture)
        {
            //GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            DiffuseLightEffectClass.TextureDiffuse = texture;
            DiffuseLightEffectClass.TextureNormalMap = textureNormalMap;
            mesh.DrawPrimitive(GraphicsDevice, DiffuseLightEffectClass.effect);
        }

        public void DrawNormalsForMesh()
        {
            //GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            visualNormals.World = Matrix.Identity;
            visualNormals.View = view;
            visualNormals.Draw(GraphicsDevice);
        }

        public void DrawTangentsForMesh()
        {
            //GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            visualTangents.World = Matrix.Identity;
            visualTangents.View = view;
            visualTangents.Draw(GraphicsDevice);
        }

        public void DrawLightLine()
        {
            //GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            visualLightNormal.World = lightTransform;
            visualLightNormal.View = view;
            visualLightNormal.Draw(GraphicsDevice);
        }

        public void DrawWireFrameMesh()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_WIREFRAME;
            DiffuseLightEffectClass.TextureDiffuse = dotTextureRed;
            mesh.DrawPrimitive(GraphicsDevice, DiffuseLightEffectClass.effect);
        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
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
            public static bool ShowOutput { get; set; } = false;

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
                List<VertexPositionNormalTextureTangentWeights> VertexLists = new List<VertexPositionNormalTextureTangentWeights>();
                List<int> IndexLists = new List<int>();

                int subdivisionWidth = strideWidth;
                int subdividsionHeight = (int)(heighColorArray.Length / strideWidth);

                if (subdivisionWidth < 2)
                    subdivisionWidth = 2;
                if (subdividsionHeight < 2)
                    subdividsionHeight = 2;

                float left = -1f;
                float right = +1f;
                float top = -1f;
                float bottom = +1f;

                // add vertices.
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

                        VertexLists.Add(GetInitialVertice(p0, uv0));
                        vertCounter += 1;
                    }
                }

                // add indices
                int quadIndice = 0;
                for (int y = 0; y < subdividsionHeight - 1; y++)
                {
                    for (int x = 0; x < subdivisionWidth - 1; x++)
                    {
                        var stride = subdivisionWidth;
                        var verticeOffset = stride * y + x;

                        var tl = verticeOffset;
                        var tr = verticeOffset + 1;
                        var bl = verticeOffset + stride;
                        var br = verticeOffset + stride + 1;

                        AddQuadIndexes(tl, tr, bl, br, ref IndexLists);

                        quadIndice += 6;
                    }
                }

                // calculate normals and tangents
                for (int n = 0; n < IndexLists.Count; n += 6)
                {
                    CalcululateNormalsAddToVertices(n, ref VertexLists, ref IndexLists);
                    CalcululateTangentsAddToVertices(n, ref VertexLists, ref IndexLists);
                }

                // vector addition normals and tangents normalized.
                for (int i = 0; i < VertexLists.Count; i++)
                {
                    var v = VertexLists[i];
                    if (negateNormalDirection)
                        v.Normal = -(Vector3.Normalize(v.Normal));
                    else
                        v.Normal = Vector3.Normalize(v.Normal);

                    if (negateNormalDirection)
                        v.Tangent = -(Vector3.Normalize(v.Tangent));
                    else
                        v.Tangent = (Vector3.Normalize(v.Tangent));
                    VertexLists[i] = v;
                }

                for (int n = 0; n < IndexLists.Count; n += 6)
                    if (ShowOutput)
                        ConsoleOutput(n, VertexLists, IndexLists);

                vertices = VertexLists.ToArray();
                indices = IndexLists.ToArray();
            }

            /// <summary>
            /// CCW
            /// 
            /// tl[0] > bl[1] > tr[2] > br[3]
            /// 
            /// 0        2
            /// tl        tr
            /// |      /  |
            /// | t0/    |
            /// |  /  t1 |
            /// |/        |
            /// bl      br
            /// 1        3
            /// 
            /// triangle 0:   
            /// tl > bl > tr   0,1,2
            /// 
            /// triangle 1:    
            /// br > tr > bl   3,2,1
            /// </summary>
            public void AddQuadIndexes( int tl, int tr, int bl, int br, ref List<int> IndexLists)
            {
                // tl > bl > tr   0,1,2
                IndexLists.Add(tl);  
                IndexLists.Add(bl); 
                IndexLists.Add(tr); 

                // br > tr > bl   3,2,1
                IndexLists.Add(br); 
                IndexLists.Add(tr);  
                IndexLists.Add(bl);  
            }

            public void CalcululateNormalsAddToVertices(int startIndice, ref List<VertexPositionNormalTextureTangentWeights> VertexLists,ref List<int> IndexLists)
            {
                // tl[0] > bl[1] > tr[2] > br[3]

                var tl = IndexLists[startIndice + 0];
                var bl = IndexLists[startIndice + 1];
                var tr = IndexLists[startIndice + 2];
                var br = IndexLists[startIndice + 3];

                var TL = VertexLists[tl];
                var BL = VertexLists[bl];
                var TR = VertexLists[tr];
                var BR = VertexLists[br];

                var d0 = BL.Position - TL.Position;
                var d1 = TR.Position - TL.Position;
                var n = Vector3.Cross(d0, d1);
                TL.Normal += n;
                BL.Normal += n;
                BR.Normal += n;

                d0 = TR.Position - BR.Position;
                d1 = BL.Position - BR.Position;
                n = Vector3.Cross(d0, d1);
                TL.Normal += n;
                TR.Normal += n;
                BR.Normal += n;

                VertexLists[tl] = TL;
                VertexLists[bl] = BL;
                VertexLists[tr] = TR;
                VertexLists[br] = BR;
            }

            public void CalcululateTangentsAddToVertices(int startIndice, ref List<VertexPositionNormalTextureTangentWeights> VertexLists, ref List<int> IndexLists)
            {
                // tl[0] > bl[1] > tr[2] > br[3]
                var tl = IndexLists[startIndice + 0];
                var bl = IndexLists[startIndice + 1];
                var tr = IndexLists[startIndice + 2];
                var br = IndexLists[startIndice + 3];

                var TL = VertexLists[tl];
                var BL = VertexLists[bl];
                var TR = VertexLists[tr];
                var BR = VertexLists[br];

                var t0 = (BL.Position - TL.Position);
                var t1 = (BR.Position - TR.Position);
                TL.Tangent += t0;
                BL.Tangent += t0;
                TR.Tangent += t1;
                BR.Tangent += t1;

                // looks to be correct for the bi tangent.
                //var t0 = -(TR.Position - TL.Position);
                //var t1 = -(BR.Position - BL.Position);
                //TL.Tangent += t0;
                //TR.Tangent += t0;
                //BR.Tangent += t1;
                //BL.Tangent += t1;

                VertexLists[tl] = TL;
                VertexLists[bl] = BL;
                VertexLists[tr] = TR;
                VertexLists[br] = BR;
            }

            public void ConsoleOutput(int k, List<VertexPositionNormalTextureTangentWeights> MeshLists, List<int> IndexLists)
            {
                System.Console.WriteLine();
                int T0_Index_0 = k + 0;  // tl
                int T0_Index_1 = k + 1;  // bl
                int T0_Index_2 = k + 2;  // br
                int T1_Index_0 = k + 3;  // br 
                int T1_Index_1 = k + 4;  // tr
                int T1_Index_2 = k + 5;  // tl

                int T0_VIndex_0 = IndexLists[T0_Index_0];  // tl
                int T0_VIndex_1 = IndexLists[T0_Index_1];  // bl
                int T0_VIndex_2 = IndexLists[T0_Index_2];  // br
                int T1_VIndex_0 = IndexLists[T1_Index_0];  // br
                int T1_VIndex_1 = IndexLists[T1_Index_1];  // tr
                int T1_VIndex_2 = IndexLists[T1_Index_2];  // tl

                System.Console.WriteLine("quad " + k / 6);
                System.Console.WriteLine("t0   TL  IndexLists [" + T0_Index_0 + "] " + "  vert  [" + T0_VIndex_0 + "] Pos: " + MeshLists[T0_VIndex_0].Position + " Norm: " + MeshLists[T0_VIndex_0].Normal + " Tangent: " + MeshLists[T0_VIndex_0].Tangent);
                System.Console.WriteLine("t0   BL  IndexLists [" + T0_Index_1 + "] " + "  vert  [" + T0_VIndex_1 + "] Pos: " + MeshLists[T0_VIndex_1].Position + " Norm: " + MeshLists[T0_VIndex_1].Normal + " Tangent: " + MeshLists[T0_VIndex_1].Tangent);
                System.Console.WriteLine("t0   BR  IndexLists [" + T0_Index_2 + "] " + "  vert  [" + T0_VIndex_2 + "] Pos: " + MeshLists[T0_VIndex_2].Position + " Norm: " + MeshLists[T0_VIndex_2].Normal + " Tangent: " + MeshLists[T0_VIndex_2].Tangent);

                System.Console.WriteLine();
                System.Console.WriteLine("t1   BR  IndexLists [" + T1_Index_0 + "] " + "  vert  [" + T1_VIndex_0 + "] Pos: " + MeshLists[T1_VIndex_0].Position + " Norm: " + MeshLists[T1_VIndex_0].Normal + " Tangent: " + MeshLists[T1_VIndex_0].Tangent);
                System.Console.WriteLine("t1   TR  IndexLists [" + T1_Index_1 + "] " + "  vert  [" + T1_VIndex_1 + "] Pos: " + MeshLists[T1_VIndex_1].Position + " Norm: " + MeshLists[T1_VIndex_1].Normal + " Tangent: " + MeshLists[T1_VIndex_1].Tangent);
                System.Console.WriteLine("t1   TL  IndexLists [" + T1_Index_2 + "] " + "  vert  [" + T1_VIndex_2 + "] Pos: " + MeshLists[T1_VIndex_2].Position + " Norm: " + MeshLists[T1_VIndex_2].Normal + " Tangent: " + MeshLists[T1_VIndex_2].Tangent);
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

            public BasicEffect basicEffect;

            public PrimitiveNormals(){}

            public void SetUpBasicEffect(GraphicsDevice device, Texture2D texture, Matrix view, Matrix proj)
            {
                basicEffect = new BasicEffect(device);
                basicEffect.VertexColorEnabled = false;
                basicEffect.TextureEnabled = true;
                //basicEffect.LightingEnabled = true;
                //basicEffect.EnableDefaultLighting();
                //basicEffect.AmbientLightColor = new Vector3(1.0f,1.0f,1.0f);
                World = Matrix.Identity;
                basicEffect.View = view;
                basicEffect.Projection = proj;
                basicEffect.Texture = texture;
            }

            public Matrix World { set { basicEffect.World = value; } get { return basicEffect.World; } }
            public Matrix View { set { basicEffect.View = value; } get { return basicEffect.View; } }
            public Matrix Projection { set { basicEffect.Projection = value; } get { return basicEffect.Projection; } }
            public Texture2D Texture { set { basicEffect.Texture = value; } get { return basicEffect.Texture; } }

            public void CreateVisualNormalsForPrimitiveMesh(VertexPositionNormalTexture[] inVertices, int[] inIndices, Texture2D t, float thickness, float scale)
            {
                texture = t;
                int len = inVertices.Length;

                // we will make a tubular line
                List<VertexPositionNormalTexture> nverts = new List<VertexPositionNormalTexture>();
                List<int> nindices = new List<int>();

                // well define the number of sides of the tube
                int sides = 4;
                // the number of vertices per line
                int lineVerts = 4 * 2;
                // the number of indices per line
                int lineIndices = sides * 6;

                // for each vertice in the model
                for (int j = 0; j < len; j++)
                {
                    int startvert = j * lineVerts;
                    int startindices = j * lineIndices;

                    var p = inVertices[j].Position;
                    var n = inVertices[j].Normal;
                    var p2 = n * scale + p;

                    int index = 0;
                    float radMult = 6.28f / sides;
                    for (int k =0; k < sides; k ++)
                    {
                        float rads = (float)(k) * radMult;
                        var m = Matrix.CreateFromAxisAngle(n, rads);
                        var mright = m.Right;
                        var np = p + m.Right * thickness;
                        var np2 = p2 + m.Right * thickness;

                        var v0 = new VertexPositionNormalTexture() { Position = np, Normal = n, TextureCoordinate = new Vector2((float)(k) / (float)(sides - 1), 0f) };
                        var v1 = new VertexPositionNormalTexture() { Position = np2, Normal = n, TextureCoordinate = new Vector2((float)(k) / (float)(sides - 1), 1f) };
                        nverts.Add( v0 );
                        nverts.Add( v1);

                        index += 2;
                    }
                }

                // build the indices and line them up to the vertices.
                for (int j = 0; j < len; j++)
                {
                    int startvert = j * lineVerts;
                    int startindices = j * lineIndices;
                    for (int quadindex = 0; quadindex < sides; quadindex++)
                    {
                        int offsetVertice = quadindex * 2 + startvert;
                        //int offsetIndice = quadindex * 6;

                        if (quadindex != sides - 1)
                        {
                            nindices.Add(offsetVertice + 0);
                            nindices.Add(offsetVertice + 1);
                            nindices.Add(offsetVertice + 2);

                            nindices.Add(offsetVertice + 2);
                            nindices.Add(offsetVertice + 1);
                            nindices.Add(offsetVertice + 3);
                        }
                        else // the last face wraps around well sort of manually attach this.
                        {
                            nindices.Add(offsetVertice + 0);
                            nindices.Add(offsetVertice + 1);
                            nindices.Add(startvert + 0);

                            nindices.Add(startvert + 0);
                            nindices.Add(offsetVertice + 1);
                            nindices.Add(startvert + 1);
                        }
                    }
                }

                this.vertices = nverts.ToArray();
                this.indices = nindices.ToArray();
            }

            public void Draw(GraphicsDevice gd)
            {
                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionNormalTexture.VertexDeclaration);
                }
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
