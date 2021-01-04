

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

            Content.RootDirectory = @"Content";
            meshEffect = Content.Load<Effect>("MeshDrawEffect");

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");
            texture2 = Content.Load<Texture2D>("TextureAlignmentTestImage2");

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");

            gridPlanes3d = new GridPlanes3D(100, 100, .0002f, Color.Red, Color.Blue, Color.Green);
            mesh.CreateMesh(new Rectangle(-100, -100, 200, 200), 8, 8, true, true, false);
            prism.CreatePrism(GraphicsDevice, 8, 10, 10, false);
            navGuide = new NavOrientation3d(20, 20, 4, .05f);

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

            DrawOurGeometryWithBasicEffect(gameTime);
            ////DrawOurGeometryWithEffect(gameTime);
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
                gridPlanes3d.DrawWithBasicEffect(GraphicsDevice, basicEffect, Matrix.Identity, 10000, MgDrawExtras.dot, true, true, false);
            }

            SetStates();

            // draw the primitive models.
            mesh.BasicEffectSettingsForThisPrimitive(GraphicsDevice, basicEffect, texture);

            basicEffect.World = Matrix.Identity;
            mesh.Draw(GraphicsDevice, basicEffect, texture2);

            prism.BasicEffectSettingsForThisPrimitive(GraphicsDevice, basicEffect, texture2);
            var m = Matrix.Identity * Matrix.CreateScale(10);
            m.Translation = new Vector3(0, -300, 0);
            basicEffect.World = m;
            prism.DrawWithBasicEffect(GraphicsDevice, basicEffect, texture2);

            var targetMatrix = Matrix.Identity; // the target is the world matrix of some other thing we have drawn.
            var poffset = cinematicCamera.World.Forward * 250 + cinematicCamera.World.Right * 250 + cinematicCamera.World.Down * 50 + cinematicCamera.World.Translation; // we offset from the camera forward right then down
            navGuide.DrawNavOrientation3DToTargetWithBasicEffect(GraphicsDevice, basicEffect, poffset, 100f, targetMatrix, MgDrawExtras.dotBlue, MgDrawExtras.dot, MgDrawExtras.dotRed);
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
                gridPlanes3d.Draw(GraphicsDevice, meshEffect, MgDrawExtras.dot, MgDrawExtras.dot, MgDrawExtras.dot);
            }

            SetStates();

            // draw the primitive models.
            meshEffect.Parameters["World"].SetValue(Matrix.Identity);
            mesh.Draw(GraphicsDevice, meshEffect);

            meshEffect.Parameters["World"].SetValue(Matrix.Identity * Matrix.CreateScale(10f));
            prism.DrawWithBasicEffect(GraphicsDevice, basicEffect, texture);
        }

        public void DrawRegularSpriteBatchStuff(GameTime gameTime)
        {
            //spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            //spriteBatch.Draw(texture, new Rectangle(0, 0, 300, 300), Color.White);
            //spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            cinematicCamera.DrawCurveThruWayPointsWithSpriteBatch(1, gameTime);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null); //basicEffect
            spriteBatch.DrawString(font, msg, new Vector2(10, 10), Color.Moccasin);
            //Test2(amount);
            spriteBatch.End();
        }

    }
}



