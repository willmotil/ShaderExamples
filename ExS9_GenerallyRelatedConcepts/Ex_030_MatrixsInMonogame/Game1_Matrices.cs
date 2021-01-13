
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderExamples
{
    public class Game1_Matrices : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        SpriteFont font3;
        Texture2D texture;
        Texture2D texture2;
        Texture2D texture3;
        Effect effect;
        MouseState mouse;

        Vector3 modelSpaceOriginOffset = new Vector3(0, 0, 0);

        static float vertDist = 40;
        Vector3[] verticesModelSpace = new Vector3[] 
        { 
            new Vector3(-vertDist, -vertDist, 0), // top left
            new Vector3(vertDist, -vertDist, 0), // top right
            new Vector3(-vertDist, vertDist, 0), // bottom left
            new Vector3(vertDist, vertDist, 0) // bottom right
        };

        Matrix view, projection = Matrix.Identity;
        Matrix world = new Matrix
            (
                  1, 0, 0, 0,
                  0, 1, 0, 0,
                  0, 0, 1, 0,
                  0, 0, 0, 1
            );
        Matrix cameraWorld = new Matrix
            (
                  1, 0, 0, 0,
                  0, 1, 0, 0,
                  0, 0, 1, 0,
                  0, 0, 0, 1
            );

        //List<string> timedMsg = new List<string>();
        float elapsed = 0;
        float elapsedTotalMsgTime = 0;
        float msgsTotalDuration = 600;
        int index = 0;
        bool respectMsgTimer = true;
        float percent = 1.0f;

        float elapsedCycleTime = 0;
        Vector2 directionToCompare = new Vector2(0, -100);
        Vector2 originPoint = new Vector2(300, 300);

        bool autoRun = true;

        bool showIdentitySpace = true;
        bool showWorldSpace = false;
        bool showViewSpace = false;
        bool showCameraWorldTranslation = false;
        bool showInverseCameraTranslation = false;
        bool showMatrixOrientationLines = false;
        bool showTriangles = false;

        public Game1_Matrices()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 800;
        }
        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            MgDrawExtras.Initialize(GraphicsDevice, spriteBatch);

            Content.RootDirectory = @"Content/Shaders";
            //effect = Content.Load<Effect>("GammaTestEffect");
            //effect.CurrentTechnique = effect.Techniques["Gamma"];

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont2");
            font3 = Content.Load<SpriteFont>("MgFont3");

            CreateMsgs();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            mouse = Mouse.GetState();

            elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            elapsedTotalMsgTime += elapsed;
            if (elapsedTotalMsgTime > msgsTotalDuration)
                elapsedTotalMsgTime = 0;

            float secondsPerCycle = 20f;
            elapsedCycleTime += elapsed * (1f / secondsPerCycle);
            if (elapsedCycleTime > 1f)
                elapsedCycleTime = 0;

            // Z axis standard 2d rotation around the objects relative forward axis.
            if (Keys.Z.IsKeyDown())
            {
                world *= Matrix.CreateFromAxisAngle(world.Forward, .01f);
            }
            if (Keys.C.IsKeyDown())
            {
                world *= Matrix.CreateFromAxisAngle(world.Forward, -.01f);
            }
            // rotation left or right around the objects relative y axis.
            if (Keys.A.IsKeyDown())
            {
                world *= Matrix.CreateFromAxisAngle(world.Up, .01f);
            }
            if (Keys.D.IsKeyDown())
            {
                world *= Matrix.CreateFromAxisAngle(world.Up, -.01f);
            }
            // rotation up or down upon the objects relative x axis.
            if (Keys.W.IsKeyDown())
            {
                world *= Matrix.CreateFromAxisAngle(world.Right, .01f);
            }
            if (Keys.S.IsKeyDown())
            {
                world *= Matrix.CreateFromAxisAngle(world.Right, -.01f);
            }
            // re- normalize the matrix.
            world = Matrix.CreateWorld(world.Translation, world.Forward, world.Up);


            if (Keys.Space.IsKeyPressedWithDelay(gameTime))
            {
                if (index > TimedMsgAndCommand.cmds.Count - 2)
                    index = 0;
                index++;
                respectMsgTimer = false;
            }
            if (Keys.Back.IsKeyPressedWithDelay(gameTime))
            {
                if (index < 1)
                    index = TimedMsgAndCommand.cmds.Count - 1;
                index--;
                respectMsgTimer = false;
            }

            if (respectMsgTimer)
            {
                index = (int)((TimedMsgAndCommand.cmds.Count - 1) * (elapsedTotalMsgTime / msgsTotalDuration));
                var tmc = TimedMsgAndCommand.cmds[index];
                ExecuteCommand(tmc);
            }
            else
            {
                var tmc = TimedMsgAndCommand.cmds[index];
                ExecuteCommand(tmc);
            }

            if (Keys.Enter.IsKeyPressedWithDelay(gameTime))
                autoRun = !autoRun;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Moccasin);

            DrawMatrixIdentityAndTheMeaningOfRowsAndColumns(gameTime);

            DrawTransformationSpacesOrientations(world, cameraWorld, projection, verticesModelSpace, modelSpaceOriginOffset.ToVector2());

            DrawTheMsgs(gameTime);

            base.Draw(gameTime);
        }

        public void DrawTheMsgs(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, null);
            index = (int)((TimedMsgAndCommand.cmds.Count - 1) * (elapsedTotalMsgTime / msgsTotalDuration));
            var tmc = TimedMsgAndCommand.cmds[index];

            if (respectMsgTimer)
            {
                spriteBatch.DrawString(font2, tmc.msg, new Vector2(10, 550), Color.Black, 0, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.DrawString(font2, tmc.msg, new Vector2(10, 550), Color.Black, 0, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 0);
            }
            spriteBatch.DrawString(font, $"index [ index { index } =  msgCount { TimedMsgAndCommand.cmds.Count - 1 }  *  multiplier {(elapsedTotalMsgTime / msgsTotalDuration).ToString("##0.000") } ]", new Vector2(950, 780), Color.Black, 0, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 0);
            spriteBatch.End();
        }

        public void DrawMatrixIdentityAndTheMeaningOfRowsAndColumns(GameTime gameTime)
        {
            int yoffset = 150;
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);

            spriteBatch.DrawString(font2, Matrix.Identity.DisplayMatrix(" model / local space,\n Identity matrix"), new Vector2(950, yoffset * 0), Color.Black, 0, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 0);

            spriteBatch.DrawString(font2, world.DisplayMatrix("vertices world transform \n into the world space") , new Vector2(950, yoffset * 1), Color.Black, 0, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 0);

            spriteBatch.DrawString(font2, cameraWorld.DisplayMatrix("cameras world transform \n our camera in the world"), new Vector2(950, yoffset * 2), Color.Black, 0, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 0);

            view = Matrix.Invert(cameraWorld);
            Matrix worldView = world * view;

            spriteBatch.DrawString(font2, view.DisplayMatrix("Inverted(camera) view transform \n for the math."), new Vector2(950, yoffset * 3), Color.Black, 0, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 0);

            spriteBatch.DrawString(font2, worldView.DisplayMatrix(" in actuality vertices are transfromed \n by world * view \n into view space."), new Vector2(950, yoffset * 4), Color.Black, 0, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 0);

            if (showMatrixOrientationLines)
            {
                float distance = 30;
                int yindex = 0;
                DrawOrientationLines(new Vector3(950 - distance, yoffset * yindex +10, 0), Matrix.Identity, distance); 
                yindex++;
                DrawOrientationLines(new Vector3(950- distance, yoffset * yindex + 10, 0), world, distance); 
                yindex++;
                DrawOrientationLines(new Vector3(950- distance, yoffset * yindex + 10, 0), cameraWorld, distance); 
                yindex++;
                DrawOrientationLines(new Vector3(950- distance, yoffset * yindex + 10, 0), view, distance); 
                yindex++;
                DrawOrientationLines(new Vector3(950- distance, yoffset * yindex + 10, 0), worldView, distance); 
                yindex++;
            }

            spriteBatch.End();
        }

        public void DrawTransformationSpacesOrientations(Matrix world, Matrix cameraWorld, Matrix projection, Vector3[] vertices, Vector2 vofset)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);

            for (int i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                if (showIdentitySpace)
                {
                    spriteBatch.DrawBasicPointWithMsg(font, $"v[{i}]", vofset + v.ToVector2(), Color.Black, 2);
                    spriteBatch.DrawBasicPointWithMsg(font, " Identity Local Space", vofset, Color.Black, 2);
                }

                v = Vector3.Transform(vertices[i], world);
                if (showWorldSpace)
                {
                    spriteBatch.DrawBasicPointWithMsg(font, $"v[{i}]", vofset + v.ToVector2(), Color.Green, 2);
                    spriteBatch.DrawBasicPointWithMsg(font, " World Space", vofset + world.Translation.ToVector2(), Color.Green, 2);
                }

                if (showCameraWorldTranslation)
                {
                    spriteBatch.DrawBasicPointWithMsg(font, $"cameraWorld", vofset + cameraWorld.Translation.ToVector2(), Color.Red, 3);
                }

                view = Matrix.Invert(cameraWorld);
                if (showInverseCameraTranslation)
                {
                    spriteBatch.DrawBasicPointWithMsg(font, " cameraView", vofset + view.Translation.ToVector2(), Color.Orange, 2);
                }

                Matrix worldView = world * view;
                v = Vector3.Transform(vertices[i], worldView);
                if (showViewSpace)
                {
                    spriteBatch.DrawBasicPointWithMsg(font, $"v[{i}]", vofset + v.ToVector2(), Color.Blue, 2);
                    spriteBatch.DrawBasicPointWithMsg(font, " (World*View) View Space", vofset + worldView.Translation.ToVector2(), Color.Blue, 2);
                    if(showTriangles)
                       DrawVerticesLikeTriangles(worldView, vofset.ToVector3(), Color.DarkBlue);
                }


                // for the sake of sanity for the moment we wont worry about how to display projection visually in 2d id have to make a fake projection matrixs to show how it works.
            }
            spriteBatch.End();
        }

        public void DrawOrientationLines(Vector3 drawOffset, Matrix m, float distance)
        {
            var v = m.Up * distance + drawOffset;
            spriteBatch.DrawBasicLine(drawOffset.ToVector2(), v.ToVector2(), 2, Color.Red);
             v = m.Right * distance + drawOffset;
            spriteBatch.DrawBasicLine(drawOffset.ToVector2(), v.ToVector2(), 2, Color.Blue);
             v = m.Forward * distance + drawOffset;
            spriteBatch.DrawBasicLine(drawOffset.ToVector2(), v.ToVector2(), 2, Color.Green);
        }

        public void DrawVerticesLikeTriangles(Matrix m, Vector3 offset, Color c)
        {
            var tl = (Vector3.Transform(verticesModelSpace[0], m) + offset);
            var tr = (Vector3.Transform(verticesModelSpace[1], m) + offset);
            var bl = (Vector3.Transform(verticesModelSpace[2], m) + offset);
            var br = (Vector3.Transform(verticesModelSpace[3], m) + offset);

            // triangle 0
            spriteBatch.DrawBasicLine(tl.ToVector2(), bl.ToVector2(), 1, c);
            spriteBatch.DrawBasicLine(bl.ToVector2(), tr.ToVector2(), 1, c);
            spriteBatch.DrawBasicLine(tr.ToVector2(), tl.ToVector2(), 1, c);
            // triangle 1
            spriteBatch.DrawBasicLine(tr.ToVector2(), bl.ToVector2(), 1, c);
            spriteBatch.DrawBasicLine(bl.ToVector2(), br.ToVector2(), 1, c);
            spriteBatch.DrawBasicLine(br.ToVector2(), tr.ToVector2(), 1, c);
            // triangle normal 0
            // triangle normal 1 ... we can use the same for both in this example.
            var n0 = Vector3.Normalize( Vector3.Cross(bl - tl, tr - bl) );
            var avg = ((tl + bl + tr) * .33f).ToVector2();
            var d0 = (n0 * 20).ToVector2();
            var d1 = (n0 * 10).ToVector2();
            //spriteBatch.DrawBasicLine(tr.ToVector2(), tl.ToVector2(), 1, c);
            spriteBatch.DrawBasicLine(avg, avg - d0, 1, c);
            spriteBatch.DrawBasicLine(avg, avg + d0, 1, Color.LightBlue);

            var tl2d = tl.ToVector2();
            var tr2d = tr.ToVector2();
            var bl2d = bl.ToVector2();
            var br2d = br.ToVector2();
            spriteBatch.DrawBasicLine(tl2d, tl2d - d0, 1, c);
            spriteBatch.DrawBasicLine(tr2d, tr2d - d0, 1, c);
            spriteBatch.DrawBasicLine(bl2d, bl2d - d0, 1, c);
            spriteBatch.DrawBasicLine(br2d, br2d - d0, 1, c);

        }

        //bool showIdentitySpace = true;
        //bool showWorldSpace = false;
        //bool showViewSpace = false;
        //bool showCameraWorldTranslation = false;
        //bool showInverseCameraTranslation = false;

        public void CreateMsgs()
        {
            // new TimedMsgAndCommand("", new Vector3(), " ");
            new TimedMsgAndCommand("world.ForwardChange", new Vector3(0, 0, 1)," Overview ");
            new TimedMsgAndCommand(" Overview \n Transformation Spaces and the World View Projection.");
            new TimedMsgAndCommand(" Transformation Spaces and the World View Projection.");
            new TimedMsgAndCommand(" Lets begin with some vertices centered around 0,0,0.");
            new TimedMsgAndCommand(" From Local or Model-(ing) space, Centered around the 0,0,0 system rotational origin");
            new TimedMsgAndCommand("modelSpaceOriginOffset", new Vector3(350, 350, 0), " ");
            new TimedMsgAndCommand( " For this example we move that offset so we can visualize that position easily.");
            new TimedMsgAndCommand("showWorldSpace = true", " ");
            new TimedMsgAndCommand(" Vertices are translated or transformed to some place in the world.");
            new TimedMsgAndCommand("world.Translation", new Vector3(250, 0, 0), " ");
            new TimedMsgAndCommand(" We call that the world space.");
            new TimedMsgAndCommand(" These spaces can be thought of as being defined or related to the origin they are offset from.");
            new TimedMsgAndCommand(" The matrices position aka translation is a new origin relative to the last.");
            new TimedMsgAndCommand
                (" " +
                "\n Matrix world = new Matrix" +
                "\n (" +
                $"\n 1, 0, 0, 0," +
                $"\n 0, 1, 0, 0," +
                $"\n 0, 0, 1, 0," +
                $"\n x, y, z, 1" +
                "); "
                );
            new TimedMsgAndCommand(" It offsets translates the vertices positions that are assocated to it.");
            new TimedMsgAndCommand("world.Translation", new Vector3(250, -250, 0), " ");
            new TimedMsgAndCommand(" They are transformed again or \n Re-oriented by the camera in our world.");
            new TimedMsgAndCommand("cameraWorld.Translation", new Vector3(150, 0, 0), " ");
            new TimedMsgAndCommand("showCameraWorldTranslation = true", " ");
            new TimedMsgAndCommand(" To be viewed from it.");
            new TimedMsgAndCommand(" While the camera is in world space");
            new TimedMsgAndCommand(" its called a world camera ");
            new TimedMsgAndCommand(" When we invert it its called a view matrix.");
            new TimedMsgAndCommand("showInverseCameraTranslation = true", "  ");
            new TimedMsgAndCommand(" The view is a Inverted world matrix, used so the math will work. ");
            new TimedMsgAndCommand("cameraWorld.Translation", new Vector3(200, 0, 0), " ");
            new TimedMsgAndCommand("cameraWorld.Translation", new Vector3(250, 0, 0), " ");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" While typically view space is said to be created from the view matrix.");
            new TimedMsgAndCommand(" In actual use the cameras view * world matrix forms the view space.");
            new TimedMsgAndCommand(" Which is used to alter the vertices into view space.");
            new TimedMsgAndCommand("showViewSpace = true", " ");
            new TimedMsgAndCommand(" For example when we rotate the camera in world space.");
            new TimedMsgAndCommand(" We expect the things we to move left or right.");
            new TimedMsgAndCommand(" This is with respect to our local unmoving screen  ....");
            new TimedMsgAndCommand("cameraWorld.ZAxisChange", new Vector3(-1, 4, 0), " space");
            new TimedMsgAndCommand("cameraWorld.ZAxisChange", new Vector3(-1, 2, 0), " ");
            new TimedMsgAndCommand("cameraWorld.ZAxisChange", new Vector3(-1, 1, 0), " ");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" This is how we transform (vertices) by (matrices) thru or into new (spaces).");
            new TimedMsgAndCommand("cameraWorld.ZAxisChange", new Vector3(-1, 3, 0), " ");
            new TimedMsgAndCommand(" Before getting to the view space vertices should be in world space. ");
            new TimedMsgAndCommand(" Otherwise everything would stack up on itself.");
            new TimedMsgAndCommand(" This is to keep things orderly. ");
            new TimedMsgAndCommand("cameraWorld.ZAxisChange", new Vector3(0, 1, 0), " ");
            new TimedMsgAndCommand(" The space is were the vertices are said to lie after being transformed by the matrices.");
            new TimedMsgAndCommand(" So we start in model space. we have a model in this case a quad or a pair of triangles.");
            new TimedMsgAndCommand("showTriangles = true", " ");
            new TimedMsgAndCommand(" ");

            new TimedMsgAndCommand(" We move those by each world transform that represnts a instance of them to the world.");
            new TimedMsgAndCommand(" All of those are transformed by the same view \n by multiplying the view by the world * the matrices.");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" Lets now talk a moment about the matrices themselves and the elements that make them up.");
            new TimedMsgAndCommand(" A breif overview of The matrice layout in monogame.");
            new TimedMsgAndCommand(" Matrices describe full Orientations that means a combination of \n position,\n rotation \n scale.");
            new TimedMsgAndCommand(" position aka... translation.");
            new TimedMsgAndCommand
    (" " +
    "\n Matrix world = new Matrix" +
    "\n (" +
    $"\n 1, 0, 0, 0," +
    $"\n 0, 1, 0, 0," +
    $"\n 0, 0, 1, 0," +
    $"\n x, y, z, 1    translation aka position" +
    "); "
    );
            new TimedMsgAndCommand(" rotation aka transform around a axis.");
            new TimedMsgAndCommand
    (" " +
    "\n Matrix world = new Matrix" +
    "\n (" +
    $"\n x, y, z, 0,   Rotational X axis... Right Normal Vector" +
    $"\n 0, 1, 0, 0,   " +
    $"\n 0, 0, 1, 0,  " +
    $"\n 0, 0, 0, 1" +
    "); "
    );
            new TimedMsgAndCommand
