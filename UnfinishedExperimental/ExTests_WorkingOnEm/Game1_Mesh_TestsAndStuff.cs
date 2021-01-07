

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;


//   Unfinished prototype not yet vetted / working.

namespace ShaderExamples
{
    public class Game1_Mesh_TestsAndStuff : Game
    {
        const int STARTprefScrWidth = 1000;
        const int STARTprefScrHeight = 750;
        string msg = "";
        string msgMisc = "";
        string displayModesMsg = "";

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D texture;
        Texture2D texture2;
        Texture2D texture3;
        Texture2D texture4;
        BasicEffect basicEffect;
        Effect meshEffect;

        MeshNonIndexed mesh = new MeshNonIndexed();
        Prism prism = new Prism();
        GridPlanes3D gridPlanes3d;
        NavOrientation3d navGuide;

        Matrix proj;
        Matrix view;
        Matrix world;

        private CinematicCamera cinematicCamera;
        private SimpleFps fps = new SimpleFps();

        private bool useDesignatedTarget = true;
        private Vector3 _targetLookAt = new Vector3(0, 0, 0);
        private static float weight = 1f;
        private static float camHeight = -30f;
        private static float Range = CinematicCamera.GetRequisitePerspectiveSpriteBatchAlignmentZdistance(STARTprefScrWidth, STARTprefScrHeight, 1f);
        private Vector4[] cameraWayPoints = new Vector4[]
        {
            new Vector4(-Range, camHeight, 0, weight), new Vector4(0, camHeight, -Range, weight), new Vector4(Range, camHeight, 0, weight), new Vector4(0, camHeight, Range , weight)
        };

        bool showVisualGrids = true;

        bool useNoCull = true;
        bool useClockwiseWinding = false;
        bool useWireFrame = false;

        RasterizerState rs_nocull_wire = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        RasterizerState rs_nocull_solid = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.Solid };
        RasterizerState rs_clockwise = new RasterizerState() { CullMode = CullMode.CullClockwiseFace };
        RasterizerState rs_counter_clockwise = new RasterizerState() { CullMode = CullMode.CullCounterClockwiseFace };


        float amount = .2f;

