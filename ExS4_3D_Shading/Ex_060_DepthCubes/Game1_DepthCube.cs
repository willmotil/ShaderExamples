
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_DepthCube : Game
    {
        bool displayMesh = true;
        bool displayMeshTerrain = true;
        bool displayWireframe = false;
        bool displayNormals = false;
        bool displayWhiteDiffuse = false;
        bool displayExtraVisualStuff = false;
        bool displayOnScreenTextInfo = false;
        bool displayReflectionRender = true;
        bool CullOutCounterClockWiseTriangles = true;

        //bool lightAutoAxisFlip = false;

        RasterizerState rasterizerState_CULLNONE_WIREFRAME = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        RasterizerState rasterizerState_CULLNONE_SOLID = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.Solid };

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont  font , font2, font3;
        TextureCube textureCubeDiffuse,   textureCubeDiffuseIrradiance, textureCubeEnvGeneratedFromSixImages, textureCubeFromRenderTarget;
        Texture2D  textureMeshTerrain, textureMeshNormalMapTerrain, textureHdrLdrSphere, textureHdrLdrSphereIllumination, textureHdrLdrSphereNormalMap, textureMonogameLogo;
        Texture2D generatedTextureHdrLdrFromManuallySetSingleImages, generatedTextureHdrLdrFromGeneratedEnvCubeMap, generatedTextureHdrLdrFromRenderTargetCube,
                        faceFront, faceBack, faceLeft, faceRight, faceTop, faceBottom
                        ;
        Texture2D[] textureArrayManuallySet, generatedTextureFaceArrayFromRenderTargetCubemap, generatedTextureFaceArrayFromGeneratedEnvCubemap ,  generatedTextureFaceArrayFromHdrLdr;

        RenderTarget2D rtScene;
        RenderTargetCube renderTargetDepthCube, renderTargetReflectionCube;
        Matrix  shadowMapProjection;

        CameraAndKeyboardControls cam = new CameraAndKeyboardControls();

        PrimitiveIndexedMesh meshTerrain;

        public static int numberOfSpheres = 2;
        PrimitiveSphere[] spheres = new PrimitiveSphere[numberOfSpheres];

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

        Vector3 lightStartPosition = new Vector3(0, 0, 0);  //new Vector3(1, 125, 3000); // new Vector3(1, 800, 300)
        Vector3 lightPosition = new Vector3(0, 0, 0);
        Matrix lightTransform = Matrix.Identity;
        float lightRotationRadiansX = 0f;
        float lightRotationRadiansY = 0f;

        bool manuallyRotateLight = true;
        bool lightAutoAxisFlip = false;


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
            MgDrawExt.Initialize(GraphicsDevice, spriteBatch);

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

            faceFront = Content.Load<Texture2D>("Face_Front");
            faceBack = Content.Load<Texture2D>("Face_Back");
            faceLeft = Content.Load<Texture2D>("Face_Left");
            faceRight = Content.Load<Texture2D>("Face_Right");
            faceTop = Content.Load<Texture2D>("Face_Top");
            faceBottom = Content.Load<Texture2D>("Face_Bottom");

            TextureCubeTypeConverter.Load(Content);

            textureCubeEnvGeneratedFromSixImages = TextureCubeTypeConverter.ConvertTexture2DsToTextureCube
            (
                GraphicsDevice,
                faceRight,
                faceLeft,
                faceTop,
                faceBottom,
                faceFront,
                faceBack,
                false, false, 256 // textureMonogameLogo.Width
            );
            generatedTextureFaceArrayFromGeneratedEnvCubemap = TextureCubeTypeConverter.ConvertTextureCubeToTexture2DArray(GraphicsDevice, textureCubeEnvGeneratedFromSixImages, false, false, 256);
            generatedTextureHdrLdrFromGeneratedEnvCubeMap = TextureCubeTypeConverter.ConvertTextureCubeToSphericalTexture2D(GraphicsDevice, textureCubeEnvGeneratedFromSixImages, false, false, 256);


            textureArrayManuallySet = new Texture2D[] { faceRight, faceLeft, faceTop, faceBottom, faceFront, faceBack };
            generatedTextureHdrLdrFromManuallySetSingleImages = TextureCubeTypeConverter.ConvertTexture2DArrayToSphericalTexture2D(GraphicsDevice, textureArrayManuallySet, false, false, 256);



            textureCubeDiffuse = textureCubeEnvGeneratedFromSixImages;
            //textureCubeDiffuse = TextureCubeTypeConverter.ConvertSphericalTexture2DToTextureCube(GraphicsDevice, textureHdrLdrSphere, false, false, textureHdrLdrSphere.Width);
            //textureCubeDiffuseIrradiance = TextureCubeTypeConverter.ConvertSphericalTexture2DToTextureCube(GraphicsDevice, textureHdrLdrSphereIllumination, false, false, textureHdrLdrSphereIllumination.Width);
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
            DepthCubeEffectClass.Load(Content);
            DepthCubeEffectClass.TextureCubeDiffuse = textureCubeDiffuse;
            DepthCubeEffectClass.View = cam.view;
            DepthCubeEffectClass.Projection = cam.projection;
            DepthCubeEffectClass.CameraPosition = cam.cameraWorld.Translation;
            DepthCubeEffectClass.LightPosition = lightPosition;
            DepthCubeEffectClass.LightColor = new Vector3(1f, 1f, 1f);
        }

        protected override void UnloadContent()
        {
        }



        //++++++++++++++++++++++++++++++++++
        // update
        //++++++++++++++++++++++++++++++++++

        float elapsed;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

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


            if (Keys.F8.IsKeyPressedWithDelay(gameTime))
                displayReflectionRender = !displayReflectionRender;

            UpdateLight(gameTime);

            base.Update(gameTime);
        }


        public void UpdateLight(GameTime gameTime)
        {
            if (Keys.Space.IsKeyPressedWithDelay(gameTime))
                manuallyRotateLight = !manuallyRotateLight;
            if (manuallyRotateLight)
            {
                if (Keys.OemPlus.IsKeyDown())
                    lightRotationRadiansX += .05f;
                if (Keys.OemMinus.IsKeyDown())
                    lightRotationRadiansX -= .05f;
                if (Keys.OemOpenBrackets.IsKeyDown())
                    lightRotationRadiansY += .05f;
                if (Keys.OemCloseBrackets.IsKeyDown())
                    lightRotationRadiansY -= .05f;

                lightTransform = Matrix.CreateRotationX(lightRotationRadiansX) * Matrix.CreateRotationY(lightRotationRadiansY);
                lightPosition = Vector3.Transform(lightStartPosition, lightTransform);
            }
            else
            {
                lightRotationRadiansX += .01f;
                if (lightRotationRadiansX > 6.28318f)
                    lightAutoAxisFlip = !lightAutoAxisFlip;
                var axisOfRotation = new Vector3(1, 0, 0);
                if (lightAutoAxisFlip)
                    axisOfRotation = new Vector3(0, 1, 0);
                lightTransform = Matrix.CreateFromAxisAngle(axisOfRotation, lightRotationRadiansX);
                lightPosition = Vector3.Transform(lightStartPosition, lightTransform);
            }
            if (lightRotationRadiansX > 6.28318f)
                lightRotationRadiansX = 0;
            if (lightRotationRadiansX < 0)
                lightRotationRadiansX = 6.283f;
            if (lightRotationRadiansY > 6.28318f)
                lightRotationRadiansY = 0;
            if (lightRotationRadiansY < 0)
                lightRotationRadiansY = 6.283f;
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

            // depth render

            //SetProjection(shadowMapProjection);
            //DepthRenderSceneFaces(false);
            //GraphicsDevice.SetRenderTarget(null);
            //generatedTextureFaceArrayFromRenderTargetCubemap = TextureCubeTypeConverter.ConvertTextureCubeToTexture2DArray(GraphicsDevice, renderTargetDepthCube, false, false, 256);

            //DepthCubeEffectClass.View = cam.view;
            //SetProjection(cam.projection);
            //DrawDepthRender();



            // reflection render.

            DepthCubeEffectClass.UseFlips = false;

            SetProjection(shadowMapProjection);
            ReflectionRenderSceneFaces(false);
            GraphicsDevice.SetRenderTarget(null);
            generatedTextureFaceArrayFromRenderTargetCubemap = TextureCubeTypeConverter.ConvertTextureCubeToTexture2DArray(GraphicsDevice, renderTargetReflectionCube, false, false, 256);
            generatedTextureHdrLdrFromRenderTargetCube = TextureCubeTypeConverter.ConvertTextureCubeToSphericalTexture2D(GraphicsDevice, renderTargetReflectionCube, false, false, 256);

            DepthCubeEffectClass.UseFlips = false;

            DepthCubeEffectClass.View = cam.view;
            SetProjection(cam.projection);

            if (displayReflectionRender)
                DrawReflectionRender();
            else
                DrawRegularRender();

            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        #region drawscene with reflection and depth.


        void CreateAndSetCubeFaceView(Matrix face, bool invert)
        {
            if (invert)
            {
                face = Matrix.Invert(face);
                //face =
                //    new Matrix
                //    (
                //    -1, 0, 0, 0,
                //    0, 1, 0, 0,
                //    0, 0, 1, 0,
                //    0, 0, 0, 1
                //    )
                //    * face
                //    ;
                //                 face = CreateLhLookAt(face.Translation, face.Forward + face.Translation, face.Up);
            }
            DepthCubeEffectClass.View = face;
        }

        public static Matrix CreateLhLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            var vector = Vector3.Normalize(cameraPosition - cameraTarget);
            var vector2 = -Vector3.Normalize(Vector3.Cross(cameraUpVector, vector));
            var vector3 = Vector3.Cross(-vector, vector2);
            Matrix result = Matrix.Identity;
            result.M11 = vector2.X;
            result.M12 = vector3.X;
            result.M13 = vector.X;
            result.M14 = 0f;
            result.M21 = vector2.Y;
            result.M22 = vector3.Y;
            result.M23 = vector.Y;
            result.M24 = 0f;
            result.M31 = vector2.Z;
            result.M32 = vector3.Z;
            result.M33 = vector.Z;
            result.M34 = 0f;
            result.M41 = -Vector3.Dot(vector2, cameraPosition);
            result.M42 = -Vector3.Dot(vector3, cameraPosition);
            result.M43 = -Vector3.Dot(vector, cameraPosition);
            result.M44 = 1f;
            return result;
        }

        void SetProjection(Matrix Projection)
        {
            DepthCubeEffectClass.Projection = Projection;
        }

        void ReflectionRenderSceneFaces(bool invert)
        {
            GraphicsDevice.SetRenderTarget(renderTargetReflectionCube, CubeMapFace.NegativeX);
            var m = TextureCubeTypeConverter.MatrixNegativeX; m.Translation = lightPosition;
            CreateAndSetCubeFaceView(m, invert);
            ReflectionRenderScene();

            GraphicsDevice.SetRenderTarget(renderTargetReflectionCube, CubeMapFace.NegativeY);
            m = TextureCubeTypeConverter.MatrixNegativeY; m.Translation = lightPosition;
            CreateAndSetCubeFaceView(m, invert);
            ReflectionRenderScene();

            GraphicsDevice.SetRenderTarget(renderTargetReflectionCube, CubeMapFace.NegativeZ);
            m = TextureCubeTypeConverter.MatrixNegativeZ; m.Translation = lightPosition;
            CreateAndSetCubeFaceView(m, invert);
            ReflectionRenderScene();

            GraphicsDevice.SetRenderTarget(renderTargetReflectionCube, CubeMapFace.PositiveX);
            m = TextureCubeTypeConverter.MatrixPositiveX; m.Translation = lightPosition;
            CreateAndSetCubeFaceView(m, invert);
            ReflectionRenderScene();

            GraphicsDevice.SetRenderTarget(renderTargetReflectionCube, CubeMapFace.PositiveY);
            m = TextureCubeTypeConverter.MatrixPositiveY; m.Translation = lightPosition;
            CreateAndSetCubeFaceView(m, invert);
            ReflectionRenderScene();

            GraphicsDevice.SetRenderTarget(renderTargetReflectionCube, CubeMapFace.PositiveZ);
            m = TextureCubeTypeConverter.MatrixPositiveZ; m.Translation = lightPosition;
            CreateAndSetCubeFaceView(m, invert);
            ReflectionRenderScene();
        }


        public void ReflectionRenderScene()
        {
            DepthCubeEffectClass.LightPosition = lightPosition;
            DepthCubeEffectClass.CameraPosition = cam.cameraWorld.Translation;


            // S P H E R E S   S K Y    

            //DepthCubeEffectClass.Technique_Render_BasicSkyCubeMapScene();
            DepthCubeEffectClass.Technique_Render_BasicUnalteredRenderTargetCubeMap();

            DepthCubeEffectClass.TextureCubeDiffuse = textureCubeEnvGeneratedFromSixImages;
            DepthCubeEffectClass.World = spheres[1].WorldTransformation;
            spheres[1].DrawPrimitiveSphere(GraphicsDevice, DepthCubeEffectClass.effect);

            // M E S H

            DepthCubeEffectClass.Technique_Render_BasicScene();

            DepthCubeEffectClass.World = meshTerrain.WorldTransformation;
            meshTerrain.DrawPrimitive(GraphicsDevice, DepthCubeEffectClass.effect);
           
        }

        void DrawReflectionRender()
        {
            DepthCubeEffectClass.TextureCubeDiffuse = renderTargetReflectionCube;

            DepthCubeEffectClass.Technique_Render_BasicUnalteredRenderTargetCubeMap();

            //DepthCubeEffectClass.Technique_Render_BasicCubeMapScene();
            DepthCubeEffectClass.World = spheres[0].WorldTransformation;
            spheres[0].DrawPrimitiveSphere(GraphicsDevice, DepthCubeEffectClass.effect);


            //DepthCubeEffectClass.Technique_Render_BasicSkyCubeMapScene();
            DepthCubeEffectClass.World = spheres[1].WorldTransformation;
            spheres[1].DrawPrimitiveSphere(GraphicsDevice, DepthCubeEffectClass.effect);
        }

        #endregion

        public void DrawRegularRender()
        {
            DepthCubeEffectClass.Technique_Render_BasicScene();
            DepthCubeEffectClass.World = meshTerrain.WorldTransformation;
            DepthCubeEffectClass.TextureDiffuse = textureMonogameLogo;
            meshTerrain.DrawPrimitive(GraphicsDevice, DepthCubeEffectClass.effect);


            DepthCubeEffectClass.TextureCubeDiffuse = textureCubeEnvGeneratedFromSixImages;

            DepthCubeEffectClass.Technique_Render_BasicCubeMapScene();
            DepthCubeEffectClass.World = spheres[0].WorldTransformation;
            spheres[0].DrawPrimitiveSphere(GraphicsDevice, DepthCubeEffectClass.effect);

            DepthCubeEffectClass.Technique_Render_BasicSkyCubeMapScene();
            DepthCubeEffectClass.World = spheres[1].WorldTransformation;
            spheres[1].DrawPrimitiveSphere(GraphicsDevice, DepthCubeEffectClass.effect);
        }


        // __ depth


        void DepthRenderSceneFaces(bool invert)
        {
            GraphicsDevice.SetRenderTarget(renderTargetDepthCube, CubeMapFace.NegativeX);
            var m = TextureCubeTypeConverter.MatrixNegativeX; m.Translation = lightPosition;
            CreateAndSetCubeFaceView(m, invert);
            DepthRenderScene();

            GraphicsDevice.SetRenderTarget(renderTargetDepthCube, CubeMapFace.NegativeY);
            m = TextureCubeTypeConverter.MatrixNegativeY; m.Translation = lightPosition;
            CreateAndSetCubeFaceView(m, invert);
            DepthRenderScene();

            GraphicsDevice.SetRenderTarget(renderTargetDepthCube, CubeMapFace.NegativeZ);
            m = TextureCubeTypeConverter.MatrixNegativeZ; m.Translation = lightPosition;
            CreateAndSetCubeFaceView(m, invert);
            DepthRenderScene();

            GraphicsDevice.SetRenderTarget(renderTargetDepthCube, CubeMapFace.PositiveX);
            m = TextureCubeTypeConverter.MatrixPositiveX; m.Translation = lightPosition;
            CreateAndSetCubeFaceView(m, invert);
            DepthRenderScene();

            GraphicsDevice.SetRenderTarget(renderTargetDepthCube, CubeMapFace.PositiveY);
            m = TextureCubeTypeConverter.MatrixPositiveY; m.Translation = lightPosition;
            CreateAndSetCubeFaceView(m, invert);
            DepthRenderScene();

            GraphicsDevice.SetRenderTarget(renderTargetDepthCube, CubeMapFace.PositiveZ);
            m = TextureCubeTypeConverter.MatrixPositiveZ; m.Translation = lightPosition;
            CreateAndSetCubeFaceView(m, invert);
            DepthRenderScene();
        }

        public void DepthRenderScene()
        {
            DepthCubeEffectClass.LightPosition = lightPosition;
            DepthCubeEffectClass.CameraPosition = cam.cameraWorld.Translation;

            DepthCubeEffectClass.Technique_Render_LightDepth();

            // S P H E R E S   S K Y    

            DepthCubeEffectClass.World = spheres[1].WorldTransformation;
            spheres[1].DrawPrimitiveSphere(GraphicsDevice, DepthCubeEffectClass.effect);

            // M E S H

            if (displayMeshTerrain)
            {
                DepthCubeEffectClass.World = meshTerrain.WorldTransformation;
                meshTerrain.DrawPrimitive(GraphicsDevice, DepthCubeEffectClass.effect);
            }
        }

        void DrawDepthRender()
        {
            DepthCubeEffectClass.Technique_Render_VisualizationDepthCube();

            for (int index = 0; index < spheres.Length; index++)
            {
                DepthCubeEffectClass.World = spheres[index].WorldTransformation;
                DepthCubeEffectClass.TextureCubeDiffuse = renderTargetDepthCube;
                if (spheres[index].IsSkyBox == false)
                    spheres[index].DrawPrimitiveSphere(GraphicsDevice, DepthCubeEffectClass.effect);
            }
        }




        public void DrawSpriteBatches(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            // Draw all the regular stuff
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);

            var r = new Rectangle();
            //spriteBatch.Draw(textureSphereHdrLdr, new Rectangle(0, 10, 100, 120), Color.White);

            r= new Rectangle(10, 10, 100, 100);
            spriteBatch.Draw(generatedTextureHdrLdrFromGeneratedEnvCubeMap, r, Color.White);
            spriteBatch.DrawRectangleOutline(r, 1, Color.White);

            r = new Rectangle(10, 110, 100, 100);
            spriteBatch.Draw(generatedTextureHdrLdrFromManuallySetSingleImages, r, Color.White);
            spriteBatch.DrawRectangleOutline(r, 1, Color.Red);

            r = new Rectangle(10, 210, 100, 100);
            spriteBatch.Draw(generatedTextureHdrLdrFromRenderTargetCube, r, Color.White);
            spriteBatch.DrawRectangleOutline(r, 1, Color.Red);

            for (int i = 0; i < generatedTextureFaceArrayFromRenderTargetCubemap.Length; i++)
            {
                r = new Rectangle((i + 1) * 140 + 10, 10, 100, 100);
                spriteBatch.Draw(generatedTextureFaceArrayFromGeneratedEnvCubemap[i], r, Color.White);
                spriteBatch.DrawRectangleOutline(r, 1, Color.White);

                r = new Rectangle((i + 1) * 140 + 40, 10 + 100, 100, 100);
                spriteBatch.Draw(textureArrayManuallySet[i], r, Color.White);
                spriteBatch.DrawRectangleOutline(r, 1, Color.Red);

                r = new Rectangle((i + 1) * 140 + 80, 10 +200, 100, 100);
                spriteBatch.Draw(generatedTextureFaceArrayFromRenderTargetCubemap[i], r, Color.White); 
                spriteBatch.DrawRectangleOutline(r, 1, Color.Red);

                //spriteBatch.Draw(generatedTextureFaceArrayFromHdrLdr[i], new Rectangle((i + 1) * 140 +70, 10 + 200, 100, 100), Color.White);
            }

            string msg =
                    $" \n The F2 toggle wireframe. F3 show normals. F4 mesh itself. F5 the texture used." +
                    $" \n Space toggle light controls." +
                    $" \n The keys WASD change the forward view direction (which is the major take away here). ZC allows for spin." +
                    $" \n The Arrow keys move the camera translation as strafing motion. " +
                    $" \n  " +
                    $" \n  CubeMaping and related techniques require a texture cube." +
                    $" \n  This is to render scene data to the faces or sample that data." +
                    $" \n  This example will rely on many other classes as we need geometry for examples." +
                    $" \n  In this class well use two primitive spheres to draw out cubemap data." +
                    $" \n  One of these will have its vertices wound backwards to be a skysphere or cube." +
                    $" \n  A TextureTypeConverter class to load images into a TextureCube." +
                    $" \n  That will call on the TextureCubeBuildEffect, a SkyCubeEffect to draw the prims." +
                    $" \n  Currently in monogame TextureCubes seem to store data a bit inverted." +
                    $" \n  While not ideal for the momemnt in the SkyCubeEffect class will flip the y on the normal." +
                    $" \n  " +
                    $" \n  A texture cube holds data in six images the call on the shader texCubeLod or one of its cousins." +
                    $" \n  Takes Texture Coordinates in the form  of  u,v,w or  x,y,z normal texturecoordinates." +
                    $" \n  This then returns texel data at the corresponding directional faces u v." +
                    $" \n  " +
                    $" \n  The primitive sphere class here is a cube when its faces w h are set to 2,2." +
                    $" \n  Higher values tesselate the cube towards a sphere on creation." +
                    $" \n  " +
                    $" \n  " +
                    $" \n"
                    ;

            if (displayOnScreenTextInfo)
                spriteBatch.DrawString(font, msg, new Vector2(10, 110), Color.White);
            else
                spriteBatch.DrawString(
                    font,
                    $"\n Press F1 for information  " +
                    $"\n displayReflectionRender: {displayReflectionRender}" +
                    $"\n " +
                    $"\n ",
                    new Vector2(10, 110),
                    Color.White
                    );


            if (Keys.End.IsKeyPressedWithDelay(gameTime))
                Console.WriteLine($"{cam.cameraWorld.ToDisplayMatrixForCopy("cameraWorld") } ");

            spriteBatch.End();
        }

        //public void DrawSpriteBatches(GameTime gameTime)
        //{
        //    GraphicsDevice.DepthStencilState = DepthStencilState.None;
        //    GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        //    // Draw all the regular stuff
        //    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);

        //    var whatIdrewWithCullWise = (CullOutCounterClockWiseTriangles == true) ? "CounterClockwise" : "ClockWise";

        //    string msg =
        //            $" \n  " +
        //            $" \n  " +
        //            $" \n  " +
        //            $" \n  " +
        //            $" \n"
        //            ;

        //    if (displayOnScreenTextInfo)
        //        spriteBatch.DrawString(font, msg, new Vector2(10, 10), Color.Red);
        //    else
        //        spriteBatch.DrawString(
        //            font, 
        //            $"Press F1 for information  " +
        //            $"\n {elapsed}" +
        //            $"" , 
        //            new Vector2(10, 10), 
        //            Color.Red
        //            );

        //    if (Keys.End.IsKeyPressedWithDelay(gameTime))
        //        Console.WriteLine( $"{cam.cameraWorld.ToDisplayMatrixForCopy("cameraWorld") } ");

        //    spriteBatch.End();
        //}

    }
}
