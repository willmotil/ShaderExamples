
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_SkyBoxes : Game
    {
        bool manuallyRotateLight = false;
        bool displayMesh = true;
        bool displayWireframe = false;
        bool displayNormals = true;
        bool displayWhiteDiffuse = false;
        bool displayOnScreenText = true;
        bool flipCullToClockWise = false;
        int whichTechnique = 0;

        RasterizerState rasterizerState_CULLNONE_WIREFRAME = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        RasterizerState rasterizerState_CULLNONE_SOLID = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.Solid };

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont
            font,
            font2,
            font3
            ;
        TextureCube cubemap, cubemap2;
        Texture2D
            textureMesh, textureMeshNormalMap,
            textureSphereHdrLdr, textureSphereNormalMap,
            miscTexture, textureMonogameLogo,
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


        public static int numberOfSpheres = 2;
        PrimitiveSphere[] spheres = new PrimitiveSphere[numberOfSpheres];
        VisualizationNormals[] visualSphereNormals = new VisualizationNormals[numberOfSpheres];
        VisualizationNormals[] visualSphereTangents = new VisualizationNormals[numberOfSpheres];
        VisualizationLine[] visualLightLineToSpheres = new VisualizationLine[numberOfSpheres];
        Vector3[] sphereCenters = new Vector3[] { new Vector3(0, 0, -50), new Vector3(150, 0, -50), new Vector3(0, 150, -50), new Vector3(150, 150, -50) };


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

        Vector3 lightStartPosition = new Vector3(300, 125, 300);
        Vector3 lightPosition = new Vector3(0, 0, 0);
        Matrix lightTransform = Matrix.Identity;
        float lightRotationRadians = 0f;
        Vector3 meshDimensions = new Vector3(300f, 250, 0);
        Vector3 meshCenter = new Vector3(150, 125, 0);

        string spectypemsg = "";

        public Game1_SkyBoxes()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Cubemaps Skyboxes ";
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

            CreateSpheres();
        }

        public void SetCamera()
        {
            cam.InitialView(GraphicsDevice, new Vector3(+0f, +0f, -381.373f), Vector3.UnitZ, -Vector3.UnitY);
            cam.UpdateProjection(GraphicsDevice, 1.4f);
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

            textureMonogameLogo = Content.Load<Texture2D>("MG_Logo_Modifyed");
            miscTexture = Content.Load<Texture2D>("TextureAlignmentTestImage2");

            // RefactionTexture has the opposite encoding walltomap wallnormmap TestNormalMap  Flower-normal , Flower-diffuse  Flower-bump  Flower-ambientocclusion  Quarry  QuarrySquare MG_Logo_Modifyed TextureAlignmentTestImage2
            textureSphereHdrLdr = Content.Load<Texture2D>("QuarrySquare");
            textureSphereNormalMap = Content.Load<Texture2D>("TestNormalMap");

            TextureCubeTypeConverter.Load(Content);
            cubemap = TextureCubeTypeConverter.ConvertSphericalTexture2DToTextureCube(GraphicsDevice, textureSphereHdrLdr, false, false, textureSphereHdrLdr.Width);
            cubemap2 = TextureCubeTypeConverter.ConvertSphericalTexture2DToTextureCube(GraphicsDevice, miscTexture, false, false, miscTexture.Width);
            generatedTextureHdrLdrFromCubeMap = TextureCubeTypeConverter.ConvertTextureCubeToSphericalTexture2D(GraphicsDevice, cubemap, false, false, 256);
            generatedTextureFaceArrayFromCubemap = TextureCubeTypeConverter.ConvertTextureCubeToTexture2DArray(GraphicsDevice, cubemap, false, false, 256);
            generatedTextureHdrLdrFromSingleImages = TextureCubeTypeConverter.ConvertTexture2DArrayToSphericalTexture2D(GraphicsDevice, generatedTextureFaceArrayFromCubemap, false, false, 256);
            generatedTextureFaceArrayFromHdrLdr = TextureCubeTypeConverter.ConvertSphericalTexture2DToTexture2DArray(GraphicsDevice, textureSphereHdrLdr, false, false, 256);

        }

        //++++++++++++++++++++++++++++++++++
        // Create
        //++++++++++++++++++++++++++++++++++

        public void CreateSpheres()
        {

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
                        usage = PrimitiveSphere.USAGE_CUBE_UNDER_CCW; // = 0
                        scale = 40;
                        break;
                    case 1:
                        usage = PrimitiveSphere.USAGE_SKYSPHERE_UNDER_CCW; // 2
                        scale = 5000;
                        break;
                    case 2:
                        usage = PrimitiveSphere.USAGE_CUBE_UNDER_CW; // 1
                        scale = 40;
                        break;
                    case 3:
                        usage = PrimitiveSphere.USAGE_SKYSPHERE_UNDER_CW; // 3
                        scale = 5000;
                        break;
                }
                if (index == 1)
                    CreateSphere(cubemap, ref spheres[index], ref visualSphereNormals[index], ref visualSphereTangents[index], ref visualLightLineToSpheres[index], sphereCenters[index], scale, usage, false, false);
                else
                    CreateSphere(cubemap2, ref spheres[index], ref visualSphereNormals[index], ref visualSphereTangents[index], ref visualLightLineToSpheres[index], sphereCenters[index], scale, usage, false, false);
            }
        }
    
        public void CreateSphere(TextureCube textureCube, ref PrimitiveSphere asphere, ref VisualizationNormals visnorm, ref VisualizationNormals vistan, ref VisualizationLine vline, Vector3 sphereCenter, float spherescale, int primUsage, bool invert, bool flatfaces)
        {
            asphere = new PrimitiveSphere(5, 5, 1f, primUsage, invert, flatfaces);
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
            SkyBoxDrawingEffectClass.Load(Content);
            SkyBoxDrawingEffectClass.TextureCubeDiffuse = cubemap;
            //SkyBoxDrawingEffectClass.TextureDiffuse = textureMesh;
            //SkyBoxDrawingEffectClass.TextureNormalMap = textureMeshNormalMap;
            SkyBoxDrawingEffectClass.AmbientStrength = 0.4f;
            SkyBoxDrawingEffectClass.DiffuseStrength = .8f;
            SkyBoxDrawingEffectClass.SpecularStrength = .6f;
            SkyBoxDrawingEffectClass.View = cam.view;
            SkyBoxDrawingEffectClass.Projection = cam.projection;
            SkyBoxDrawingEffectClass.CameraPosition = cam.cameraWorld.Translation;
            SkyBoxDrawingEffectClass.LightPosition = lightPosition;
            SkyBoxDrawingEffectClass.LightColor = new Vector3(1f, 1f, 1f);
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
            
            if (Keys.F7.IsKeyPressedWithDelay(gameTime))
                flipCullToClockWise = !flipCullToClockWise;

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

            for (int index = 0; index < spheres.Length; index++)
            {
                visualLightLineToSpheres[index].ReCreateVisualLine(dotTextureWhite, spheres[index].Position, lightPosition, 1, Color.White);
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
            SkyBoxDrawingEffectClass.View = cam.view;
            SkyBoxDrawingEffectClass.Projection = cam.projection;
            SkyBoxDrawingEffectClass.LightPosition = lightPosition;
            SkyBoxDrawingEffectClass.CameraPosition = cam.cameraWorld.Translation;

            if (flipCullToClockWise)
                GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            else
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;


            // S P H E R E S   S K Y    

            for (int index = 0; index < spheres.Length; index++)
            {
                if (spheres[index].IsSkyBox)
                {
                    SkyBoxDrawingEffectClass.Technique_Render_Skybox();
                }
                else
                {
                    SkyBoxDrawingEffectClass.Technique_Render_Cube();
                }
                SkyBoxDrawingEffectClass.World = spheres[index].WorldTransformation;
                SkyBoxDrawingEffectClass.TextureCubeDiffuse = spheres[index].textureCube;
                spheres[index].DrawPrimitiveSphere(GraphicsDevice, SkyBoxDrawingEffectClass.effect);
            }


            // W I R E F R A M E

            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_WIREFRAME;
            if (displayWireframe)
            {
                for (int index = 0; index < spheres.Length; index++)
                {
                    SkyBoxDrawingEffectClass.World = spheres[index].WorldTransformation;
                    spheres[index].DrawPrimitiveSphere(GraphicsDevice, SkyBoxDrawingEffectClass.effect);
                }
            }
        }

        public void DrawNormalsTangentsAndLightLines()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;

            if (displayNormals)
            {
                DrawNormalsAndTangentsForSphere();
            }
            DrawLightLineToSphere();
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

            spriteBatch.Draw(textureSphereHdrLdr, new Rectangle(0, 10, 100, 120), Color.White);
            for (int i = 0; i < generatedTextureFaceArrayFromCubemap.Length; i++)
            {
                spriteBatch.Draw(generatedTextureFaceArrayFromCubemap[i], new Rectangle((i + 1) * 140, 10, 100, 100), Color.White);
                spriteBatch.Draw(generatedTextureFaceArrayFromHdrLdr[i], new Rectangle((i + 1) * 140 + 35, 10 + 15, 100, 100), Color.White);
            }
            //spriteBatch.Draw(generatedTextureHdrLdrFromSingleImages, new Rectangle(0, 180, 100, 120), Color.White);
            //spriteBatch.Draw(generatedTextureHdrLdrFromCubeMap, new Rectangle(150, 190, 100, 120), Color.White);

            string msg =
                    $" \n The F2 toggle wireframe. F3 show normals. F4 mesh itself. F5 the texture used." +
                    $" \n F6 switch techniques {spectypemsg}. Space toggle light controls." +
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

            if (displayOnScreenText)
                spriteBatch.DrawString(font, msg, new Vector2(10, 110), Color.White);
            else
                spriteBatch.DrawString(
                    font, 
                    $"Press F1 for information  " +
                    $"\n{spectypemsg} " +
                    $"\nIs GraphicsDevice CullClockwise : { flipCullToClockWise}" +
                    $"" +
                    $"" , 
                    new Vector2(10, 110), 
                    Color.White
                    );


            if (Keys.End.IsKeyPressedWithDelay(gameTime))
                Console.WriteLine( $"{cam.cameraWorld.ToDisplayMatrixForCopy("cameraWorld") } ");

            spriteBatch.End();
        }

    }
}
