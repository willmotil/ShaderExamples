
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_EnviromentalMapping : Game
    {
        bool manuallyRotateLight = false;
        bool displayMesh = true;
        bool displayWireframe = false;
        bool displayNormals = true;
        bool displayWhiteDiffuse = false;
        bool displayOnScreenText = false;
        bool flipCullToClockWise = false;
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
        TextureCube cubemap;
        Texture2D 
            textureMesh , textureMeshNormalMap,
            textureHdrLdrSphere, textureSphereNormalMap, 
            textureMonogameLogo, 
            dotTextureRed, dotTextureBlue, dotTextureGreen, dotTextureYellow, dotTextureWhite
            ;
        Texture2D
            generatedTextureHdrLdrFromSingleImages, generatedTextureHdrLdrFromCubeMap
            ;
        Texture2D[]
            generatedTextureFaceArrayFromCubemap, generatedTextureFaceArrayFromHdrLdr //, loadedOrAssignedArray
            ;
        RenderTarget2D rtScene;
        CameraAndKeyboardControls cam = new CameraAndKeyboardControls();

        PrimitiveIndexedMesh mesh;
        Vector3 meshScale = new Vector3(300f, 250, 0f);
        VisualizationNormals visualMeshNormals = new VisualizationNormals();
        VisualizationNormals visualMeshTangents = new VisualizationNormals();
        VisualizationLine visualLightLineToMesh;


        PrimitiveSphere[] spheres = new PrimitiveSphere[4];
        VisualizationNormals[] visualSphereNormals = new VisualizationNormals[4];
        VisualizationNormals[] visualSphereTangents = new VisualizationNormals[4];
        VisualizationLine[] visualLightLineToSpheres = new VisualizationLine[4];

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

        Vector3 lightStartPosition = new Vector3(150, 125, 300); // new Vector3(1, 800, 300)
        Vector3 lightPosition = new Vector3(0, 0, 0);
        Matrix lightTransform = Matrix.Identity;
        float lightRotationRadians = 0f;
        Vector3 meshDimensions = new Vector3(300f, 250,0);
        Vector3 meshCenter = new Vector3(150,125,0);

        string spectypemsg = "";

        public Game1_EnviromentalMapping()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Envirmental Mapping Cubemaps Skyboxes ";
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

            SetCamera();

            LoadFontsTextures();

            LoadAndSetupInitialEnvEffect();

            CreateMesh();

            CreateSpheres();
        }

        public void SetCamera()
        {
            cam.InitialView(GraphicsDevice, new Vector3(+0f, +0f, -381.373f), Vector3.UnitZ, -Vector3.UnitY);
            cam.UpdateProjection(GraphicsDevice);
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

            textureMonogameLogo = Content.Load<Texture2D>("QuarrySquare");  //MG_Logo_Modifyed TextureAlignmentTestImage2

            // RefactionTexture has the opposite encoding
            // walltomap wallnormmap  Flower-normal , Flower-diffuse  Flower-bump  Flower-ambientocclusion  Quarry  QuarrySquare TextureAlignmentTestImage2
            textureHdrLdrSphere = Content.Load<Texture2D>("QuarrySquare");
            textureSphereNormalMap = Content.Load<Texture2D>("wallnormmap");
            textureMesh = Content.Load<Texture2D>("QuarrySquare");  // TestNormalMap
            textureMeshNormalMap = Content.Load<Texture2D>("TestNormalMap");

            TextureTypeConverter.Load(Content);
            cubemap = TextureTypeConverter.ConvertSphericalTexture2DToTextureCube(GraphicsDevice, textureHdrLdrSphere, false, false, textureHdrLdrSphere.Width);
            generatedTextureHdrLdrFromCubeMap = TextureTypeConverter.ConvertTextureCubeToSphericalTexture2D(GraphicsDevice, cubemap, false, false, 256);
            generatedTextureFaceArrayFromCubemap = TextureTypeConverter.ConvertTextureCubeToTexture2DArray(GraphicsDevice, cubemap, false, false, 256);
            generatedTextureHdrLdrFromSingleImages = TextureTypeConverter.ConvertTexture2DArrayToSphericalTexture2D(GraphicsDevice, generatedTextureFaceArrayFromCubemap, false, false, 256);
            generatedTextureFaceArrayFromHdrLdr = TextureTypeConverter.ConvertSphericalTexture2DToTexture2DArray(GraphicsDevice, textureHdrLdrSphere, false, false, 256);
        }

        public void LoadAndSetupInitialEnvEffect()
        {
            EnviromentalMapEffectClass.Load(Content);
            EnviromentalMapEffectClass.Technique_Lighting_Phong();
            EnviromentalMapEffectClass.TextureCubeDiffuse = cubemap;
            EnviromentalMapEffectClass.TextureDiffuse = textureMesh;
            EnviromentalMapEffectClass.TextureNormalMap = textureMeshNormalMap;
            EnviromentalMapEffectClass.AmbientStrength = 1.0f;
            EnviromentalMapEffectClass.DiffuseStrength = 1f;
            EnviromentalMapEffectClass.SpecularStrength = .8f;
            EnviromentalMapEffectClass.View = cam.view;
            EnviromentalMapEffectClass.Projection = cam.projection;
            EnviromentalMapEffectClass.CameraPosition = cam.cameraWorld.Translation;
            EnviromentalMapEffectClass.LightPosition = lightPosition;
            EnviromentalMapEffectClass.LightColor = new Vector3(1f, 1f, 1f);
        }

        //++++++++++++++++++++++++++++++++++
        // Create
        //++++++++++++++++++++++++++++++++++

        public void CreateMesh()
        {
            PrimitiveIndexedMesh.ShowOutput = false;
            PrimitiveIndexedMesh.AveragingOption = PrimitiveIndexedMesh.AVERAGING_OPTION_USE_HIGHEST;

            // different ways to create the mesh regular via a height array or a texture used as a height / displacement map 

            mesh = new PrimitiveIndexedMesh(5, 5, meshScale, false, false);
            float thickness = .1f;
            float normtanLinescale = .1f;

            //mesh = new PrimitiveIndexedMesh(heightMap, 9, new Vector3( 300f, 250, 70f ), false, false);
            //float thickness = .1f; normtanLinescale = 10f;

            //mesh = new PrimitiveIndexedMesh(texture, new Vector3(300f, 250, 5f), false, false);
            //float thickness = .01f; float normtanLinescale = 1f;


            mesh.DiffuseTexture = textureMesh;
            mesh.NormalMapTexture = textureMeshNormalMap;
            visualMeshNormals = CreateVisualNormalLines(mesh.vertices, mesh.indices, dotTextureGreen, thickness, normtanLinescale, false);
            visualMeshTangents = CreateVisualNormalLines(mesh.vertices, mesh.indices, dotTextureYellow, thickness, normtanLinescale, true);
            visualLightLineToMesh = CreateVisualLine(dotTextureWhite, meshCenter, lightStartPosition, 1, Color.White);
        }

        public void CreateSpheres()
        {
            Vector3[] sphereCenters = new Vector3[] { new Vector3(0, 0, -50), new Vector3(150, 0, -50), new Vector3(0, 150, -50), new Vector3(150, 150, -50) };

            for (int index = 0; index < 4; index++)
            {
                int snum = index;
                while (snum > 3)
                    snum = snum - 4;
                int usage = 0;
                float scale = 50;
                switch (snum)
                {
                    case 0:
                        usage = PrimitiveSphere.USAGE_CUBE_UNDER_CCW; // = 0
                        scale = 40;
                        break;
                    case 1:
                        usage = PrimitiveSphere.USAGE_SKYSPHERE_UNDER_CCW; // 2
                        scale = 500;
                        break;
                    case 2:
                        usage = PrimitiveSphere.USAGE_CUBE_UNDER_CW; // 1
                        scale = 40;
                        break;
                    case 3:
                        usage = PrimitiveSphere.USAGE_SKYSPHERE_UNDER_CW; // 3
                        scale = 500;
                        break;
                }
                CreateSphere(ref spheres[index], ref visualSphereNormals[index], ref visualSphereTangents[index], ref visualLightLineToSpheres[index], sphereCenters[index], scale, usage, false, false);
            }
        }

        public void CreateSphere(ref PrimitiveSphere asphere, ref VisualizationNormals visnorm, ref VisualizationNormals vistan, ref VisualizationLine vline, Vector3 sphereCenter, float spherescale, int primUsage, bool invert, bool flatfaces)
        {
            asphere = new PrimitiveSphere(5, 5, 1f, primUsage, invert, flatfaces);
            asphere.SetWorldTransformation(sphereCenter, Vector3.Forward, Vector3.Up, spherescale);
            asphere.textureCube = cubemap;
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
                displayOnScreenText = !displayOnScreenText;
            if (Keys.F2.IsKeyPressedWithDelay(gameTime))
                displayWireframe = !displayWireframe;
            if (Keys.F3.IsKeyPressedWithDelay(gameTime))
                displayNormals = !displayNormals;
            if (Keys.F4.IsKeyPressedWithDelay(gameTime))
                displayMesh = !displayMesh;
            if (Keys.F5.IsKeyPressedWithDelay(gameTime))
                displayWhiteDiffuse = !displayWhiteDiffuse;
            
            if (Keys.F6.IsKeyPressedWithDelay(gameTime))
            {
                whichTechnique++;
                if(whichTechnique > 1)
                    whichTechnique = 0;
                switch(whichTechnique)
                {
                    case 0:
                        EnviromentalMapEffectClass.Technique_Lighting_Phong();
                        spectypemsg = "Phong";
                        break;
                    case 1:
                        EnviromentalMapEffectClass.Technique_Lighting_Blinn();
                        spectypemsg = "Blinn";
                        break;
                }
            }

            if (Keys.F7.IsKeyPressedWithDelay(gameTime))
                flipCullToClockWise = !flipCullToClockWise;

            if (Keys.Home.IsKeyPressedWithDelay(gameTime))
                cam.InitialView(GraphicsDevice);
            if (Keys.Space.IsKeyPressedWithDelay(gameTime))
                manuallyRotateLight = !manuallyRotateLight;

            if (manuallyRotateLight)
            {
                if (Keys.OemPlus.IsKeyDown())
                    lightRotationRadians += .05f;
                if (Keys.OemMinus.IsKeyDown())
                    lightRotationRadians -= .05f;
                if (Keys.OemCloseBrackets.IsKeyDown())
                    lightStartPosition.Z += 5f;
                if (Keys.OemOpenBrackets.IsKeyDown())
                    lightStartPosition.Z -= 5f;
            }
            else 
            {
                lightRotationRadians += .005f;
            }
            if (lightRotationRadians > 6.28318f)
                lightRotationRadians = 0;
            if (lightRotationRadians < 0)
                lightRotationRadians = 6.283f;

            var axisOfRotation = new Vector3(1, 0, 0);
            lightTransform = Matrix.CreateFromAxisAngle(axisOfRotation, lightRotationRadians);
            lightPosition = Vector3.Transform(lightStartPosition, lightTransform);

            visualLightLineToMesh.ReCreateVisualLine(dotTextureRed, meshCenter, lightPosition, 1, Color.White);
            for (int index = 0; index < 4; index++)
            {
                visualLightLineToSpheres[index].ReCreateVisualLine(dotTextureWhite, spheres[index].Position, lightPosition, 1, Color.Blue);
            }

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

            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            if (displayMesh)
                DrawMeshAndSphere();

            DrawNormalsTangentsAndLightLines();

            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        public void DrawMeshAndSphere()
        {
            EnviromentalMapEffectClass.View = cam.view;
            EnviromentalMapEffectClass.Projection = cam.projection;
            EnviromentalMapEffectClass.LightPosition = lightPosition;
            EnviromentalMapEffectClass.CameraPosition = cam.cameraWorld.Translation;

            if (flipCullToClockWise)
                GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            else
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;


            // S P H E R E S   S K Y    

            for (int index = 0; index < 4; index++)
            {
                if (spheres[index].IsSkyBox)
                {
                    EnviromentalMapEffectClass.Technique_Render_Skybox();
                    //EnviromentalMapEffectClass.Technique_Render_CubeSkyboxWithNormalMap();
                }
                else
                {
                    EnviromentalMapEffectClass.Technique_Render_Cube();
                }
                EnviromentalMapEffectClass.World = spheres[index].WorldTransformation;
                EnviromentalMapEffectClass.TextureCubeDiffuse = spheres[index].textureCube;
                EnviromentalMapEffectClass.TextureDiffuse = textureHdrLdrSphere;
                EnviromentalMapEffectClass.TextureNormalMap = textureSphereNormalMap;
                spheres[index].DrawPrimitiveSphere(GraphicsDevice, EnviromentalMapEffectClass.effect);
            }


            // M E S H

            if (displayWhiteDiffuse)
                mesh.DiffuseTexture = dotTextureWhite;
            else
                mesh.DiffuseTexture = mesh.DiffuseTexture;

            EnviromentalMapEffectClass.Technique_Lighting_Phong();
            EnviromentalMapEffectClass.World = Matrix.Identity;
            EnviromentalMapEffectClass.TextureDiffuse = mesh.DiffuseTexture;
            EnviromentalMapEffectClass.TextureNormalMap = mesh.NormalMapTexture;
            mesh.DrawPrimitive(GraphicsDevice, EnviromentalMapEffectClass.effect);


            // W I R E F R A M E

            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_WIREFRAME;
            EnviromentalMapEffectClass.Technique_Lighting_Phong();
            EnviromentalMapEffectClass.TextureDiffuse = dotTextureRed;
            if (displayWireframe)
            {
                for (int index = 0; index < 4; index++)
                {
                    EnviromentalMapEffectClass.World = spheres[index].WorldTransformation;
                    spheres[index].DrawPrimitiveSphere(GraphicsDevice, EnviromentalMapEffectClass.effect);
                }

                EnviromentalMapEffectClass.World = Matrix.Identity;
                mesh.DrawPrimitive(GraphicsDevice, EnviromentalMapEffectClass.effect);
            }
        }

        public void DrawNormalsTangentsAndLightLines()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            EnviromentalMapEffectClass.Technique_Lighting_Phong();

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
            visualMeshNormals.World = visualMeshNormals.WorldTransformation;
            visualMeshNormals.View = cam.view;
            visualMeshNormals.Projection = cam.projection;
            visualMeshNormals.Draw(GraphicsDevice);

            visualMeshTangents.World = visualMeshTangents.WorldTransformation; //Matrix.Identity;
            visualMeshTangents.View = cam.view;
            visualMeshTangents.Projection = cam.projection;
            visualMeshTangents.Draw(GraphicsDevice);
        }

        public void DrawNormalsAndTangentsForSphere()
        {
            for (int index = 0; index < 4; index++)
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
            visualLightLineToMesh.World = Matrix.Identity;
            visualLightLineToMesh.View = cam.view;
            visualLightLineToMesh.Projection = cam.projection;
            visualLightLineToMesh.Draw(GraphicsDevice);
        }

        public void DrawLightLineToSphere()
        {
            for (int index = 0; index < 4; index++)
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

            spriteBatch.Draw(textureHdrLdrSphere, new Rectangle(0, 10, 100, 120), Color.White);
            for (int i = 0; i < generatedTextureFaceArrayFromCubemap.Length; i++)
            {
                spriteBatch.Draw(generatedTextureFaceArrayFromCubemap[i], new Rectangle((i + 1) * 140, 10, 100, 100), Color.White);
                spriteBatch.Draw(generatedTextureFaceArrayFromHdrLdr[i], new Rectangle((i + 1) * 140 + 35, 10 + 15, 100, 100), Color.White);
            }
            spriteBatch.Draw(generatedTextureHdrLdrFromSingleImages, new Rectangle(0, 180, 100, 120), Color.White);
            spriteBatch.Draw(generatedTextureHdrLdrFromCubeMap, new Rectangle(150, 190, 100, 120), Color.White);

            string msg =
                    $" \n The F2 toggle wireframe. F3 show normals. F4 mesh itself. F5 the texture used." +
                    $" \n F6 switch techniques {spectypemsg}. Space toggle light controls." +
                    $" \n The keys WASD change the forward view direction (which is the major take away here). ZC allows for spin." +
                    $" \n The Arrow keys move the camera translation as strafing motion. " +
                    $" \n  " +
                    $" \n  " +
                    $" \n  " +
                    $" \n  " +
                    $" \n  " +
                    $" \n  " +
                    $" \n  " +
                    $" \n  " +
                    $" \n  " +
                    $" \n  " +
                    $" \n  " +
                    $" \n  " +
                    $" \n  " +
                    $" \n  " +
                    $" \n  " +
                    $" \n"
                    ;

            if (displayOnScreenText)
                spriteBatch.DrawString(font, msg, new Vector2(10, 10), Color.Red);
            else
                spriteBatch.DrawString(
                    font, 
                    $"Press F1 for information  " +
                    $"\n{spectypemsg} " +
                    $"\nIs GraphicsDevice CullClockwise : { flipCullToClockWise}" +
                    $"" +
                    $"" , 
                    new Vector2(10, 10), 
                    Color.Red
                    );


            if (Keys.End.IsKeyPressedWithDelay(gameTime))
                Console.WriteLine( $"{cam.cameraWorld.DisplayMatrixForCopy("cameraWorld") } ");

            spriteBatch.End();
        }

    }
}
