
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_NormalMapping : Game
    {
        /* Excerpt. https://dreamlight.com/how-to-create-normal-maps-from-photographs/
         Normal maps use the three color channels R-red, G-green and B-blue to encode the X, Y and Z normal vector data in an 8-bit image.

The R channel in the image ranges from 0 to 255 and represents the left to right direction. 
        R = 0 corresponds to X = -1 pointing toward the left side of the image 
        R = 255 corresponds to X = 1 pointing toward the right side of the image.

Then the G channel in the image ranges from 0 to 255 and represents the bottom to top direction. 
        G = 0 corresponds to Y = -1 pointing toward the bottom of the image 
        G = 255 corresponds to Y = 1 pointing toward the top of the image.

Finally the B channel in the image ranges from 128 to 255 
        represents how far out the vector points toward the viewer. B = 128 corresponds to Z = 0  pointing along the same plane as the image 
        B = 255 corresponds to Z = -1 pointing perpendicular to the image. 
        The Z channel only uses half of the range, B = 128 to 255 and Z = 0 to -1 because the normal can only point between parallel to the image plane or perpendicular to the front of the image plane. 
        The normal can’t go backward behind the image plane.
         */


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
        VisualizationNormals visualLightNormal = new VisualizationNormals();


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
            cam.UpdateProjection(GraphicsDevice);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("walltomap");
            textureNormalMap = Content.Load<Texture2D>("wallnormmap");

            dotTextureRed = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Red);
            dotTextureGreen = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Green);
            dotTextureBlue = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Blue);
            dotTextureWhite = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.White);
            dotTextureYellow = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Yellow);

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");
            font3 = Content.Load<SpriteFont>("MgFont3");

            cam.InitialView(GraphicsDevice, new Matrix
                    (
                     -1.000f, +0.000f, +0.000f, +0.000f,
                     +0.000f, +0.999f, +0.032f, +0.000f,
                     +0.000f, +0.032f, -0.999f, +0.000f,
                     +204.500f, +204.527f, -620.156f, +1.000f
                    ));
            cam.UpdateProjection(GraphicsDevice);

            NormalMapEffectClass.Load(Content);
            NormalMapEffectClass.TextureDiffuse = dotTextureWhite;
            NormalMapEffectClass.View = cam.view;
            NormalMapEffectClass.Projection = cam.projection;


            PrimitiveIndexedMesh.ShowOutput = false;
            PrimitiveIndexedMesh.AveragingOption = PrimitiveIndexedMesh.AVERAGING_OPTION_USE_HIGHEST; //PrimitiveIndexedMesh.AVERAGING_OPTION_USE_RED;

            //mesh = new PrimitiveIndexedMesh(5, 5, new Vector3(300f, 250, 0f), false, false, false);
            //float thickness = .1f; float lineScale = 10f;

            //mesh = new PrimitiveIndexedMesh(heightMap, 9, new Vector3( 300f, 250, 70f ), false, false, false);
            //float thickness = .1f; float lineScale = 10f;

            mesh = new PrimitiveIndexedMesh(texture, new Vector3(300f, 250, 5f), false, false, false);
            float thickness = .01f; float lineScale = 1f;

            mesh.DiffuseTexture = dotTextureWhite; // texture;
            mesh.NormalMapTexture = textureNormalMap;

            CreateVisualMeshNormals(mesh, dotTextureGreen, thickness, lineScale);  // .6  3
            CreateVisualMeshTangents(mesh, dotTextureYellow, thickness, lineScale);
            CreateVisualLightNormal(dotTextureWhite, 1, 300);
        }

        public void CreateVisualMeshNormals(PrimitiveIndexedMesh mesh, Texture2D texture, float thickness, float lineLength)
        {
            VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; i++)
                tmp[i] = new VertexPositionNormalTexture() { Position = mesh.vertices[i].Position, Normal = mesh.vertices[i].Normal, TextureCoordinate = mesh.vertices[i].TextureCoordinate };
            visualNormals.CreateVisualNormalsForPrimitiveMesh(tmp, mesh.indices, texture, thickness, lineLength);
            visualNormals.SetUpBasicEffect(GraphicsDevice, texture, cam.view, cam.projection);
        }

        public void CreateVisualMeshTangents(PrimitiveIndexedMesh mesh, Texture2D texture, float thickness, float lineLength)
        {
            VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; i++)
                tmp[i] = new VertexPositionNormalTexture() { Position = mesh.vertices[i].Position, Normal = mesh.vertices[i].Tangent, TextureCoordinate = mesh.vertices[i].TextureCoordinate };
            visualTangents.CreateVisualNormalsForPrimitiveMesh(tmp, mesh.indices, texture, thickness, lineLength);
            visualTangents.SetUpBasicEffect(GraphicsDevice, texture, cam.view, cam.projection);
        }

        public void CreateVisualLightNormal(Texture2D texture, float thickness, float lineLength)
        {
            VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[1];
            tmp[0] = new VertexPositionNormalTexture() { Position = new Vector3(0, 0, 0), Normal = new Vector3(0, 0, 1), TextureCoordinate = new Vector2(0, 0) };
            int[] tmpindices = new int[1];
            tmpindices[0] = 0;
            visualLightNormal.CreateVisualNormalsForPrimitiveMesh(tmp, tmpindices, texture, thickness, lineLength);
            visualLightNormal.SetUpBasicEffect(GraphicsDevice, texture, cam.view, cam.projection);
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
                lightRotationRadians += .005f;
                if (lightRotationRadians > 6.12)
                    lightRotationRadians = 0;
                lightTransform = Matrix.CreateFromAxisAngle(new Vector3(1,1,0), lightRotationRadians);
                lightPosition = Vector3.Transform(new Vector3(0, 0, 500), lightTransform);
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

            NormalMapEffectClass.View = cam.view;
            NormalMapEffectClass.Projection = cam.projection;
            NormalMapEffectClass.World = Matrix.Identity;
            NormalMapEffectClass.LightPosition = lightPosition;

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


            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        public void DrawMesh()
        {
            if (displayWhiteDiffuse)
                mesh.DiffuseTexture = dotTextureWhite;
            else
                mesh.DiffuseTexture = texture;
            NormalMapEffectClass.TextureDiffuse = mesh.DiffuseTexture;
            NormalMapEffectClass.TextureNormalMap = mesh.NormalMapTexture;
            mesh.DrawPrimitive(GraphicsDevice, NormalMapEffectClass.effect);
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
            visualLightNormal.World = lightTransform;
            visualLightNormal.View = cam.view;
            visualLightNormal.Draw(GraphicsDevice);
        }

        public void DrawWireFrameMesh()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_WIREFRAME;
            NormalMapEffectClass.TextureDiffuse = dotTextureRed;
            mesh.DrawPrimitive(GraphicsDevice, NormalMapEffectClass.effect);
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
            $" \n { cam.cameraWorld.ToWellFormatedString("cameraWorld") }" +
            $" \n { cam.view.ToWellFormatedString("view") }" +
            $" \n { cam.projection.ToWellFormatedString("projection") }" +
            $" \n" +
            $" \n {lightPosition.ToTrimmedString("LightPosition")}" +
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
}
