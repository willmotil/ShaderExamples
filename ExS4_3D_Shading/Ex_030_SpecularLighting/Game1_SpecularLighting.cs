
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_SpecularLighting : Game
    {
        bool manuallyRotateLight = false;
        bool displayMesh = true;
        bool displayWireframe = false;
        bool displayNormals = false;
        bool displayWhiteDiffuse = false;
        bool displayOnScreenText = false;
        int whichTechnique = 0;

        string switchCreateMeshId = "Simple";

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        SpriteFont font3;
        Texture2D textureMesh; 
        Texture2D textureMeshNormalMap;
        Texture2D dotTextureRed;
        Texture2D dotTextureBlue;
        Texture2D dotTextureGreen;
        Texture2D dotTextureYellow;
        Texture2D dotTextureWhite;
        RenderTarget2D rtScene;
        CameraAndKeyboardControls cam = new CameraAndKeyboardControls();
        PrimitiveIndexedMesh mesh;
        VisualizationNormals visualMeshNormals = new VisualizationNormals();
        VisualizationNormals visualMeshTangents = new VisualizationNormals();
        VisualizationLine visualLightLineToMesh;

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
        Vector3 lightPosition = new Vector3(150, 1, 300);
        Matrix lightTransform = Matrix.Identity;
        float lightRotationRadians = 0f;
        Vector3 meshDimensions = new Vector3(300f, 250,0);
        Vector3 meshCenter = new Vector3(150f,125f, 0.0f);

        string spectypemsg = "";

        RasterizerState rasterizerState_CULLNONE_WIREFRAME = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        RasterizerState rasterizerState_CULLNONE_SOLID = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.Solid };


        public Game1_SpecularLighting()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Normal Mapping with Diffuse and Specular lighting ";
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

            dotTextureRed = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Red);
            dotTextureGreen = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Green);
            dotTextureBlue = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Blue);
            dotTextureWhite = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.White);
            dotTextureYellow = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Yellow);

            Content.RootDirectory = @"Content/Images";
            textureMesh = dotTextureWhite;
            textureMeshNormalMap = Content.Load<Texture2D>("TestNormalMap");

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");
            font3 = Content.Load<SpriteFont>("MgFont3");

            cam.InitialView(GraphicsDevice, new Matrix
                (
                +1.000f, +0.000f, +0.000f, +0.000f,
                +0.000f, -0.986f, -0.165f, +0.000f,
                +0.000f, +0.165f, -0.986f, +0.000f,
                +89.500f, +157.642f, -381.373f, +1.000f
               ));
            cam.UpdateProjection(GraphicsDevice);


            PrimitiveIndexedMesh.ShowOutput = false;
            PrimitiveIndexedMesh.AveragingOption = PrimitiveIndexedMesh.AVERAGING_OPTION_USE_HIGHEST;

            float thickness = .1f; 
            float scale = 10f;
            switch (switchCreateMeshId)
            {
                case "Simple":
                    mesh = new PrimitiveIndexedMesh(5, 5, new Vector3(300f, 250, 0f), false, false, false);
                    thickness = .1f; scale = 10f;
                    break;
                case "HeightArrayMap":
                    mesh = new PrimitiveIndexedMesh(heightMap, 9, new Vector3(300f, 250, 70f), false, false, false);
                    thickness = .1f; scale = 10f;
                    break;
                case "HeightTextureMap":
                    mesh = new PrimitiveIndexedMesh(textureMesh, new Vector3(300f, 250, 5f), false, false, false);
                    thickness = .01f; scale = 1f;
                    break;
            };


            mesh.NormalMapTexture = textureMeshNormalMap;
            if (displayWhiteDiffuse)
                mesh.DiffuseTexture = dotTextureWhite;
            else
                mesh.DiffuseTexture = textureMesh; 

            visualMeshNormals = CreateVisualNormalLines(mesh.vertices, mesh.indices, dotTextureGreen, thickness, scale, false);
            visualMeshTangents = CreateVisualNormalLines(mesh.vertices, mesh.indices, dotTextureYellow, thickness, scale, true);
            visualLightLineToMesh = new VisualizationLine(dotTextureWhite, meshCenter, lightStartPosition, 1.0f, Color.White);
            visualLightLineToMesh.SetUpBasicEffect(GraphicsDevice, dotTextureWhite, cam.view, cam.projection);

            SpecularLightEffectClass.Load(Content);
            SpecularLightEffectClass.Technique_Lighting_Phong();
            SpecularLightEffectClass.TextureDiffuse = textureMesh;
            SpecularLightEffectClass.TextureNormalMap = textureMeshNormalMap;
            SpecularLightEffectClass.AmbientStrength = .10f;
            SpecularLightEffectClass.View = cam.view;
            SpecularLightEffectClass.Projection = cam.projection;
            SpecularLightEffectClass.CameraPosition = cam.cameraWorld.Translation;
            SpecularLightEffectClass.LightPosition = lightPosition;
            SpecularLightEffectClass.LightColor = new Vector3(1f, 1f, 1f);
        }

        public VisualizationNormals CreateVisualNormalLines(VertexPositionNormalTextureTangentWeights[] verts, int[] indices, Texture2D texture, float thickness, float lineLength, bool grabTangentsInstead)
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
                if(whichTechnique > 2)
                    whichTechnique = 0;
                switch(whichTechnique)
                {
                    case 0:
                        SpecularLightEffectClass.Technique_Lighting_Phong();
                        spectypemsg = "Phong";
                        break;
                    case 1:
                        SpecularLightEffectClass.Technique_Lighting_Blinn();
                        spectypemsg = "Blinn";
                        break;
                    case 2:
                        SpecularLightEffectClass.Technique_Lighting_Wills();
                        spectypemsg = "generic";
                        break;
                }
            }


            //ReusedUpdateCode(gameTime);


            if (Keys.End.IsKeyPressedWithDelay(gameTime))
                Console.WriteLine($"{cam.cameraWorld.ToDisplayMatrixForCopy("cameraWorld") }");

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

            visualLightLineToMesh.ReCreateVisualLine(dotTextureWhite, meshCenter, lightPosition, 1, Color.White);

            base.Update(gameTime);
        }

        // well move this down here as it's just going to stay the same pretty much.
        public void ReusedUpdateCode(GameTime gameTime)
        {

        }

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
                DrawNormalsAndTangents();

            DrawLightLines();

            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }


        public void DrawMesh()
        {
            SpecularLightEffectClass.View = cam.view;
            SpecularLightEffectClass.Projection = cam.projection;
            SpecularLightEffectClass.World = Matrix.Identity;
            SpecularLightEffectClass.TextureDiffuse = mesh.DiffuseTexture;
            SpecularLightEffectClass.TextureNormalMap = mesh.NormalMapTexture;
            SpecularLightEffectClass.LightPosition = lightPosition;
            SpecularLightEffectClass.CameraPosition = cam.cameraWorld.Translation;
            mesh.DrawPrimitive(GraphicsDevice, SpecularLightEffectClass.effect);

            if (displayWireframe)
            {
                GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_WIREFRAME;
                visualMeshNormals.World = Matrix.Identity;
                visualMeshNormals.View = cam.view;
                SpecularLightEffectClass.TextureDiffuse = dotTextureRed;
                mesh.DrawPrimitive(GraphicsDevice, SpecularLightEffectClass.effect);
            }
        }

        public void DrawNormalsAndTangents()
        {
            visualMeshNormals.World = Matrix.Identity;
            visualMeshNormals.View = cam.view;
            visualMeshNormals.Projection = cam.projection;
            visualMeshNormals.Draw(GraphicsDevice);

            visualMeshTangents.World = Matrix.Identity;
            visualMeshTangents.View = cam.view;
            visualMeshTangents.Projection = cam.projection;
            visualMeshTangents.Draw(GraphicsDevice);
        }

        public void DrawLightLines()
        {

            visualLightLineToMesh.World = Matrix.Identity;
            visualLightLineToMesh.View = cam.view;
            visualLightLineToMesh.Projection = cam.projection;
            visualLightLineToMesh.Draw(GraphicsDevice);
        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);

                string msg =
                    $" \n The F2 toggle wireframe. F3 show normals. F4 mesh itself. F5 the texture used." +
                    $" \n F6 switch techniques {spectypemsg}. Space toggle light controls." +
                    $" \n The keys WASD change the forward view direction (which is the major take away here). ZC allows for spin." +
                    $" \n The Arrow keys move the camera translation as strafing motion. " +
                    $" \n  " +
                    $" \n In this example we make a shader that creates a diffuse light." +
                    $" \n The light rotates around the mesh illuminating faces depending on the triangle normals." +
                    $" \n We also create a class that allows us to visualize normals per vertice and for the light." +
                    $" \n Well place the light into rotation so we can see how the diffuse shader works." +
                    $" \n Simple diffuse lighting is achieved via a dot product on the light and normals aka NdotL ." +
                    $" \n this can be found in the shader" +
                    $" \n  " +
                    $" \n {lightPosition.ToTrimmedString("LightPosition")}" +
                    $" \n"
                    ;

            if (displayOnScreenText)
                spriteBatch.DrawString(font, msg, new Vector2(10, 10), Color.Red);
            else
                spriteBatch.DrawString(font, $"Press F1 for information  {lightPosition.ToTrimmedString("LightPosition")}  \n{spectypemsg}", new Vector2(10, 10), Color.Red);

            spriteBatch.End();
        }

    }
}
