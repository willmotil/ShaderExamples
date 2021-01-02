

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
        string displayModesMsg = "";

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D texture;
        Texture2D texture2;
        BasicEffect basicEffect;
        Effect meshEffect;

        MeshSimple mesh = new MeshSimple();
        Prism prism = new Prism();
        GridPlanes3D gridPlanes3d;

        Matrix proj;
        Matrix view;
        Matrix world;

        private CinematicCamera cinematicCamera;

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
        RasterizerState rs_clockwise = new RasterizerState() { CullMode = CullMode.CullClockwiseFace};
        RasterizerState rs_counter_clockwise = new RasterizerState() { CullMode = CullMode.CullCounterClockwiseFace };




        public Game1_Mesh_TestsAndStuff()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
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
            if(cinematicCamera != null)
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

            Content.RootDirectory = @"Content";
            meshEffect = Content.Load<Effect>("MeshDrawEffect");

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");
            texture2 = Content.Load<Texture2D>("wallToMap");

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");

            gridPlanes3d = new GridPlanes3D(100, 100, .0002f, Color.Red, Color.Blue, Color.Green);
            mesh.GetMesh(new Rectangle(-100, -100, 200, 200), 8, 8, true, true, false);
            prism = Prism.Load(GraphicsDevice, 5, 8, 20, texture);

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

        protected override void UnloadContent(){  }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

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
           $"\n F8 showVisualGrids: {showVisualGrids}"
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

            DrawOurGeometryWithBasicEffect(gameTime);
            //DrawOurGeometryWithEffect(gameTime);
            DrawRegularSpriteBatchStuff(gameTime);
            base.Draw(gameTime);
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
                gridPlanes3d.DrawWithBasicEffect(GraphicsDevice, basicEffect, Matrix.Identity, 10000, DrawHelpers.Dot, true, true, false);
            }

            SetStates();

            // draw the primitive models.
            basicEffect.World = Matrix.Identity;
            mesh.Draw(GraphicsDevice, basicEffect, texture);

            var m = Matrix.Identity * Matrix.CreateScale(10); 
            m.Translation = new Vector3(0, -300, 0);
            basicEffect.World = m;
            prism.DrawWithBasicEffect(GraphicsDevice, basicEffect, texture2);
        }


        public void DrawOurGeometryWithEffect(GameTime gameTime)
        {
            ///  K Ok wtf did i do and were....

            meshEffect.CurrentTechnique = meshEffect.Techniques["TriangleDraw"];
            meshEffect.Parameters["SpriteTexture"].SetValue(texture);
            meshEffect.Parameters["View"].SetValue(cinematicCamera.View);
            meshEffect.Parameters["Projection"].SetValue(cinematicCamera.Projection);

            // draw the primitive grid first it has different requisites including state.
            if (showVisualGrids)
            {
                GraphicsDevice.RasterizerState = rs_nocull_solid;
                meshEffect.Parameters["World"].SetValue(Matrix.CreateScale(1000));
                gridPlanes3d.Draw(GraphicsDevice, meshEffect, DrawHelpers.Dot, DrawHelpers.Dot, DrawHelpers.Dot);
            }

            SetStates();

            // draw the primitive models.
            meshEffect.Parameters["World"].SetValue(Matrix.Identity);
            mesh.Draw(GraphicsDevice, meshEffect);

            meshEffect.Parameters["World"].SetValue(Matrix.Identity * Matrix.CreateScale(10f) );
            prism.DrawWithBasicEffect(GraphicsDevice, basicEffect, texture);
        }

        public void DrawRegularSpriteBatchStuff(GameTime gameTime)
        {
            //spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            //spriteBatch.Draw(texture, new Rectangle(0, 0, 300, 300), Color.White);
            //spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            cinematicCamera.DrawCurveThruWayPointsWithSpriteBatch( 1, gameTime);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null); //basicEffect
            spriteBatch.DrawString(font, msg, new Vector2(10, 10), Color.Moccasin);
            spriteBatch.End();
        }
    }

    public class MeshSimple
    {
        VertexPositionNormalTexture[] vertices;
        int w;
        int h;
        public bool invertU = false;
        public bool invertV = false;
        public VertexPositionNormalTexture[] GetMesh(Rectangle modelRectangle, int verticesWidth, int verticesHeight, bool flipNormalDirection, bool reverseU, bool reverseV)
        {
            invertU = reverseU;
            invertV = reverseV;
            w = verticesWidth;
            h = verticesHeight;
            List<VertexPositionNormalTexture> vertlist = new List<VertexPositionNormalTexture>();
            
            var tl = new Vector2(0, 0);
            var tr = new Vector2(1, 0);
            var bl = new Vector2(0, 1);
            var br = new Vector2(1, 1);

            // initial calculation
            for (int y = 0; y < w - 1; y++)
            {
                for (int x = 0; x < h - 1; x++)
                {
                    var uvXy = new Vector2(x, y);
                    // t0
                    vertlist.Add(new VertexPositionNormalTexture(new Vector3(x, y, 0) + tl.ToVector3(), new Vector3(0, 0, 1), uvFromXy(uvXy + tl, invertU, invertV)));
                    vertlist.Add(new VertexPositionNormalTexture(new Vector3(x, y, 0) + bl.ToVector3(), new Vector3(0, 0, 1), uvFromXy(uvXy + bl, invertU, invertV)));
                    vertlist.Add(new VertexPositionNormalTexture(new Vector3(x, y, 0) + tr.ToVector3(), new Vector3(0, 0, 1), uvFromXy(uvXy + tr, invertU, invertV)));
                    // t1
                    vertlist.Add(new VertexPositionNormalTexture(new Vector3(x, y, 0) + tr.ToVector3(), new Vector3(0, 0, 1), uvFromXy(uvXy + tr, invertU, invertV)));
                    vertlist.Add(new VertexPositionNormalTexture(new Vector3(x, y, 0) + bl.ToVector3(), new Vector3(0, 0, 1), uvFromXy(uvXy + bl, invertU, invertV)));
                    vertlist.Add(new VertexPositionNormalTexture(new Vector3(x, y, 0) + br.ToVector3(), new Vector3(0, 0, 1), uvFromXy(uvXy + br, invertU, invertV)));
                }
            }
            RePositionVerticesInModelSpace(ref vertlist, modelRectangle);
            DetermineQuadNormals(ref vertlist, flipNormalDirection);
            vertices = vertlist.ToArray();
            return vertices;
        }

        private void RePositionVerticesInModelSpace(ref List<VertexPositionNormalTexture> vertlist, Rectangle modelSpaceRectangle)
        {
            // resize to the world rectangle this is still in object space. 
            // so the rectangle might want to be like -100,100  and have a size of 200,200 to center it in local object space.
            var loc = modelSpaceRectangle.Location.ToVector2();
            var size = modelSpaceRectangle.Size.ToVector2();
            for (int i = 0; i < vertlist.Count; i += 1)
            {
                var v = vertlist[i];
                var ratio = (v.Position / new Vector3(w-1, h-1, 1));
                v.Position = ratio * size.ToVector3(1f) + loc.ToVector3(0);
                vertlist[i] = v;
            }
        }

        // flat normals.
        public void DetermineQuadNormals(ref List<VertexPositionNormalTexture> vertlist, bool flipDirection)
        {
            // generate the normals
            for (int i = 0; i < vertlist.Count; i += 6)
            {
                var tn0 = Vector3.Normalize( CrossProduct3d(Vector3.Normalize(vertlist[i + 0].Position), Vector3.Normalize(vertlist[i + 1].Position), Vector3.Normalize(vertlist[i + 2].Position)) );
                var tn1 = Vector3.Normalize(CrossProduct3d(Vector3.Normalize(vertlist[i + 3].Position), Vector3.Normalize(vertlist[i + 4].Position), Vector3.Normalize(vertlist[i + 5].Position)) );
                if (flipDirection)
                {
                    tn0 = -tn0;
                    tn0 = -tn0;
                }
                // t0
                vertlist[i + 0] = new VertexPositionNormalTexture(vertlist[i + 0].Position, tn0, vertlist[i + 0].TextureCoordinate);
                vertlist[i + 1] = new VertexPositionNormalTexture(vertlist[i + 1].Position, tn0, vertlist[i + 1].TextureCoordinate);
                vertlist[i + 2] = new VertexPositionNormalTexture(vertlist[i + 2].Position, tn0, vertlist[i + 2].TextureCoordinate);
                // t1
                vertlist[i + 3] = new VertexPositionNormalTexture(vertlist[i + 3].Position, tn1, vertlist[i + 3].TextureCoordinate);
                vertlist[i + 4] = new VertexPositionNormalTexture(vertlist[i + 4].Position, tn1, vertlist[i + 4].TextureCoordinate);
                vertlist[i + 5] = new VertexPositionNormalTexture(vertlist[i + 5].Position, tn1, vertlist[i + 5].TextureCoordinate);
            }
        }

        public static Vector3 CrossProduct3d(Vector3 a, Vector3 b, Vector3 c)
        {
            return new Vector3
                (
                ((b.Y - a.Y) * (c.Z - b.Z)) - ((c.Y - b.Y) * (b.Z - a.Z)),
                ((b.Z - a.Z) * (c.X - b.X)) - ((c.Z - b.Z) * (b.X - a.X)),
                ((b.X - a.X) * (c.Y - b.Y)) - ((c.X - b.X) * (b.Y - a.Y))
                );
        }

        public void Draw(GraphicsDevice gd, BasicEffect effect, Texture2D texture)
        {
            effect.VertexColorEnabled = false;
            effect.TextureEnabled = true;
            effect.Texture = texture;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3, VertexPositionNormalTexture.VertexDeclaration);
            }
        }

        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length /3, VertexPositionNormalTexture.VertexDeclaration);
            }
        }


        int GetIndex(int x, int y)
        {
            return x + y * w;
        }

        Vector2 uvFromXy(Vector2 v, bool reverseU, bool reverseV)
        {
            var uv = new Vector2((float)v.X / (float)(w -1), (float)v.Y / (float)(h -1));
            if (reverseU)
                uv.X = 1f- uv.X;
            if (reverseV)
                uv.Y = 1f - uv.Y;
            return uv;
        }
        Vector2 uvFromXy(int x, int y, bool reverseU, bool reverseV)
        {
            var uv = new Vector2((float)x / (float)(w -1), (float)y / (float)(h - 1));
            if (reverseU)
                uv.X = 1f - uv.X;
            if (reverseV)
                uv.Y = 1f - uv.Y;
            return uv;
        }
    }

    public static class Ext
    {
        public static Vector3 ToVector3(this Vector2 v, float z)
        {
            return new Vector3(v.X, v.Y, z);
        }
        public static Vector3 ToVector3(this Vector2 v)
        {
            return new Vector3(v.X, v.Y, 0);
        }
    }
}
