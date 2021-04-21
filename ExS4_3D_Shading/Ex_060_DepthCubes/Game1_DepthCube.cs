
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_DepthCube : Game
    {
        bool manuallyRotateLight = false;
        bool displayMesh = true;
        bool displayMeshTerrain = true;
        bool displayWireframe = false;
        bool displayNormals = false;
        bool displayWhiteDiffuse = false;
        bool displayExtraVisualStuff = false;
        bool displayOnScreenTextInfo = false;
        bool CullOutCounterClockWiseTriangles = true;

        //bool lightAutoAxisFlip = false;

        RasterizerState rasterizerState_CULLNONE_WIREFRAME = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        RasterizerState rasterizerState_CULLNONE_SOLID = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.Solid };

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont  font , font2, font3;
        TextureCube textureCubeDiffuse,   textureCubeDiffuseIrradiance;
        Texture2D  textureMeshTerrain, textureMeshNormalMapTerrain, textureHdrLdrSphere, textureHdrLdrSphereIllumination, textureHdrLdrSphereNormalMap, textureMonogameLogo ;
        RenderTargetCube renderTargetDepthCube, renderTargetReflectionCube;
        RenderTarget2D rtScene;
        Matrix  shadowMapProjection;

        CameraAndKeyboardControls cam = new CameraAndKeyboardControls();

        PrimitiveIndexedMesh meshTerrain;
        //VisualizationNormals visualMesh1Normals = new VisualizationNormals();
        //VisualizationNormals visualMesh1Tangents = new VisualizationNormals();

        public static int numberOfSpheres = 2;
        PrimitiveSphere[] spheres = new PrimitiveSphere[numberOfSpheres];
        //VisualizationNormals[] visualSphereNormals = new VisualizationNormals[numberOfSpheres];
        //VisualizationNormals[] visualSphereTangents = new VisualizationNormals[numberOfSpheres];
        //VisualizationLine[] visualLightLineToSpheres = new VisualizationLine[numberOfSpheres];

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

        Vector3 lightStartPosition = new Vector3(1, 125, 3000); // new Vector3(1, 800, 300)
        Vector3 lightPosition = new Vector3(0, 0, 0);
        Matrix lightTransform = Matrix.Identity;


        public Game1_DepthCube()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Depth Cube Mapping";
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

            // aspect ratio 1 needs a corresponding pfov aspect.
            shadowMapProjection = Matrix.CreatePerspectiveFieldOfView((float)MathHelper.Pi * .5f, 1, .1f, 10000f);
            
            // render target cubes 512
            renderTargetDepthCube = new RenderTargetCube(GraphicsDevice, 512, false, SurfaceFormat.Single, DepthFormat.Depth24);
            renderTargetReflectionCube = new RenderTargetCube(GraphicsDevice, 512, false, SurfaceFormat.Color, DepthFormat.Depth24);

            SetCamera();

            LoadFontsTextures();

            LoadAndSetupInitialEnvEffect();

            CreateMeshGrids();

            CreateSpheres();
        }

        public void LoadFontsTextures()
        {
            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");
            font3 = Content.Load<SpriteFont>("MgFont3");

            Content.RootDirectory = @"Content/Images";
            
            textureMonogameLogo = Content.Load<Texture2D>("MG_Logo_Modifyed");
            textureMeshTerrain = Content.Load<Texture2D>("MG_Logo_Modifyed");
            textureMeshNormalMapTerrain = Content.Load<Texture2D>("wallnormmapGimp");

            textureHdrLdrSphere = Content.Load<Texture2D>("Eqr001_Diffuse");
            textureHdrLdrSphereNormalMap = Content.Load<Texture2D>("wallnormmap");
            textureHdrLdrSphereIllumination = Content.Load<Texture2D>("Eqr001_Diffuse_irradiance");

            TextureCubeTypeConverter.Load(Content);
            textureCubeDiffuse = TextureCubeTypeConverter.ConvertSphericalTexture2DToTextureCube(GraphicsDevice, textureHdrLdrSphere, false, false, textureHdrLdrSphere.Width);
            textureCubeDiffuseIrradiance = TextureCubeTypeConverter.ConvertSphericalTexture2DToTextureCube(GraphicsDevice, textureHdrLdrSphereIllumination, false, false, textureHdrLdrSphereIllumination.Width);
        }

        public void SetCamera()
        {
            cam.InitialView(GraphicsDevice, new Vector3(+0f, +0f, -381.373f), Vector3.Backward, Vector3.Down);
            cam.UpdateProjection(GraphicsDevice);
        }

        //++++++++++++++++++++++++++++++++++
        // Create
        //++++++++++++++++++++++++++++++++++

        public void CreateMeshGrids()
        {
            // different ways to create the mesh regular via a height array or a texture used as a height / displacement map 
            float thickness = .2f; 
            float normtanLinescale = 20f;

            // mesh terrain

            string option = "textureAsHeightMap";
            switch (option)
            {
                case "regularGrid":
                    meshTerrain = new PrimitiveIndexedMesh(5, 5, new Vector3(1000f, 1000, 0f), false, false, false);
                    break;
                case "heightMap":
                    meshTerrain = new PrimitiveIndexedMesh(heightMap, 9, new Vector3(1000f, 1000, 70f), false, false, false);
                    break;
                case "textureAsHeightMap":
                    PrimitiveIndexedMesh.AveragingOption = PrimitiveIndexedMesh.AVG_OPTION_USE_PREMULT_NON_ALPHA_AS_ONE;
                    meshTerrain = new PrimitiveIndexedMesh(textureMeshTerrain, new Vector3(1000f, 1000, 50f), false, false, false);
                    break;
            }
            meshTerrain.SetWorldTransformation(new Vector3(-300, +300, 550), Vector3.Down, Vector3.Forward, Vector3.One);
            meshTerrain.DiffuseTexture = textureMeshTerrain;
            meshTerrain.NormalMapTexture = textureMeshNormalMapTerrain;
        }

        public void CreateSpheres()
        {
            Vector3[] sphereCenters = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 150, 50), new Vector3(150, 150, 50) };

            for (int index = 0; index < spheres.Length; index++)
            {
                int snum = index;
                while (snum > 3)
                    snum = snum - 4;
                int usage = 0;
                float scale = 50;
                switch (snum)
                {
                    case 0:
                        usage = PrimitiveSphere.USAGE_CUBE_UNDER_CCW; 
                        scale = 40;
                        CreateSphere( textureCubeDiffuse, ref spheres[index], sphereCenters[index], scale, usage, false, false);
                        break;
                    case 1:
                        usage = PrimitiveSphere.USAGE_SKYSPHERE_UNDER_CCW; 
                        scale = 1000;
                        CreateSphere(textureCubeDiffuse, ref spheres[index], sphereCenters[index], scale, usage, false, false);
                        break;
                }
            }
        }

        public void CreateSphere(TextureCube textureCube, ref PrimitiveSphere asphere, Vector3 sphereCenter, float spherescale, int primUsage, bool invert, bool flatfaces)
        {
            asphere = new PrimitiveSphere(2, 2, 1f, primUsage, invert, flatfaces);
            asphere.SetWorldTransformation(sphereCenter, Vector3.Forward, Vector3.Up, spherescale);
            asphere.textureCube = textureCube;
        }

        public VisualizationLine CreateVisualLine(Texture2D texture, Vector3 startPosition, Vector3 endPosition, float thickness, Color color)
        {
            var vln = new VisualizationLine(texture, startPosition, endPosition, thickness, color);
            vln.SetUpBasicEffect(GraphicsDevice, texture, cam.view, cam.projection);
            return vln;
        }

        public void LoadAndSetupInitialEnvEffect()
        {
            ReflectionCubeEffectClass.Load(Content);
            ReflectionCubeEffectClass.Technique_Render_PhongWithNormMapEnviromentalLight();
            ReflectionCubeEffectClass.TextureCubeDiffuse = textureCubeDiffuse;
            ReflectionCubeEffectClass.TextureCubeEnviromental = textureCubeDiffuseIrradiance;
            ReflectionCubeEffectClass.AmbientStrength = 0.4f;
            ReflectionCubeEffectClass.DiffuseStrength = .30f;
            ReflectionCubeEffectClass.SpecularStrength = .30f;
            ReflectionCubeEffectClass.View = cam.view;
            ReflectionCubeEffectClass.Projection = cam.projection;
            ReflectionCubeEffectClass.CameraPosition = cam.cameraWorld.Translation;
            ReflectionCubeEffectClass.LightPosition = lightPosition;
            ReflectionCubeEffectClass.LightColor = new Vector3(1f, 1f, 1f);
        }

        protected override void UnloadContent()
        {
        }



        //++++++++++++++++++++++++++++++++++
        // update
        //++++++++++++++++++++++++++++++++++

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            cam.Update(gameTime);

            if (Keys.F1.IsKeyPressedWithDelay(gameTime))
                displayOnScreenTextInfo = !displayOnScreenTextInfo;
            if (Keys.F2.IsKeyPressedWithDelay(gameTime))
                displayWireframe = !displayWireframe;
            if (Keys.F3.IsKeyPressedWithDelay(gameTime))
                displayNormals = !displayNormals;
            if (Keys.F4.IsKeyPressedWithDelay(gameTime))
                displayMesh = !displayMesh;
            if (Keys.F5.IsKeyPressedWithDelay(gameTime))
                displayExtraVisualStuff = !displayExtraVisualStuff;
            if (Keys.F6.IsKeyPressedWithDelay(gameTime))
                displayWhiteDiffuse = !displayWhiteDiffuse;

            if (Keys.F7.IsKeyPressedWithDelay(gameTime))
                CullOutCounterClockWiseTriangles = !CullOutCounterClockWiseTriangles;
            if (Keys.Home.IsKeyPressedWithDelay(gameTime))
                cam.InitialView(GraphicsDevice);


            base.Update(gameTime);
        }



        //++++++++++++++++++++++++++++++++++
        // draw
        //++++++++++++++++++++++++++++++++++

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            if (CullOutCounterClockWiseTriangles)
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            else
                GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            DepthRenderSceneFaces();
            GraphicsDevice.SetRenderTarget(null);

            DrawDepthRender();

            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        #region drawscene with reflection and depth.

        void DepthRenderSceneFaces()
        {
            GraphicsDevice.SetRenderTarget(renderTargetDepthCube, CubeMapFace.NegativeX);
            CreateAndSetCubeFaceView(TextureCubeTypeConverter.MatrixNegativeX);
            DepthRenderScene();
            GraphicsDevice.SetRenderTarget(renderTargetDepthCube, CubeMapFace.NegativeY);
            CreateAndSetCubeFaceView(TextureCubeTypeConverter.MatrixNegativeY);
            DepthRenderScene();
            GraphicsDevice.SetRenderTarget(renderTargetDepthCube, CubeMapFace.NegativeZ);
            CreateAndSetCubeFaceView(TextureCubeTypeConverter.MatrixNegativeZ);
            DepthRenderScene();
            GraphicsDevice.SetRenderTarget(renderTargetDepthCube, CubeMapFace.PositiveX);
            CreateAndSetCubeFaceView(TextureCubeTypeConverter.MatrixPositiveX);
            DepthRenderScene();
            GraphicsDevice.SetRenderTarget(renderTargetDepthCube, CubeMapFace.PositiveY);
            CreateAndSetCubeFaceView(TextureCubeTypeConverter.MatrixPositiveY);
            DepthRenderScene();
            GraphicsDevice.SetRenderTarget(renderTargetDepthCube, CubeMapFace.PositiveZ);
            CreateAndSetCubeFaceView(TextureCubeTypeConverter.MatrixPositiveZ);
            DepthRenderScene();
        }

        void CreateAndSetCubeFaceView(Matrix face)
        {
            var view = Matrix.Invert(face);
            ReflectionCubeEffectClass.View = view;
        }

        void SetProjection(Matrix Projection)
        {
            ReflectionCubeEffectClass.Projection = Projection;
        }

        public void DepthRenderScene()
        {
            ReflectionCubeEffectClass.View = cam.view;
            ReflectionCubeEffectClass.Projection = cam.projection;
            ReflectionCubeEffectClass.LightPosition = lightPosition;
            ReflectionCubeEffectClass.CameraPosition = cam.cameraWorld.Translation;

            ReflectionCubeEffectClass.Technique_Render_LightDepth();

            // S P H E R E S   S K Y    

            for (int index = 0; index < spheres.Length; index++)
            {
                ReflectionCubeEffectClass.World = spheres[index].WorldTransformation;
                if (spheres[index].IsSkyBox)
                    spheres[index].DrawPrimitiveSphere(GraphicsDevice, ReflectionCubeEffectClass.effect);
                else
                    spheres[index].DrawPrimitiveSphere(GraphicsDevice, ReflectionCubeEffectClass.effect);
            }

            // M E S H

            if (displayMeshTerrain)
            {
                ReflectionCubeEffectClass.World = meshTerrain.WorldTransformation;
                meshTerrain.DrawPrimitive(GraphicsDevice, ReflectionCubeEffectClass.effect);
            }
        }

        void DrawDepthRender()
        {
            for (int index = 0; index < spheres.Length; index++)
            {
                ReflectionCubeEffectClass.World = spheres[index].WorldTransformation;
                ReflectionCubeEffectClass.TextureCubeDiffuse = renderTargetDepthCube;
                ReflectionCubeEffectClass.TextureCubeEnviromental = renderTargetDepthCube;

                ReflectionCubeEffectClass.Technique_Render_VisualizationDepthCube();
                if (spheres[index].IsSkyBox == false)
                    spheres[index].DrawPrimitiveSphere(GraphicsDevice, ReflectionCubeEffectClass.effect);
            }
        }


        #endregion


        public void DrawSpriteBatches(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            // Draw all the regular stuff
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);

            var whatIdrewWithCullWise = (CullOutCounterClockWiseTriangles == true) ? "CounterClockwise" : "ClockWise";

            string msg =
                    $" \n  " +
                    $" \n  " +
                    $" \n  " +
                    $" \n  " +
                    $" \n"
                    ;

            if (displayOnScreenTextInfo)
                spriteBatch.DrawString(font, msg, new Vector2(10, 10), Color.Red);
            else
                spriteBatch.DrawString(
                    font, 
                    $"Press F1 for information  " +
                    $"\n" +
                    $"" , 
                    new Vector2(10, 10), 
                    Color.Red
                    );

            if (Keys.End.IsKeyPressedWithDelay(gameTime))
                Console.WriteLine( $"{cam.cameraWorld.ToDisplayMatrixForCopy("cameraWorld") } ");

            spriteBatch.End();
        }

    }
}
