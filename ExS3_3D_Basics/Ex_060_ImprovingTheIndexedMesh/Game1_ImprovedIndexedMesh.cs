
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
// cause something broke the easy console way grrrrr.
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ShaderExamples
{
    public class Game1_ImprovedIndexedMesh : Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        SpriteFont font3;
        Texture2D texture;
        Texture2D dotTexture;
        Texture2D dotTexture2;
        RenderTarget2D rtScene;
        CameraAndKeyboardControls cam = new CameraAndKeyboardControls();
        PrimitiveIndexedMesh mesh;
        PrimitiveNormalArrows visualNormals = new PrimitiveNormalArrows();

        float[] heightMap = new float[]
        {
            0.0f, 0.1f, 0.2f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.8f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.5f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f
        };

        Vector3 lightPosition = new Vector3(0, 0, 500f);
        float lightRotationRadians = 0f;

        bool rotateLight = true;
        bool displayWireframe = false;
        bool displayNormals = false;

        public Game1_ImprovedIndexedMesh()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Improved Indexed Mesh ";
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
            texture = Content.Load<Texture2D>("MG_Logo_Modifyed"); // MG_Logo_Med_exCanvs  blue_atmosphere
            dotTexture = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Red);
            dotTexture2 = MgDrawExt.CreateDotTexture(GraphicsDevice, Color.Green);

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");
            font3 = Content.Load<SpriteFont>("MgFont3");

            cam.InitialView(GraphicsDevice);
            cam.UpdateProjection(GraphicsDevice);

            ImprovedIndexMeshEffectClass.Load(Content);
            ImprovedIndexMeshEffectClass.SpriteTexture = texture;
            ImprovedIndexMeshEffectClass.View = cam.view;
            ImprovedIndexMeshEffectClass.Projection = cam.projection;


            PrimitiveIndexedMesh.ShowOutput = false;
            //mesh = new PrimitiveIndexedMesh(4,4, new Vector3(300f, 250, 0f ), true);
            //mesh = new PrimitiveIndexedMesh(heightMap, 6, new Vector3( 300f, 250, 10f ), true);
            mesh = new PrimitiveIndexedMesh(texture, new Vector3(300f, 250, 5f), true);

            CreateVisualMeshNormals(mesh, dotTexture2, 0.50f, 2.0f);

        }

        public void CreateVisualMeshNormals(PrimitiveIndexedMesh mesh, Texture2D texture, float thickness, float scale)
        {
            VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; i++)
                tmp[i] = new VertexPositionNormalTexture() { Position = mesh.vertices[i].Position, Normal = mesh.vertices[i].Normal, TextureCoordinate = mesh.vertices[i].TextureCoordinate };
            visualNormals.CreateVisualNormalsForPrimitiveMesh(tmp, mesh.indices, texture, thickness, scale);
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

            ImprovedIndexMeshEffectClass.View = cam.view;
            ImprovedIndexMeshEffectClass.Projection = cam.projection;
            ImprovedIndexMeshEffectClass.World = Matrix.Identity;
            ImprovedIndexMeshEffectClass.SpriteTexture = texture;

            DrawMesh();

            if (displayWireframe)
                DrawWireFrameMesh();

            if (displayNormals)
                DrawNormalsForMesh();

            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        public void DrawMesh()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            ImprovedIndexMeshEffectClass.SpriteTexture = texture;
            mesh.DrawPrimitive(GraphicsDevice, ImprovedIndexMeshEffectClass.effect);
        }

        public void DrawWireFrameMesh()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_WIREFRAME;
            ImprovedIndexMeshEffectClass.SpriteTexture = dotTexture;
            mesh.DrawPrimitive(GraphicsDevice, ImprovedIndexMeshEffectClass.effect);
        }

        public void DrawNormalsForMesh()
        {
            GraphicsDevice.RasterizerState = rasterizerState_CULLNONE_SOLID;
            ImprovedIndexMeshEffectClass.SpriteTexture = visualNormals.texture;
            visualNormals.Draw(GraphicsDevice, ImprovedIndexMeshEffectClass.effect);
        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
            // Draw all the regular stuff
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            string msg =
                $" \n The camera exists as a world matrix that holds a position and orientation." +
                $" \n The keys WASD change the forward view direction (which is the major take away here). ZC allows for spin." +
                $" \n The Arrows move the camera translation as strafing motion. " +
                $" \n The F2 key will turn a wireframe on or off." +
                $" \n  " +
                $" \n This time around we move out our helper classes from game1 to keep things clearer." +
                $" \n While the shaders and things alone are fairly straight forward they tend to take up space." +
                $" \n We place the ImprovedIndexMeshEffectClass in ContentEffectClasses. " +
                $" \n We place are other requisite classes in the ExampleSupportClasses folder." +
                $" \n  " +
                $" \n In this example. We improve the previous mesh we create normals and allow the mesh to take a height map." +
                $" \n The map can be in the form of a array or of a texture we also create normals for the mesh." +
                $" \n This is created on the cpu at runtime." +
                $" \n In addition we create a new VertexDefinition that allows us to pass our own vertice structure to the shader." +
                $" \n In this way we prepare to add the ability to do more complex shader effects with the extra information." +
                $" \n " +
                $" \n { cam.cameraWorld.DisplayMatrix("cameraWorld") }" +
                $" \n { cam.view.DisplayMatrix("view") }" +
                $" \n { cam.projection.DisplayMatrix("projection") }" +
                $" \n" +
                $" \n" +
                $" \n"
                ;
            spriteBatch.DrawString(font, msg, new Vector2(10, 10), Color.Blue);
            spriteBatch.End();
        }

    }
}