        public Game1_Mesh_TestsAndStuff()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;

            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / 100d);

            Content.RootDirectory = "Content";
            Window.Title = " ex Game1_Mesht.";
            Window.AllowAltF4 = true;
            Window.AllowUserResizing = true;



            //Register recievers with the notifying sender.
            Window.ClientSizeChanged += CalledOnClientSizeChanged;

            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = STARTprefScrWidth;
            graphics.PreferredBackBufferHeight = STARTprefScrHeight;
            graphics.ApplyChanges();

        }


        public void CalledOnClientSizeChanged(object sender, EventArgs e)
        {
            //Re Setup Cameras;
            if (cinematicCamera != null)
                cinematicCamera.VisualizationOffset = new Vector3(GraphicsDevice.Viewport.Bounds.Right - 100, 1, GraphicsDevice.Viewport.Bounds.Bottom - 100);
        }

        protected override void Initialize()
        {
            displayModesMsg = GraphicsDevice.GetListingOfSupportedDisplayModesToString();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            MgDrawExtras.Initialize(GraphicsDevice, spriteBatch);

            Content.RootDirectory = @"Content/Shaders";
            meshEffect = Content.Load<Effect>("MeshDrawEffect");

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");
            texture2 = Content.Load<Texture2D>("TextureAlignmentTestImage2");
            texture3 = Content.Load<Texture2D>("Terran02");   //  "Terran02" , "GeneratedSphere" "GeneratedSphereBlue" "TextureAlignmentTestImage2" "MG_Logo_Med_exCanvs" "cutePuppy"
            texture4 = Content.Load<Texture2D>("Nasa_DEM_Mars");  // Nasa_DEM_Mars  Nasa_DEM_Earth low res

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");

            //mesh.CreateMesh(new Rectangle(-100, -100, 200, 200), 8, 8, true, true, false);
            mesh.CreateMesh(GraphicsDevice, texture4, new Rectangle(-300, -300, 600, 600), 20f, false, true, false, true, false);
            gridPlanes3d = new GridPlanes3D(100, 100, .0002f, Color.Red, Color.Blue, Color.Green);
            prism.CreatePrism(GraphicsDevice, 8, 14, 10, false);
            navGuide = new NavOrientation3d(20, 20, 20, .06f, Color.White, Color.White, Color.White);

            SetupMyCamera();
            SetUpOrthographicBasicEffect(GraphicsDevice);
            SetUpDirectWorldViewPerspectiveProjectionMatrices();
        }

        public void SetupMyCamera()
        {
            // a 90 degree field of view is needed for the projection matrix.
            var f90 = 90.0f * (3.14159265358f / 180f);
            cinematicCamera = new CinematicCamera(GraphicsDevice, spriteBatch, null, new Vector3(0, 0, 0), Vector3.Forward, Vector3.Up, 0.01f, 10000f, f90, true, true, false);
            cinematicCamera.SetWayPoints(cameraWayPoints, 30, true, true);
            cinematicCamera.WayPointCycleDurationInTotalSeconds = 30f;
            cinematicCamera.MovementSpeedPerSecond = 30f;
            cinematicCamera.LookAtSpeedPerSecond = 4;
            cinematicCamera.UseForwardPathLook = false;
            cinematicCamera.UseWayPointMotion = true;
            cinematicCamera.VisualizationScale = .10f;
            cinematicCamera.VisualizationOffset = new Vector3(GraphicsDevice.Viewport.Bounds.Right - 100, 1, GraphicsDevice.Viewport.Bounds.Bottom - 100);

            //Register recievers with the notifying sender.  In this case well register our camera to recieve callbacks as well.
            Window.ClientSizeChanged += cinematicCamera.CalledOnClientSizeChanged;
        }

        public void SetUpOrthographicBasicEffect(GraphicsDevice device)
        {
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = texture;
            // use ours camera matrices instead.
            basicEffect.World = Matrix.Identity;
            basicEffect.View = cinematicCamera.View;
            basicEffect.Projection = cinematicCamera.Projection;
        }

        public void SetUpDirectWorldViewPerspectiveProjectionMatrices()
        {
            proj = Matrix.CreatePerspectiveFieldOfView(1f, GraphicsDevice.Viewport.AspectRatio, .01f, 1000f); // once orthographic or perspective.
            view = Matrix.CreateLookAt(new Vector3(0, 0, +1), new Vector3(0, 0, .01f), new Vector3(0, 0, -1f)); // cameras place and lookat direction in world
            world = Matrix.CreateWorld(new Vector3(0, 0, 0), Vector3.Forward, new Vector3(0, 0, -1f)); // per object orientation place lookat and scale.
        }

        protected override void UnloadContent() { }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keys.Up.IsKeyDown())
                amount += .01f;
            if (Keys.Down.IsKeyDown())
                amount -= .01f;

            // camera settings.
            if (Keys.F1.IsKeyPressedWithDelay(gameTime))
                cinematicCamera.UseForwardPathLook = !cinematicCamera.UseForwardPathLook;
            if (Keys.F2.IsKeyPressedWithDelay(gameTime))
                cinematicCamera.UseWayPointMotion = !cinematicCamera.UseWayPointMotion;
            if (Keys.F3.IsKeyPressedWithDelay(gameTime))
                useDesignatedTarget = !useDesignatedTarget;

            // graphic states.
            if (Keys.F5.IsKeyPressedWithDelay(gameTime))
                useWireFrame = !useWireFrame;
            if (Keys.F6.IsKeyPressedWithDelay(gameTime))
                useNoCull = !useNoCull;
            if (Keys.F7.IsKeyPressedWithDelay(gameTime))
                useClockwiseWinding = !useClockwiseWinding;

            if (Keys.F8.IsKeyPressedWithDelay(gameTime))
                showVisualGrids = !showVisualGrids;

            if (useDesignatedTarget)
                cinematicCamera.Update(_targetLookAt, gameTime);
            else
                cinematicCamera.Update(gameTime);

            fps.Update(gameTime);

            ComposeMsgs();

            base.Update(gameTime);
        }

        public void ComposeMsgs()
        {
            string msg2 = "";
            if (useDesignatedTarget)
                msg2 += $"\n Camera TargetPosition: {cinematicCamera.TargetPosition}";

            string msg3 = "";
            if (useDesignatedTarget == false && cinematicCamera.UseForwardPathLook == false)
                msg3 += $"\n Use Keyboard keys w a s d to look z and q to spin. ";
            if (cinematicCamera.UseWayPointMotion == false)
                msg3 += $"\n Use arrow keys and or q and e to move.";

            string msg4 = $"F5 useWireFrame: {useWireFrame} ";
            if (useWireFrame == true)
            {
                msg4 += $"\n No Cull is on for wire, and neither winding is backface culled.";
            }
            else
            {
                if (useNoCull == false)
                {
                    if (useClockwiseWinding == true)
                        msg4 += $"\n F6 useNoCull {useNoCull} \n F7 winding: Clockwise";
                    if (useClockwiseWinding == false)
                        msg4 += $"\n F6 useNoCull {useNoCull} \n F7 winding: CounterClockwise";
                }
                else
                {
                    msg4 += $"\n F6 useNoCull {useNoCull}";
                }
            }

            float g = 0f;

            msg =
           $"\n Camera.World.Translation: \n  { cinematicCamera.World.Translation.X.ToString("N3") } { cinematicCamera.World.Translation.Y.ToString("N3") } { cinematicCamera.World.Translation.Z.ToString("N3") }" +
           $"\n Camera.Forward: \n  { cinematicCamera.Forward.X.ToString("N3") } { cinematicCamera.Forward.Y.ToString("N3") } { cinematicCamera.Forward.Z.ToString("N3") }" +
           $"\n Up: \n { cinematicCamera.Up.X.ToString("N3") } { cinematicCamera.Up.Y.ToString("N3") } { cinematicCamera.Up.Z.ToString("N3") } " +
           $"\n Camera IsSpriteBatchStyled: {cinematicCamera.IsSpriteBatchStyled}" +
           $"\n Camera F1 UseForwardPathLook: {cinematicCamera.UseForwardPathLook}" +
           $"\n Camera F2 UseWayPointMotion: {cinematicCamera.UseWayPointMotion}" +
           $"\n Camera F3 UseDesignatedTarget: {useDesignatedTarget + msg2}" +
           $"\n Ui Control : {msg3}" +
           $"\n RasterizerState controls: F5 F6 F7" +
           $"\n " + msg4 +
           $"\n F8 showVisualGrids: {showVisualGrids}" +
           $"\n " + msgMisc
           ;

            if (Keys.OemQuestion.IsKeyDown())
                msg = displayModesMsg;
        }

        public void SetStates()
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // wireframe needs to overide all of these.
            if (useWireFrame)
            {
                GraphicsDevice.RasterizerState = rs_nocull_wire;
            }
            else
            {
                if (useNoCull)
                {
                    GraphicsDevice.RasterizerState = rs_nocull_solid;
                }
                else
                {
                    if (useClockwiseWinding)
                        GraphicsDevice.RasterizerState = rs_clockwise;
                    else
                        GraphicsDevice.RasterizerState = rs_counter_clockwise;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SlateGray);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //DrawOurGeometryWithBasicEffect(gameTime);
            DrawOurGeometryWithEffect(gameTime);
            DrawRegularSpriteBatchStuff(gameTime);
            base.Draw(gameTime);
        }

        public void DrawOurGeometryWithEffect(GameTime gameTime)
        {
            meshEffect.Parameters["SpriteTexture"].SetValue(texture);
            meshEffect.Parameters["View"].SetValue(cinematicCamera.View);
            meshEffect.Parameters["Projection"].SetValue(cinematicCamera.Projection);

            //draw the primitive grid first it has different requisites including state.
            if (showVisualGrids)
            {
                GraphicsDevice.RasterizerState = rs_nocull_solid;
                meshEffect.CurrentTechnique = meshEffect.Techniques["TriangleDrawPCT"];
                meshEffect.Parameters["SpriteTexture"].SetValue(MgDrawExtras.dot);
                meshEffect.Parameters["World"].SetValue(Matrix.CreateScale(10000));
                gridPlanes3d.Draw(GraphicsDevice, meshEffect, true, true, false);
            }

            SetStates();

            // holy shit thats fat.
            // draw the primitive models.  NonIndexedMeshDraw
            meshEffect.CurrentTechnique = meshEffect.Techniques["NonIndexedMeshDraw"];
            meshEffect.Parameters["World"].SetValue(Matrix.Identity);
            meshEffect.Parameters["SpriteTexture"].SetValue(texture4);
            mesh.Draw(GraphicsDevice, meshEffect);

            var m = Matrix.Identity * Matrix.CreateScale(10);
            m.Translation = new Vector3(0, -500, 0);
            meshEffect.CurrentTechnique = meshEffect.Techniques["TriangleDrawPNT"];
            meshEffect.Parameters["World"].SetValue(m);
            meshEffect.Parameters["SpriteTexture"].SetValue(texture);
            prism.Draw(GraphicsDevice, meshEffect);


            var targetMatrix = Matrix.Identity; // the target is the world matrix of some other thing we have drawn.
            var poffset = cinematicCamera.World.Forward * 250 + cinematicCamera.World.Right * 250 + cinematicCamera.World.Down * 50 + cinematicCamera.World.Translation; // we offset from the camera forward right then down
            meshEffect.CurrentTechnique = meshEffect.Techniques["TriangleDrawPCT"];
            meshEffect.Parameters["World"].SetValue(m);
            meshEffect.Parameters["SpriteTexture"].SetValue(texture);
            navGuide.DrawNavOrientation(GraphicsDevice, meshEffect, poffset, 100f, targetMatrix, MgDrawExtras.dotBlue, MgDrawExtras.dot, MgDrawExtras.dotRed);
            //navGuide.Draw(GraphicsDevice, meshEffect);
        }

        public void DrawOurGeometryWithBasicEffect(GameTime gameTime)
        {
            basicEffect.Projection = cinematicCamera.Projection;
            basicEffect.View = cinematicCamera.View;
            basicEffect.VertexColorEnabled = false;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = texture;

            // draw the primitive grid first it has different requisites including state.
            if (showVisualGrids)
            {
                GraphicsDevice.RasterizerState = rs_nocull_solid;
                basicEffect.World = Matrix.Identity;
                gridPlanes3d.DrawWithBasicEffect(GraphicsDevice, basicEffect, Matrix.Identity, 10000, MgDrawExtras.dot, true, true, false);
            }

            SetStates();

            // draw the primitive models.
            mesh.BasicEffectSettingsForThisPrimitive(GraphicsDevice, basicEffect, texture4);
            basicEffect.World = Matrix.Identity;
            mesh.DrawWithBasicEffect(GraphicsDevice, basicEffect, texture4);

            prism.BasicEffectSettingsForThisPrimitive(GraphicsDevice, basicEffect, texture);
            var m = Matrix.Identity * Matrix.CreateScale(10);
            m.Translation = new Vector3(0, -500, 0);
            basicEffect.World = m;
            prism.DrawWithBasicEffect(GraphicsDevice, basicEffect, texture);

            var targetMatrix = Matrix.Identity; // the target is the world matrix of some other thing we have drawn.
            var poffset = cinematicCamera.World.Forward * 250 + cinematicCamera.World.Right * 250 + cinematicCamera.World.Down * 50 + cinematicCamera.World.Translation; // we offset from the camera forward right then down
            navGuide.DrawNavOrientation3DToTargetWithBasicEffect(GraphicsDevice, basicEffect, poffset, 100f, targetMatrix, MgDrawExtras.dotBlue, MgDrawExtras.dot, MgDrawExtras.dotRed);
        }

        public void DrawRegularSpriteBatchStuff(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);

            cinematicCamera.DrawCurveThruWayPointsWithSpriteBatch(1, gameTime);
            fps.DrawFps(spriteBatch, font, new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(font, msg, new Vector2(10, 210), Color.Moccasin);

            spriteBatch.End();
        }

    }
}