(" " +
"\n Matrix world = new Matrix" +
"\n (" +
$"\n 1, 0, 0, 0,   " +
$"\n x, y, z, 0,   Rotational Y axis... Up Normal Vector" +
$"\n 0, 0, 1, 0,   " +
$"\n 0, 0, 0, 1" +
"); "
);
            new TimedMsgAndCommand
(" " +
"\n Matrix world = new Matrix" +
"\n (" +
$"\n 1, 0, 0, 0,   " +
$"\n 0, 1, 0, 0,   " +
$"\n x, y, z, 0,   Rotational Z axis... Forward Normal Vector" +
$"\n 0, 0, 0, 1" +
"); "
);
            new TimedMsgAndCommand(" A transformation is more then one thing rotation and translation typically \n but it can just mean a rotation.");
            new TimedMsgAndCommand(" scale.");
            new TimedMsgAndCommand
    (" " +
    "\n Matrix world = new Matrix" +
    "\n (" +
    $"\n x, 0, 0, 0," +
    $"\n 0, y, 0, 0," +
    $"\n 0, 0, z, 0," +
    $"\n 0, 0, 0, 1" +
    "); "
    );
            new TimedMsgAndCommand(" So the matrixs rows here have meanings.");
            new TimedMsgAndCommand(" The Forward Up Right affect the opposite vectors when a system rotation is made.");
            new TimedMsgAndCommand(" These 3 vectors must be perpendicular to each other aka.. crosses, or the matrice is said to be ");
            new TimedMsgAndCommand(" De-Normalized. Or no longer Orthogonal. Basically its messed up. ");
            new TimedMsgAndCommand(" You may have noticed the 1 1 1 1 runing down the diagnal. \n along the scalar components and seen the 1 at the bottom right is odd.");
            new TimedMsgAndCommand(" This is what is known as the W component.\n It is needed to differentiate between normals and positions it's also a math construct.");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" Matrixs heavily rely on cross products for rotation and normalized unit length vectors.");
            new TimedMsgAndCommand(" You should already be familiar with cross products from the previous example.");
            new TimedMsgAndCommand( " If not go take a quick look and come back...");
            new TimedMsgAndCommand("showMatrixOrientationLines = true", " ");

            new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-0, +3, 0), " When we perform a 2D rotation.");
            new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-1, +2, 0), " We rotate upon a single axis.");
            new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-2, +1, 0), " The Z axis.");
            new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-3, +0, 0), " The Z axis is related to the Forward Vector of a matrix."); //
            new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-2, -1, 0), " The matrix elements M31, M32, M33  respectively.");
            new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-1, -2, 0), " This is the Forward's (x,y,z)");
            new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-0, -3, 0), " That vector tells you in what direction the Forward axis points."); //
            new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-1, -2, 0), " Z axis rotations affect the up and right primarily  Matrix.CreateRotationZ(..)");
            new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-2, -1, 0), " When you use Matrix.CreateWorld.");
            new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-3, +0, 0), " Setting the Up vector creates a Z axis rotation.");
            new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-2, +1, 0), " Changing the Forward Vector ");
            new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-1, +2, 0), " Changing the Forward Vector");
            new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-1, +3, 0), " creates a Y and or X axis rotation.");
            new TimedMsgAndCommand("world.ForwardChange", new Vector3(-1, +1, 3), " creates a Y and or X axis rotation.");
            new TimedMsgAndCommand("world.ForwardChange", new Vector3(-1, +1, 1), " creates a Y and or X axis rotation.");
            new TimedMsgAndCommand("world.ForwardChange", new Vector3(-1, +1, -1), " ");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" Remember however these vectors must be perpendicular to each other in the matrix.");
            new TimedMsgAndCommand(" That is achieved by the cross product");
            new TimedMsgAndCommand(" Up = Vector3.Cross( Forward, Right ); ");
            new TimedMsgAndCommand(" Right = Vector3.Cross( Forward, Up ); ");
            new TimedMsgAndCommand(" Forward = Vector3.Cross( Right, Up ); ");
            new TimedMsgAndCommand(" These vectors must also be normalized Forward = Vector3.Normalize( Forward ).");

            new TimedMsgAndCommand(" With this lets momentarily digress.");
            new TimedMsgAndCommand(" To the illustration of our triangles and the previous topic of dot and cross products.");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" As you know triangles in graphics are typically assigned surface normals.");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" These are generated by the cross product of two normals.");
            new TimedMsgAndCommand(" Depending on the order of input to the cross product. (A,B) or (B,A)");
            new TimedMsgAndCommand(" You will get a Normal off the Triangle surface either In or Out aka ...");
            new TimedMsgAndCommand(" Clock wise or Counter Clock wise.");
            new TimedMsgAndCommand(" This normal is heavily used in lighting and culling on the gpu.");
            new TimedMsgAndCommand(" As well it can be used for collision detection on the cpu.");
            new TimedMsgAndCommand(" In your Game1 the RasterizerState can be set to respect the cw or ccw winding.");
            new TimedMsgAndCommand(" This is to perform a gpu culling technique known as 'BackFace Removal' ");
            new TimedMsgAndCommand(" As you can see the cross product is essential to graphics as is the dot product.");
            new TimedMsgAndCommand(" As well as it is to maintain the orthagonality of the matrix vectors.");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" Now you might say well this is all interesting but how do i use these matrices?");
            new TimedMsgAndCommand(" Surely im not expected to fill in all those values by hand. ");
            new TimedMsgAndCommand(" Of course you are not.");
            new TimedMsgAndCommand(" MonoGame has two particular Matrix functions that will do nearly everything.");
            new TimedMsgAndCommand(" Matrix.CreateWorld( position, forward, up);");
            new TimedMsgAndCommand(" Matrix.CreateLookAt( position, target, up);");
            new TimedMsgAndCommand(" Now one very important thing here ... the TARGET parameter ==");
            new TimedMsgAndCommand(" forward + position.   or   targetWorldPosition - playerWorldPosition thats were to look.");
            new TimedMsgAndCommand(" In 2D when you use the forward direction you can actually use a sine cosine.");
            new TimedMsgAndCommand(" or you can make directions based on positions but \n the target is offset from the position inside CreateLookAt");
            new TimedMsgAndCommand(" So you have to be aware of the difference");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" Projection matrix's meh a fancy complicated scaling matrix. ");
            new TimedMsgAndCommand(" fire and forget... The usage of this is simple.");
            new TimedMsgAndCommand(" You make it one time in say your constructor or load function typically.");
            new TimedMsgAndCommand(" After you move transform your camera a bit in update.");
            new TimedMsgAndCommand(" You multiply it with the view matrix you derived from it.");
            new TimedMsgAndCommand(" vp = view * projection;");
            new TimedMsgAndCommand(" Typically you simply pass all 3 of these seperately to a shader though individually.");
            new TimedMsgAndCommand(" There are two types of projection.");
            new TimedMsgAndCommand(" Orthographic which is a box like view like a cad program.");
            new TimedMsgAndCommand(" Perspective with the 3D feild of depth distortion.");
            new TimedMsgAndCommand(" To understand this you should watch a video on painting ... yes painting.");
            new TimedMsgAndCommand(" search for Focal Point ... ");
            new TimedMsgAndCommand(" Also another related topic is the painters algorithm.");
            new TimedMsgAndCommand(" Both are fundamentlly relevant to 3d rendering.");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" Now maybe ill get to setting states here But i think for now this covers the topic.");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" Now for the bonus stuff.");
            new TimedMsgAndCommand
