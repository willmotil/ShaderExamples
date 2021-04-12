
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{
    public class Game1_DiffuseLighting : Game
    {
        bool rotateLight = true;
        bool displayWireframe = false;
        bool displayNormals = false;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        SpriteFont font3;
        Texture2D texture;
        Texture2D dotTexture;
        Texture2D dotTexture2;
        Texture2D dotTexture3;
        RenderTarget2D rtScene;
        CameraAndKeyboardControls cam = new CameraAndKeyboardControls();
        PrimitiveIndexedMesh mesh;
        VisualizationNormals visualNormals = new VisualizationNormals();
        VisualizationNormals visualLightNormal = new VisualizationNormals();

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

        public Vector3 lightPosition = new Vector3(0, 0, 500f);
        public Matrix lightTransform = Matrix.Identity;
        public float lightRotationRadians = 0f;

        public Game1_DiffuseLighting()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Diffuse lighting ";
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
            DiffuseLightEffectClass.Projection = cam.projection;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Modifyed"); // MG_Logo_Med_exCanvs  blue_atmosphere
            
            dotTexture = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Red);
            dotTexture2 = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Green);
            dotTexture3 = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.White);

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

            DiffuseLightEffectClass.Load(Content);
            DiffuseLightEffectClass.SpriteTexture = dotTexture3;
            DiffuseLightEffectClass.View = cam.view;
            DiffuseLightEffectClass.Projection = cam.projection;

            PrimitiveIndexedMesh.ShowOutput = false;
            PrimitiveIndexedMesh.AveragingOption = PrimitiveIndexedMesh.AVERAGING_OPTION_USE_RED;

            //mesh = new PrimitiveIndexedMesh(4,4, new Vector3(300f, 250, 0f ), false, false);
            //mesh = new PrimitiveIndexedMesh(heightMap, 9, new Vector3( 300f, 250, 70f ), false, false);
            mesh = new PrimitiveIndexedMesh(texture, new Vector3(300f, 250, 5f), false, false);

            CreateVisualMeshNormals(mesh, dotTexture2, 0.10f, 3.0f);
            CreateVisualLightNormal(dotTexture2, 2, 3000);
        }

        public void CreateVisualMeshNormals(PrimitiveIndexedMesh mesh, Texture2D texture, float thickness, float lineLength)
        {
            VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; i++)
                tmp[i] = new VertexPositionNormalTexture() { Position = mesh.vertices[i].Position, Normal = mesh.vertices[i].Normal, TextureCoordinate = mesh.vertices[i].TextureCoordinate };
            visualNormals.CreateVisualNormalsForPrimitiveMesh(tmp, mesh.indices, texture, thickness, .1f);
        }

        public void CreateVisualLightNormal(Texture2D texture, float thickness, float lineLength)
        {
            VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[1];
            tmp[0] = new VertexPositionNormalTexture() { Position = new Vector3(0, 0, 0), Normal = new Vector3(0, 0, 1), TextureCoordinate = new Vector2(0, 0) };
            int[] tmpindices = new int[1];
            tmpindices[0] = 0;
            visualLightNormal.CreateVisualNormalsForPrimitiveMesh(tmp, tmpindices, texture, thickness, lineLength);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            cam.Update(gameTime);

            // Reset the view projection matrix.
            if (Keys.F1.IsKeyPressedWithDelay(gameTime))
                cam.InitialView(GraphicsDevice);

            if (Keys.F2.IsKeyPressedWithDelay(gameTime))
                displayWireframe = !displayWireframe;
            if (Keys.F3.IsKeyPressedWithDelay(gameTime))
                displayNormals = !displayNormals;

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



        RasterizerState rasterizerState_CULLNONE_WIREFRAME = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        RasterizerState rasterizerState_CULLNONE_SOLID = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.Solid };

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Moccasin);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;

            DiffuseLightEffectClass.View = cam.view;
            DiffuseLightEffectClass.Projection = cam.projection;
            DiffuseLightEffectClass.World = Matrix.Identity;
            DiffuseLightEffectClass.LightPosition = lightPosition;

            DrawMesh(dotTexture3);

            if(displayWireframe)
                DrawWireFrameMesh();

            if(displayNormals)
                DrawNormalsForMesh();

            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        public void DrawMesh(Texture2D texture)
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            DiffuseLightEffectClass.SpriteTexture = texture;
            mesh.DrawPrimitive(GraphicsDevice, DiffuseLightEffectClass.effect);
        }

        public void DrawWireFrameMesh()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_WIREFRAME;
            DiffuseLightEffectClass.SpriteTexture = dotTexture;
            mesh.DrawPrimitive(GraphicsDevice, DiffuseLightEffectClass.effect);
        }

        public void DrawNormalsForMesh()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            DiffuseLightEffectClass.SpriteTexture = visualNormals.texture;
            visualNormals.Draw(GraphicsDevice, DiffuseLightEffectClass.effect);

            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            DiffuseLightEffectClass.World = lightTransform;
            DiffuseLightEffectClass.SpriteTexture = dotTexture;
            visualLightNormal.Draw(GraphicsDevice, DiffuseLightEffectClass.effect);
        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
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
                $" \n { cam.cameraWorld.DisplayMatrix("cameraWorld") }" +
                $" \n { cam.view.DisplayMatrix("view") }" +
                $" \n { cam.projection.DisplayMatrix("projection") }" +
                $" \n" +
                $" \n {lightPosition.VectorToString("LightPosition")}" +
                $" \n"
                ;
            spriteBatch.DrawString(font, msg, new Vector2(10, 10), Color.Blue);
            spriteBatch.End();
        }

    }
}
