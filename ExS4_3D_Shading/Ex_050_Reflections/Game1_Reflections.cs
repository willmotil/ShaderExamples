
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_Reflections : Game
    {
        bool manuallyRotateLight = false;
        bool displayMesh = true;
        bool displayMeshTerrain = false;
        bool displaySphere = true;
        bool displayWireframe = false;
        bool displayNormals = false;
        bool displayWhiteDiffuse = false;
        bool displayExtraVisualStuff = false;
        bool displayOnScreenTextInfo = false;
        bool CullOutCounterClockWiseTriangles = true;
        int whichTechnique = 0;

        RasterizerState rasterizerState_CULLNONE_WIREFRAME = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        RasterizerState rasterizerState_CULLNONE_SOLID = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.Solid };

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont 
            font , 
            font2, 
            font3
            ;
        TextureCube textureCubeDiffuse, textureCubeDiffuseIrradiance, textureCubeEnvGeneratedFromSixImages;
        Texture2D 
            textureMeshDemoQuad , textureMeshNormalMapDemoQuad,
            textureMeshTerrain, textureMeshNormalMapTerrain,
            textureMeshWater, textureMeshNormalMapWater,
            textureHdrLdrSphere, textureHdrLdrSphereIllumination, textureHdrLdrSphereNormalMap,
            textureMonogameLogo, miscTexture, 
            dotTextureRed, dotTextureBlue, dotTextureGreen, dotTextureYellow, dotTextureWhite
            ;
        Texture2D
            generatedTextureHdrLdrFromSingleImages, generatedTextureHdrLdrFromCubeMap,
            faceFront, faceBack, faceLeft, faceRight, faceTop, faceBottom
            ;
        Texture2D[]
            generatedTextureFaceArrayFromCubemap, generatedTextureFaceArrayFromHdrLdr //, loadedOrAssignedArray
            ;
        RenderTarget2D rtScene;
        CameraAndKeyboardControls cam = new CameraAndKeyboardControls();

        PrimitiveIndexedMesh meshDemoQuad, meshTerrain, meshWater;
        VisualizationNormals visualMesh1Normals = new VisualizationNormals();
        VisualizationNormals visualMesh1Tangents = new VisualizationNormals();
        VisualizationLine visualLightLineToMesh1;

        public static int numberOfSpheres = 2;
        PrimitiveSphere[] spheres = new PrimitiveSphere[numberOfSpheres];
        VisualizationNormals[] visualSphereNormals = new VisualizationNormals[numberOfSpheres];
        VisualizationNormals[] visualSphereTangents = new VisualizationNormals[numberOfSpheres];
        VisualizationLine[] visualLightLineToSpheres = new VisualizationLine[numberOfSpheres];

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
        float lightRotationRadiansX = 0f;
        float lightRotationRadiansY = 0f;


        string spectypemsg = "";

        public Game1_Reflections()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Reflection Mapping";
            IsMouseVisible = true;
            Window.ClientSizeChanged += OnResize;

            //var up = new Vector3(0, 1, 0);
            //var forward1 = new Vector3(0, 1, .1f);
            //var forward2 = new Vector3(0, 1, -.1f);
            //var n1 = Vector3.Normalize(Vector3.Cross(forward1, up));
            //var n2 = Vector3.Normalize(Vector3.Cross(forward2, up));
            //Console.WriteLine($"The problem with a fixed up vector.  the gimple point.");
            //Console.WriteLine($"var {n1} = Vector3.Normalize(Vector3.Cross({forward1}, {up}));");
            //Console.WriteLine($"var {n2} = Vector3.Normalize(Vector3.Cross({forward2}, {up}));");
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

            dotTextureRed = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Red);
            dotTextureGreen = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Green);
            dotTextureBlue = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Blue);
            dotTextureWhite = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.White);
            dotTextureYellow = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Yellow);

            Content.RootDirectory = @"Content/Images";

            //
            // RefactionTexture has the opposite encoding walltomap wallnormmap TestNormalMap  Flower-normal , Flower-diffuse  Flower-bump  Flower-ambientocclusion  Quarry  QuarrySquare MG_Logo_Modifyed TextureAlignmentTestImage2
            // TestNormalMap  FlatNormalMap Brick_em,  walltomap wallnormmap  wallnormmapGimp
            // QuarrySquare  Eqr001_Diffuse  Eqr001_Diffuse_irradiance    Content.Load<Texture2D>("");
            //

            miscTexture = Content.Load<Texture2D>("Nasa_DEM_Earth");
            textureMonogameLogo = Content.Load<Texture2D>("MG_Logo_Modifyed");


            textureMeshDemoQuad = dotTextureWhite;
            textureMeshNormalMapDemoQuad = Content.Load<Texture2D>("TestNormalMap");
            
            textureMeshWater = dotTextureWhite;
            textureMeshNormalMapWater = Content.Load<Texture2D>("FlatNormalMap"); //dotTextureWhite;

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
                faceTop,
                faceFront,
                faceLeft,
                faceBottom,
                faceBack,
                false, false, textureMonogameLogo.Width
            );
            generatedTextureHdrLdrFromCubeMap = TextureCubeTypeConverter.ConvertTextureCubeToSphericalTexture2D(GraphicsDevice, textureCubeEnvGeneratedFromSixImages, false, false, 256);
            
            textureCubeDiffuse = TextureCubeTypeConverter.ConvertSphericalTexture2DToTextureCube(GraphicsDevice, generatedTextureHdrLdrFromCubeMap, false, false, generatedTextureHdrLdrFromCubeMap.Width);

            //textureCubeDiffuse = TextureCubeTypeConverter.ConvertSphericalTexture2DToTextureCube(GraphicsDevice, textureHdrLdrSphere, false, false, textureHdrLdrSphere.Width);
            textureCubeDiffuseIrradiance = TextureCubeTypeConverter.ConvertSphericalTexture2DToTextureCube(GraphicsDevice, textureHdrLdrSphereIllumination, false, false, textureHdrLdrSphereIllumination.Width);

            generatedTextureFaceArrayFromCubemap = TextureCubeTypeConverter.ConvertTextureCubeToTexture2DArray(GraphicsDevice, textureCubeDiffuseIrradiance, false, false, 256);
            generatedTextureHdrLdrFromSingleImages = TextureCubeTypeConverter.ConvertTexture2DArrayToSphericalTexture2D(GraphicsDevice, generatedTextureFaceArrayFromCubemap, false, false, 256);
            generatedTextureFaceArrayFromHdrLdr = TextureCubeTypeConverter.ConvertSphericalTexture2DToTexture2DArray(GraphicsDevice, textureHdrLdrSphere, false, false, 256);
        }

        public void SetCamera()
        {
            //cam.InitialView(GraphicsDevice, new Vector3(+0f, +0f, 381.373f), -Vector3.UnitZ, -Vector3.UnitY);
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

            // mesh demo quad

            string option = "regularGrid";
            switch (option)
            {
                case "regularGrid":
                    meshDemoQuad = new PrimitiveIndexedMesh(5, 5, new Vector3(300f, 250, 0f), false, false, false);
                    thickness = .2f; normtanLinescale = 20f;
                    break;
                case "heightMap":
                    meshDemoQuad = new PrimitiveIndexedMesh(heightMap, 9, new Vector3(300f, 250, 70f), false, false, false);
                    thickness = .1f; normtanLinescale = 10f;
                    break;
                case "textureAsHeightMap":
                    PrimitiveIndexedMesh.AveragingOption = PrimitiveIndexedMesh.AVG_OPTION_USE_HIGHEST_RGB;
                    meshDemoQuad = new PrimitiveIndexedMesh(textureMeshDemoQuad, new Vector3(300f, 250, 5f), false, false, false);
                    thickness = .01f; normtanLinescale = 10f;
                    break;
            }
            meshDemoQuad.SetWorldTransformation(new Vector3(0, -125, 0), Vector3.Forward, Vector3.Up , Vector3.One);
            meshDemoQuad.DiffuseTexture = textureMeshDemoQuad;
            meshDemoQuad.NormalMapTexture = textureMeshNormalMapDemoQuad;

            visualMesh1Normals = CreateVisualNormalLines(meshDemoQuad.vertices, meshDemoQuad.indices, dotTextureGreen, thickness, normtanLinescale, false);
            visualMesh1Tangents = CreateVisualNormalLines(meshDemoQuad.vertices, meshDemoQuad.indices, dotTextureYellow, thickness, normtanLinescale, true);
            visualLightLineToMesh1 = CreateVisualLine(dotTextureWhite, meshDemoQuad.Center, lightStartPosition, 1, Color.White);

            // mesh water

            option = "regularGrid";
            switch (option)
            {
                case "regularGrid":
                    meshWater = new PrimitiveIndexedMesh(5, 5, new Vector3(1000f, 1000f, 0f), false, false, false);
                    break;
                case "heightMap":
                    meshWater = new PrimitiveIndexedMesh(heightMap, 9, new Vector3(1000f, 1000f, 70f), false, false, false);
                    break;
                case "textureAsHeightMap":
                    PrimitiveIndexedMesh.AveragingOption = PrimitiveIndexedMesh.AVG_OPTION_USE_HIGHEST_RGB;
                    meshWater = new PrimitiveIndexedMesh(textureMeshNormalMapWater, new Vector3(1000f, 1000f, 20f), false, false, false);
                    break;
            }
            // 
            //mesh3Water.SetWorldTransformation(new Vector3(-300, +280, 550), Vector3.Down, Vector3.Forward, Vector3.One);
            meshWater.SetWorldTransformation(new Vector3(-550, -200, 100), Vector3.Forward, Vector3.Up, Vector3.One);
            meshWater.DiffuseTexture = textureMeshWater;
            meshWater.NormalMapTexture = textureMeshNormalMapWater;

            // mesh terrain

            option = "textureAsHeightMap";
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
                        CreateSphere( textureCubeDiffuse, ref spheres[index], ref visualSphereNormals[index], ref visualSphereTangents[index], ref visualLightLineToSpheres[index], sphereCenters[index], scale, usage, false, false);
                        break;
                    case 1:
                        usage = PrimitiveSphere.USAGE_SKYSPHERE_UNDER_CCW; 
                        scale = 1000;
                        CreateSphere(textureCubeDiffuse, ref spheres[index], ref visualSphereNormals[index], ref visualSphereTangents[index], ref visualLightLineToSpheres[index], sphereCenters[index], scale, usage, false, false);
                        break;
                    case 2:
                        usage = PrimitiveSphere.USAGE_CUBE_UNDER_CW; 
                        scale = 40;
                        CreateSphere(textureCubeDiffuse, ref spheres[index], ref visualSphereNormals[index], ref visualSphereTangents[index], ref visualLightLineToSpheres[index], sphereCenters[index], scale, usage, false, false);
                        break;
                    case 3:
                        usage = PrimitiveSphere.USAGE_SKYSPHERE_UNDER_CW; 
                        scale = 1000;
                        CreateSphere(textureCubeDiffuse, ref spheres[index], ref visualSphereNormals[index], ref visualSphereTangents[index], ref visualLightLineToSpheres[index], sphereCenters[index], scale, usage, false, false);
                        break;
                }
            }
        }

        public void CreateSphere(TextureCube textureCube, ref PrimitiveSphere asphere, ref VisualizationNormals visnorm, ref VisualizationNormals vistan, ref VisualizationLine vline, Vector3 sphereCenter, float spherescale, int primUsage, bool invert, bool flatfaces)
        {
            asphere = new PrimitiveSphere(2, 2, 1f, primUsage, invert, flatfaces);
            asphere.SetWorldTransformation(sphereCenter, Vector3.Forward, Vector3.Up, spherescale);
            asphere.textureCube = textureCube;
            float thickness = .005f;
            float normtanLinescale = .15f;
            visnorm = CreateVisualNormalLines(asphere.vertices, asphere.indices, dotTextureGreen, thickness, normtanLinescale, false);
            vistan = CreateVisualNormalLines(asphere.vertices, asphere.indices, dotTextureYellow, thickness, normtanLinescale, true);
            vline = CreateVisualLine(dotTextureWhite, sphereCenter, lightStartPosition, 1, Color.White);
        }

        public VisualizationNormals CreateVisualNormalLines(VertexPositionNormalTextureTangentWeights[] verts, int[] indices, Texture2D texture, float thickness, float lineLength , bool grabTangentsInstead)
        {
            var vsn = new VisualizationNormals();
            VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[verts.Length];
            if (grabTangentsInstead)
            {
                for (int i = 0; i < verts.Length; i++)
                    tmp[i] = new VertexPositionNormalTexture() { Position = verts[i].Position, Normal = verts[i].Tangent, TextureCoordinate = verts[i].TextureCoordinate };
            }
            else
            {
                for (int i = 0; i < verts.Length; i++)
                    tmp[i] = new VertexPositionNormalTexture() { Position = verts[i].Position, Normal = verts[i].Normal, TextureCoordinate = verts[i].TextureCoordinate };
            }
            vsn.CreateVisualNormalsForPrimitiveMesh(tmp, indices, texture, thickness, lineLength);
            vsn.SetUpBasicEffect(GraphicsDevice, texture, cam.view, cam.projection);
            return vsn;
        }

        public VisualizationLine CreateVisualLine(Texture2D texture, Vector3 startPosition, Vector3 endPosition, float thickness, Color color)
        {
            var vln = new VisualizationLine(texture, startPosition, endPosition, thickness, color);
            vln.SetUpBasicEffect(GraphicsDevice, texture, cam.view, cam.projection);
            return vln;
        }

        public void LoadAndSetupInitialEnvEffect()
        {
            ReflectionsEffectClass.Load(Content);
            ReflectionsEffectClass.Technique_Render_PhongWithNormMapEnviromentalLight();
            ReflectionsEffectClass.TextureCubeDiffuse = textureCubeDiffuse;
            ReflectionsEffectClass.TextureCubeEnviromental = textureCubeDiffuseIrradiance;
            ReflectionsEffectClass.TextureDiffuse = textureMeshDemoQuad;
            ReflectionsEffectClass.TextureNormalMap = textureMeshNormalMapDemoQuad;
            ReflectionsEffectClass.AmbientStrength = 0.4f;
            ReflectionsEffectClass.DiffuseStrength = .30f;
            ReflectionsEffectClass.SpecularStrength = .30f;
            ReflectionsEffectClass.View = cam.view;
            ReflectionsEffectClass.Projection = cam.projection;
            ReflectionsEffectClass.CameraPosition = cam.cameraWorld.Translation;
            ReflectionsEffectClass.LightPosition = lightPosition;
            ReflectionsEffectClass.LightColor = new Vector3(1f, 1f, 1f);
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

            // move objects.

            if (Keys.U.IsKeyDown())
            {
                meshDemoQuad.Position = meshDemoQuad.Position + new Vector3(.1f, 0, 0);
                spheres[0].Position = spheres[0].Position + new Vector3(.1f, 0f, 0f);
            }
            if (Keys.J.IsKeyDown())
            {
                meshDemoQuad.Position = meshDemoQuad.Position + new Vector3(-.1f, 0f, 0f);
                spheres[0].Position = spheres[0].Position + new Vector3(-.1f, 0f, 0f);
            }
            if (Keys.H.IsKeyDown())
            {
                meshDemoQuad.Position = meshDemoQuad.Position + new Vector3(0f, .1f, 0);
                spheres[0].Position = spheres[0].Position + new Vector3(0f, .1f, 0f);
            }
            if (Keys.K.IsKeyDown())
            {
                meshDemoQuad.Position = meshDemoQuad.Position + new Vector3(0f, -.1f, 0f);
                spheres[0].Position = spheres[0].Position + new Vector3(0f, -.1f, 0f);
            }
            if (Keys.Y.IsKeyDown())
            {
                meshDemoQuad.Position = meshDemoQuad.Position + new Vector3(0f, 0f, .1f);
                spheres[0].Position = spheres[0].Position + new Vector3(0f, 0f, .1f);
            }
            if (Keys.I.IsKeyDown())
            {
                meshDemoQuad.Position = meshDemoQuad.Position + new Vector3(0f, 0f, -.1f);
                spheres[0].Position = spheres[0].Position + new Vector3(0f, 0f, -.1f);
            }
            if (Keys.F12.IsKeyDown())
            {
                meshDemoQuad.Position = new Vector3(0f, 0f, 0);
                spheres[0].Position = new Vector3(0f, 0f, 0);
            }


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

                lightTransform = Matrix.CreateRotationX( lightRotationRadiansX) * Matrix.CreateRotationY(lightRotationRadiansY);
                lightPosition = Vector3.Transform(lightStartPosition, lightTransform);
            }
            else 
            {
                lightRotationRadiansX += .01f;
                if (lightRotationRadiansX > 6.28318f)
                    lightAutoAxisFlip = ! lightAutoAxisFlip;
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

            visualLightLineToMesh1.ReCreateVisualLine(dotTextureRed, meshDemoQuad.Center, lightPosition, 1, Color.Green);
            for (int index = 0; index < spheres.Length; index++)
            {
                visualLightLineToSpheres[index].ReCreateVisualLine(dotTextureWhite, spheres[index].Position, lightPosition, 1, Color.Blue);
            }

            base.Update(gameTime);
        }

        bool lightAutoAxisFlip = false;




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

            if (displayMesh)
                DrawMeshAndSphere();

            DrawNormalsTangentsAndLightLines();

            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        public void DrawMeshAndSphere()
        {
            ReflectionsEffectClass.View = cam.view;
            ReflectionsEffectClass.Projection = cam.projection;
            ReflectionsEffectClass.LightPosition = lightPosition;
            ReflectionsEffectClass.CameraPosition = cam.cameraWorld.Translation;


            // S P H E R E S   S K Y    

            for (int index = 0; index < spheres.Length; index++)
            {
                ReflectionsEffectClass.World = spheres[index].WorldTransformation;
                ReflectionsEffectClass.TextureCubeDiffuse = spheres[index].textureCube;
                ReflectionsEffectClass.TextureCubeEnviromental = textureCubeDiffuseIrradiance;

                if (spheres[index].IsSkyBox)
                {
                    ReflectionsEffectClass.Technique_Render_Skybox();
                    spheres[index].DrawPrimitiveSphere(GraphicsDevice, ReflectionsEffectClass.effect);
                }
                else
                {
                    ReflectionsEffectClass.Technique_Render_CubeWithEnviromentalLight();
                    if(displaySphere)
                        spheres[index].DrawPrimitiveSphere(GraphicsDevice, ReflectionsEffectClass.effect);
                }
            }


            // M E S H

            if (displayWhiteDiffuse)
                meshDemoQuad.DiffuseTexture = dotTextureWhite;
            else
                meshDemoQuad.DiffuseTexture = meshDemoQuad.DiffuseTexture;

            ReflectionsEffectClass.TextureCubeEnviromental = textureCubeDiffuse;
            //ReflectionsEffectClass.TextureCubeEnviromental = textureCubeDiffuseIrradiance;

            // mesh Terrain

            if (displayMeshTerrain)
            {
                ReflectionsEffectClass.Technique_Render_PhongWithNormMapEnviromentalLight();
                ReflectionsEffectClass.World = meshTerrain.WorldTransformation;
                ReflectionsEffectClass.TextureDiffuse = meshTerrain.DiffuseTexture;
                ReflectionsEffectClass.TextureNormalMap = meshTerrain.NormalMapTexture;
                meshTerrain.DrawPrimitive(GraphicsDevice, ReflectionsEffectClass.effect);
            }

            // mesh DemoQuad

            ReflectionsEffectClass.Technique_Render_PhongWithNormMapEnviromentalLight();
            ReflectionsEffectClass.World = meshDemoQuad.WorldTransformation;
            ReflectionsEffectClass.TextureDiffuse = meshDemoQuad.DiffuseTexture;
            ReflectionsEffectClass.TextureNormalMap = meshDemoQuad.NormalMapTexture;
            meshDemoQuad.DrawPrimitive(GraphicsDevice, ReflectionsEffectClass.effect);

            // mesh Water

            //ReflectionsEffectClass.Technique_Render_PhongWithEnviromentalMap();
            ReflectionsEffectClass.World = meshWater.WorldTransformation;
            ReflectionsEffectClass.TextureDiffuse = meshWater.DiffuseTexture;
            ReflectionsEffectClass.TextureNormalMap = meshWater.NormalMapTexture;
            ReflectionsEffectClass.TextureCubeEnviromental = textureCubeDiffuse;
            meshWater.DrawPrimitive(GraphicsDevice, ReflectionsEffectClass.effect);

            // W I R E F R A M E

            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_WIREFRAME;
            ReflectionsEffectClass.Technique_Render_PhongWithNormMapEnviromentalLight();
            ReflectionsEffectClass.TextureDiffuse = dotTextureRed;
            if (displayWireframe)
            {
                for (int index = 0; index < spheres.Length; index++)
                {
                    ReflectionsEffectClass.World = spheres[index].WorldTransformation;
                    spheres[index].DrawPrimitiveSphere(GraphicsDevice, ReflectionsEffectClass.effect);
                }

                ReflectionsEffectClass.World = meshDemoQuad.WorldTransformation;
                meshDemoQuad.DrawPrimitive(GraphicsDevice, ReflectionsEffectClass.effect);
                ReflectionsEffectClass.World = meshTerrain.WorldTransformation;
                meshTerrain.DrawPrimitive(GraphicsDevice, ReflectionsEffectClass.effect);
            }
        }

        public void DrawNormalsTangentsAndLightLines()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            ReflectionsEffectClass.Technique_Render_PhongWithNormMapEnviromentalLight();

            if (displayNormals)
            {
                DrawNormalsAndTangentsForMesh();
                DrawNormalsAndTangentsForSphere();
            }
            
            DrawLightLineToSphere();
            DrawLightLineToMesh();
        }

        public void DrawNormalsAndTangentsForMesh()
        {
            // these all use basic effect internally so we must set the world view projection up for them again seperately.
            visualMesh1Normals.World = meshDemoQuad.WorldTransformation; //visualMeshNormals.WorldTransformation;
            visualMesh1Normals.View = cam.view;
            visualMesh1Normals.Projection = cam.projection;
            visualMesh1Normals.Draw(GraphicsDevice);

            visualMesh1Tangents.World = meshDemoQuad.WorldTransformation; //visualMeshTangents.WorldTransformation; //Matrix.Identity;
            visualMesh1Tangents.View = cam.view;
            visualMesh1Tangents.Projection = cam.projection;
            visualMesh1Tangents.Draw(GraphicsDevice);
        }

        public void DrawNormalsAndTangentsForSphere()
        {
            for (int index = 0; index < spheres.Length; index++)
            {
                visualSphereNormals[index].World = spheres[index].WorldTransformation;   //Matrix.CreateTranslation(spheres[index].Position);
                visualSphereNormals[index].View = cam.view;
                visualSphereNormals[index].Projection = cam.projection;
                visualSphereNormals[index].Draw(GraphicsDevice);

                visualSphereTangents[index].World = spheres[index].WorldTransformation;  //Matrix.CreateTranslation(spheres[index].Position);
                visualSphereTangents[index].View = cam.view;
                visualSphereTangents[index].Projection = cam.projection;
                visualSphereTangents[index].Draw(GraphicsDevice);
            }
        }

        public void DrawLightLineToMesh()
        {
            visualLightLineToMesh1.World = Matrix.Identity;
            visualLightLineToMesh1.View = cam.view;
            visualLightLineToMesh1.Projection = cam.projection;
            visualLightLineToMesh1.Draw(GraphicsDevice);
        }

        public void DrawLightLineToSphere()
        {
            for (int index = 0; index < spheres.Length; index++)
            {
                visualLightLineToSpheres[index].World = Matrix.Identity;
                visualLightLineToSpheres[index].View = cam.view;
                visualLightLineToSpheres[index].Projection = cam.projection;
                visualLightLineToSpheres[index].Draw(GraphicsDevice);
            }
        }


        public void DrawSpriteBatches(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            // Draw all the regular stuff
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);

            if (displayExtraVisualStuff)
            {
                spriteBatch.Draw(textureHdrLdrSphere, new Rectangle(0, 10, 100, 120), Color.White);
                for (int i = 0; i < generatedTextureFaceArrayFromCubemap.Length; i++)
                {
                    spriteBatch.Draw(generatedTextureFaceArrayFromCubemap[i], new Rectangle((i + 1) * 140, 10, 100, 100), Color.White);
                    spriteBatch.Draw(generatedTextureFaceArrayFromHdrLdr[i], new Rectangle((i + 1) * 140 + 35, 10 + 15, 100, 100), Color.White);
                }
                spriteBatch.Draw(generatedTextureHdrLdrFromSingleImages, new Rectangle(0, 180, 100, 120), Color.White);
                spriteBatch.Draw(generatedTextureHdrLdrFromCubeMap, new Rectangle(150, 190, 100, 120), Color.White);
            }

            var whatIdrewWithCullWise = (CullOutCounterClockWiseTriangles == true) ? "CounterClockwise" : "ClockWise";
            var whatIMadeTheMeshWithCullWise = (meshDemoQuad.IsWindingCcw == true) ? "CounterClockwise" : "ClockWise";
            var whatIMadeTheSphereWithCullWise = (meshDemoQuad.IsWindingCcw == true) ? "CounterClockwise" : "ClockWise";

            string msg =
                    $" \n The F2 toggle wireframe. F3 show normals. F4 mesh itself. F5 the texture used." +
                    $" \n F6 switch techniques {spectypemsg}. Space toggle light controls." +
                    $" \n The keys WASD change the forward view direction (which is the major take away here). ZC allows for spin." +
                    $" \n The Arrow keys move the camera translation as strafing motion. " +
                    $" \n  " +
                    $"\n{spectypemsg} " +
                    $" \n  " +
                    $"\nDefault Begin has been called. I am in SpriteBatch.DrawString." +
                    $"\nSpritebatch draws with ClockWise Triangles. " +
                    $"\nGraphicsDevice CullMode : {GraphicsDevice.RasterizerState.CullMode}" +
                    $"\n" +
                    $"\nI am Culling : { whatIdrewWithCullWise } triangles so i draw with the opposite winding.   " +
                    $"\nmesh Winding: {whatIMadeTheMeshWithCullWise}   sphere[0] Winding: {whatIMadeTheSphereWithCullWise}" +
                    $"\n" +
                    $"\n cam : {cam.cameraWorld.ToWellFormatedString("cameraWorld")}" +
                    $"\n" +
                    $"\n mesh : {meshDemoQuad.WorldTransformation.ToWellFormatedString("World")}" +
                    $"\n" +
                    $"\n spheres[0] : {spheres[0].WorldTransformation.ToWellFormatedString("World")}" +
                    $" \n  " +
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