/*
 
 
         bool justonce = true;
        public void Test2(float amountMultiplier)
        {
            var iterations = (int)(amountMultiplier * 50f);
            var start = new Vector2(350, 100);
            var radius = 650f;
            var textureU = 0f;
            var textureV = 0f;
            for (int i = 0; i < iterations; i++)
            {
                var radians = ((i / (float)iterations) * 6.28318530717f);
                float sin = (float)(Math.Sin(radians));
                float cos = (float)(Math.Cos(radians));
                var x = radius * sin;
                var z = radius * cos;

                var ss = Sign(sin);
                var sc = Sign(cos);

                var asin = sin * sin;
                var acos = cos * cos;

                var signedAsin = asin * ss;
                var signedAcos = acos * sc;

                bool higherIsX = true;
                var lowest = signedAsin;
                var highest = signedAsin;
                if (acos < asin) 
                { 
                    lowest = signedAcos; 
                    higherIsX = true; 
                }
                if (acos > asin) 
                { 
                    highest = signedAcos; 
                    higherIsX = false; 
                }

               // var squarePosition = GetOuterSquareVector(asin, acos, signedAsin, signedAcos);
                var squarePosition = GetOuterSquareVector(sin, cos);
               // var squarePosition = GetOuterSquareVectorSinCos(sin, cos);

                textureU = signedAsin *.5f + .5f;
                textureV = signedAcos * .5f + .5f;

                var half = Vector2.One * radius / 2;

                var top = new Vector2(-radius / 2, 0) + start + half;
                var right = new Vector2(0, radius / 2) + start + half;
                var bottom = new Vector2(radius / 2, 0) + start + half;
                var left = new Vector2(0, -radius / 2) + start + half;
                spriteBatch.DrawBasicLine(top, right, 1, Color.Black);
                spriteBatch.DrawBasicLine( right, bottom, 1, Color.Black);
                spriteBatch.DrawBasicLine(bottom, left, 1, Color.Black);
                spriteBatch.DrawBasicLine(left, top, 1, Color.Black);
                spriteBatch.DrawRectangleOutline(new Rectangle( (start).ToPoint() , new Point((int)radius, (int)radius) ), 1, Color.Blue);

                var squaruvend = (squarePosition * .5f + new Vector2(.5f, .5f)) * radius + start;
                spriteBatch.DrawLineWithStringAtEnd(font, $" [{i}] {squarePosition.X.ToString("0.00")} {squarePosition.Y.ToString("0.00")}  ", start + half, squaruvend, 1, Color.Blue);
                
                var end = (new Vector2(sin, cos) * .5f + new Vector2(.5f , .5f) )* radius + start;
                spriteBatch.DrawLineWithStringAtEnd(font, $" [{i}] {sin.ToString("0.00")} {cos.ToString("0.00")}", start + half, end, 1, Color.Red);

                var uvend = new Vector2(textureU, textureV) * radius + start;
                spriteBatch.DrawLineWithStringAtEnd(font, $" [{i}] {signedAsin.ToString("0.00")} {signedAcos.ToString("0.00")} ", start + half, uvend, 1, Color.Black);


                if (justonce)
                {
                    Console.WriteLine($" calculation .. sin {sin.ToString("0.000")} signedAsin {signedAsin.ToString("0.000")} asin {asin.ToString("0.000")}    low {lowest.ToString("0.000")} high {highest.ToString("0.000")}  ");
                    Console.WriteLine($" calculation .. cos {cos.ToString("0.000")} signedAcos {signedAcos.ToString("0.000")} acos {acos.ToString("0.000")}  ");
                    Console.WriteLine($" calculation .. uv result -----  squarePosition.X {squarePosition.X.ToString("0.000")}, squarePosition.Y {squarePosition.Y.ToString("0.000")}  \n" );
                }
            }
            justonce = false;
        }

        public Vector2 GetOuterSquareVectorSinCos(float sin, float cos)
        {
            var ss = (sin < 0) ? -1f : 1f;
            var sc = (cos < 0) ? -1f : 1f;
            var asin = sin * sin;
            var acos = cos * cos;
            if (asin > acos) //  x is higher
                return new Vector2(ss, cos * sc * 2f); // re-signed acosine
            else // x is lower
                return new Vector2(sin * ss * 2f, sc); // re-signed asin
        }

        public Vector2 GetOuterSquareVector(float sin, float cos)
        {
            var ss = (sin < 0) ? -1f : 1f;
            var sc = (cos < 0) ? -1f : 1f;
            var asin = sin * sin;
            var acos = cos * cos;
            if (asin > acos) //  x is higher
                return new Vector2(ss , acos * sc * 2f); // re-signed acosine
            else // x is lower
                return new Vector2(asin * ss * 2f , sc); // re-signed asin
        }

        public float Sign(float n)
        {
            return n < 0 ? -1f : 1f;
        }

        public Vector2 GetOuterSquareVector(float asin, float acos, float signedAsin, float signedAcos)
        {
            bool pic = IsXhigher(asin, acos, signedAsin, signedAcos);
            if (pic) // y is lower
            {
                return new Vector2( Sign(signedAsin), signedAcos * 2f);
            }
            else // x is lower
            {
                return new Vector2(signedAsin * 2f, Sign(signedAcos));
            }
        }

        public bool IsXhigher(float asin, float acos, float signedAsin, float signedAcos)
        {
            bool higherIsX = true;
            //var lowest = signedAsin;
            //var highest = signedAsin;
            if (acos < asin)
            {
                //lowest = signedAcos;
                return true;
            }
            else//if (acos > asin)
            {
                //highest = signedAcos;
                return false;
            }
            //return higherIsX;
        }


        //if (sin < .01f)
        //    textureU = 0;
        //else
        //    textureU = ((sin / (sin * sin * ss))  * sin) * .5f + .5f;
        //if (cos < .01f)
        //    textureV = 0;
        //else
        //    textureV = ((cos / (cos * cos * sc))  * cos) * .5f + .5f;


        public string OutThis( string msg, float a, string sa, float b, string sb, float c)
        {
            return $"{msg} { a.ToString("0.000") } { sa  } {b.ToString("0.000")  } { sb  } { c.ToString("0.000")  } ";
        }

 
 
 
 */