(" " +
"\n Matrix world = new Matrix" +
"\n (" +
$"\n 1, 0, 0, X,  First rule of ScaleRotationTranslation club, you don't talk about...  " +  
$"\n 0, 1, 0, Y,  the Skew Matrix  " +
$"\n 0, 0, 1, Z,  Cause its pretty  " +
$"\n 0, 0, 0, 1   Worthless  " +
"); "
);
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" ");

            //new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-0, +3, 0), " ");
            //new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-1, +2, 0), " ");
            //new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-2, +1, 0), " ");
            //new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-3, +0, 0), " "); //
            //new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-2, -1, 0), " ");
            //new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-1, -2, 0), " ");
            //new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-0, -3, 0), " "); //
            //new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-1, -2, 0), " ");
            //new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-2, -1, 0), " ");
            //new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-3, +0, 0), " ");
            //new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-2, +1, 0), " ");
            //new TimedMsgAndCommand("world.ZAxisChange", new Vector3(-1, +2, 0), " ");

            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand("", new Vector3(), " ");
            new TimedMsgAndCommand("", new Vector3(), " ");
            new TimedMsgAndCommand("", new Vector3(), " ");
            new TimedMsgAndCommand("", new Vector3(), " ");
            new TimedMsgAndCommand("", new Vector3(), " ");
            new TimedMsgAndCommand("", new Vector3(), " ");
            new TimedMsgAndCommand("", new Vector3(), " ");
            new TimedMsgAndCommand("", new Vector3(), " ");
            new TimedMsgAndCommand("", new Vector3(), " ");
            new TimedMsgAndCommand("", new Vector3(), " ");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" ");
            new TimedMsgAndCommand(" ");

            //new TimedMsgAndCommand(" ");
            //new TimedMsgAndCommand(" ");
            //new TimedMsgAndCommand(" ");
            //new TimedMsgAndCommand("", new Vector3(), " ");
            //new TimedMsgAndCommand("", new Vector3(), " ");
            //new TimedMsgAndCommand("", new Vector3(), " ");
            //new TimedMsgAndCommand("", new Vector3(), " ");
            //new TimedMsgAndCommand("", new Vector3(), " ");
            //new TimedMsgAndCommand("", new Vector3(), " ");
            //new TimedMsgAndCommand("", new Vector3(), " ");

        }

        public void ExecuteCommand(TimedMsgAndCommand tmc)
        {
            if (tmc.order == "modelSpaceOriginOffset")
            {
                modelSpaceOriginOffset = tmc.changeVector;
            }
            if (tmc.order == "world.Translation")
            {
                world.Translation = tmc.changeVector;
            }
            if (tmc.order == "world.Forward")
            {
                world.Forward = Vector3.Normalize(tmc.changeVector);
            }
            if (tmc.order == "cameraWorld.Translation")
            {
                cameraWorld.Translation = tmc.changeVector;
            }
            if (tmc.order == "cameraWorld.Forward")
            {
                cameraWorld.Forward = Vector3.Normalize(tmc.changeVector);
            }

            if (tmc.order == "world.ZAxisChange")
            {
                world = Matrix.CreateWorld(world.Translation, world.Forward, tmc.changeVector);
            }
            if (tmc.order == "cameraWorld.ZAxisChange")
            {
                cameraWorld = Matrix.CreateWorld(cameraWorld.Translation, cameraWorld.Forward, tmc.changeVector);
            }
            if (tmc.order == "world.ForwardChange")
            {
                world = Matrix.CreateWorld(world.Translation, tmc.changeVector, world.Up);
            }
            if (tmc.order == "cameraWorld.ForwardChange")
            {
                cameraWorld = Matrix.CreateWorld(cameraWorld.Translation, tmc.changeVector, cameraWorld.Up );
            }

            
            if (tmc.order == "showTriangles = true")
                showTriangles = true;
            if (tmc.order == "showTriangles = false")
                showTriangles = false;

            if (tmc.order == "showMatrixOrientationLines = true")
                showMatrixOrientationLines = true;
            if (tmc.order == "showMatrixOrientationLines = false")
                showMatrixOrientationLines = false;

            if (tmc.order == "showIdentitySpace = true")
                showIdentitySpace = true;
            if (tmc.order == "showIdentitySpace = false")
                showIdentitySpace = false;

            if (tmc.order == "showWorldSpace = true")
                showWorldSpace = true;
            if (tmc.order == "showWorldSpace = false")
                showWorldSpace = false;

            if (tmc.order == "showViewSpace = true")
                showViewSpace = true;
            if (tmc.order == "showViewSpace = false")
                showViewSpace = false;

            if (tmc.order == "showCameraWorldTranslation = true")
                showCameraWorldTranslation = true;
            if (tmc.order == "showCameraWorldTranslation = false")
                showCameraWorldTranslation = false;

            if (tmc.order == "showInverseCameraTranslation = true")
                showInverseCameraTranslation = true;
            if (tmc.order == "showInverseCameraTranslation = false")
                showInverseCameraTranslation = false;

            
        }

        public class TimedMsgAndCommand
        {
            public string msg = "";
            public string order = "";
            public Vector3 changeVector = Vector3.Zero;

            public static List<TimedMsgAndCommand> cmds = new List<TimedMsgAndCommand>();

            public TimedMsgAndCommand( string msg)
            {
                SelfRegister("", new Vector3(0,0,0), msg);
            }
            public TimedMsgAndCommand(string order, string msg)
            {
                SelfRegister(order, new Vector3(0, 0, 0), msg);
            }
            public TimedMsgAndCommand( string order, Vector3 changeVector, string msg) 
            {
                SelfRegister(order, changeVector, msg);
            }
            private void SelfRegister( string order, Vector3 changeVector, string msg)
            {
                this.order = order;
                this.changeVector = changeVector;
                this.msg = msg;
                cmds.Add(this);
            }
        }
    }
}
