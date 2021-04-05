
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_SpecularLighting : Game
    {
        bool rotateLight = true;
        bool displayMesh = true;
        bool displayWireframe = false;
        bool displayNormals = true;
        bool displayWhiteDiffuse = false;
        bool displayOnScreenText = false;

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
        Texture2D dotTextureYellow;
        Texture2D dotTextureWhite;
        RenderTarget2D rtScene;
        CameraAndKeyboardControls cam = new CameraAndKeyboardControls();
        PrimitiveIndexedMesh mesh;
        VisualizationNormals visualNormals = new VisualizationNormals();
        VisualizationNormals visualTangents = new VisualizationNormals();
        VisualizationLine visualLightNormal = new VisualizationLine();

        PrimitiveSphere sphere = new PrimitiveSphere();
        VisualizationNormals visualSphereNormals = new VisualizationNormals();


        float[] heightMap = new float[]
        {
            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f, 0.2f, 0.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.2f, 0.5f, 0.2f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.2f, 0.5f, 0.7f, 0.5f, 0.2f, 0.0f, 0.0f,
            0.0f, 0.2f, 0.5f, 0.7f, 0.8f, 0.7f, 0.5f, 0.2f, 0.0f,
            0.0f, 0.0f, 0.2f, 0.5f, 0.7f, 0.5f, 0.2f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.2f, 0.5f, 0.2f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f, 0.2f, 0.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f
        };

        Vector3 lightStartPosition = new Vector3(1, 800, -300);
        Vector3 lightPosition = new Vector3(0, 0, 0);
        Matrix lightTransform = Matrix.Identity;
        float lightRotationRadians = 0f;
        Vector3 meshDimensions = new Vector3(300f, 250,0);
        Vector3 meshCenter = new Vector3(150,125,0);


        public Game1_SpecularLighting()
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
            cam.UpdateProjection(GraphicsDevice);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("TestNormalMap");
            textureNormalMap = Content.Load<Texture2D>("TestNormalMap");
            //texture = Content.Load<Texture2D>("Flower-normal");
            //textureNormalMap = Content.Load<Texture2D>("Flower-normal");
            //textureNormalMap = Content.Load<Texture2D>("RefactionTexture"); // with the opposite encoding    walltomap wallnormmap  Flower-normal , Flower-diffuse  Flower-bump  Flower-ambientocclusion

            dotTextureRed = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Red);
            dotTextureGreen = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Green);
            dotTextureBlue = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Blue);
            dotTextureWhite = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.White);
            dotTextureYellow = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Yellow);

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");
            font3 = Content.Load<SpriteFont>("MgFont3");

            cam.InitialView(GraphicsDevice);
            cam.UpdateProjection(GraphicsDevice);

            SpecularLightEffectClass.Load(Content);
            SpecularLightEffectClass.TextureDiffuse = dotTextureWhite;
            SpecularLightEffectClass.TextureNormalMap = textureNormalMap;
            SpecularLightEffectClass.AmbientStrength = .10f;
            SpecularLightEffectClass.View = cam.view;
            SpecularLightEffectClass.Projection = cam.projection;
            SpecularLightEffectClass.CameraPosition = cam.cameraWorld.Translation;


            DiffuseLightEffectClass.Load(Content);
            DiffuseLightEffectClass.SpriteTexture = dotTextureWhite;
            DiffuseLightEffectClass.View = cam.view;
            DiffuseLightEffectClass.Projection = cam.projection;
            DiffuseLightEffectClass.LightPosition = lightStartPosition;


            PrimitiveIndexedMesh.ShowOutput = false;
            PrimitiveIndexedMesh.AveragingOption = PrimitiveIndexedMesh.AVERAGING_OPTION_USE_HIGHEST; //PrimitiveIndexedMesh.AVERAGING_OPTION_USE_RED;

            mesh = new PrimitiveIndexedMesh(5, 5, new Vector3(300f, 250, 0f), false, false);
            float thickness = .1f; float scale = 10f;

            //mesh = new PrimitiveIndexedMesh(heightMap, 9, new Vector3( 300f, 250, 70f ), false, false);
            //float thickness = .1f; float scale = 10f;

            //mesh = new PrimitiveIndexedMesh(texture, new Vector3(300f, 250, 5f), false, false);
            //float thickness = .01f; float scale = 1f;

            mesh.DiffuseTexture = dotTextureWhite; // texture;
            mesh.NormalMapTexture = textureNormalMap;

            CreateVisualMeshNormals(mesh, dotTextureGreen, thickness, scale);  // .6  3
            CreateVisualMeshTangents(mesh, dotTextureYellow, thickness, scale);
            CreateVisualLightNormal(dotTextureWhite, meshCenter, lightStartPosition, 1, Color.White);

            sphere = new PrimitiveSphere(10, 10, 50, false, false, false);
            CreateVisualSphereNormals(sphere, dotTextureGreen, thickness, scale);

        }

        public void CreateVisualMeshNormals(PrimitiveIndexedMesh mesh, Texture2D texture, float thickness, float scale)
        {
            VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; i++)
                tmp[i] = new VertexPositionNormalTexture() { Position = mesh.vertices[i].Position, Normal = mesh.vertices[i].Normal, TextureCoordinate = mesh.vertices[i].TextureCoordinate };
            visualNormals.CreateVisualNormalsForPrimitiveMesh(tmp, mesh.indices, texture, thickness, scale);
            visualNormals.SetUpBasicEffect(GraphicsDevice, texture, cam.view, cam.projection);
        }

        public void CreateVisualMeshTangents(PrimitiveIndexedMesh mesh, Texture2D texture, float thickness, float scale)
        {
            VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; i++)
                tmp[i] = new VertexPositionNormalTexture() { Position = mesh.vertices[i].Position, Normal = mesh.vertices[i].Tangent, TextureCoordinate = mesh.vertices[i].TextureCoordinate };
            visualTangents.CreateVisualNormalsForPrimitiveMesh(tmp, mesh.indices, texture, thickness, scale);
            visualTangents.SetUpBasicEffect(GraphicsDevice, texture, cam.view, cam.projection);
        }

        public void CreateVisualLightNormal(Texture2D texture, Vector3 startPosition, Vector3 endPosition, float thickness, Color color)
        {
            visualLightNormal = new VisualizationLine();
            visualLightNormal.CreateVisualLine(texture, startPosition, endPosition, thickness, color);
            visualLightNormal.SetUpBasicEffect(GraphicsDevice, texture, cam.view, cam.projection);
        }

        public void CreateVisualSphereNormals(PrimitiveSphere sphere, Texture2D texture, float thickness, float scale)
        {
            VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[sphere.cubesFaceVertices.Length];
            for (int i = 0; i < sphere.cubesFaceVertices.Length; i++)
                tmp[i] = new VertexPositionNormalTexture() { Position = sphere.cubesFaceVertices[i].Position, Normal = sphere.cubesFaceVertices[i].Normal, TextureCoordinate = sphere.cubesFaceVertices[i].TextureCoordinate };
            visualSphereNormals.CreateVisualNormalsForPrimitiveMesh(tmp, sphere.cubesFacesIndices, texture, thickness, scale);
            visualSphereNormals.SetUpBasicEffect(GraphicsDevice, texture, cam.view, cam.projection);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            cam.Update(gameTime);

            if (Keys.F1.IsKeyPressedWithDelay(gameTime))
                displayOnScreenText = !displayOnScreenText;
            if (Keys.F2.IsKeyPressedWithDelay(gameTime))
                displayWireframe = !displayWireframe;
            if (Keys.F3.IsKeyPressedWithDelay(gameTime))
                displayNormals = !displayNormals;
            if (Keys.F4.IsKeyPressedWithDelay(gameTime))
                displayMesh = !displayMesh;
            if (Keys.F5.IsKeyPressedWithDelay(gameTime))
                displayWhiteDiffuse = !displayWhiteDiffuse;

            // Reset the view projection matrix.
            if (Keys.Home.IsKeyPressedWithDelay(gameTime))
                cam.InitialView(GraphicsDevice);

            if (rotateLight)
            {
                if (Keys.OemPlus.IsKeyDown())
                    lightRotationRadians += .005f;
                if (Keys.OemMinus.IsKeyDown())
                    lightRotationRadians -= .005f;
                if (Keys.OemCloseBrackets.IsKeyDown())
                    lightStartPosition.Z += 5f;
                if (Keys.OemOpenBrackets.IsKeyDown())
                    lightStartPosition.Z -= 5f;

                if (lightRotationRadians > 6.28318f)
                    lightRotationRadians = 0;
                if (lightRotationRadians < 0 )
                    lightRotationRadians = 6.283f;

                var axisOfRotation = new Vector3(0, 0, 1);
                lightTransform = Matrix.CreateFromAxisAngle(axisOfRotation, lightRotationRadians);
                lightPosition = Vector3.Transform(lightStartPosition, lightTransform); // + meshCenter;
                var start = meshCenter;
                var end = lightPosition + meshCenter;
                CreateVisualLightNormal(dotTextureWhite, start, end, 1, Color.White);
            }

            base.Update(gameTime);
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


            if (displayMesh)
                DrawMesh();

            if (displayNormals)
            {
                DrawNormalsForMesh();
                DrawTangentsForMesh();
                DrawLightLine();
            }

            if (displayWireframe)
                DrawWireFrameMesh();


            DrawSphere();
            DrawNormalsForSphere();


            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        public void DrawMesh()
        {
            if (displayWhiteDiffuse)
                mesh.DiffuseTexture = dotTextureWhite;
            else
                mesh.DiffuseTexture = texture;

            SpecularLightEffectClass.View = cam.view;
            SpecularLightEffectClass.Projection = cam.projection;
            SpecularLightEffectClass.World = Matrix.Identity;
            SpecularLightEffectClass.TextureDiffuse = mesh.DiffuseTexture;
            SpecularLightEffectClass.TextureNormalMap = mesh.NormalMapTexture;
            SpecularLightEffectClass.LightPosition = lightPosition;
            SpecularLightEffectClass.CameraPosition = cam.cameraWorld.Translation;
            mesh.DrawPrimitive(GraphicsDevice, SpecularLightEffectClass.effect);
        }

        public void DrawNormalsForMesh()
        {
            visualNormals.World = Matrix.Identity;
            visualNormals.View = cam.view;
            visualNormals.Draw(GraphicsDevice);
        }

        public void DrawTangentsForMesh()
        {
            visualTangents.World = Matrix.Identity;
            visualTangents.View = cam.view;
            visualTangents.Draw(GraphicsDevice);
        }

        public void DrawLightLine()
        {
            visualLightNormal.World = Matrix.Identity; //lightTransform;
            visualLightNormal.View = cam.view;
            visualLightNormal.Draw(GraphicsDevice);
        }

        public void DrawWireFrameMesh()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_WIREFRAME;
            visualNormals.World = Matrix.Identity;
            visualNormals.View = cam.view;
            SpecularLightEffectClass.TextureDiffuse = dotTextureRed;
            mesh.DrawPrimitive(GraphicsDevice, SpecularLightEffectClass.effect);
        }

        public void DrawSphere()
        {
            DiffuseLightEffectClass.SpriteTexture = texture;
            DiffuseLightEffectClass.LightPosition = lightPosition;
            DiffuseLightEffectClass.World = Matrix.CreateTranslation(new Vector3(0, 0, -50));
            DiffuseLightEffectClass.View = cam.view;
            DiffuseLightEffectClass.Projection = cam.projection;
            sphere.DrawPrimitiveSphere(GraphicsDevice, DiffuseLightEffectClass.effect);
        }

        public void DrawNormalsForSphere()
        {
            visualSphereNormals.World = Matrix.CreateTranslation(new Vector3(0, 0, -50));
            visualSphereNormals.View = cam.view;
            visualSphereNormals.Projection = cam.projection;
            visualSphereNormals.Draw(GraphicsDevice);
        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            // Draw all the regular stuff
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);

                string msg =
                    $" \n The keys WASD change the forward view direction (which is the major take away here). ZC allows for spin." +
                    $" \n The Arrow keys move the camera translation as strafing motion. " +
                    $" \n The F2 toggle wireframe. F3 show normals. F4 mesh itself. F5 the texture used." +
                    $" \n  " +
                    $" \n In this example we make a shader that creates a diffuse light." +
                    $" \n The light rotates around the mesh illuminating faces depending on the triangle normals." +
                    $" \n We also create a class that allows us to visualize normals per vertice and for the light." +
                    $" \n Well place the light into rotation so we can see how the diffuse shader works." +
                    $" \n Simple diffuse lighting is achieved via a dot product on the light and normals aka NdotL ." +
                    $" \n this can be found in the shader" +
                    $" \n  " +
                    $" \n { cam.cameraWorld.DisplayMatrix("cameraWorld") }" +
                    $" \n { cam.view.DisplayMatrix("view") }" +
                    $" \n { cam.projection.DisplayMatrix("projection") }" +
                    $" \n" +
                    $" \n {lightPosition.VectorToString("LightPosition")}" +
                    $" \n"
                    ;

            // quik test
            //MgDrawExt.Initialize(GraphicsDevice, spriteBatch);
            //spriteBatch.DrawCircleOutline(new Vector2(300, 300), 100, 25, 1, Color.Yellow);

            if (displayOnScreenText)
                spriteBatch.DrawString(font, msg, new Vector2(10, 10), Color.Blue);
            else
                spriteBatch.DrawString(font, "Press F1 for information" , new Vector2(10, 10), Color.Blue);

            spriteBatch.End();
        }


    }


    /// <summary>
    /// This class creates renderable lines to aid in visualizing vertex normals or tangents.
    /// </summary>
    public class VisualizationLine
    {
        public VertexPositionNormalTexture[] vertices;
        public int[] indices;
        public Texture2D texture;

        public BasicEffect basicEffect;

        public VisualizationLine() { }

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

        public void CreateVisualLine(Texture2D t, Vector3 start, Vector3 end, float thickness, Color c)
        {
            texture = t;
            int inVertsLen = 1;

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
            for (int j = 0; j < inVertsLen; j++)
            {
                int startvert = j * lineVerts;
                int startindices = j * lineIndices;

                var dir = end - start;
                var scale = Vector3.Distance(end, start);

                var p = start;
                var n = Vector3.Normalize(dir);
                var p2 = n * scale + p;

                int index = 0;
                float radMult = 6.28f / sides;
                for (int k = 0; k < sides; k++)
                {
                    float rads = (float)(k) * radMult;
                    var m = Matrix.CreateFromAxisAngle(n, rads);
                    var mright = m.Right;
                    var np = p + m.Right * thickness;
                    var np2 = p2 + m.Right * thickness;

                    var v0 = new VertexPositionNormalTexture() { Position = np, Normal = n, TextureCoordinate = new Vector2((float)(k) / (float)(sides - 1), 0f) };
                    var v1 = new VertexPositionNormalTexture() { Position = np2, Normal = n, TextureCoordinate = new Vector2((float)(k) / (float)(sides - 1), 1f) };
                    nverts.Add(v0);
                    nverts.Add(v1);

                    index += 2;
                }
            }

            // build the indices and line them up to the vertices.
            for (int j = 0; j < inVertsLen; j++)
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
