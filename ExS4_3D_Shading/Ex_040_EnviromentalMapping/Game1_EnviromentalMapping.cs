
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
        bool displayNormals = false;
        bool displayWhiteDiffuse = false;
        bool displayOnScreenText = false;
        int whichTechnique = 0;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont 
            font , 
            font2, 
            font3
            ;
        Texture2D 
            textureMesh ,
            textureMeshNormalMap,
            textureSphere, 
            textureNormalMapSphere, 
            textureMonogameLogo, 
            dotTextureRed, 
            dotTextureBlue, 
            dotTextureGreen, 
            dotTextureYellow, 
            dotTextureWhite
            ;
        Texture2D[] generatedTextureFaceArrayFromCubemap , generatedTextureFaceArrayFromHdrLdr, loadedOrAssignedArray;
        Texture2D
            generatedTextureFromSingleImages,
            generatedTextureFromCubeMap
            ;

        RenderTarget2D rtScene;
        CameraAndKeyboardControls cam = new CameraAndKeyboardControls();
        
        PrimitiveIndexedMesh mesh;
        VisualizationNormals visualMeshNormals = new VisualizationNormals();
        VisualizationNormals visualMeshTangents = new VisualizationNormals();
        VisualizationLine visualLightLineToMesh = new VisualizationLine();
        VisualizationLine visualLightLineToSphere = new VisualizationLine();

        PrimitiveSphere sphere;
        VisualizationNormals visualSphereNormals = new VisualizationNormals();
        VisualizationNormals visualSphereTangents = new VisualizationNormals();

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

        Vector3 lightStartPosition = new Vector3(150, 1, 300); // new Vector3(1, 800, 300)
        Vector3 lightPosition = new Vector3(0, 0, 0);
        Matrix lightTransform = Matrix.Identity;
        float lightRotationRadians = 0f;
        Vector3 meshDimensions = new Vector3(300f, 250,0);
        Vector3 meshCenter = new Vector3(150,125,0);
        Vector3 sphereCenter = new Vector3(0, 0, -50);

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

            // RefactionTexture with the opposite encoding
            // walltomap wallnormmap  Flower-normal , Flower-diffuse  Flower-bump  Flower-ambientocclusion
            // Quarry TextureAlignmentTestImage2
            textureSphere = Content.Load<Texture2D>("QuarrySquare");  
            textureNormalMapSphere = Content.Load<Texture2D>("wallnormmap"); 
            textureMesh = Content.Load<Texture2D>("Quarry");  // TestNormalMap
            textureMeshNormalMap = Content.Load<Texture2D>("TestNormalMap");

            textureMesh = textureMonogameLogo;

            cam.InitialView(GraphicsDevice, new Vector3(89.500f, +157.642f, -381.373f), -new Vector3(0.000f, +0.165f, -0.986f), Vector3.Down);
            //cam.InitialView(GraphicsDevice, new Matrix
            //        (
            //         -1.000f, +0.000f, +0.000f, +0.000f,
            //         +0.000f, +0.999f, +0.032f, +0.000f,
            //         +0.000f, +0.032f, -0.999f, +0.000f,
            //         +204.500f, +204.527f, -620.156f, +1.000f
            //        ));
            cam.UpdateProjection(GraphicsDevice);


            TextureTypeConverter.Load(Content);

            // Exhaustive Test.

            var temp = textureMonogameLogo; /*textureMonogameLogo*/; /*textureSphere*/
            var cubemap = TextureTypeConverter.ConvertSphericalTexture2DToTextureCube(GraphicsDevice, temp /*textureSphere*/, false, false, temp.Width);
            generatedTextureFaceArrayFromCubemap = TextureTypeConverter.ConvertTextureCubeToTexture2DArray(GraphicsDevice, cubemap, false, false, temp.Width);

            // Prove that the faces are assigned specifically to were they belong ... expected result is that the bug will not change and continuty is maintained as well.

            cubemap = new TextureCube(GraphicsDevice, temp.Width, false, SurfaceFormat.Color);
            Color[] data = new Color[temp.Width * temp.Height];
            loadedOrAssignedArray = new Texture2D[6];

            loadedOrAssignedArray[(int)CubeMapFace.PositiveX] = generatedTextureFaceArrayFromCubemap[(int)CubeMapFace.PositiveX];
            loadedOrAssignedArray[(int)CubeMapFace.PositiveX].GetData(data);
            cubemap.SetData(CubeMapFace.PositiveX, data);
            loadedOrAssignedArray[(int)CubeMapFace.PositiveY] = generatedTextureFaceArrayFromCubemap[(int)CubeMapFace.PositiveY];
            loadedOrAssignedArray[(int)CubeMapFace.PositiveY].GetData(data);
            cubemap.SetData(CubeMapFace.PositiveY, data);
            loadedOrAssignedArray[(int)CubeMapFace.PositiveZ] = generatedTextureFaceArrayFromCubemap[(int)CubeMapFace.PositiveZ];
            loadedOrAssignedArray[(int)CubeMapFace.PositiveZ].GetData(data);
            cubemap.SetData(CubeMapFace.PositiveZ, data);
            loadedOrAssignedArray[(int)CubeMapFace.NegativeX] = generatedTextureFaceArrayFromCubemap[(int)CubeMapFace.NegativeX];
            loadedOrAssignedArray[(int)CubeMapFace.NegativeX].GetData(data);
            cubemap.SetData(CubeMapFace.NegativeX, data);
            loadedOrAssignedArray[(int)CubeMapFace.NegativeY] = generatedTextureFaceArrayFromCubemap[(int)CubeMapFace.NegativeY];
            loadedOrAssignedArray[(int)CubeMapFace.NegativeY].GetData(data);
            cubemap.SetData(CubeMapFace.NegativeY, data);
            loadedOrAssignedArray[(int)CubeMapFace.NegativeZ] = generatedTextureFaceArrayFromCubemap[(int)CubeMapFace.NegativeZ];
            loadedOrAssignedArray[(int)CubeMapFace.NegativeZ].GetData(data);
            cubemap.SetData(CubeMapFace.NegativeZ, data);

            //var cubemap = TextureTypeConverter.ConvertTexture2DArrayToTextureCube(GraphicsDevice, loadedOrAssignedArray, false, false, 256);


            generatedTextureFromSingleImages = TextureTypeConverter.ConvertTexture2DArrayToSphericalTexture2D(GraphicsDevice, generatedTextureFaceArrayFromCubemap, false, false, 256);
            generatedTextureFaceArrayFromHdrLdr = TextureTypeConverter.ConvertSphericalTexture2DToTexture2DArray(GraphicsDevice, textureSphere /*generatedTextureFromSingleImages*/, false, false, 256);
            generatedTextureFromCubeMap = TextureTypeConverter.ConvertTextureCubeToSphericalTexture2D(GraphicsDevice, cubemap, false, false, 256);

            

            float thickness = .1f; float scale = 10f;

            // sphere.

            sphere = new PrimitiveSphere(2, 2, 50, false, false, false);  //sphere = new PrimitiveSphere(2, 2, 50, false, false, false);
            sphere.textureCube = cubemap;
            visualSphereNormals = CreateVisualNormalLines(sphere.vertices, sphere.indices, dotTextureGreen, thickness, scale, false);
            visualSphereTangents= CreateVisualNormalLines(sphere.vertices, sphere.indices, dotTextureYellow, thickness, scale, true);
            visualLightLineToSphere = CreateVisualLine(dotTextureWhite, sphereCenter, lightStartPosition, 1, Color.White);


            // mesh.

            PrimitiveIndexedMesh.ShowOutput = false;
            PrimitiveIndexedMesh.AveragingOption = PrimitiveIndexedMesh.AVERAGING_OPTION_USE_HIGHEST; //PrimitiveIndexedMesh.AVERAGING_OPTION_USE_RED;

            mesh = new PrimitiveIndexedMesh(5, 5, new Vector3(300f, 250, 0f), false, false);
            thickness = .1f; scale = 10f;

            //mesh = new PrimitiveIndexedMesh(heightMap, 9, new Vector3( 300f, 250, 70f ), false, false);
            //thickness = .1f; scale = 10f;

            //mesh = new PrimitiveIndexedMesh(texture, new Vector3(300f, 250, 5f), false, false);
            //thickness = .01f; scale = 1f;

            mesh.DiffuseTexture = textureMesh; 
            mesh.NormalMapTexture = textureMeshNormalMap;
            visualMeshNormals = CreateVisualNormalLines(mesh.vertices, mesh.indices, dotTextureGreen, thickness, scale, false);
            visualMeshTangents = CreateVisualNormalLines(mesh.vertices, mesh.indices, dotTextureYellow, thickness, scale, true);
            visualLightLineToMesh = CreateVisualLine(dotTextureWhite, meshCenter, lightStartPosition, 1, Color.White);


            EnviromentalMapEffectClass.Load(Content);
            EnviromentalMapEffectClass.Technique_Lighting_Phong();
            EnviromentalMapEffectClass.TextureCubeDiffuse = cubemap;
            EnviromentalMapEffectClass.TextureDiffuse = mesh.DiffuseTexture;
            EnviromentalMapEffectClass.TextureNormalMap = mesh.NormalMapTexture;
            EnviromentalMapEffectClass.AmbientStrength = .10f;
            EnviromentalMapEffectClass.View = cam.view;
            EnviromentalMapEffectClass.Projection = cam.projection;
            EnviromentalMapEffectClass.CameraPosition = cam.cameraWorld.Translation;
            EnviromentalMapEffectClass.LightPosition = lightPosition;
            EnviromentalMapEffectClass.LightColor = new Vector3(1f, 1f, 1f);
        }

        public VisualizationNormals CreateVisualNormalLines(VertexPositionNormalTextureTangentWeights[] verts, int[] indices, Texture2D texture, float thickness, float scale , bool grabTangentsInstead)
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
            vsn.CreateVisualNormalsForPrimitiveMesh(tmp, indices, texture, thickness, scale);
            vsn.SetUpBasicEffect(GraphicsDevice, texture, cam.view, cam.projection);
            return vsn;
        }

        public VisualizationLine CreateVisualLine(Texture2D texture, Vector3 startPosition, Vector3 endPosition, float thickness, Color color)
        {
            var vln = new VisualizationLine();
            vln.CreateVisualLine(texture, startPosition, endPosition, thickness, color);
            vln.SetUpBasicEffect(GraphicsDevice, texture, cam.view, cam.projection);
            return vln;
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


            if (Keys.Home.IsKeyPressedWithDelay(gameTime))
                cam.InitialView(GraphicsDevice);
            if (Keys.Space.IsKeyPressedWithDelay(gameTime))
                manuallyRotateLight = !manuallyRotateLight;

            if (manuallyRotateLight)
            {
                if (Keys.OemPlus.IsKeyDown())
                    lightRotationRadians += .005f;
                if (Keys.OemMinus.IsKeyDown())
                    lightRotationRadians -= .005f;
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
            visualLightLineToMesh = CreateVisualLine(dotTextureWhite, meshCenter, lightPosition, 1, Color.White);
            visualLightLineToSphere = CreateVisualLine(dotTextureWhite, sphereCenter, lightPosition, 1, Color.White);

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
            }
            DrawLightLineToMesh();

            
            if (displayWireframe)
                DrawWireFrameMesh();


            DrawSphere();
            if (displayNormals)
            {
                DrawNormalsForSphere();
                DrawTangentsForSphere();
            }
            DrawLightLineToSphere();


            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        public void DrawMesh()
        {
            if (displayWhiteDiffuse)
                mesh.DiffuseTexture = dotTextureWhite;
            else
                mesh.DiffuseTexture = mesh.DiffuseTexture;

            EnviromentalMapEffectClass.Technique_Lighting_Phong();
            EnviromentalMapEffectClass.World = Matrix.Identity;
            EnviromentalMapEffectClass.View = cam.view;
            EnviromentalMapEffectClass.Projection = cam.projection;
            EnviromentalMapEffectClass.LightPosition = lightPosition;
            EnviromentalMapEffectClass.CameraPosition = cam.cameraWorld.Translation;
            EnviromentalMapEffectClass.TextureDiffuse = mesh.DiffuseTexture;
            EnviromentalMapEffectClass.TextureNormalMap = mesh.NormalMapTexture;
            mesh.DrawPrimitive(GraphicsDevice, EnviromentalMapEffectClass.effect);
        }

        public void DrawSphere()
        {
            EnviromentalMapEffectClass.Technique_PhongCubeMap();
            EnviromentalMapEffectClass.World = Matrix.CreateTranslation(sphereCenter); //Matrix.Identity; // Matrix.CreateTranslation(sphereCenter);
            EnviromentalMapEffectClass.View = cam.view;
            EnviromentalMapEffectClass.Projection = cam.projection;
            EnviromentalMapEffectClass.LightPosition = lightPosition;
            EnviromentalMapEffectClass.CameraPosition = cam.cameraWorld.Translation;
            EnviromentalMapEffectClass.TextureCubeDiffuse = sphere.textureCube;
            EnviromentalMapEffectClass.TextureDiffuse = textureSphere;
            EnviromentalMapEffectClass.TextureNormalMap = textureNormalMapSphere;
            sphere.DrawPrimitiveSphere(GraphicsDevice, EnviromentalMapEffectClass.effect);
        }

        public void DrawNormalsForMesh()
        {
            visualMeshNormals.World = Matrix.Identity;
            visualMeshNormals.View = cam.view;
            visualMeshNormals.Draw(GraphicsDevice);
        }

        public void DrawNormalsForSphere()
        {
            visualSphereNormals.World = Matrix.CreateTranslation(sphereCenter);
            visualSphereNormals.View = cam.view;
            visualSphereNormals.Projection = cam.projection;
            visualSphereNormals.Draw(GraphicsDevice);
        }

        public void DrawTangentsForMesh()
        {
            visualMeshTangents.World = Matrix.Identity;
            visualMeshTangents.View = cam.view;
            visualMeshTangents.Draw(GraphicsDevice);
        }

        public void DrawTangentsForSphere()
        {
            visualSphereTangents.World = Matrix.CreateTranslation(sphereCenter);
            visualSphereTangents.View = cam.view;
            visualSphereTangents.Projection = cam.projection;
            visualSphereTangents.Draw(GraphicsDevice);
        }

        public void DrawLightLineToMesh()
        {
            visualLightLineToMesh.World = Matrix.Identity; //lightTransform;
            visualLightLineToMesh.View = cam.view;
            visualLightLineToMesh.Draw(GraphicsDevice);
        }

        public void DrawLightLineToSphere()
        {
            visualLightLineToSphere.World = Matrix.Identity;
            visualLightLineToSphere.View = cam.view;
            visualLightLineToSphere.Draw(GraphicsDevice);
        }

        public void DrawWireFrameMesh()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_WIREFRAME;
            visualMeshNormals.World = Matrix.Identity;
            visualMeshNormals.View = cam.view;
            EnviromentalMapEffectClass.TextureDiffuse = dotTextureRed;
            mesh.DrawPrimitive(GraphicsDevice, EnviromentalMapEffectClass.effect);
        }


        public void DrawSpriteBatches(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            // Draw all the regular stuff
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);

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
                spriteBatch.DrawString(font, $"Press F1 for information  \n{spectypemsg}", new Vector2(10, 10), Color.Red);

            
            spriteBatch.Draw(textureSphere, new Rectangle(0, 10, 100, 120), Color.White);
            for (int i =0; i <  generatedTextureFaceArrayFromCubemap.Length; i++)
            {
                spriteBatch.Draw(generatedTextureFaceArrayFromCubemap[i], new Rectangle((i+1) * 140 , 10, 100, 100), Color.White);
                spriteBatch.Draw(generatedTextureFaceArrayFromHdrLdr[i], new Rectangle((i + 1) * 140 +35, 10 +15, 100, 100), Color.White);
            }
            spriteBatch.Draw(generatedTextureFromSingleImages, new Rectangle(0, 180, 100, 120), Color.White);
            spriteBatch.Draw(generatedTextureFromCubeMap, new Rectangle(150, 190, 100, 120), Color.White);


            if (Keys.End.IsKeyPressedWithDelay(gameTime))
                Console.WriteLine( $"{cam.cameraWorld.DisplayMatrixForCopy("cameraWorld") }");

            spriteBatch.End();
        }

    }
}
